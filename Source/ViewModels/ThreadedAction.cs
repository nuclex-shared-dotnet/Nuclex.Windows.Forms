#region Apache License 2.0
/*
Nuclex .NET Framework
Copyright (C) 2002-2024 Markus Ewald / Nuclex Development Labs

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion // Apache License 2.0

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Nuclex.Support;
using Nuclex.Support.Threading;

// Possible problem:
//
// After Run() is called, the action may not actually run if
// it is using another thread runner and that one is cancelled.
//
// Thus, a second call to Run() has to schedule the action again,
// even if it might already be scheduled, but should also not execute
// the action a second time if is was indeed still scheduled.

namespace Nuclex.Windows.Forms.ViewModels {

  /// <summary>Encapsulates an action that can run in a thread</summary>
  /// <remarks>
  ///   <para>
	///     Sometimes a view model wants to allow multiple actions to take place
	///     at the same time. Think multiple panels on the view require updating
	///     from a web service - you can make both requests at the same time
	///     instead of sequentially.
  ///   </para>
  ///   <para>
  ///     This class is also of use for things that need to be done sequentially
	///     by sharing the thread runner of the threaded view model. That way,
	///     you still have cancellable actions you can run at will and they
	///     automatically queue themselves to be executed one after another.
  ///   </para>
  /// </remarks>
  public abstract class ThreadedAction : Observable, IDisposable {

    #region class ThreadedActionThreadRunner

    /// <summary>Thread runner for the threaded action</summary>
    private class ThreadedActionThreadRunner : ThreadRunner {

      /// <summary>Initializes a new thread runner for the threaded view model</summary>
      public ThreadedActionThreadRunner(ThreadedAction viewModel) {
        this.threadedAction = viewModel;
      }

      /// <summary>Reports an error</summary>
      /// <param name="exception">Error that will be reported</param>
      protected override void ReportError(Exception exception) {
        this.threadedAction.reportErrorFromThread(exception);
      }

      /// <summary>Called when the status of the busy flag changes</summary>
      protected override void BusyChanged() {
        // Narf. Can't use this.
      }

      /// <summary>View model the thread runner belongs to</summary>
      private ThreadedAction threadedAction;

    }

    #endregion // class ThreadedActionThreadRunner

    /// <summary>Initializes all common fields of the instance</summary>
    private ThreadedAction() {
      this.callRunIfNotCancelledDelegate = new Action<CancellationTokenSource>(
        callThreadedExecuteIfNotCancelled
      );
      this.reportErrorDelegate = new Action<Exception>(ReportError);
    }

    /// <summary>Initializes a threaded action that uses its own thread runner</summary>
    public ThreadedAction(ISynchronizeInvoke uiContext = null) : this() {
#if NET6_0_OR_GREATER
      if(OperatingSystem.IsWindows()) {
#endif
      if(uiContext == null) {
        this.uiContext = LateCheckedSynchronizer.GetMainWindow();
        if(this.uiContext == null) {
          this.uiContext = new LateCheckedSynchronizer(updateUiContext);
        }
      } else {
        this.uiContext = uiContext;
      }
#if NET6_0_OR_GREATER
      }
#endif

      this.ownThreadRunner = new ThreadedActionThreadRunner(this);
    }

    /// <summary>
    ///   Initializes a threaded action that uses the view model's thread runner
    /// </summary>
    /// <param name="viewModel">View model whose thread runner will be used</param>
    /// <param name="uiContext">
    ///   UI dispatcher that can be used to run callbacks in the UI thread
    /// </param>
    public ThreadedAction(
      ThreadedViewModel viewModel, ISynchronizeInvoke uiContext = null
    ) : this() {
#if NET6_0_OR_GREATER
      if(OperatingSystem.IsWindows()) {
#endif
      if(uiContext == null) {
        this.uiContext = LateCheckedSynchronizer.GetMainWindow();
        if(this.uiContext == null) {
          this.uiContext = new LateCheckedSynchronizer(updateUiContext);
        }
      } else {
        this.uiContext = uiContext;
      }
#if NET6_0_OR_GREATER
      }
#endif

      this.externalThreadRunner = viewModel.ThreadRunner;
    }

    /// <summary>Immediately releases all resources owned by the instance</summary>
    public virtual void Dispose() {
      if(this.isBusy) {
        Cancel();
      }
      if(this.ownThreadRunner != null) {
        this.ownThreadRunner.Dispose();
        this.ownThreadRunner = null;
      }
      if(this.currentCancellationTokenSource != null) {
        this.currentCancellationTokenSource.Dispose();
        this.currentCancellationTokenSource = null;
      }
    }

    /// <summary>Whether the view model is currently busy executing a task</summary>
    public bool IsBusy {
      get { return this.isBusy; }
      private set {
        if(value != this.isBusy) {
          this.isBusy = value;
          OnPropertyChanged(nameof(IsBusy));
        }
      }
    }

    /// <summary>Cancels the running background task, if any</summary>
    public void Cancel() {
      lock(this.runningTaskSyncRoot) {

        // If the background task is not running, do nothing. This also allows
        // us to avoid needless recreation of the same cancellation token source.
        if(!this.isBusy) {
          return;
        }

        // If a task is currently running, cancel it
        if(this.isRunning) {
          if(this.currentCancellationTokenSource != null) {
            this.currentCancellationTokenSource.Cancel();
            this.currentCancellationTokenSource = null;
          }
        }

        // If the task was scheduled to be repeated, we also have to mark
        // the upcoming cancellation token source as canceled because the scheduled
        // run will still be happening (it will just cancel out immediately).
        if(this.nextCancellationTokenSource != null) {
          this.nextCancellationTokenSource.Cancel();
          this.nextCancellationTokenSource = null;
        }
        this.isScheduledAgain = false;

        // If the task was not running, we can clear the busy state because it
        // is not going to reach the running state.
        if(!this.isRunning) {
          this.isBusy = false;
        }

      }
    }

    /// <summary>
    ///   Starts the task, cancelling the running task before doing so
    /// </summary>
    public void Restart() {
      bool reportBusyChange = false;

      lock(this.runningTaskSyncRoot) {

        // If we're already in the execution phase, schedule another execution right
        // after this one is finished (because now, data might have changed after
        // execution has finished).
        if(this.isRunning) {
          //System.Diagnostics.Debug.WriteLine("Restart() - interrupting execution");
          if(this.currentCancellationTokenSource != null) {
            this.currentCancellationTokenSource.Cancel();
          }

          this.currentCancellationTokenSource = this.nextCancellationTokenSource;
          this.nextCancellationTokenSource = null;
          this.isScheduledAgain = false;
        }

        // If there's no cancellation token source, create one. If an execution
        // was already scheduled and the cancellation token source is still valid,
        // then reuse that in order to be able to cancel all scheduled executions.
        if(this.currentCancellationTokenSource == null) {
          //System.Diagnostics.Debug.WriteLine("Restart() - creating new cancellation token");
          this.currentCancellationTokenSource = new CancellationTokenSource();
        }

        // Schedule another execution of the action
        scheduleExecution();

        reportBusyChange = (this.isBusy == false);
        this.isBusy = true;
      }

      if(reportBusyChange) {
        OnPropertyChanged(nameof(IsBusy));
      }
    }

    /// <summary>Starts the task</summary>
    public void Start() {
      bool reportBusyChange = false;

      lock(this.runningTaskSyncRoot) {

        // If we're already in the execution phase, schedule another execution right
        // after this one is finished (because now, data might have changed after
        // execution has finished).
        if(this.isRunning) {

          // If we already created a new cancellation token source, keep it,
          // otherwise create a new one for the next execution
          if(!this.isScheduledAgain) {
            this.nextCancellationTokenSource = new CancellationTokenSource();
            this.isScheduledAgain = true;
          }

        } else {

          // If there's no cancellation token source, create one. If an execution
          // was already scheduled and the cancellation token source is still valid,
          // then reuse that in order to be able to cancel all scheduled executions.
          if(this.currentCancellationTokenSource == null) {
            this.currentCancellationTokenSource = new CancellationTokenSource();
          }

          // Schedule another execution of the action
          scheduleExecution();

        }

        reportBusyChange = (this.isBusy == false);
        this.isBusy = true;
      }

      if(reportBusyChange) {
        OnPropertyChanged(nameof(IsBusy));
      }
    }

    /// <summary>Reports an error</summary>
    /// <param name="exception">Error that will be reported</param>
    protected abstract void ReportError(Exception exception);

    /// <summary>Executes the threaded action from the background thread</summary>
    /// <param name="cancellationToken">Token by which execution can be canceled</param>
    protected abstract void Run(CancellationToken cancellationToken);

    /// <summary>
    ///   Calls the Run() method from the background thread and manages the flags
    /// </summary>
    /// <param name="cancellationTokenSource"></param>
    private void callThreadedExecuteIfNotCancelled(
      CancellationTokenSource cancellationTokenSource
    ) {
      lock(this) {
        if(cancellationTokenSource.Token.IsCancellationRequested) {
          return;
        }

        this.isRunning = true;
      }

      try {
        Run(cancellationTokenSource.Token);
      }
      finally {
        bool reportBusyChange = false;

        lock(this) {
          this.isRunning = false;

          // Cancel the current cancellation token because this execution may have
          // been scheduled multiple times (there's no way for the Run() method to
          // know if the currently scheduled execution was cancelled, so it is forced
          // to reschedule on each call - accepting redundant schedules).
          cancellationTokenSource.Cancel();

          // Pick the next cancellation token source. Normally it is null, but this
          // is more elegant because we can avoid an while if statement this way :)
          this.currentCancellationTokenSource = nextCancellationTokenSource;
          this.nextCancellationTokenSource = null;

          // If Start() was called while we were executing, another execution is required
          // (because the data may have changed during the call to Start()).
          if(this.isScheduledAgain) {
            this.isScheduledAgain = false;
            scheduleExecution();
          } else { // We're idle now
            reportBusyChange = (this.isBusy == true);
            this.isBusy = false;
          }
        }

        if(reportBusyChange) {
          OnPropertyChanged(nameof(IsBusy));
        }
      }
    }

    /// <summary>Schedules one execution of the action</summary>
    private void scheduleExecution() {
      //System.Diagnostics.Debug.WriteLine("Scheduling execution");

      ThreadRunner runner = this.externalThreadRunner;
      if(runner != null) {
        runner.RunInBackground(
          this.callRunIfNotCancelledDelegate, this.currentCancellationTokenSource
        );
      }

      runner = this.ownThreadRunner;
      if(runner != null) {
        runner.RunInBackground(
          this.callRunIfNotCancelledDelegate, this.currentCancellationTokenSource
        );
      }
    }

    /// <summary>Reports an error that occurred in the runner's background thread</summary>
    /// <param name="exception">Exception that the thread has encountered</param>
    private void reportErrorFromThread(Exception exception) {
      this.uiContext.Invoke(this.reportErrorDelegate, new object[1] { exception });
    }

    /// <summary>Sets the UI context that will be used by the threaded action</summary>
    /// <param name="uiContext">The UI context the threaded action will use</param>
    private void updateUiContext(ISynchronizeInvoke uiContext) {
      this.uiContext = uiContext;
    }

    /// <summary>Synchronization context of the thread in which the view runs</summary>
    private ISynchronizeInvoke uiContext;
    /// <summary>Delegate for the ReportError() method</summary>
    private Action<Exception> reportErrorDelegate;
    /// <summary>Delegate for the callThreadedExecuteIfNotCancelled() method</summary>
    private Action<CancellationTokenSource> callRunIfNotCancelledDelegate;

    /// <summary>Thread runner on which the action can run its background task</summary>
    private ThreadedActionThreadRunner ownThreadRunner;
    /// <summary>
    ///   External thread runner on which the action runs its background task if assigned
    /// </summary>
    private ThreadRunner externalThreadRunner;

    /// <summary>Synchronization root for the threaded execute method</summary>
    private object runningTaskSyncRoot = new object();
    /// <summary>Used to cancel the currently running task</summary>
    private CancellationTokenSource currentCancellationTokenSource;
    /// <summary>Used to cancel the upcoming task if a re-run was scheduled</summary>
    private CancellationTokenSource nextCancellationTokenSource;
    /// <summary>Whether the background task is running or waiting to run</summary>
    private volatile bool isBusy;
    /// <summary>Whether execution is taking place right now</summary>
    /// <remarks>
    ///   If this flag is set and the Start() method is called, another run needs to
    ///   be scheduled.
    /// </remarks>
    private bool isRunning;
    /// <summary>Whether run was called while the action was already running</summary>
    private bool isScheduledAgain;

  }

} // namespace Nuclex.Windows.Forms.ViewModels
