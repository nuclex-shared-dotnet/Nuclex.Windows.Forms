using System;

using NUnit.Framework;

namespace Nuclex.Windows.Forms.ViewModels {

  /// <summary>Unit test for the dialog view model</summary>
  [TestFixture]
  public class DialogViewModelTest {

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

    /// <summary>Verifies that the dialog view model has a default constructor</summary>
    [Test]
    public void HasDefaultConstructor() {
      Assert.DoesNotThrow(
        delegate() { new DialogViewModel(); }
      );
    }

    /// <summary>
    ///   Verifies that calling Confirm() on the dialog view model triggers
    ///   the 'Confirmed' event
    /// </summary>
    [Test]
    public void ConfirmTriggersConfirmedEvent() {
      var viewModel = new DialogViewModel();
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
      var viewModel = new DialogViewModel();
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
      var viewModel = new DialogViewModel();
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
    private DialogViewModelSubscriber createSubscriber(DialogViewModel viewModel) {
      var subscriber = new DialogViewModelSubscriber();
      viewModel.Confirmed += subscriber.Confirmed;
      viewModel.Canceled += subscriber.Cancelled;
      viewModel.Submitted += subscriber.Submitted;
      return subscriber;
    }

  }

} // namespace Nuclex.Windows.Forms.ViewModels
