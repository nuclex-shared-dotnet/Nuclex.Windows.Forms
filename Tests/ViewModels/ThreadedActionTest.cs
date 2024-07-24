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

#if UNITTEST

using System;
using System.ComponentModel;
using System.Threading;

using NUnit.Framework;

namespace Nuclex.Windows.Forms.ViewModels {

  /// <summary>Unit test for the threaded action class</summary>
  [TestFixture]
  public class ThreadedActionTest {

    #region class DummyContext

    /// <summary>Synchronization context that does absolutely nothing</summary>
    private class DummyContext : ISynchronizeInvoke {

      #region class SimpleAsyncResult

      /// <summary>Barebones implementation of an asynchronous result</summary>
      private class SimpleAsyncResult : IAsyncResult {

        /// <summary>Ehether the asynchronous operation is complete</summary>
        /// <remarks>
        ///   Always true because it completes synchronously
        /// </remarks>
        public bool IsCompleted { get { return true; } }

        /// <summary>
        ///   Wait handle that can be used to wait for the asynchronous operation
        /// </summary>
        public WaitHandle AsyncWaitHandle {
          get { throw new NotImplementedException("Not implemented"); }
        }

        /// <summary>Custom state that can be used to pass information around</summary>
        public object AsyncState {
          get { throw new NotImplementedException("Not implemented"); }
        }

        /// <summary>Whether the asynchronous operation completed synchronously</summary>
        public bool CompletedSynchronously { get { return true; } }

        /// <summary>The value returned from the asynchronous operation</summary>
        public object ReturnedValue;

      }

      #endregion // class SimpleAsyncResult

      /// <summary>Whether the calling thread needs to use Invoke()</summary>
      public bool InvokeRequired {
        get { return true; }
      }

      /// <summary>Schedules the specified method for execution in the target thread</summary>
      /// <param name="method">Method the target thread will execute when it is idle</param>
      /// <param name="arguments">Arguments that will be passed to the method</param>
      /// <returns>
      ///   An asynchronous result handle that can be used to check on the status of
      ///   the call and wait for its completion
      /// </returns>
      public IAsyncResult BeginInvoke(Delegate method, object[] arguments) {
        var asyncResult = new SimpleAsyncResult();
        asyncResult.ReturnedValue = method.Method.Invoke(method.Target, arguments);
        return asyncResult;
      }

      /// <summary>Waits for the asychronous call to complete</summary>
      /// <param name="result">
      ///   Asynchronous result handle returned by the <see cref="BeginInvoke" /> method
      /// </param>
      /// <returns>The original result returned by the asychronously called method</returns>
      public object EndInvoke(IAsyncResult result) {
        return ((SimpleAsyncResult)result).ReturnedValue;
      }

      /// <summary>
      ///   Schedules the specified method for execution in the target thread and waits
      ///   for it to complete
      /// </summary>
      /// <param name="method">Method that will be executed by the target thread</param>
      /// <param name="arguments">Arguments that will be passed to the method</param>
      /// <returns>The result returned by the specified method</returns>
      public object Invoke(Delegate method, object[] arguments) {
        return method.Method.Invoke(method.Target, arguments);
      }

    }

    #endregion // class DummyContext

    #region class DummyThreadedAction

    /// <summary>Implementation of a threaded action for the unit test</summary>
    private class DummyThreadedAction : ThreadedAction {

      /// <summary>
      ///   Initializes a new threaded action, letting the base class figure out the UI thread
      /// </summary>
      public DummyThreadedAction() : base() {
        this.finishedGate = new ManualResetEvent(initialState: false);
      }

      /// <summary>
      ///   Initializes a new view model using the specified UI context explicitly
      /// </summary>
      public DummyThreadedAction(ISynchronizeInvoke uiContext) : base(uiContext) {
        this.finishedGate = new ManualResetEvent(initialState: false);
      }

      /// <summary>Immediately releases all resources owned by the instance</summary>
      public override void Dispose() {
        base.Dispose();

        if(this.finishedGate != null) {
          this.finishedGate.Dispose();
          this.finishedGate = null;
        }
      }

      /// <summary>Waits until the first background operation is finished</summary>
      /// <returns>
      ///   True if the background operation is finished, false if it is ongoing
      /// </returns>
      public bool WaitUntilFinished() {
        return this.finishedGate.WaitOne(100);
      }

      /// <summary>Selects the value that will be assigned when the action runs</summary>
      /// <param name="valueToAssign">Value the action will assigned when it runs</param>
      public void SetValueToAssign(int valueToAssign) {
        this.valueToAssign = valueToAssign;
      }

      /// <summary>Sets up an error the action will fail with when run</summary>
      /// <param name="errorToFailWith">Error the action will fail with</param>
      public void SetErrorToFailWith(Exception errorToFailWith) {
        this.errorToFailWith = errorToFailWith;
      }

      /// <summary>Last error that was reported by the threaded view model</summary>
      public Exception ReportedError {
        get { return this.reportedError; }
      }

      /// <summary>Value that has been assigned from the background thread</summary>
      public int AssignedValue {
        get { return this.assignedValue; }
      }

      /// <summary>Executes the threaded action from the background thread</summary>
      /// <param name="cancellationToken">Token by which execution can be canceled</param>
      protected override void Run(CancellationToken cancellationToken) {
        if(this.errorToFailWith != null) {
          throw this.errorToFailWith;
        }

        this.assignedValue = this.valueToAssign;
        this.finishedGate.Set();
      }

      /// <summary>Called when an error occurs in the background thread</summary>
      /// <param name="exception">Exception that was thrown in the background thread</param>
      protected override void ReportError(Exception exception) {
        this.reportedError = exception;
        this.finishedGate.Set();
      }

      /// <summary>Error the action will fail with, if set</summary>
      private Exception errorToFailWith;
      /// <summary>Value the action will assign to its same-named field</summary>
      private int valueToAssign;

      /// <summary>Last error that was reported by the threaded view model</summary>
      private volatile Exception reportedError;
      /// <summary>Triggered when the </summary>
      private ManualResetEvent finishedGate;
      /// <summary>Value that is assigned through the background thread</summary>
      private volatile int assignedValue;

    }

    #endregion // class DummyThreadedAction

    /// <summary>Verifies that the threaded action has a default constructor</summary>
    [Test, Explicit]
    public void HasDefaultConstructor() {
      using(var mainForm = new System.Windows.Forms.Form()) {
        mainForm.Show();
        try {
          mainForm.Visible = false;
          using(new DummyThreadedAction()) { }
        }
        finally {
          mainForm.Close();
        }
      }
    }

    /// <summary>
    ///   Verifies that the threaded action can be constructed with a custom UI context
    /// </summary>
    [Test]
    public void HasCustomSychronizationContextConstructor() {
      using(new DummyThreadedAction(new DummyContext())) { }
    }

    /// <summary>Checks that a new threadd action starts out idle and not busy</summary>
    [Test]
    public void NewInstanceIsNotBusy() {
      using(var action = new DummyThreadedAction(new DummyContext())) {
        Assert.IsFalse(action.IsBusy);
      }
    }

    /// <summary>
    ///   Verifies that errors happening in the background processing threads are
    ///   reported to the main thread
    /// </summary>
    [Test]
    public void ErrorsInBackgroundThreadAreReported() {
      using(var action = new DummyThreadedAction(new DummyContext())) {
        var testError = new ArgumentException("Mooh");
        action.SetErrorToFailWith(testError);
        action.Start();
        action.WaitUntilFinished();
        Assert.AreSame(testError, action.ReportedError);
      }
    }

    /// <summary>
    ///   Verifies that the background thread actually executes and can do work
    /// </summary>
    [Test]
    public void BackgroundThreadExecutesTasks() {
      using(var action = new DummyThreadedAction(new DummyContext())) {
        action.SetValueToAssign(42001);
        action.Start();
        action.WaitUntilFinished();
        Assert.AreEqual(42001, action.AssignedValue);
      }
    }

  }

} // namespace Nuclex.Windows.Forms.ViewModels

#endif // UNITTEST