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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;

#if WITH_NUCLEX_SUPPORT_TRANSACTIONS

using Nuclex.Support.Tracking;

namespace Nuclex.Windows.Forms {

  /// <summary>Progress bar for tracking the progress of background operations</summary>
  public partial class TrackingBar : AsyncProgressBar {

    /// <summary>Initializes a new tracking bar</summary>
    public TrackingBar() {
      InitializeComponent();

      // We start off being in the idle state (and thus, being invisible)
      this.isIdle = true;
      this.Visible = false;

      // Initialize the delegates we use to update the control's state and those
      // we use to register ourselfes to the tracker's events
      this.updateIdleStateDelegate = new MethodInvoker(updateIdleState);
      this.asyncIdleStateChangedDelegate = new EventHandler<IdleStateEventArgs>(
        asyncIdleStateChanged
      );
      this.asyncProgressUpdateDelegate = new EventHandler<ProgressReportEventArgs>(
        asyncProgressUpdated
      );

      // Create the tracker and attach ourselfes to its events
      this.tracker = new ProgressTracker();
      this.tracker.AsyncIdleStateChanged += this.asyncIdleStateChangedDelegate;
      this.tracker.AsyncProgressChanged += this.asyncProgressUpdateDelegate;
    }

    /// <summary>Tracks the specified transaction in the tracking bar</summary>
    /// <param name="transaction">Transaction to be tracked</param>
    public void Track(Transaction transaction) {
      this.tracker.Track(transaction);
    }

    /// <summary>Tracks the specified transaction in the tracking bar</summary>
    /// <param name="transaction">Transaction to be tracked</param>
    /// <param name="weight">Weight of this transaction in the total progress</param>
    public void Track(Transaction transaction, float weight) {
      this.tracker.Track(transaction, weight);
    }

    /// <summary>Stops tracking the specified transaction</summary>
    /// <param name="transaction">Transaction to stop tracking</param>
    public void Untrack(Transaction transaction) {
      this.tracker.Untrack(transaction);
    }

    /// <summary>
    ///   Called when the summed progressed of the tracked transaction has changed
    /// </summary>
    /// <param name="sender">Transaction whose progress has changed</param>
    /// <param name="arguments">Contains the progress achieved by the transaction</param>
    private void asyncProgressUpdated(
      object sender, ProgressReportEventArgs arguments
    ) {
      AsyncSetValue(arguments.Progress);
    }

    /// <summary>Called when the tracker becomes enters of leaves the idle state</summary>
    /// <param name="sender">Tracker that has entered or left the idle state</param>
    /// <param name="arguments">Contains the new idle state</param>
    private void asyncIdleStateChanged(object sender, IdleStateEventArgs arguments) {

      // Do a fully synchronous update of the idle state. This update must not be
      // lost because otherwise, the progress bar might stay on-screen when in fact,
      // the background operation has already finished and nothing is happening anymore.
      this.isIdle = arguments.Idle;

      // Update the bar's idle state
      if(InvokeRequired) {
        Invoke(this.updateIdleStateDelegate);
      } else {
        updateIdleState();
      }

    }

    /// <summary>
    ///   Updates the idle state of the progress bar
    ///   (controls whether the progress bar is shown or invisible)
    /// </summary>
    private void updateIdleState() {

      // Only show the progress bar when something is happening
      base.Visible = !this.isIdle;

    }

    /// <summary>Whether the progress bar is in the idle state</summary>
    private volatile bool isIdle;
    /// <summary>Tracker used to sum and update the total progress</summary>
    private ProgressTracker tracker;
    /// <summary>Delegate for the idle state update method</summary>
    private MethodInvoker updateIdleStateDelegate;
    /// <summary>Delegate for the asyncIdleStateChanged() method</summary>
    private EventHandler<IdleStateEventArgs> asyncIdleStateChangedDelegate;
    /// <summary>Delegate for the asyncProgressUpdate() method</summary>
    private EventHandler<ProgressReportEventArgs> asyncProgressUpdateDelegate;

  }

} // namespace Nuclex.Windows.Forms

#endif // WITH_NUCLEX_SUPPORT_TRANSACTIONS
