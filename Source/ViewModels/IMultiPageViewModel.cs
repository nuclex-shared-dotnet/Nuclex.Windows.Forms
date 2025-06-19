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

namespace Nuclex.Windows.Forms.ViewModels {

  /// <summary>Interface for vew models that can switch between different pages</summary>
  public interface IMultiPageViewModel {

    /// <summary>Retrieves (and, if needed, creates) the view model for the active page</summary>
    /// <returns>A view model for the active page on the multi-page view model</returns>
    object GetActivePageViewModel();

  }

} // namespace Nuclex.Windows.Forms.ViewModels
