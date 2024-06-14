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
using System.Windows.Forms;

namespace Nuclex.Windows.Forms {

  /// <summary>Stores informations about an embedded control</summary>
  public class ListViewEmbeddedControl {

    /// <summary>Initializes a new embedded control holder</summary>
    /// <param name="control">Control being embedded in a list view</param>
    /// <param name="row">List row at which the control will be embedded</param>
    /// <param name="column">List column at which the control will be embedded</param>
    public ListViewEmbeddedControl(Control control, int row, int column) {
      this.control = control;
      this.row = row;
      this.column = column;
    }

    /// <summary>Control that is being embedded in the ListView</summary>
    public Control Control {
      get { return this.control; }
    }

    /// <summary>Row the control has been embedded in</summary>
    public int Row {
      get { return this.row; }
    }

    /// <summary>Column the control has been embedded in</summary>
    public int Column {
      get { return this.column; }
    }

    /// <summary>Embedded control</summary>
    private Control control;
    /// <summary>Row where the control is embedded</summary>
    private int row;
    /// <summary>Column where the control is embedded</summary>
    private int column;

  }

} // namespace Nuclex.Windows.Forms
