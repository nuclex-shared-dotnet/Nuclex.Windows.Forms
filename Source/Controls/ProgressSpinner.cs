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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace Nuclex.Windows.Forms.Controls {

  /// <summary>Displays a progress spinner to entertain the user while waiting</summary>
#if NET6_0_OR_GREATER
  [SupportedOSPlatform("windows")]
#endif
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

    /// <summary>Calculates the optimal size for the spinner control</summary>
    /// <returns>The optimal size for the spinner control to have</returns>
    /// <remarks>
    ///   Thanks to WinForms limited control transparency, the progress spinner needs to
    ///   redraw every control behind it each time it updates. Thus it's wise to keep it
    ///   as small as possible, but wide enough to fit the status text, if any.
    /// </remarks>
    public Size GetOptimalSize() {
      SizeF textRectangle;
      using(var dummyImage = new Bitmap(1, 1)) {
        using(Graphics graphics = Graphics.FromImage(dummyImage)) {
          textRectangle = graphics.MeasureString(
            this.statusText, this.statusFont
          );
        }
      }

      return new Size(
        Math.Max(128, (int)(textRectangle.Width + 2.0f)),
        this.statusFont.Height + 128
      );
    }

    /// <summary>Font that is used to display the status text</summary>
    public Font StatusFont {
      get { return this.statusFont; }
      set { this.statusFont = value; }
    }

    /// <summary>Text that will be displayed as the control's status</summary>
    public string StatusText {
      get { return this.statusText; }
      set { this.statusText = value; }
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
      paintControlsBehindMe(arguments);
      paintAnimatedDots(arguments);
      paintStatusMessage(arguments);
    }

    /// <summary>Forcefully redraws the controls below this one</summary>
    /// <param name="arguments">Provides access to the drawing surface and tools</param>
    /// <remarks>
    ///   <para>
    ///     WinForms has very poor transparency support. A transparent control will only
    ///     be transparent to its immediate parent (so the parent needs to be a container
    ///     control and hold the transparent control as its preferrably only child).
    ///   </para>
    ///   <para>
    ///     Worse yet, if you manually establish this relationship in your .Designer.cs
    ///     file, the Visual Studio WinForms designer will dismantle it next time you
    ///     edit something. This method fixes those issues by repainting all controls
    ///     that are behind this control and whose bounding box intersect this control.
    ///   </para>
    /// </remarks>
    private void paintControlsBehindMe(PaintEventArgs arguments) {
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
    }

    /// <summary>Draws a simple animated dots animation</summary>
    /// <param name="arguments">Provides access to the drawing surface and tools</param>
    private void paintAnimatedDots(PaintEventArgs arguments) {
      if(this.dotOutlinePen == null) {
        this.dotOutlinePen = new Pen(this.dotOutlineColor);
      }
      if(this.dotFillBrush == null) {
        this.dotFillBrush = new SolidBrush(this.dotFillColor);
      }

      SmoothingMode prevousSmoothingMode = arguments.Graphics.SmoothingMode;
      arguments.Graphics.SmoothingMode = SmoothingMode.HighQuality;
      try {
        PointF center = new PointF(Width / 2.0f, (Height - this.statusFont.Height - 2) / 2.0f);

        int diameter = Math.Min(Width, Height - this.statusFont.Height - 2);
        int bigRadius = diameter / 2 - DotRadius - (DotCount - 1) * ScaleFactor;

        // Draw the dots
        float unitAngle = 360.0f / DotCount;
        for(int index = 0; index < DotCount; ++index) {
          int dotIndex = (index + leadingDotIndex) % DotCount;

          var dotPosition = new PointF(
            center.X + (float)(bigRadius * Math.Cos(unitAngle * dotIndex * Math.PI / 180.0f)),
            center.Y + (float)(bigRadius * Math.Sin(unitAngle * dotIndex * Math.PI / 180.0f))
          );

          int currentDotRadius = DotRadius + index * ScaleFactor;

          var corner = new PointF(
            dotPosition.X - currentDotRadius, dotPosition.Y - currentDotRadius
          );
          arguments.Graphics.FillEllipse(
            this.dotFillBrush, corner.X, corner.Y, 2 * currentDotRadius, 2 * currentDotRadius
          );
          arguments.Graphics.DrawEllipse(
            this.dotOutlinePen, corner.X, corner.Y, 2 * currentDotRadius, 2 * currentDotRadius
          );
        }
      }
      finally {
        arguments.Graphics.SmoothingMode = prevousSmoothingMode;
      }
    }

    /// <summary>Draws the status message under the animated dots</summary>
    /// <param name="arguments">Provides access to the drawing surface and tools</param>
    private void paintStatusMessage(PaintEventArgs arguments) {
      if(!string.IsNullOrEmpty(this.statusText)) {
        SizeF textRectangle = arguments.Graphics.MeasureString(
          this.statusText, this.statusFont
        );

        var messageArea = new RectangleF(
          (Width - textRectangle.Width) / 2.0f,
          Height - this.statusFont.Height - 1.0f,
          textRectangle.Width,
          this.statusFont.Height
        );

        // Draw text with a white halo. This is a little bit ugly...
        {
          messageArea.Offset(-1.0f, 0.0f);
          arguments.Graphics.DrawString(
            this.statusText, this.statusFont, Brushes.White, messageArea
          );

          messageArea.Offset(2.0f, 0.0f);
          arguments.Graphics.DrawString(
            this.statusText, this.statusFont, Brushes.White, messageArea
          );

          messageArea.Offset(-1.0f, -1.0f);
          arguments.Graphics.DrawString(
            this.statusText, this.statusFont, Brushes.White, messageArea
          );

          messageArea.Offset(0.0f, 2.0f);
          arguments.Graphics.DrawString(
            this.statusText, this.statusFont, Brushes.White, messageArea
          );

          messageArea.Offset(0.0f, -1.0f);
          arguments.Graphics.DrawString(
            this.statusText, this.statusFont, this.dotFillBrush, messageArea
          );
        }
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
    /// <summary>Index of the currently leading dot</summary>
    private int leadingDotIndex = 0;
    /// <summary>Text that will be displayed under the control as the current status</summary>
    private string statusText;

    /// <summary>Color in which the dots will be filled</summary>
    private Color dotFillColor = Color.RoyalBlue;
    /// <summary>Color that will be used for the dots' outline</summary>
    private Color dotOutlineColor = Color.White;
    /// <summary>Brush used to fill the dots</summary>
    private Brush dotFillBrush;
    /// <summary>Brush used for the dots' outline</summary>
    private Pen dotOutlinePen;
    /// <summary>Font that is used to display the status text</summary>
    private Font statusFont = SystemFonts.SmallCaptionFont;

  }

} // namespace Nuclex.Windows.Forms.Controls
