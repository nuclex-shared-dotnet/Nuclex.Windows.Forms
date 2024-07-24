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

#if UNITTEST

using System;
using System.IO;
using System.Windows.Forms;

using NUnit.Framework;

using Nuclex.Support;

namespace Nuclex.Windows.Forms {

  /// <summary>Unit Test for the control container list view</summary>
  [TestFixture, Explicit]
  public class ContainerListViewTest {

    /// <summary>
    ///   Verifies that the asynchronous progress bar's constructor is working
    /// </summary>
    [Test]
    public void TestConstructor() {
      using(ContainerListView listView = new ContainerListView()) {

        // Let the control create its window handle
        listView.CreateControl();
        listView.Columns.Add("Numeric");
        listView.Columns.Add("Spelled");
        listView.Columns.Add("Nonsense");

        addRow(listView, "1", "One");
        addRow(listView, "2", "Two");
        addRow(listView, "3", "Three");

        using(CheckBox checkBox = new CheckBox()) {
          listView.EmbeddedControls.Add(new ListViewEmbeddedControl(checkBox, 2, 0));
          listView.EmbeddedControls.Clear();

          listView.Refresh();

          ListViewEmbeddedControl embeddedControl = new ListViewEmbeddedControl(
            checkBox, 2, 0
          );
          listView.EmbeddedControls.Add(embeddedControl);
          listView.EmbeddedControls.Remove(embeddedControl);

          listView.Refresh();
        }

      }
    }

    /// <summary>Adds a row to a control container list view</summary>
    /// <param name="listView">List view control the row will be added to</param>
    /// <param name="columns">Values that will appear in the individual columns</param>
    private void addRow(ContainerListView listView, params string[] columns) {
      listView.Items.Add(new ListViewItem(columns));
    }

  }

} // namespace Nuclex.Windows.Forms

#endif // UNITTEST
