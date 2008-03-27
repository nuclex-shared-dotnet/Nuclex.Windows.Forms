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
  /// <example>
  ///   class Test : Nuclex.Support.Scheduling.ThreadOperation {
  ///     static void Main() {
  ///       Test myTest = new Test();
  ///       myTest.Begin();
  ///       Nuclex.Windows.Forms.ProgressReporterForm.Track(myTest);
  ///       myTest.End();
  ///     }
  ///     protected override void Execute() {
  ///       for(int i = 0; i &lt; 10000000; ++i)
  ///         OnAsyncProgressUpdated((float)i / 10000000.0f);
  ///     }
  ///   }
  /// </example>
  public partial class ProgressReporterForm : Form {

    /// <summary>Initializes a new progress reporter</summary>
    internal ProgressReporterForm() {
      InitializeComponent();

      this.asyncEndedDelegate = new EventHandler(asyncEnded);
      this.asyncProgressChangedDelegate = new EventHandler<ProgressReportEventArgs>(
        asyncProgressChanged
      );
    }

    /// <summary>
    ///   Shows the progress reporter until the specified progression has ended.
    /// </summary>
    /// <param name="progression">
    ///   Progression for whose duration to show the progress reporter
    /// </param>
    public static void Track(Waitable progression) {
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
    public static void Track(string windowTitle, Waitable progression) {

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
      e.Cancel = (Thread.VolatileRead(ref this.state) < 2);
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
    private void track(string windowTitle, Waitable progression) {

      // Set the window title if the user wants to use a custom one
      if(windowTitle != null)
        Text = windowTitle;

      // Only enable the cancel button if the progression can be aborted
      this.abortReceiver = (progression as IAbortable);
      this.cancelButton.Enabled = (this.abortReceiver != null);

      // Subscribe the form to the progression it is supposed to monitor
      progression.AsyncEnded += this.asyncEndedDelegate;
      IProgressReporter progressReporter = progression as IProgressReporter;
      if(progressReporter != null)
        progressReporter.AsyncProgressChanged += this.asyncProgressChangedDelegate;

      // The progression might have ended before this line was reached, if that's
      // the case, we don't show the dialog at all.
      if(!progression.Ended)
        ShowDialog();

      // We're done, unsubscribe from the progression's events again
      progressReporter = progression as IProgressReporter;
      if(progressReporter != null)
        progressReporter.AsyncProgressChanged -= this.asyncProgressChangedDelegate;
      progression.AsyncEnded -= this.asyncEndedDelegate;

    }

    /// <summary>Called when the progression has ended</summary>
    /// <param name="sender">Progression that has ended</param>
    /// <param name="arguments">Not used</param>
    private void asyncEnded(object sender, EventArgs arguments) {

      // If the new state is 2, the form was ready to close (since the state
      // is incremented once when the form becomes ready to be closed)
      if(Interlocked.Increment(ref this.state) == 2) {

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
    private void asyncProgressChanged(object sender, ProgressReportEventArgs arguments) {

      // See if this is the first progress update we're receiving. If yes, we need to
      // switch the progress bar from marquee into its normal mode!
      int haveProgress = Interlocked.Exchange(ref this.areProgressUpdatesIncoming, 1);
      if(haveProgress == 0) {
        this.progressBar.BeginInvoke(
          (MethodInvoker)delegate() { this.progressBar.Style = ProgressBarStyle.Blocks; }
        );
      }

      // Send the new progress to the progress bar
      this.progressBar.AsyncSetValue(arguments.Progress);

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
      if(Interlocked.Increment(ref this.state) == 2)
        Close();

    }

    /// <summary>
    ///   Aborts the background operation when the user clicks the cancel button
    /// </summary>
    /// <param name="sender">Button that has been clicked</param>
    /// <param name="e">Not used</param>
    private void cancelClicked(object sender, EventArgs e) {

      if(this.abortReceiver != null) {

        // Do this first because the abort receiver might trigger the AsyncEnded()
        // event in the calling thread (us!) and thus destroy our window even in
        // the safe and synchronous UI thread :)
        this.cancelButton.Enabled = false;

        // Now we're ready to abort!
        this.abortReceiver.AsyncAbort();
        this.abortReceiver = null;

      }

    }

    /// <summary>Delegate for the asyncEnded() method</summary>
    private EventHandler asyncEndedDelegate;
    /// <summary>Delegate for the asyncProgressUpdated() method</summary>
    private EventHandler<ProgressReportEventArgs> asyncProgressChangedDelegate;
    /// <summary>Whether the form can be closed and should be closed</summary>
    /// <remarks>
    ///   0: Nothing happened yet
    ///   1: Ready to close or close requested
    ///   2: Ready to close and close requested, triggers close
    /// </remarks>
    private int state;
    /// <summary>Whether we're receiving progress updates from the progression</summary>
    /// <remarks>
    ///   0: No progress updates have arrived so far
    ///   1: We have received at least one progress update from the progression
    /// </remarks>
    private int areProgressUpdatesIncoming;
    /// <summary>
    ///   If set, reference to an object implementing IAbortable by which the
    ///   ongoing background process can be aborted.
    /// </summary>
    private IAbortable abortReceiver;

  }

} // namespace Nuclex.Windows.Forms
