#region CPL License
/*
Nuclex Framework
Copyright (C) 2002-2019 Nuclex Development Labs

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

namespace Nuclex.Windows.Forms.Views {

  /// <summary>View with support for data binding</summary>
  public interface IView {

    /// <summary>Provides the data binding target for the view</summary>
    /// <remarks>
    ///   This property is identical to the same-named one in WPF, it provides
    ///   the view model to which the view should bind its controls.
    /// </remarks>
    object DataContext { get; set; }

    // Whether the view owns its view model and it needs to be disposed after
    // the view ceases to exist
    //bool IsOwnedByView { get; set; }

  }

} // namespace Nuclex.Windows.Forms.Views
