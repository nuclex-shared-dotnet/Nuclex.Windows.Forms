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
