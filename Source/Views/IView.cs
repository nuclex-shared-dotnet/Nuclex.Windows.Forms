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
