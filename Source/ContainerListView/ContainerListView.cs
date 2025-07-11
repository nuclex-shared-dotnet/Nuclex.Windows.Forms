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
using System.Runtime.InteropServices;

#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Nuclex.Support.Collections;

namespace Nuclex.Windows.Forms {

  /// <summary>ListView allowing for other controls to be embedded in its cells</summary>
  /// <remarks>
  ///   <para>
  ///     There basically were two possible design choices: Provide a specialized
  ///     ListViewSubItem that carries a Control instead of a string or manage the
  ///     embedded controls seperate of the ListView's items.
  ///   </para>
  ///   <para>
  ///     The first option requires a complete rewrite of the ListViewItem class
  ///     and its related support classes, all of which are surprisingly large and
  ///     complex. Thus, I chose the less clean but more doable latter option.
  ///   </para>
  ///   <para>
  ///     This control is useful for simple item lists where you want to provide
  ///     a combo box, checkbox or other control to the user for a certain column.
  ///     It will not perform well for lists with hundreds of items since it
  ///     requires a control to be created per row and management of the embedded
  ///     controls is designed for limited usage.
  ///   </para>
  /// </remarks>
#if NET6_0_OR_GREATER
  [SupportedOSPlatform("windows")]
#endif
  public partial class ContainerListView : System.Windows.Forms.ListView {

    /// <summary>Message sent to a control to let it paint itself</summary>
    private const int WM_PAINT = 0x000F;

    /// <summary>Initializes a new ContainerListView</summary>
    public ContainerListView() {
      this.embeddedControlClickedDelegate = new EventHandler(embeddedControlClicked);

      this.embeddedControls = new ObservableList<ListViewEmbeddedControl>();
      this.embeddedControls.ItemAdded +=
        new EventHandler<ItemEventArgs<ListViewEmbeddedControl>>(embeddedControlAdded);
      this.embeddedControls.ItemRemoved +=
        new EventHandler<ItemEventArgs<ListViewEmbeddedControl>>(embeddedControlRemoved);
      this.embeddedControls.Clearing += new EventHandler(embeddedControlsClearing);

      InitializeComponent();
      
      // Eliminate flickering
      SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
      SetStyle(ControlStyles.AllPaintingInWmPaint, true);

      base.View = View.Details;

#if NET6_0_OR_GREATER
      if(OperatingSystem.IsWindows()) {
#else
      if(Environment.OSVersion.Platform == PlatformID.Win32NT) {
#endif
        this.columnHeaderHeight = Font.Height;
      } else {
        // Font sizes *should* be in one 72ths of an inch. And screens *might*
        // be set to 96 DPI. So this is the best we can do, cross-platform,
        // due to Microsoft's shortsighted design. Sorry high DPI users.
        //this.columnHeaderHeight = (int)(Font.Size * 96 / 72);
        this.columnHeaderHeight = 16;
      }
    }

    /// <summary>Controls being embedded in the ListView</summary>
    public ICollection<ListViewEmbeddedControl> EmbeddedControls {
      get { return this.embeddedControls; }
    }

    /// <summary>Updates the controls embeded into the list view</summary>
    public void UpdateEmbeddedControls() {
      if(View != View.Details) {
        for(int index = 0; index < this.embeddedControls.Count; ++index) {
          this.embeddedControls[index].Control.Visible = false;
        }
      } else {
        for(int index = 0; index < this.embeddedControls.Count; ++index) {
          ListViewEmbeddedControl embeddedControl = this.embeddedControls[index];

          Rectangle cellBounds = this.GetSubItemBounds(
            Items[embeddedControl.Row], embeddedControl.Column
          );

          bool intersectsColumnHeader =
            (base.HeaderStyle != ColumnHeaderStyle.None) &&
            (cellBounds.Top < this.columnHeaderHeight);

          embeddedControl.Control.Visible = !intersectsColumnHeader;
          embeddedControl.Control.Bounds = cellBounds;
        }
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
      if(order == null) { // No Columns
        return Rectangle.Empty;
      }

      if(subItem >= order.Length) {
        throw new IndexOutOfRangeException("SubItem " + subItem + " out of range");
      }

      // Determine the border of the entire ListViewItem, including all sub items
      Rectangle itemBounds = item.GetBounds(ItemBoundsPortion.Entire);
      int subItemX = itemBounds.Left;

      // Find the horizontal position of the sub item. Because the column order can vary,
      // we need to use Columns[order[i]] instead of simply doing Columns[i] here!
      ColumnHeader columnHeader;
      int i;
      for(i = 0; i < order.Length; ++i) {
        columnHeader = this.Columns[order[i]];
        if(columnHeader.Index == subItem) {
          break;
        }

        subItemX += columnHeader.Width;
      }

      return new Rectangle(
        subItemX, itemBounds.Top, this.Columns[order[i]].Width, itemBounds.Height
      );
    }

    /// <summary>Responds to window messages sent by the operating system</summary>
    /// <param name="message">Window message that will be processed</param>
    protected override void WndProc(ref Message message) {
      switch(message.Msg) {
        case WM_PAINT: {
          UpdateEmbeddedControls();
          break;
        }
      }

      base.WndProc(ref message);
    }

    /// <summary>Called when the list of embedded controls has been cleared</summary>
    /// <param name="sender">Collection that has been cleared of its controls</param>
    /// <param name="arguments">Not used</param>
    private void embeddedControlsClearing(object sender, EventArgs arguments) {
      this.BeginUpdate();
      try {
        foreach(ListViewEmbeddedControl embeddedControl in this.embeddedControls) {
          embeddedControl.Control.Click -= this.embeddedControlClickedDelegate;
          this.Controls.Remove(embeddedControl.Control);
        }
      }
      finally {
        this.EndUpdate();
      }
    }

    /// <summary>Called when a control gets removed from  the embedded controls list</summary>
    /// <param name="sender">List from which the control has been removed</param>
    /// <param name="arguments">
    ///   Event arguments providing a reference to the removed control
    /// </param>
    private void embeddedControlAdded(
      object sender, ItemEventArgs<ListViewEmbeddedControl> arguments
    ) {
      arguments.Item.Control.Click += this.embeddedControlClickedDelegate;
      this.Controls.Add(arguments.Item.Control);
    }

    /// <summary>Called when a control gets added to the embedded controls list</summary>
    /// <param name="sender">List to which the control has been added</param>
    /// <param name="arguments">
    ///   Event arguments providing a reference to the added control
    /// </param>
    private void embeddedControlRemoved(
      object sender, ItemEventArgs<ListViewEmbeddedControl> arguments
    ) {
      if(this.Controls.Contains(arguments.Item.Control)) {
        arguments.Item.Control.Click -= this.embeddedControlClickedDelegate;
        this.Controls.Remove(arguments.Item.Control);
      }
    }

    /// <summary>Called when an embedded control has been clicked on</summary>
    /// <param name="sender">Embedded control that has been clicked</param>
    /// <param name="arguments">Not used</param>
    private void embeddedControlClicked(object sender, EventArgs arguments) {
      this.BeginUpdate();

      try {
        SelectedItems.Clear();

        foreach(ListViewEmbeddedControl embeddedControl in this.embeddedControls) {
          if(ReferenceEquals(embeddedControl.Control, sender)) {
            if((embeddedControl.Row > 0) && (embeddedControl.Row < Items.Count)) {
              Items[embeddedControl.Row].Selected = true;
            }
          }
        }
      }
      finally {
        this.EndUpdate();
      }
    }

    /// <summary>Obtains the current column order of the list</summary>
    /// <returns>An array indicating the order of the list's columns</returns>
    private int[] GetColumnOrder() {
      int[] order = new int[this.Columns.Count];

      for(int index = 0; index < this.Columns.Count; ++index) {
        order[this.Columns[index].DisplayIndex] = index;
      }

      return order;
    }

    /// <summary>Height of the list view's column header</summary>
    private int columnHeaderHeight;
    /// <summary>Event handler for when embedded controls are clicked on</summary>
    private EventHandler embeddedControlClickedDelegate;
    /// <summary>Controls being embedded in this ListView</summary>
    private ObservableList<ListViewEmbeddedControl> embeddedControls;

  }

} // namespace Nuclex.Windows.Forms
