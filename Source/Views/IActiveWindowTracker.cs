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

	/// <summary>Enables consumer to look up the currently active window</summary>
	public interface IActiveWindowTracker {

		/// <summary>The currently active top-level or modal window</summary>
		/// <remarks>
		///   If windows live in multiple threads, the property change notification for
		///   this property, if supported, might be fired from a different thread.
		/// </remarks>
		Form ActiveWindow { get; }

	}

} // namespace Nuclex.Windows.Forms
