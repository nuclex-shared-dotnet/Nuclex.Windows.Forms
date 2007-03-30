using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nuclex.Windows.Forms {

  /// <summary>Collection of controls embedded in another control</summary>
  public class EmbeddedControlCollection : Control.ControlCollection {

    #region class ControlEventArgs

    /// <summary>Arguments class for events that need to pass a control</summary>
    public class ControlEventArgs : EventArgs {

      /// <summary>Initializes a new event arguments provider</summary>
      /// <param name="control">Control to be supplied to the event handler</param>
      public ControlEventArgs(Control control) {
        this.control = control;
      }

      /// <summary>Obtains the control the event arguments are carrying</summary>
      public Control Control { get { return this.control; } }

      /// <summary>Control that's passed to the event handler</summary>
      private Control control;

    }

    #endregion // class ControlEventArgs

    /// <summary>Raised when a control has been added to the collection</summary>
    public event EventHandler<ControlEventArgs> ControlAdded;
    /// <summary>Raised when a control is removed from the collection</summary>
    public event EventHandler<ControlEventArgs> ControlRemoved;

    /// <summary>Initializes a new instance of the EmbeddedControlCollection class</summary>
    /// <param name="owner">
    ///   A System.Windows.Forms.Control representing the control
    ///   that owns the control collection
    /// </param>
    EmbeddedControlCollection(Control owner) : base(owner) { }

    /// <summary>Adds the specified control to the control collection</summary>
    /// <param name="value">
    ///   The System.Windows.Forms.Control to add to the control collection
    /// </param>
    public override void Add(Control value) {
      base.Add(value);

      OnControlAdded(value);
    }

    /// <summary>Removes the specified control from the control collection</summary>
    /// <param name="value">
    ///   The System.Windows.Forms.Control to remove from the EmbeddedControlCollection
    /// </param>
    public override void Remove(Control value) {
      base.Remove(value);

      OnControlRemoved(value);
    }

    /*
    /// <summary>Adds an array of control objects to the collection</summary>
    /// <param name="controls">
    ///   An array of System.Windows.Forms.Control objects to add to the collection
    /// </param>
    public override void AddRange(Control[] controls) {
      base.AddRange(controls);
    }

    /// <summary>Removes all controls from the collection</summary>
    public override void Clear() {
      base.Clear();
    }

    /// <summary>Removes the child control with the specified key</summary>
    /// <param name="key">The name of the child control to remove</param>
    public override void RemoveByKey(string key) {
      base.RemoveByKey(key);
    }
    */

    /// <summary>
    ///   Called when a control has been added to the collection,
    ///   fires the ControlAdded event
    /// </summary>
    /// <param name="control">Control that has been added to the collection</param>
    protected virtual void OnControlAdded(Control control) {
      if(ControlAdded != null)
        ControlAdded(this, new ControlEventArgs(control));
    }

    /// <summary>
    ///   Called when a control has been removed to the collection,
    ///   fires the ControlRemoved event
    /// </summary>
    /// <param name="control">Control that has been removed from the collection</param>
    protected virtual void OnControlRemoved(Control control) {
      if(ControlRemoved != null)
        ControlRemoved(this, new ControlEventArgs(control));
    }

  }

} // namespace Nuclex.Windows.Forms
