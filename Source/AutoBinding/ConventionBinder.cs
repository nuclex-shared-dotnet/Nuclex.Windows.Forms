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

using Nuclex.Windows.Forms.Views;
using System;
using System.Windows.Forms;

namespace Nuclex.Windows.Forms.AutoBinding {

  /// <summary>
  ///   Binds a view to its model using a convention-over-configuration approach
  /// </summary>
  public class ConventionBinder : IAutoBinder {

    /// <summary>Binds the specified view to an explicitly selected view model</summary>
    /// <typeparam name="TViewModel">
    ///   Type of view model the view will be bound to
    /// </typeparam>
    /// <param name="view">View that will be bound to a view model</param>
    /// <param name="viewModel">View model the view will be bound to</param>
    public void Bind<TViewModel>(Control view, TViewModel viewModel)
      where TViewModel : class {
      bind(view, viewModel);
    }

    /// <summary>
    ///   Binds the specified view to the view model specified in its DataContext
    /// </summary>
    /// <param name="viewControl">View that will be bound</param>
    public void Bind(Control viewControl) {
      IView viewControlAsView = viewControl as IView;
      if(viewControlAsView == null) {
        throw new InvalidOperationException(
          "The specified view has no view model associated. Either assign your " +
          "view model to the view's data context beforehand or use the overload " +
          "of Bind() that allows you to explicitly specify the view model."
        );
      }

      bind(viewControl, viewControlAsView.DataContext);
    }

    /// <summary>Binds a view to a view model</summary>
    /// <param name="view">View that will be bound</param>
    /// <param name="viewModel">View model the view will be bound to</param>
    private void bind(Control view, object viewModel) {
    }

  }
} // namespace Nuclex.Windows.Forms.AutoBinding
