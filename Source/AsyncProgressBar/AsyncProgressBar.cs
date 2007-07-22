using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Nuclex.Windows.Forms {

  /// <summary>Progress bar with optimized multi-threading behavior</summary>
  /// <remarks>
  ///   If a background thread is generating lots of progress updates, using synchronized
  ///   calls can drastically reduce performance. This progress bar optimizes this case
  ///   by performing the update asynchronously and keeping only the most recent update
  ///   when multiple updates arrive while the asynchronous update call is still running.
  /// </remarks>
  public partial class AsyncProgressBar : ProgressBar {

    /// <summary>Initializes a new asynchronous progress bar</summary>
    public AsyncProgressBar() {
      InitializeComponent();

      this.updateProgressDelegate = new MethodInvoker(updateProgress);
      this.Disposed += new EventHandler(progressBarDisposed);

      Interlocked.Exchange(ref this.newProgress, -1.0f);
    }

    /// <summary>Called when the progress bar is being disposed</summary>
    /// <param name="sender">Progress bar that is being disposed</param>
    /// <param name="arguments">Not used</param>
    private void progressBarDisposed(object sender, EventArgs arguments) {

      // Since this has to occur in the UI thread, there's no way that updateProgress()
      // could be executing just now. But the final call to updateProgress() will not
      // have EndInvoke() called on it yet, so we do this here before the control
      // is finally disposed.
      if(this.progressUpdateAsyncResult != null)
        EndInvoke(this.progressUpdateAsyncResult);

      // CHECK: This method is only called on an explicit Dispose() of the control.
      //        Microsoft officially states that it's allowed to call Control.BeginInvoke()
      //        without calling Control.EndInvoke(), so this code is quite correct,
      //        but is it also clean? :>

    }

    /// <summary>Asynchronously updates the value to be shown in the progress bar</summary>
    /// <param name="value">New value to set the progress bar to</param>
    public void AsyncSetValue(float value) {

      // Update the value to be shown on the progress bar. If this happens multiple
      // times, that's not a problem, the progress bar updates as fast as it can
      // and always tries to show the most recent value assigned.
      float oldValue = Interlocked.Exchange(ref this.newProgress, value);

      // If the previous value was -1, the UI thread has already taken out the most recent
      // value and assigned it (or is about to assign it) to the progress bar control.
      // In this case, we'll wait until the current update has completed and immediately
      // begin the next update - since we know that the value the UI thread has extracted
      // is no longer the most recent one.
      if(oldValue == -1.0f) {
        if(this.progressUpdateAsyncResult != null)
          EndInvoke(this.progressUpdateAsyncResult);

        this.progressUpdateAsyncResult = BeginInvoke(this.updateProgressDelegate);
      }

    }

    /// <summary>Synchronously updates the value visualized in the progress bar</summary>
    private void updateProgress() {

      // Cache these to shorten the code that follows :)
      int minimum = base.Minimum;
      int maximum = base.Maximum;

      // Take out the most recent value that has been given to the asynchronous progress
      // bar up until now and replace it by -1. This enables the updater to see when
      // the update has actually been performed and whether it needs to start a new
      // invocation to ensure the most recent value will remain at the end.
      float progress = Interlocked.Exchange(ref this.newProgress, -1.0f);

      // Convert the value to the progress bar's configured range and assign it
      // to the progress bar
      int value = (int)(progress * (maximum - minimum)) + minimum;
      base.Value = Math.Min(Math.Max(value, minimum), maximum);

    }

    /// <summary>New progress being assigned to the progress bar</summary>
    private float newProgress;
    /// <summary>Delegate for the progress update method</summary>
    private MethodInvoker updateProgressDelegate;
    /// <summary>Async result for the invoked control state update method</summary>
    private volatile IAsyncResult progressUpdateAsyncResult;

  }

} // namespace Nuclex.Windows.Forms
