using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nuclex.Windows.Forms {

  /// <summary>Collection of controls embedded in a ListView</summary>
  public class ListViewEmbeddedControlCollection : Collection<ListViewEmbeddedControl> {

    #region class ListViewEmbeddedControlEventArgs

    /// <summary>Arguments class for events that need to pass a control</summary>
    public class ListViewEmbeddedControlEventArgs : EventArgs {

      /// <summary>Initializes a new event arguments supplier</summary>
      /// <param name="embeddedControl">Control to be supplied to the event handler</param>
      public ListViewEmbeddedControlEventArgs(ListViewEmbeddedControl embeddedControl) {
        this.embeddedControl = embeddedControl;
      }

      /// <summary>Obtains the control the event arguments are carrying</summary>
      public ListViewEmbeddedControl EmbeddedControl {
        get { return this.embeddedControl; }
      }

      /// <summary>Control that's passed to the event handler</summary>
      private ListViewEmbeddedControl embeddedControl;

    }

    #endregion // class ListViewEmbeddedControlEventArgs

    /// <summary>Raised when a control has been added to the collection</summary>
    public event EventHandler<ListViewEmbeddedControlEventArgs> EmbeddedControlAdded;
    /// <summary>Raised when a control is removed from the collection</summary>
    public event EventHandler<ListViewEmbeddedControlEventArgs> EmbeddedControlRemoved;

    /// <summary>Removes all elements from the ListViewEmbeddedControlCollection</summary>
    protected override void ClearItems() {
      base.ClearItems();
    }

    /// <summary>
    ///   Inserts an element into the ListViewEmbeddedControlCollection at the specified index
    /// </summary>
    /// <param name="index">
    ///   The object to insert. The value can be null for reference types
    /// </param>
    /// <param name="item">The zero-based index at which item should be inserted</param>
    protected override void InsertItem(int index, ListViewEmbeddedControl item) {
      base.InsertItem(index, item);
    }

    /// <summary>
    ///   Removes the element at the specified index of the ListViewEmbeddedControlCollection
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove</param>
    protected override void RemoveItem(int index) {
      base.RemoveItem(index);
    }

    /// <summary>Replaces the element at the specified index</summary>
    /// <param name="index">
    ///   The new value for the element at the specified index. The value can be null
    ///   for reference types
    /// </param>
    /// <param name="item">The zero-based index of the element to replace</param>
    protected override void SetItem(int index, ListViewEmbeddedControl item) {
      base.SetItem(index, item);
    }

    /// <summary>Fires the EmbeddedControlAdded event</summary>
    /// <param name="embeddedControl">
    ///   Embedded control that has been added to the collection
    /// </param>
    protected virtual void OnEmbeddedControlAdded(ListViewEmbeddedControl embeddedControl) {
      if(EmbeddedControlAdded != null)
        EmbeddedControlAdded(this, new ListViewEmbeddedControlEventArgs(embeddedControl));
    }

    /// <summary>Fires the EmbeddedControlRemoved event</summary>
    /// <param name="embeddedControl">
    ///   Embedded control that has been removed from the collection
    /// </param>
    protected virtual void OnEmbeddedControlRemoved(ListViewEmbeddedControl embeddedControl) {
      if(EmbeddedControlRemoved != null)
        EmbeddedControlRemoved(this, new ListViewEmbeddedControlEventArgs(embeddedControl));
    }

  }

} // namespace Nuclex.Windows.Forms
