using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nuclex.Windows.Forms.Controls {

  /// <summary>Displays a progress spinner to entertain the user while waiting</summary>
  public partial class ProgressSpinner : UserControl {

    /// <summary>Number of dots the progress spinner will display</summary>
    private const int DotCount = 8;
    /// <summary>Size of a normal dot (only ever assumed by the trailing dot)</summary>
    private const int DotRadius = 4;
    /// <summary>
    ///   The leading dot will be DotCount times this larger than a normal dot
    /// </summary>
    private const int ScaleFactor = 1;

    /// <summary>Initializes a new progress spinner</summary>
    public ProgressSpinner() {
      SetStyle(
        (
          ControlStyles.AllPaintingInWmPaint |
          ControlStyles.OptimizedDoubleBuffer |
          ControlStyles.ResizeRedraw | ControlStyles.UserPaint |
          ControlStyles.SupportsTransparentBackColor
        ),
        true
      );

      InitializeComponent();

      Disposed += new EventHandler(OnDisposed);

      if(!DesignMode) {
        StartSpinner();
      }
    }

    /// <summary>Releases all resources owned by the control when it is destroyed</summary>
    /// <param name="sender">Control that is being destroyed</param>
    /// <param name="arguments">Not used</param>
    private void OnDisposed(object sender, EventArgs arguments) {
      if(this.dotOutlinePen != null) {
        this.dotOutlinePen.Dispose();
        this.dotOutlinePen = null;
      }
      if(this.dotFillBrush != null) {
        this.dotFillBrush.Dispose();
        this.dotFillBrush = null;
      }
    }

    /// <summary>Starts the spinner's animation</summary>
    public void StartSpinner() {
      this.spinnerRunning = true;
      this.animationUpdateTimer.Enabled = true;
    }

    /// <summary>Stops the spinner's animation</summary>
    public void StopSpinner() {
      this.animationUpdateTimer.Enabled = false;
      this.spinnerRunning = false;
    }

    /// <summary>Color used to fill the dots</summary>
    public Color DotFillColor {
      get { return this.dotFillColor; }
      set {
        if(value != this.dotFillColor) {
          this.dotFillColor = value;
          if(this.dotFillBrush != null) {
            this.dotFillBrush.Dispose();
            this.dotFillBrush = null;
          }
        }
      }
    }

    /// <summary>Color used for the dots' outline</summary>
    public Color DotOutlineColor {
      get { return this.dotOutlineColor; }
      set {
        if(value != this.dotOutlineColor) {
          this.dotOutlineColor = value;
          if(this.dotOutlinePen != null) {
            this.dotOutlinePen.Dispose();
            this.dotOutlinePen = null;
          }
        }
      }
    }

    /// <summary>Called when the control is hidden or shown</summary>
    /// <param name="arguments">Not used</param>
    protected override void OnVisibleChanged(EventArgs arguments) {
      base.OnVisibleChanged(arguments);

      this.animationUpdateTimer.Enabled = this.spinnerRunning && Visible;
    }

    /// <summary>Called when the control should redraw itself</summary>
    /// <param name="arguments">Provides access to the drawing surface and tools</param>
    protected override void OnPaint(PaintEventArgs arguments) {
      if(Parent != null && this.BackColor == Color.Transparent) {
        using(var bmp = new Bitmap(Parent.Width, Parent.Height)) {
          Parent.Controls.Cast<Control>()
                .Where(c => Parent.Controls.GetChildIndex(c) > Parent.Controls.GetChildIndex(this))
                .Where(c => c.Bounds.IntersectsWith(this.Bounds))
                .OrderByDescending(c => Parent.Controls.GetChildIndex(c))
                .ToList()
                .ForEach(c => c.DrawToBitmap(bmp, c.Bounds));

          arguments.Graphics.DrawImage(bmp, -Left, -Top);
        }
      }

      if(this.dotOutlinePen == null) {
        this.dotOutlinePen = new Pen(this.dotOutlineColor);
      }
      if(this.dotFillBrush == null) {
        this.dotFillBrush = new SolidBrush(this.dotFillColor);
      }

      //e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

      int diameter = Math.Min(Width, Height);
      PointF center = new PointF(diameter / 2.0f, diameter / 2.0f);

      int bigRadius = diameter / 2 - DotRadius - (DotCount - 1) * ScaleFactor;

      float unitAngle = 360 / DotCount;

      for(int index = 0; index < DotCount; ++index) {
        int dotIndex = (index + leadingDotIndex) % DotCount;

        var dotPosition = new PointF(
          center.X + (float)(bigRadius * Math.Cos(unitAngle * dotIndex * Math.PI / 180)),
          center.Y + (float)(bigRadius * Math.Sin(unitAngle * dotIndex * Math.PI / 180))
        );

        int currentDotRadius = DotRadius + index * ScaleFactor;

        PointF c1 = new PointF(dotPosition.X - currentDotRadius, dotPosition.Y - currentDotRadius);
        arguments.Graphics.FillEllipse(this.dotFillBrush, c1.X, c1.Y, 2 * currentDotRadius, 2 * currentDotRadius);
        arguments.Graphics.DrawEllipse(this.dotOutlinePen, c1.X, c1.Y, 2 * currentDotRadius, 2 * currentDotRadius);
      }

    }

    /// <summary>Called when the animation timer ticks to update the animation state</summary>
    /// <param name="sender">Animation timer that has ticked</param>
    /// <param name="arguments">Not used</param>
    private void animationTimerTicked(object sender, EventArgs arguments) {
      this.leadingDotIndex = (this.leadingDotIndex + 1) % DotCount; // Advance the animation
      Invalidate(); // Request a redraw at the earliest opportune time
    }

    /// <summary>Whether the spinner has been started</summary>
    private bool spinnerRunning;
    /// <summary>Color in which the dots will be filled</summary>
    private Color dotFillColor = Color.RoyalBlue;
    /// <summary>Color that will be used for the dots' outline</summary>
    private Color dotOutlineColor = Color.White;
    /// <summary>Brush used to fill the dots</summary>
    private Brush dotFillBrush;
    /// <summary>Brush used for the dots' outline</summary>
    private Pen dotOutlinePen;
    /// <summary>Index of the currently leading dot</summary>
    private int leadingDotIndex = 0;

  }

} // namespace Nuclex.Windows.Forms.Controls
