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
using Nuclex.Windows.Forms.Views;

namespace Nuclex.Windows.Forms.AutoBinding {

  /// <summary>Binds views to their view models</summary>
  public interface IAutoBinder {

    /// <summary>Binds the specified view to an explicitly selected view model</summary>
    /// <typeparam name="TViewModel">
    ///   Type of view model the view will be bound to
    /// </typeparam>
    /// <param name="view">View that will be bound to a view model</param>
    /// <param name="viewModel">View model the view will be bound to</param>
    void Bind<TViewModel>(Control view, TViewModel viewModel)
      where TViewModel : class;

    /// <summary>
    ///   Binds the specified view to the view model specified in its DataContext
    /// </summary>
    /// <param name="view">View that will be bound</param>
    void Bind(Control view);

  }

} // namespace Nuclex.Windows.Forms.AutoBinding
