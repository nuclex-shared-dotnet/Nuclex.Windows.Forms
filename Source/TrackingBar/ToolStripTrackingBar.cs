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

    /// <summary>Tracks the specified progression in the tracking bar</summary>
    /// <param name="progression">Progression to be tracked</param>
    public void Track(Waitable progression) {
      TrackingBarControl.Track(progression);
    }

    /// <summary>Tracks the specified progression in the tracking bar</summary>
    /// <param name="progression">Progression to be tracked</param>
    /// <param name="weight">Weight of this progression in the total progress</param>
    public void Track(Waitable progression, float weight) {
      TrackingBarControl.Track(progression, weight);
    }

    /// <summary>Stops tracking the specified progression</summary>
    /// <param name="progression">Progression to stop tracking</param>
    public void Untrack(Waitable progression) {
      TrackingBarControl.Untrack(progression);
    }

    /// <summary>Default size of the hosted control</summary>
    protected override Size DefaultSize {
      get { return new Size(100, 15); }
    }

    /// <summary>Default margin to leave around the control in the tool strip</summary>
    protected override Padding DefaultMargin {
      get {
        if((base.Owner != null) && (base.Owner is StatusStrip))
          return new Padding(1, 3, 1, 3);

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
      if(usageMode == LicenseUsageMode.Runtime)
        base.Visible = false;
    }

    /// <summary>
    ///   Toggles the visibility of the tool strip host when the tracking bar control's
    ///   visibility changes.
    /// </summary>
    /// <param name="sender">Tracking bar control whose visiblity has changed</param>
    /// <param name="e">Not used</param>
    private void trackingBarVisibleChanged(object sender, EventArgs e) {
      base.Visible = TrackingBarControl.Visible;
    }

  }

} // namespace Nuclex.Windows.Forms
