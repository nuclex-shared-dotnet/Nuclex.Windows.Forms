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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;

using Nuclex.Support;
using Nuclex.Windows.Forms.AutoBinding;
using Nuclex.Windows.Forms.Views;

namespace Nuclex.Windows.Forms {

  /// <summary>Manages an application's windows and views</summary>
  public class WindowManager : Observable, IWindowManager {

    #region class CancellableDisposer

    /// <summary>Disposes an object that potentially implements IDisposable</summary>
    private struct CancellableDisposer : IDisposable {

      /// <summary>Initializes a new cancellable disposer</summary>
      /// <param name="potentiallyDisposable">
      ///   Object that potentially implements IDisposable
      /// </param>
      public CancellableDisposer(object potentiallyDisposable = null) {
        this.potentiallyDisposable = potentiallyDisposable;
      }

      /// <summary>
      ///   Disposes the assigned object if the disposer has not been cancelled
      /// </summary>
      public void Dispose() {
        var disposable = this.potentiallyDisposable as IDisposable;
        if(disposable != null) {
          disposable.Dispose();
        }
      }

      /// <summary>Cancels the call to Dispose(), keeping the object alive</summary>
      public void Dismiss() {
        this.potentiallyDisposable = null;
      }

      /// <summary>Assigns a new potentially disposable object</summary>
      /// <param name="potentiallyDisposable">
      ///   Potentially disposable object that the disposer will dispose
      /// </param>
      public void Set(object potentiallyDisposable) {
        this.potentiallyDisposable = potentiallyDisposable;
      }

      /// <summary>Object that will be disposed unless the disposer is cancelled</summary>
      private object potentiallyDisposable;

    }

    #endregion // class CancellableDisposer

    /// <summary>Initializes a new window manager</summary>
    /// <param name="autoBinder">
    ///   View model binder that will be used to bind all created views to their models
    /// </param>
    public WindowManager(IAutoBinder autoBinder = null) {
      this.autoBinder = autoBinder;

      this.rootWindowActivatedDelegate = rootWindowActivated;
      this.rootWindowClosedDelegate = rootWindowClosed;
      this.viewTypesForViewModels = new ConcurrentDictionary<Type, Type>();
    }

    /// <summary>The currently active top-level or modal window</summary>
    public Form ActiveWindow {
      get { return this.activeWindow; }
      private set {
        if(value != this.activeWindow) {
          this.activeWindow = value;
          OnPropertyChanged(nameof(ActiveWindow));
        }
      }
    }

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
    public Form OpenRoot<TViewModel>(
      TViewModel viewModel = null, bool disposeOnClose = true
    ) where TViewModel : class {

      Form window = (Form)CreateView(viewModel);
      window.Activated += this.rootWindowActivatedDelegate;
      window.Closed += this.rootWindowClosedDelegate;

      // If we either created the view model or the user explicitly asked us to
      // dispose his view model, tag the window so that we know to dispose it
      // when we're done (but still allow the user to change his mind)
      if((viewModel == null) || disposeOnClose) {
        window.Tag = "DisposeViewModelOnClose"; // TODO: Wrap SetProp() instead?
        //window.SetValue(DisposeViewModelOnCloseProperty, true);
      }

      window.Show();

      return window;
    }

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
    public bool? ShowModal<TViewModel>(
      TViewModel viewModel = null, bool disposeOnClose = true
    ) where TViewModel : class {
      Form window = (Form)CreateView(viewModel);
      window.Owner = this.activeWindow;
      window.Activated += this.rootWindowActivatedDelegate;

      try {
        // If we either created the view model or the user explicitly asked us to
        // dispose his view model, tag the window so that we know to dispose it
        // when we're done (but still allow the user to change his mind)
        if((viewModel == null) || disposeOnClose) {
          window.Tag = "DisposeViewModelOnClose"; // TODO: Wrap SetProp() instead?
          //window.SetValue(DisposeViewModelOnCloseProperty, true);
        }

        DialogResult result = window.ShowDialog(this.activeWindow);
        if((result == DialogResult.OK) || (result == DialogResult.Yes)) {
          return true;
        } else if((result == DialogResult.Cancel) || (result == DialogResult.No)) {
          return false;
        } else {
          return null;
        }
      }
      finally {
        window.Activated -= this.rootWindowActivatedDelegate;
        ActiveWindow = window.Owner;

        if(shouldDisposeViewModelOnClose(window)) {
          IView windowAsView = window as IView;
          if(windowAsView != null) {
            object viewModelAsObject = windowAsView.DataContext;
            windowAsView.DataContext = null;
            disposeIfDisposable(viewModelAsObject);
          }
        }
        disposeIfDisposable(window);
      }
    }

    /// <summary>Creates the view for the specified view model</summary>
    /// <typeparam name="TViewModel">
    ///   Type of view model for which a view will be created
    /// </typeparam>
    /// <param name="viewModel">
    ///   View model a view will be created for. If null, the view model will be
    ///   created as well (unless the dialog already specifies one as a resource)
    /// </param>
    /// <returns>The view for the specified view model</returns>
    public virtual Control CreateView<TViewModel>(
      TViewModel viewModel = null
    ) where TViewModel : class {
      Type viewType = LocateViewForViewModel(typeof(TViewModel));
      Control viewControl = (Control)CreateInstance(viewType);
      using(var viewDisposer = new CancellableDisposer(viewControl)) {

        // Create a view model if none was provided, and in either case assign
        // the view model to the view (provided it implements IView).
        using(var viewModelDisposer = new CancellableDisposer()) {
          IView viewControlAsView = viewControl as IView;
          if(viewModel == null) { // No view model provided, create one
            if(viewControlAsView == null) { // View doesn't implement IView
              viewModel = (TViewModel)CreateInstance(typeof(TViewModel));
              viewModelDisposer.Set(viewModel);
            } else if(viewControlAsView.DataContext == null) { // View has no view model
              viewModel = (TViewModel)CreateInstance(typeof(TViewModel));
              viewModelDisposer.Set(viewModel);
              viewControlAsView.DataContext = viewModel;
            } else { // There's an existing view model
              viewModel = viewControlAsView.DataContext as TViewModel;
              if(viewModel == null) { // The existing view model is another type
                viewModel = (TViewModel)CreateInstance(typeof(TViewModel));
                viewModelDisposer.Set(viewModel);
                viewControlAsView.DataContext = viewModel;
              }
            }
          } else if(viewControlAsView != null) { // Caller has provided a view model
            viewControlAsView.DataContext = viewModel;
          }

          // If an auto binder was provided, automatically bind the view to the view model
          if(this.autoBinder != null) {
            this.autoBinder.Bind(viewControl, viewModel);
          }

          viewModelDisposer.Dismiss(); // Everything went well, we keep the view model
        }

        viewDisposer.Dismiss(); // Everything went well, we keep the view

      }

      return viewControl;
    }

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
    public TViewModel CreateViewModel<TViewModel>() where TViewModel : class {
      return (TViewModel)CreateInstance(typeof(TViewModel));
    }

    /// <summary>Locates the view that will be used to a view model</summary>
    /// <param name="viewModelType">
    ///   Type of view model for which the view will be located
    /// </param>
    /// <returns>The type of view that should be used for the specified view model</returns>
    protected virtual Type LocateViewForViewModel(Type viewModelType) {
      Type viewType;
      if(!this.viewTypesForViewModels.TryGetValue(viewModelType, out viewType)) {
        string viewName = viewModelType.Name;
        if(viewName.EndsWith("ViewModel")) {
          viewName = viewName.Substring(0, viewName.Length - 9);
        }

        Type[] exportedTypes = viewModelType.Assembly.GetExportedTypes();
        Type[] namespaceTypes = filterTypesByNamespace(exportedTypes, viewModelType.Namespace);

        // First, search the own namespace (because if two identical view models exist in
        // different namespaces, the one in the same namespace is most likely the desired one)
        viewType = findBestMatch(
          namespaceTypes,
          viewName + "View",
          viewName + "Page",
          viewName + "Form",
          viewName + "Window",
          viewName + "Dialog",
          viewName + "Control"
        );

        // If the view model doesn't exist in the same namespace, expand the search to
        // the entire assembly the view is in.
        if(viewType == null) {
          viewType = findBestMatch(
            exportedTypes,
            viewName + "View",
            viewName + "Page",
            viewName + "Form",
            viewName + "Window",
            viewName + "Dialog",
            viewName + "Control"
          );
        }

        // Still no view found? We give up!
        if(viewType == null) {
          throw new InvalidOperationException(
            string.Format("Could not locate view for view model '{0}'", viewModelType.Name)
          );
        }

        this.viewTypesForViewModels.TryAdd(viewModelType, viewType);
      }

      return viewType;
    }

    /// <summary>Creates an instance of the specified type</summary>
    /// <param name="type">Type an instance will be created of</param>
    /// <returns>The created instance</returns>
    /// <remarks>
    ///   Use this to wire up your dependency injection container. By default,
    ///   the Activator class will be used to create instances which only works
    ///   if all of your view models are concrete classes.
    /// </remarks>
    protected virtual object CreateInstance(Type type) {
      return Activator.CreateInstance(type);
    }

    /// <summary>Called when one of the application's root windows is closed</summary>
    /// <param name="sender">Window that has been closed</param>
    /// <param name="arguments">Not used</param>
    private void rootWindowClosed(object sender, EventArgs arguments) {
      Form closedWindow = (Form)sender;
      closedWindow.Closed -= this.rootWindowClosedDelegate;
      closedWindow.Activated -= this.rootWindowActivatedDelegate;

      // If the view model was created just for this view or if the user asked us
      // to dispose of his view model, do so now.
      if(shouldDisposeViewModelOnClose(closedWindow)) {
        IView windowAsView = closedWindow as IView;
        if(windowAsView != null) {
          object viewModelAsObject = windowAsView.DataContext;
          windowAsView.DataContext = null;
          disposeIfDisposable(viewModelAsObject);
        }
      }

      lock(this) {
        ActiveWindow = null;
      }
    }

    /// <summary>Called when one of the application's root windows is activated</summary>
    /// <param name="sender">Window that has been put in the foreground</param>
    /// <param name="arguments">Not used</param>
    private void rootWindowActivated(object sender, EventArgs arguments) {
      lock(this) {
        ActiveWindow = (Form)sender;
      }
    }

    /// <summary>Tries to find the best match for a named type in a list of types</summary>
    /// <param name="types">List of types the search will take place in</param>
    /// <param name="typeNames">
    ///   The candidates the method will look for, starting with the best match
    /// </param>
    /// <returns>The best match in the list of types, if any match was found</returns>
    private static Type findBestMatch(Type[] types, params string[] typeNames) {
      int bestMatchFound = typeNames.Length;

      Type type = null;
      for(int index = 0; index < types.Length; ++index) {
        for(int nameIndex = 0; nameIndex < bestMatchFound; ++nameIndex) {
          if(types[index].Name == typeNames[nameIndex]) {
            bestMatchFound = nameIndex;
            type = types[index];

            if(bestMatchFound == 0) { // There can be no better match
              return type;
            }

            break;
          }
        }
      }

      return type;
    }

    /// <summary>Disposes the specified object if it implements IDisposable</summary>
    /// <typeparam name="T">Type of object that will disposed if possible</typeparam>
    /// <param name="instance">Object that the method will attempt to dispose</param>
    private static void disposeIfDisposable<T>(T instance) where T : class {
      var disposable = instance as IDisposable;
      if(disposable != null) {
        disposable.Dispose();
      }
    }

    /// <summary>Determines if the view owns the view model</summary>
    /// <param name="view">View that will be checked for ownership</param>
    /// <returns>True if the view owns the view model</returns>
    private static bool shouldDisposeViewModelOnClose(Control view) {
      string tagAsString = view.Tag as string;
      if(tagAsString != null) {
        return tagAsString.Contains("DisposeViewModelOnClose");
      } else {
        return false;
      }
    }

    /// <summary>Filters a list of types to contain only those in a specific namespace</summary>
    /// <param name="exportedTypes">List of exported types that will be filtered</param>
    /// <param name="filteredNamespace">
    ///   Namespace the types in the filtered list will be in
    /// </param>
    /// <returns>A subset of the specified types that are in the provided namespace</returns>
    private static Type[] filterTypesByNamespace(Type[] exportedTypes, string filteredNamespace) {
      var filteredTypes = new List<Type>(exportedTypes.Length / 2);
      for(int index = 0; index < exportedTypes.Length; ++index) {
        Type exportedType = exportedTypes[index];
        if(exportedType.Namespace == filteredNamespace) {
          filteredTypes.Add(exportedType);
        }
      }

      return filteredTypes.ToArray();
    }

    /// <summary>The application's currently active root window</summary>
    private Form activeWindow;
    /// <summary>Invoked when a root window is put in the foreground</summary>
    private EventHandler rootWindowActivatedDelegate;
    /// <summary>Invoked when a root window has been closed</summary>
    private EventHandler rootWindowClosedDelegate;
    /// <summary>View model binder that will be used on all created views</summary>
    private IAutoBinder autoBinder;
    /// <summary>Caches the view types to use for a view model</summary>
    private ConcurrentDictionary<Type, Type> viewTypesForViewModels;

  }

} // namespace Nuclex.Windows.Forms
