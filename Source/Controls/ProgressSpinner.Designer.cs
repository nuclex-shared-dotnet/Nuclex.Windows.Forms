namespace Nuclex.Windows.Forms.Controls {

  partial class ProgressSpinner {

    /// <summary> Required designer variable.</summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> Clean up any resources being used.</summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if(disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.animationUpdateTimer = new System.Windows.Forms.Timer();
      this.SuspendLayout();
      // 
      // animationUpdateTimer
      // 
      this.animationUpdateTimer.Tick += new System.EventHandler(this.animationTimerTicked);
      // 
      // ProgressSpinner
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Transparent;
      this.DoubleBuffered = true;
      this.Name = "ProgressSpinner";
      this.ResumeLayout(false);

    }

    #endregion

    /// <summary>Timer used to update the progress animation</summary>
    private System.Windows.Forms.Timer animationUpdateTimer;

  }

} // namespace Nuclex.Windows.Forms.Controls
