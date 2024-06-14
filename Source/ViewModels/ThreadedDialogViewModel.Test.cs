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

using NUnit.Framework;

namespace Nuclex.Windows.Forms.ViewModels {

  /// <summary>Unit test for the threaded dialog view model</summary>
  [TestFixture]
  public class ThreadedDialogViewModelTest {

    #region class DialogViewModelSubscriber

    /// <summary>Subscriber for the events offered by a dialog view model</summary>
    private class DialogViewModelSubscriber {

      /// <summary>Indicates that the user has accepted the dialog</summary>
      public void Confirmed(object sender, EventArgs arguments) {
        ++this.confirmCallCount;
      }

      /// <summary>Indicates that the user has cancelled the dialog</summary>
      public void Cancelled(object sender, EventArgs arguments) {
        ++this.cancelCallCount;
      }

      /// <summary>Indicates that the dialog was simply closed</summary>
      public void Submitted(object sender, EventArgs arguments) {
        ++this.submitCallCount;
      }

      /// <summary>How many times the Confirmed() method was called</summary>
      public int ConfirmCallCount {
        get { return this.confirmCallCount; }
      }

      /// <summary>How many times the Cancelled() method was called</summary>
      public int CancelCallCount {
        get { return this.cancelCallCount; }
      }

      /// <summary>How many times the Submitted() method was called</summary>
      public int SubmitCallCount {
        get { return this.submitCallCount; }
      }

      /// <summary>How many times the Confirmed() method was called</summary>
      private int confirmCallCount;
      /// <summary>How many times the Cancelled() method was called</summary>
      private int cancelCallCount;
      /// <summary>How many times the Submitted() method was called</summary>
      private int submitCallCount;

    }

    #endregion // class DialogViewModelSubscriber

    #region class TestViewModel

    private class TestViewModel : ThreadedDialogViewModel {

      public Exception ReportedError {
        get { return this.reportedError; }
      }

      protected override void ReportError(Exception exception) {
        this.reportedError = exception;
      }

      private Exception reportedError;

    }

    #endregion // class TestViewModel

    /// <summary>Verifies that the dialog view model has a default constructor</summary>
    [Test]
    public void HasDefaultConstructor() {
      Assert.DoesNotThrow(
        delegate() { new TestViewModel(); }
      );
    }

    /// <summary>
    ///   Verifies that calling Confirm() on the dialog view model triggers
    ///   the 'Confirmed' event
    /// </summary>
    [Test]
    public void ConfirmTriggersConfirmedEvent() {
      var viewModel = new TestViewModel();
      var subscriber = createSubscriber(viewModel);

      Assert.AreEqual(0, subscriber.ConfirmCallCount);
      Assert.AreEqual(0, subscriber.CancelCallCount);
      Assert.AreEqual(0, subscriber.SubmitCallCount);
      viewModel.Confirm();
      Assert.AreEqual(1, subscriber.ConfirmCallCount);
      Assert.AreEqual(0, subscriber.CancelCallCount);
      Assert.AreEqual(0, subscriber.SubmitCallCount);
    }

    /// <summary>
    ///   Verifies that calling Cancel() on the dialog view model triggers
    ///   the 'Cancelled' event
    /// </summary>
    [Test]
    public void CancelTriggersCancelledEvent() {
      var viewModel = new TestViewModel();
      var subscriber = createSubscriber(viewModel);

      Assert.AreEqual(0, subscriber.ConfirmCallCount);
      Assert.AreEqual(0, subscriber.CancelCallCount);
      Assert.AreEqual(0, subscriber.SubmitCallCount);
      viewModel.Cancel();
      Assert.AreEqual(0, subscriber.ConfirmCallCount);
      Assert.AreEqual(1, subscriber.CancelCallCount);
      Assert.AreEqual(0, subscriber.SubmitCallCount);
    }

    /// <summary>
    ///   Verifies that calling Submitm() on the dialog view model triggers
    ///   the 'Submitted' event
    /// </summary>
    [Test]
    public void SubmitTriggersSubmittedEvent() {
      var viewModel = new TestViewModel();
      var subscriber = createSubscriber(viewModel);

      Assert.AreEqual(0, subscriber.ConfirmCallCount);
      Assert.AreEqual(0, subscriber.CancelCallCount);
      Assert.AreEqual(0, subscriber.SubmitCallCount);
      viewModel.Submit();
      Assert.AreEqual(0, subscriber.ConfirmCallCount);
      Assert.AreEqual(0, subscriber.CancelCallCount);
      Assert.AreEqual(1, subscriber.SubmitCallCount);
    }

    /// <summary>Constructs a new subscriber for the dialog view model's events</summary>
    /// <param name="viewModel">View model a subscriber will be created for</param>
    /// <returns>A subscriber for the events of the specified view model</returns>
    private DialogViewModelSubscriber createSubscriber(ThreadedDialogViewModel viewModel) {
      var subscriber = new DialogViewModelSubscriber();
      viewModel.Confirmed += subscriber.Confirmed;
      viewModel.Canceled += subscriber.Cancelled;
      viewModel.Submitted += subscriber.Submitted;
      return subscriber;
    }

  }

} // namespace Nuclex.Windows.Forms.ViewModels

#endif // UNITTEST
