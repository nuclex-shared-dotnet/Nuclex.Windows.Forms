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
using System.Windows.Forms;

namespace Nuclex.Windows.Forms {

  /// <summary>Dummy implementation of the active window tracker service</summary>
  internal class NullActiveWindowTracker : IActiveWindowTracker {

    /// <summary>The default instance of the dummy window tracker</summary>
    public static readonly NullActiveWindowTracker Default = new NullActiveWindowTracker();

    /// <summary>The currently active top-level or modal window</summary>
    public Form ActiveWindow { get { return null; } }

  }

} // namespace Nuclex.Windows.Forms
