using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Nuclex.Windows.Forms {

  /// <summary>ListView allowing for other controls to be embedded in its cells</summary>
  /// <remarks>
  ///   There basically were two possible design choices: Provide a specialized
  ///   ListViewSubItem that carries a Control instead of a string or manage the
  ///   embedded controls in seperation of the ListView's items. The first option
  ///   would require a complete rewrite of the ListViewItem class and its related
  ///   support classes, all of which are surprisingly large and complex. Thus,
  ///   the less clean but more doable latter option has been chosen.
  /// </remarks>
  public partial class ContainerListView : System.Windows.Forms.ListView {

    /// <summary>Initializes a new ContainerListView</summary>
    public ContainerListView() {
      this.embeddedControlClickedHandler = new EventHandler(embeddedControlClicked);

      this.embeddedControls = new ListViewEmbeddedControlCollection();
      this.embeddedControls.Added +=
        new EventHandler<ListViewEmbeddedControlCollection.ListViewEmbeddedControlEventArgs>(
          embeddedControlAdded
        );
      this.embeddedControls.Removed +=
        new EventHandler<ListViewEmbeddedControlCollection.ListViewEmbeddedControlEventArgs>(
          embeddedControlRemoved
        );
      this.embeddedControls.Clearing +=
        new EventHandler(embeddedControlsClearing);

      InitializeComponent();

      base.View = View.Details;
      base.AllowColumnReorder = false;
    }

    /// <summary>Called when the list of embedded controls has been cleared</summary>
    /// <param name="sender">Collection that has been cleared of its controls</param>
    /// <param name="e">Not used</param>
    private void embeddedControlsClearing(object sender, EventArgs e) {
      this.BeginUpdate();
      try {
        foreach(ListViewEmbeddedControl embeddedControl in this.embeddedControls) {
          embeddedControl.Control.Click -= this.embeddedControlClickedHandler;
          this.Controls.Remove(embeddedControl.Control);
        }
      }
      finally {
        this.EndUpdate();
      }
    }

    /// <summary>Called when a control gets removed from  the embedded controls list</summary>
    /// <param name="sender">List from which the control has been removed</param>
    /// <param name="e">Event arguments providing a reference to the removed control</param>
    private void embeddedControlAdded(
      object sender, ListViewEmbeddedControlCollection.ListViewEmbeddedControlEventArgs e
    ) {
      e.EmbeddedControl.Control.Click += this.embeddedControlClickedHandler;
      this.Controls.Add(e.EmbeddedControl.Control);
    }

    /// <summary>Called when a control gets added to the embedded controls list</summary>
    /// <param name="sender">List to which the control has been added</param>
    /// <param name="e">Event arguments providing a reference to the added control</param>
    private void embeddedControlRemoved(
      object sender, ListViewEmbeddedControlCollection.ListViewEmbeddedControlEventArgs e
    ) {
      if(this.Controls.Contains(e.EmbeddedControl.Control)) {
        e.EmbeddedControl.Control.Click -= this.embeddedControlClickedHandler;
        this.Controls.Remove(e.EmbeddedControl.Control);
      }
    }

    /// <summary>Called when an embedded control has been clicked on</summary>
    /// <param name="sender">Embedded control that has been clicked</param>
    /// <param name="e">Not used</param>
    private void embeddedControlClicked(object sender, EventArgs e) {
      this.BeginUpdate();

      try {
        SelectedItems.Clear();

        foreach(ListViewEmbeddedControl embeddedControl in this.embeddedControls) {
          if(ReferenceEquals(embeddedControl.Control, sender)) {
            if((embeddedControl.Row > 0) && (embeddedControl.Row < Items.Count))
              Items[embeddedControl.Row].Selected = true;
          }
        }
      }
      finally {
        this.EndUpdate();
      }
    }

    /// <summary>Calculates the boundaries of a cell in the list view</summary>
    /// <param name="item">Item in the list view from which to calculate the cell</param>
    /// <param name="subItem">Index der cell whose boundaries to calculate</param>
    /// <returns>The boundaries of the specified list view cell</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   When the specified sub item index is not in the range of valid sub items
    /// </exception>
    protected Rectangle GetSubItemBounds(ListViewItem item, int subItem) {

      int[] order = GetColumnOrder();
      if(order == null) // No Columns
        return Rectangle.Empty;

      if(subItem >= order.Length)
        throw new IndexOutOfRangeException("SubItem " + subItem + " out of range");

      // Determine the border of the entire ListViewItem, including all sub items
      Rectangle itemBounds = item.GetBounds(ItemBoundsPortion.Entire);
      int subItemX = itemBounds.Left;

      // Find the horizontal position of the sub item. Because the column order can vary,
      // we need to use Columns[order[i]] instead of simply doing Columns[i] here!
      ColumnHeader columnHeader;
      int i;
      for(i = 0; i < order.Length; ++i) {
        columnHeader = this.Columns[order[i]];
        if(columnHeader.Index == subItem)
          break;

        subItemX += columnHeader.Width;
      }

      return new Rectangle(
        subItemX, itemBounds.Top, this.Columns[order[i]].Width, itemBounds.Height
      );

    }

    /// <summary>Obtains the current column order of the list</summary>
    /// <returns>An array indicating the order of the list's columns</returns>
    private int[] GetColumnOrder() {
      int[] order = new int[this.Columns.Count];

      for(int i = 0; i < this.Columns.Count; ++i)
        order[this.Columns[i].DisplayIndex] = i;

      return order;
    }

    /// <summary>Event handler for when embedded controls are clicked on</summary>
    private EventHandler embeddedControlClickedHandler;
    /// <summary>Controls being embedded in this ListView</summary>
    private ListViewEmbeddedControlCollection embeddedControls;

  }

} // namespace Nuclex.Windows.Forms
