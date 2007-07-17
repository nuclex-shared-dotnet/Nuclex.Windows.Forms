using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using Nuclex.Support.Tracking;
using Nuclex.Support.Scheduling;

namespace Nuclex.Windows.Forms {

  /// <summary>
  ///   Blocking progress dialog that prevents the user from accessing the application
  ///   window during all-blocking background processes.
  /// </summary>
  public partial class ProgressReporterForm : Form {

    /// <summary>Initializes a new progress reporter</summary>
    internal ProgressReporterForm() {
      InitializeComponent();

      this.updateProgressDelegate = new MethodInvoker(updateProgress);
      this.asyncEndedDelegate = new EventHandler(asyncEnded);
      this.asyncProgressUpdatedDelegate = new EventHandler<ProgressUpdateEventArgs>(
        asyncProgressUpdated
      );
    }

    /// <summary>
    ///   Shows the progress reporter until the specified progression has ended.
    /// </summary>
    /// <param name="progression">
    ///   Progression for whose duration to show the progress reporter
    /// </param>
    public static void Track(Progression progression) {
      Track(null, progression);
    }

    /// <summary>
    ///   Shows the progress reporter until the specified progression has ended.
    /// </summary>
    /// <param name="windowTitle">
    ///   Text to be shown in the progress reporter's title bar
    /// </param>
    /// <param name="progression">
    ///   Progression for whose duration to show the progress reporter
    /// </param>
    public static void Track(string windowTitle, Progression progression) {

      // Small optimization to avoid the lengthy control creation when
      // the progression has already ended
      if(progression.Ended)
        return;

      // Open the form and let it monitor the progression's state
      using(ProgressReporterForm theForm = new ProgressReporterForm()) {
        theForm.track(windowTitle, progression);
      }

    }

    /// <summary>Called when the user tries to close the form manually</summary>
    /// <param name="e">Contains flag that can be used to abort the close attempt</param>
    protected override void OnClosing(CancelEventArgs e) {
      base.OnClosing(e);

      // Only allow the form to close when the form is ready to close and the
      // progression being tracked has also finished.
      e.Cancel = (this.state < 2);
    }

    /// <summary>
    ///   Shows the progress reporter until the specified progression has ended.
    /// </summary>
    /// <param name="windowTitle">
    ///   Text to be shown in the progress reporter's title bar
    /// </param>
    /// <param name="progression">
    ///   Progression for whose duration to show the progress reporter
    /// </param>
    private void track(string windowTitle, Progression progression) {

      // Set the window title if the user wants to use a custom one
      if(windowTitle != null)
        Text = windowTitle;

      // Only enable the cancel button if the progression can be aborted
      this.cancelButton.Enabled = (progression is IAbortable);

      // Subscribe the form to the progression it is supposed to monitor
      progression.AsyncEnded += this.asyncEndedDelegate;
      progression.AsyncProgressUpdated += this.asyncProgressUpdatedDelegate;

      // The progression might have ended before this line was reached, if that's
      // the case, we don't show the dialog at all.
      if(!progression.Ended)
        ShowDialog();

      // We're done, unsubscribe from the progression's events
      progression.AsyncProgressUpdated -= this.asyncProgressUpdatedDelegate;
      progression.AsyncEnded -= this.asyncEndedDelegate;

    }

    /// <summary>Called when the progression has ended</summary>
    /// <param name="sender">Progression that has ended</param>
    /// <param name="arguments">Not used</param>
    private void asyncEnded(object sender, EventArgs arguments) {

      // If the new state is 2, the form was ready to close (since the state
      // is incremented once when the form becomes ready to be closed)
      int newState = Interlocked.Increment(ref this.state);
      if(newState == 2) {

        // Close the dialog. Ensure the Close() method is invoked from the
        // same thread the dialog was created in.
        if(InvokeRequired)
          Invoke(new MethodInvoker(Close));
        else
          Close();

      }

    }

    /// <summary>Called when the tracked progression's progress updates</summary>
    /// <param name="sender">Progression whose progress has been updated</param>
    /// <param name="arguments">
    ///   Contains the new progress achieved by the progression
    /// </param>
    private void asyncProgressUpdated(object sender, ProgressUpdateEventArgs arguments) {

      // Set the new progress without any synchronization
      this.currentProgress = arguments.Progress;

      // Another use of the double-checked locking idiom, here we're trying to optimize
      // away the lock in case some "trigger-happy" progressions send way more
      // progress updates than the poor control can process :)
      if(!this.progressUpdatePending) {
        lock(this) {
          if(!this.progressUpdatePending) {
            this.progressUpdatePending = true;
            this.progressUpdateAsyncResult = BeginInvoke(this.updateProgressDelegate);
          }
        } // lock
      }

    }

    /// <summary>Synchronously updates the value visualized in the progress bar</summary>
    private void updateProgress() {
      lock(this) {

        // Reset the update flag so incoming updates will cause the control to
        // update itself another time.
        this.progressUpdatePending = false;
        EndInvoke(this.progressUpdateAsyncResult);

        // Until the first progress event is received, the progress reporter shows
        // a marquee bar to entertain the user even when no progress reports are
        // being made at all.
        if(this.progressBar.Style == ProgressBarStyle.Marquee)
          this.progressBar.Style = ProgressBarStyle.Blocks;

        // Transform the progress into an integer in the range of the progress bar's
        // min and max values (these should normally be set to 0 and 100).
        int min = this.progressBar.Minimum;
        int max = this.progressBar.Maximum;
        int progress = (int)(this.currentProgress * (max - min)) + min;

        // Update the control
        this.progressBar.Value = Math.Min(Math.Max(progress, min), max);;

        // Assigning the value sends PBM_SETPOS to the control which,
        // according to MSDN, already causes a redraw!
        //base.Invalidate();

      } // lock
    }

    /// <summary>
    ///   One-time timer callback that ensurs the form doesn't stay open when the
    ///   close request arrives at an inappropriate time.
    /// </summary>
    /// <param name="sender">Timer that has ticked</param>
    /// <param name="e">Not used</param>
    private void controlCreationTimerTicked(object sender, EventArgs e) {

      // This timer is intended to run only once to find out when the dialog has
      // been fully constructed and is running its message pump. So we'll disable
      // it as soon as it has been triggered once.
      this.controlCreationTimer.Enabled = false;

      // If the new state is 2, then the form was requested to close before it had
      // been fully constructed, so we should close it now!
      int newState = System.Threading.Interlocked.Increment(ref this.state);
      if(newState == 2)
        Close();

    }

    /// <summary>
    ///   Aborts the background operation when the user clicks the cancel button
    /// </summary>
    /// <param name="sender">Button that has been clicked</param>
    /// <param name="e">Not used</param>
    private void cancelClicked(object sender, EventArgs e) {

      if(this.abortReceiver != null) {

        // Do this first because the abort receiver might trigger the AsyncEnded
        // event in the calling thread and thus destroy our window even in
        // the safe and synchronous UI thread :)
        this.cancelButton.Enabled = false;

        // Now we're ready to abort!
        this.abortReceiver.AsyncAbort();
        this.abortReceiver = null;

      }

    }

    /// <summary>Whether an update of the control state is pending</summary>
    private volatile bool progressUpdatePending;
    /// <summary>Async result for the invoked control state update method</summary>
    private volatile IAsyncResult progressUpdateAsyncResult;
    /// <summary>Most recently reported progress of the tracker</summary>
    private volatile float currentProgress;

    /// <summary>Delegate for the asyncEnded() method</summary>
    private EventHandler asyncEndedDelegate;
    /// <summary>Delegate for the asyncProgressUpdated() method</summary>
    private EventHandler<ProgressUpdateEventArgs> asyncProgressUpdatedDelegate;
    /// <summary>Delegate for the progress update method</summary>
    private MethodInvoker updateProgressDelegate;
    /// <summary>Whether the form can be closed and should be closed</summary>
    /// <remarks>
    ///   0: Nothing happened yet
    ///   1: Ready to close or close requested
    ///   2: Ready to close and close requested, triggers close
    /// </remarks>
    private int state;
    /// <summary>
    ///   If set, reference to an object implementing IAbortable by which the
    ///   ongoing background process can be aborted.
    /// </summary>
    private IAbortable abortReceiver;

  }

} // namespace Nuclex.Windows.Forms