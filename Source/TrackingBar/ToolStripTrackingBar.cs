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
using System.Drawing;
using System.Windows.Forms;

using Nuclex.Support.Tracking;

namespace Nuclex.Windows.Forms {

  /// <summary>Tracking bar that can be hosted in a tool strip container</summary>
  public class ToolStripTrackingBar : ToolStripControlHost {

    /// <summary>Initializes a new tool strip tracking bar</summary>
    public ToolStripTrackingBar() : base(createTrackingBar()) {
      hideControlAtRuntime();
    }

    /// <summary>Initializes a new tool strip tracking bar with a name</summary>
    /// <param name="name">Name of the tracking bar control</param>
    public ToolStripTrackingBar(string name) : base(createTrackingBar(), name) {
      hideControlAtRuntime();
    }
    
    /// <summary>The tracking bar control being hosted by the tool strip host</summary>
    public TrackingBar TrackingBarControl {
      get { return base.Control as TrackingBar; }
    }

    /// <summary>Tracks the specified transaction in the tracking bar</summary>
    /// <param name="transaction">Transaction to be tracked</param>
    public void Track(Transaction transaction) {
      TrackingBarControl.Track(transaction);
    }

    /// <summary>Tracks the specified transaction in the tracking bar</summary>
    /// <param name="transaction">Transaction to be tracked</param>
    /// <param name="weight">Weight of this transaction in the total progress</param>
    public void Track(Transaction transaction, float weight) {
      TrackingBarControl.Track(transaction, weight);
    }

    /// <summary>Stops tracking the specified transaction</summary>
    /// <param name="transaction">Transaction to stop tracking</param>
    public void Untrack(Transaction transaction) {
      TrackingBarControl.Untrack(transaction);
    }

    /// <summary>Default size of the hosted control</summary>
    protected override Size DefaultSize {
      get { return new Size(100, 15); }
    }

    /// <summary>Default margin to leave around the control in the tool strip</summary>
    protected override Padding DefaultMargin {
      get {
        if((base.Owner != null) && (base.Owner is StatusStrip)) {
          return new Padding(1, 3, 1, 3);
        }

        return new Padding(1, 2, 1, 1);
      }
    }

    /// <summary>Creates a new tracking bar</summary>
    /// <returns>A new tracking bar</returns>
    private static TrackingBar createTrackingBar() {
      TrackingBar trackingBar = new TrackingBar();
      trackingBar.Size = new Size(100, 15);
      return trackingBar;
    }

    /// <summary>Hides the control during runtime usage</summary>
    private void hideControlAtRuntime() {
      TrackingBarControl.VisibleChanged += new EventHandler(trackingBarVisibleChanged);

      LicenseUsageMode usageMode = System.ComponentModel.LicenseManager.UsageMode;
      if(usageMode == LicenseUsageMode.Runtime) {
        base.Visible = false;
      }
    }

    /// <summary>
    ///   Toggles the visibility of the tool strip host when the tracking bar control's
    ///   visibility changes.
    /// </summary>
    /// <param name="sender">Tracking bar control whose visiblity has changed</param>
    /// <param name="arguments">Not used</param>
    private void trackingBarVisibleChanged(object sender, EventArgs arguments) {
      base.Visible = TrackingBarControl.Visible;
    }

  }

} // namespace Nuclex.Windows.Forms
