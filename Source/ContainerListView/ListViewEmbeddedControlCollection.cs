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
    public event EventHandler<ListViewEmbeddedControlEventArgs> Added;
    /// <summary>Raised when a control is removed from the collection</summary>
    public event EventHandler<ListViewEmbeddedControlEventArgs> Removed;
    /// <summary>Raised when the collection is about to be cleared</summary>
    public event EventHandler Clearing;

    /// <summary>Removes all elements from the ListViewEmbeddedControlCollection</summary>
    protected override void ClearItems() {
      OnClearing();

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

      OnAdded(item);
    }

    /// <summary>
    ///   Removes the element at the specified index of the ListViewEmbeddedControlCollection
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove</param>
    protected override void RemoveItem(int index) {
      ListViewEmbeddedControl control = base[index];

      base.RemoveItem(index);

      OnRemoved(control);
    }

    /// <summary>Replaces the element at the specified index</summary>
    /// <param name="index">
    ///   The new value for the element at the specified index. The value can be null
    ///   for reference types
    /// </param>
    /// <param name="item">The zero-based index of the element to replace</param>
    protected override void SetItem(int index, ListViewEmbeddedControl item) {
      ListViewEmbeddedControl control = base[index];

      base.SetItem(index, item);

      OnRemoved(control);
      OnAdded(item);
    }

    /// <summary>Fires the Added event</summary>
    /// <param name="embeddedControl">
    ///   Embedded control that has been added to the collection
    /// </param>
    protected virtual void OnAdded(ListViewEmbeddedControl embeddedControl) {
      if(Added != null)
        Added(this, new ListViewEmbeddedControlEventArgs(embeddedControl));
    }

    /// <summary>Fires the Removed event</summary>
    /// <param name="embeddedControl">
    ///   Embedded control that has been removed from the collection
    /// </param>
    protected virtual void OnRemoved(ListViewEmbeddedControl embeddedControl) {
      if(Removed != null)
        Removed(this, new ListViewEmbeddedControlEventArgs(embeddedControl));
    }

    /// <summary>Fires the Clearing event</summary>
    protected virtual void OnClearing() {
      if(Clearing != null)
        Clearing(this, EventArgs.Empty);
    }

  }

} // namespace Nuclex.Windows.Forms
