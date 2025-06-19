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
using System.Windows.Forms;

using Nuclex.Support;
using Nuclex.Support.Threading;

namespace Nuclex.Windows.Forms.ViewModels {

  /// <summary>View model that can execute tasks in a background thread</summary>
  public abstract class ThreadedViewModel : Observable, IDisposable {

    #region class ViewModelThreadRunner

    /// <summary>Thread runner for the threaded view model</summary>
    private class ViewModelThreadRunner : ThreadRunner {

      /// <summary>Initializes a new thread runner for the threaded view model</summary>
      public ViewModelThreadRunner(ThreadedViewModel viewModel) {
        this.viewModel = viewModel;
      }

      /// <summary>Reports an error</summary>
      /// <param name="exception">Error that will be reported</param>
      protected override void ReportError(Exception exception) {
        this.viewModel.reportErrorFromThread(exception);
      }

      /// <summary>Called when the status of the busy flag changes</summary>
      protected override void BusyChanged() {
        this.viewModel.OnIsBusyChanged();
      }

      /// <summary>View model the thread runner belongs to</summary>
      private ThreadedViewModel viewModel;

    }

    #endregion // class ViewModelThreadRunner

    /// <summary>Initializes a new view model for background processing</summary>
    /// <param name="uiContext">
    ///   UI dispatcher that can be used to run callbacks in the UI thread
    /// </param>
    protected ThreadedViewModel(ISynchronizeInvoke uiContext = null) {
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

      this.reportErrorDelegate = new Action<Exception>(ReportError);

      this.threadRunner = new ViewModelThreadRunner(this);
    }

    /// <summary>Immediately releases all resources owned by the instance</summary>
    public virtual void Dispose() {
      if(this.threadRunner != null) {
        this.threadRunner.Dispose();
        this.threadRunner = null;
      }
    }

    /// <summary>Whether the view model is currently busy executing a task</summary>
    public bool IsBusy {
      get { return this.threadRunner.IsBusy; }
    }

    /// <summary>Reports an error to the user</summary>
    /// <param name="exception">Error that will be reported</param>
    /// <remarks>
    ///   <para>
    ///     You can use this method as a default handling method for your own error reporting
    ///     (displaying the error to the user, logging it or whatever else is appropriate).
    ///   </para>
    ///   <para>
    ///     When <see cref="RunInBackground(System.Action)" /> is used, this method will also
    ///     be called in case an exception within the asynchronously running code goes unhandled.
    ///     This choice was made because, in the context of UI code, you would wrap any
    ///     operations that might fail in a try..catch pair anyway in order to inform
    ///     the user instead of aborting the entire application.
    ///   </para>
    /// </remarks>
    protected abstract void ReportError(Exception exception);

    /// <summary>Executes the specified operation in the background</summary>
    /// <param name="action">Action that will be executed in the background</param>
    protected void RunInBackground(Action action) {
      this.threadRunner.RunInBackground(action);
    }

    /// <summary>Executes the specified operation in the background</summary>
    /// <param name="action">Action that will be executed in the background</param>
    protected void RunInBackground(CancellableAction action) {
      this.threadRunner.RunInBackground(action);
    }

    /// <summary>Executes the specified operation in the background</summary>
    /// <param name="action">Action that will be executed in the background</param>
    /// <param name="parameter1">Parameter that will be passed to the action</param>
    protected void RunInBackground<P1>(Action<P1> action, P1 parameter1) {
      this.threadRunner.RunInBackground(action, parameter1);
    }

    /// <summary>Executes the specified operation in the background</summary>
    /// <param name="action">Action that will be executed in the background</param>
    /// <param name="parameter1">Parameter that will be passed to the action</param>
    protected void RunInBackground<P1>(CancellableAction<P1> action, P1 parameter1) {
      this.threadRunner.RunInBackground(action, parameter1);
    }

    /// <summary>Executes the specified operation in the background</summary>
    /// <param name="action">Action that will be executed in the background</param>
    /// <param name="parameter1">First parameter that will be passed to the action</param>
    /// <param name="parameter2">Second parameter that will be passed to the action</param>
    protected void RunInBackground<P1, P2>(Action<P1, P2> action, P1 parameter1, P2 parameter2) {
      this.threadRunner.RunInBackground(action, parameter1, parameter2);
    }

    /// <summary>Executes the specified operation in the background</summary>
    /// <param name="action">Action that will be executed in the background</param>
    /// <param name="parameter1">First parameter that will be passed to the action</param>
    /// <param name="parameter2">Second parameter that will be passed to the action</param>
    protected void RunInBackground<P1, P2>(
      CancellableAction<P1, P2> action, P1 parameter1, P2 parameter2
    ) {
      this.threadRunner.RunInBackground(action, parameter1, parameter2);
    }

    /// <summary>Cancels the currently running background operation</summary>
    protected void CancelBackgroundOperation() {
      this.threadRunner.CancelBackgroundOperation();
    }

    /// <summary>Cancels all queued and the currently running background operation</summary>
    protected void CancelAllBackgroundOperations() {
      this.threadRunner.CancelAllBackgroundOperations();
    }

    /// <summary>Whether the background operation has been cancelled</summary>
    //[Obsolete("Please use a method accepting a cancellation token instead of using this")]
    protected bool IsBackgroundOperationCancelled {
      get { return this.threadRunner.IsBackgroundOperationCancelled; }
    }

    /// <summary>Throws an exception if the background operation was cancelled</summary>
    //[Obsolete("Please use a method accepting a cancellation token instead of using this")]
    protected void ThrowIfBackgroundOperationCancelled() {
      this.threadRunner.ThrowIfBackgroundOperationCancelled();
    }

    /// <summary>Executes the specified action in the UI thread</summary>
    /// <param name="action">Action that will be executed in the UI thread</param>
    protected void RunInUIThread(Action action) {
      this.uiContext.Invoke(action, EmptyObjectArray);
    }

    /// <summary>Executes the specified action in the UI thread</summary>
    /// <param name="action">Action that will be executed in the UI thread</param>
    /// <param name="parameter1">Parameter that will be passed to the action</param>
    protected void RunInUIThread<P1>(Action<P1> action, P1 parameter1) {
      this.uiContext.Invoke(action, new object[1] { parameter1 });
    }

    /// <summary>Executes the specified action in the UI thread</summary>
    /// <param name="action">Action that will be executed in the UI thread</param>
    /// <param name="parameter1">First parameter that will be passed to the action</param>
    /// <param name="parameter2">Second parameter that will be passed to the action</param>
    protected void RunInUIThread<P1, P2>(Action<P1, P2> action, P1 parameter1, P2 parameter2) {
      this.uiContext.Invoke(action, new object[2] { parameter1, parameter2 });
    }

    /// <summary>Called when the thread runner's busy flag changes</summary>
    protected virtual void OnIsBusyChanged() {
      OnPropertyChanged(nameof(IsBusy));
    }

    // For the ThreadedAction class - there should be a better way!
    /// <summary>Thread runner that manages the view model's thread</summary>
    internal ThreadRunner ThreadRunner {
      get { return this.threadRunner; }
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

    /// <summary>An array of zero objects</summary>
    private static readonly object[] EmptyObjectArray = new object[0];

    /// <summary>UI dispatcher of the thread in which the view runs</summary>
    private ISynchronizeInvoke uiContext;
    /// <summary>Delegate for the ReportError() method</summary>
    private Action<Exception> reportErrorDelegate;
    /// <summary>Thread runner that manages the view model's thread</summary>
    private ViewModelThreadRunner threadRunner;

  }

} // namespace Nuclex.Windows.Forms.ViewModels
