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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;

#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Nuclex.Support;
using Nuclex.Windows.Forms.AutoBinding;
using Nuclex.Windows.Forms.Views;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Nuclex.Windows.Forms {

  /// <summary>Manages an application's windows and views</summary>
#if NET6_0_OR_GREATER
  [SupportedOSPlatform("windows")]
#endif
  public class WindowManager : Observable, IWindowManager {

    #region class WindowManagerScope

    /// <summary>Global scope that uses the WindowManager's CreateInstance()</summary>
    public class WindowManagerScope : IWindowScope {

      /// <summary>Initializes a new global window scope</summary>
      /// <param name="windowManager">
      ///   Window manager whose CreateInstance() method will be used
      /// </param>
      public WindowManagerScope(WindowManager windowManager) {
        this.windowManager = windowManager;
      }

      /// <summary>Creates an instance of the specified type in the scope</summary>
      /// <param name="type">Type an instance will be created of</param>
      /// <returns>The created instance</returns>
      object IWindowScope.CreateInstance(Type type) {
        return this.windowManager.CreateInstance(type);
      }

      /// <summary>Does nothing because this is the global fallback scope</summary>
      void IDisposable.Dispose() {}

      /// <summary>WindowManager whose CreateInstance() method will be wrapped</summary>
      private WindowManager windowManager;

    }

    #endregion // class WindwoManagerScope

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
      this.windowManagerAsScope = new WindowManagerScope(this);
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
      // dispose his view model, prepare the window so that it will dispose its
      // view model when the window is done.
      if((viewModel == null) || disposeOnClose) {
        setupViewModelDisposal(window);
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
        // dispose his view model, prepare the window so that it will dispose its
        // view model when the window is done.
        if((viewModel == null) || disposeOnClose) {
          setupViewModelDisposal(window);
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
      Control viewControl;
      {
        Type viewType = LocateViewForViewModel(typeof(TViewModel));

        IWindowScope scope = CreateWindowScope();
        using(var scopeDisposer = new CancellableDisposer(scope)) {
          viewControl = (Control)scope.CreateInstance(viewType);
          using(var viewDisposer = new CancellableDisposer(viewControl)) {

            // Create a view model if none was provided, and in either case assign
            // the view model to the view (provided it implements IView).
            using(var viewModelDisposer = new CancellableDisposer()) {
              IView viewControlAsView = viewControl as IView;
              if(viewModel == null) { // No view model provided, create one
                if(viewControlAsView == null) { // View doesn't implement IView
                  viewModel = (TViewModel)scope.CreateInstance(typeof(TViewModel));
                  viewModelDisposer.Set(viewModel);
                } else if(viewControlAsView.DataContext == null) { // View has no view model
                  viewModel = (TViewModel)scope.CreateInstance(typeof(TViewModel));
                  viewModelDisposer.Set(viewModel);
                  viewControlAsView.DataContext = viewModel;
                } else { // There's an existing view model
                  viewModel = viewControlAsView.DataContext as TViewModel;
                  if(viewModel == null) { // The existing view model is another type
                    viewModel = (TViewModel)scope.CreateInstance(typeof(TViewModel));
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

          scopeDisposer.Dismiss(); // Everything went well, we keep the scope
        }

        setupScopeDisposal(viewControl, scope);
      } // beauty scope

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

    /// <summary>Creates an instance of the specified type in a new scope</summary>
    /// <param name="type">Type an instance will be created of</param>
    /// <returns>The created instance and the scope in which it lives</returns>
    /// <remarks>
    ///   This is identical to <see cref="CreateInstance" /> but, if used together
    ///   with a dependency injector, should also create a service scope. This way,
    ///   an implicit service scope will cover the lifetime of a view model and
    ///   any non-singleton services will use new instances, avoiding, for example,
    ///   that multiple dialogs access the same database connection simultaneously.
    /// </remarks>
    protected virtual IWindowScope CreateWindowScope() {
      return this.windowManagerAsScope;
    }

    /// <summary>Called when one of the application's root windows is closed</summary>
    /// <param name="sender">Window that has been closed</param>
    /// <param name="arguments">Not used</param>
    private void rootWindowClosed(object sender, EventArgs arguments) {
      Form closedWindow = (Form)sender;
      closedWindow.Closed -= this.rootWindowClosedDelegate;
      closedWindow.Activated -= this.rootWindowActivatedDelegate;

      lock(this) {
        ActiveWindow = null;
      }

      // Application.Run() already does this and it's the user's responsibility anyways
      //disposeIfDisposable(closedWindow);
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

    /// <summary>Attaches a view model disposer to a control</summary>
    /// <param name="control">
    ///   Control whose view model will be disposed when it is itself disposed
    /// </param>
    private static void setupViewModelDisposal(Control control) {
      IView controlAsView = control as IView;
      if(controlAsView != null) {
        IDisposable disposableViewModel = controlAsView.DataContext as IDisposable;
        if(disposableViewModel != null) {
          control.Disposed += delegate(object sender, EventArgs arguments) {
            disposableViewModel.Dispose();
            controlAsView.DataContext = null;
          };
        }
      }

      //window.Tag = "DisposeViewModelOnClose"; // TODO: Wrap SetProp() instead?
      //window.SetValue(DisposeViewModelOnCloseProperty, true);
    }

    /// <summary>Attaches a scope disposer to a control</summary>
    /// <param name="control">
    ///   Control that will dispose a scope when it is itself disposed
    /// </param>
    private void setupScopeDisposal(Control control, IWindowScope scope) {
      if(!ReferenceEquals(scope, this.windowManagerAsScope)) {
        IDisposable disposableScope = scope as IDisposable;
        if(disposableScope != null) {
          control.Disposed += delegate(object sender, EventArgs arguments) {
            disposableScope.Dispose();
          };
        }
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
    /// <summary>Scope that uses the WindowManager's global CreateInstance() method</summary>
    private WindowManagerScope windowManagerAsScope;
    /// <summary>View model binder that will be used on all created views</summary>
    private IAutoBinder autoBinder;
    /// <summary>Caches the view types to use for a view model</summary>
    private ConcurrentDictionary<Type, Type> viewTypesForViewModels;

  }

} // namespace Nuclex.Windows.Forms
