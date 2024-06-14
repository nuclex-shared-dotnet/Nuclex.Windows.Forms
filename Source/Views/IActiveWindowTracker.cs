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
