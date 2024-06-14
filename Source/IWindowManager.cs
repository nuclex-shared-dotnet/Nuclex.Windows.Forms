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

  /// <summary>Interface for a window manager used in an MVVM environment</summary>
  public interface IWindowManager : IActiveWindowTracker {

    /// <summary>Opens a view as a new root window of the application</summary>
    /// <typeparam name="TViewModel">
    ///   Type of view model a root window will be opened for
    /// </typeparam>
    /// <param name="viewModel">
    ///   View model a window will be opened for. If null, the view model will be
    ///   created as well (unless the dialog already specifies one as a resource)
    /// </param>
    /// <param name="disposeOnClose">
    ///   Whether the view model should be disposed when the view is closed
    /// </param>
    /// <returns>The window that has been opened by the window manager</returns>
    Form OpenRoot<TViewModel>(
      TViewModel viewModel = null, bool disposeOnClose = true
    ) where TViewModel : class;

    /// <summary>Displays a view as a modal window</summary>
    /// <typeparam name="TViewModel">
    ///   Type of the view model for which a view will be displayed
    /// </typeparam>
    /// <param name="viewModel">
    ///   View model a modal window will be displayed for. If null, the view model will
    ///   be created as well (unless the dialog already specifies one as a resource)
    /// </param>
    /// <param name="disposeOnClose">
    ///   Whether the view model should be disposed when the view is closed
    /// </param>
    /// <returns>The return value of the modal window</returns>
    bool? ShowModal<TViewModel>(
      TViewModel viewModel = null, bool disposeOnClose = true
    ) where TViewModel : class;

    /// <summary>Creates the view for the specified view model</summary>
    /// <typeparam name="TViewModel">
    ///   Type of view model for which a view will be created
    /// </typeparam>
    /// <param name="viewModel">
    ///   View model a view will be created for. If null, the view model will be
    ///   created as well (unless the dialog already specifies one as a resource)
    /// </param>
    /// <returns>The view for the specified view model</returns>
    Control CreateView<TViewModel>(TViewModel viewModel = null)
      where TViewModel : class;

    /// <summary>Creates a view model without a matching view</summary>
    /// <typeparam name="TViewModel">Type of view model that will be created</typeparam>
    /// <returns>The new view model</returns>
    /// <remarks>
    ///   <para>
    ///     This is useful if a view model needs to create child view models (i.e. paged container
    ///     and wants to ensure the same dependency injector (if any) if used as the window
    ///     manager uses for other view models it creates.
    ///   </para>
    ///   <para>
    ///     This way, view models can set up their child view models without having to immediately
    ///     bind a view to them. Later on, views can use the window manager to create a matching
    ///     child view and store it in a container.
    ///   </para>
    /// </remarks>
    TViewModel CreateViewModel<TViewModel>()
      where TViewModel : class;

  }

} // namespace Nuclex.Windows.Forms
