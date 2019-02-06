#if UNITTEST

using System;
using System.ComponentModel;
using System.Threading;
using NUnit.Framework;

namespace Nuclex.Windows.Forms.ViewModels {

  /// <summary>Unit test for the threaded view model base class</summary>
  [TestFixture]
  public class ThreadedViewModelTest {

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

    #region class TestViewModel

    /// <summary>View model used to unit test the threaded view model base class</summary>
    private class TestViewModel : ThreadedViewModel {

      /// <summary>
      ///   Initializes a new view model, letting the base class figure out the UI thread
      /// </summary>
      public TestViewModel() : base() {
        this.finishedGate = new ManualResetEvent(initialState: false);
      }

      /// <summary>
      ///   Initializes a new view model, using the specified context for the UI thread
      /// </summary>
      /// <param name="uiContext">Synchronization context of the UI thread</param>
      public TestViewModel(ISynchronizeInvoke uiContext) : base(uiContext) {
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

      /// <summary>Runs a background process that causes the specified error</summary>
      /// <param name="error">Error that will be caused in the background process</param>
      public void CauseErrorInBackgroundThread(Exception error) {
        RunInBackground(() => throw error);
      }

      /// <summary>Last error that was reported by the threaded view model</summary>
      public Exception ReportedError {
        get { return this.reportedError; }
      }

      /// <summary>Called when an error occurs in the background thread</summary>
      /// <param name="exception">Exception that was thrown in the background thread</param>
      protected override void ReportError(Exception exception) {
        this.reportedError = exception;
        this.finishedGate.Set();
      }

      /// <summary>Last error that was reported by the threaded view model</summary>
      private Exception reportedError;
      /// <summary>Triggered when the </summary>
      private ManualResetEvent finishedGate;

    }

    #endregion // class TestViewModel

    /// <summary>Verifies that the threaded view model has a default constructor</summary>
    [Test]
    public void HasDefaultConstructor() {
      using(var mainForm = new System.Windows.Forms.Form()) {
        mainForm.Show();
        try {
          mainForm.Visible = false;
          using(new TestViewModel()) { }
        }
        finally {
          mainForm.Close();
        }
      }
    }

    /// <summary>
    ///   Verifies that the threaded view model can be constructed with a custom UI context
    /// </summary>
    [Test]
    public void HasCustomSychronizationContextConstructor() {
      using(new TestViewModel(new DummyContext())) { }
    }

    /// <summary>Checks that a new view model starts out idle and not busy</summary>
    [Test]
    public void NewInstanceIsNotBusy() {
      using(var viewModel = new TestViewModel(new DummyContext())) {
        Assert.IsFalse(viewModel.IsBusy);
      }
    }

    /// <summary>
    ///   Verifies that errors happening in the background processing threads are
    ///   reported to the main thread
    /// </summary>
    [Test]
    public void ErrorsInBackgroundThreadAreReported() {
      using(var viewModel = new TestViewModel(new DummyContext())) {
        var testError = new ArgumentException("Mooh");
        viewModel.CauseErrorInBackgroundThread(testError);
        viewModel.WaitUntilFinished();
        Assert.AreSame(testError, viewModel.ReportedError);
      }
    }

  }

} // namespace Nuclex.Windows.Forms.ViewModels

#endif // UNITTEST