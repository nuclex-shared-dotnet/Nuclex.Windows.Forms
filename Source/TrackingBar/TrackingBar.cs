#region CPL License
/*
Nuclex Framework
Copyright (C) 2002-2007 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
IBM Common Public License for more details.

You should have received a copy of the IBM Common Public
License along with this library
*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using Nuclex.Support.Tracking;

namespace Nuclex.Windows.Forms {

  /// <summary>Progress bar for tracking the progress of background operations</summary>
  public partial class TrackingBar : ProgressBar {

    /// <summary>Initializes a new tracking bar</summary>
    public TrackingBar() {
      InitializeComponent();

      // We start off being in the idle state (and thus, being invisible)
      this.isIdle = true;
      base.Visible = false;

      // Initialize the delegates we use to update the control's state and those
      // we use to register ourselfes to the tracker's events
      this.updateIdleStateDelegate = new MethodInvoker(updateIdleState);
      this.updateProgressDelegate = new MethodInvoker(updateProgress);
      this.asyncIdleStateChangedDelegate = new EventHandler<IdleStateEventArgs>(
        asyncIdleStateChanged
      );
      this.asyncProgressUpdateDelegate = new EventHandler<ProgressUpdateEventArgs>(
        asyncProgressUpdated
      );

      // Create the tracker and attach ourselfes to its events
      this.tracker = new ProgressionTracker();
      this.tracker.AsyncIdleStateChanged += this.asyncIdleStateChangedDelegate;
      this.tracker.AsyncProgressUpdated += this.asyncProgressUpdateDelegate;
    }

    /// <summary>Tracks the specified progression in the tracking bar</summary>
    /// <param name="progression">Progression to be tracked</param>
    public void Track(Progression progression) {
      this.tracker.Track(progression);
    }

    /// <summary>Tracks the specified progression in the tracking bar</summary>
    /// <param name="progression">Progression to be tracked</param>
    /// <param name="weight">Weight of this progression in the total progress</param>
    public void Track(Progression progression, float weight) {
      this.tracker.Track(progression, weight);
    }

    /// <summary>Stops tracking the specified progression</summary>
    /// <param name="progression">Progression to stop tracking</param>
    public void Untrack(Progression progression) {
      this.tracker.Untrack(progression);
    }

    /// <summary>
    ///   Called when the summed progressed of the tracked progressions has changed
    /// </summary>
    /// <param name="sender">Progression whose progress has changed</param>
    /// <param name="arguments">Contains the progress achieved by the progression</param>
    private void asyncProgressUpdated(
      object sender, ProgressUpdateEventArgs arguments
    ) {

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
    
    /// <summary>Called when the tracker becomes enters of leaves the idle state</summary>
    /// <param name="sender">Tracker that has entered or left the idle state</param>
    /// <param name="arguments">Contains the new idle state</param>
    private void asyncIdleStateChanged(object sender, IdleStateEventArgs arguments) {
      
      // Do a fully synchronous update of the idle state. This update must not be
      // lost because otherwise, the progress bar might stay on-screen when in fact,
      // the background operation has already finished and nothing is happening anymore.
      this.isIdle = arguments.Idle;
      Invoke(this.updateIdleStateDelegate);

    }

    /// <summary>Synchronously updates the value visualized in the progress bar</summary>
    private void updateProgress() {
      lock(this) {

        // Reset the update flag so incoming updates will cause the control to
        // update itself another time.
        this.progressUpdatePending = false;
        EndInvoke(this.progressUpdateAsyncResult);

        // Transform the progress into an integer in the range of the progress bar's
        // min and max values (these should normally be set to 0 and 100).
        int min = base.Minimum;
        int max = base.Maximum;
        int progress = (int)(this.currentProgress * (max - min)) + min;

        // Update the control
        base.Value = progress;

        // Assigning the value sends PBM_SETPOS to the control which,
        // according to MSDN, already causes a redraw!
        //base.Invalidate();

      } // lock
    }

    /// <summary>
    ///   Updates the idle state of the progress bar
    ///   (controls whether the progress bar is shown or invisible)
    /// </summary>
    private void updateIdleState() {

      base.Visible = !this.isIdle;

    }

    /// <summary>Whether an update of the control state is pending</summary>
    private volatile bool progressUpdatePending;
    /// <summary>Async result for the invoked control state update method</summary>
    private volatile IAsyncResult progressUpdateAsyncResult;
    /// <summary>Whether the progress bar is in the idle state</summary>
    private volatile bool isIdle;
    /// <summary>Most recently reported progress of the tracker</summary>
    private volatile float currentProgress;

    /// <summary>Tracker used to sum and update the total progress</summary>
    private ProgressionTracker tracker;
    /// <summary>Delegate for the progress update method</summary>
    private MethodInvoker updateProgressDelegate;
    /// <summary>Delegate for the idle state update method</summary>
    private MethodInvoker updateIdleStateDelegate;
    /// <summary>Delegate for the OnAsyncProgressionEnded method</summary>
    private EventHandler<IdleStateEventArgs> asyncIdleStateChangedDelegate;
    /// <summary>Delegate for the OnAsyncProgressionProgressUpdated method</summary>
    private EventHandler<ProgressUpdateEventArgs> asyncProgressUpdateDelegate;

  }

} // namespace Nuclex.Windows.Forms
