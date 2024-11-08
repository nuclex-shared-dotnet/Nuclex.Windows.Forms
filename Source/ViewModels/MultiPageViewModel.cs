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

using Nuclex.Support;

namespace Nuclex.Windows.Forms.ViewModels {

  /// <summary>Base class for view models that have multiple child view models</summary>
  /// <typeparam name="TPageEnumeration">Enum type by which pages can be indicated</typeparam>
  public abstract class MultiPageViewModel<TPageEnumeration> :
    Observable, IMultiPageViewModel, IDisposable { 

    /// <summary>Initializes a new multi-page view model</summary>
    /// <param name="windowManager">
    ///   Window manager the view model uses to create child views
    /// </param>
    /// <param name="cachePageViewModels">
    ///   Whether child view models will be kept alive and reused
    /// </param>
    public MultiPageViewModel(IWindowManager windowManager, bool cachePageViewModels = false) {
      this.windowManager = windowManager;
      if(cachePageViewModels) {
        this.cachedViewModels = new ConcurrentDictionary<TPageEnumeration, object>();
      }
    }

    /// <summary>Immediately releases all resources owned by the instance</summary>
    public virtual void Dispose() {
      if(this.cachedViewModels != null) {
        foreach(object cacheViewModel in this.cachedViewModels.Values) {
          disposeIfSupported(cacheViewModel);
        }
        this.activePageViewModel = null;

        this.cachedViewModels.Clear();
        this.cachedViewModels = null;
      } else if(this.activePageViewModel != null) {
        disposeIfSupported(this.activePageViewModel);
        this.activePageViewModel = null;
      }
    }

    /// <summary>Child page that is currently being displayed by the view model</summary>
    public TPageEnumeration ActivePage {
      get { return this.activePage; }
      set {
        if(!this.activePage.Equals(value)) {
          this.activePage = value;
          if(this.activePageViewModel != null) {
            if(this.cachedViewModels == null) {
              disposeIfSupported(this.activePageViewModel);
            }
            this.activePageViewModel = null;
          }
          OnPropertyChanged(nameof(ActivePage));
        }
      }
    }

    /// <summary>Retrieves (and, if needed, creates) the view model for the active page</summary>
    /// <returns>A view model for the active page on the multi-page view model</returns>
    public object GetActivePageViewModel() {
      if(this.cachedViewModels == null) {
        if(this.activePageViewModel == null) {
          this.activePageViewModel = CreateViewModelForPage(this.activePage);
        }
      } else if(this.activePageViewModel == null) {
        this.activePageViewModel = this.cachedViewModels.GetOrAdd(
          this.activePage,
          delegate(TPageEnumeration activePage) {
            return CreateViewModelForPage(this.activePage);
          }
        );
      }

      return this.activePageViewModel;
    }

    /// <summary>Windowmanager that can create view models and display other views</summary>
    protected IWindowManager WindowManager {
      get { return this.windowManager; }
    }

    /// <summary>Creates a view model for the specified page</summary>
    /// <param name="page">Page for which a view model will be created</param>
    /// <returns>The view model for the specified page</returns>
    protected abstract object CreateViewModelForPage(TPageEnumeration page);

    /// <summary>Disposes the specified object if it is disposable</summary>
    /// <param name="potentiallyDisposable">Object that will be disposed if supported</param>
    private static void disposeIfSupported(object potentiallyDisposable) {
      var disposable = potentiallyDisposable as IDisposable;
      if(disposable != null) {
        disposable.Dispose();
      }
    }

    /// <summary>Page that is currently active in the multi-page view model</summary>
    private TPageEnumeration activePage;
    /// <summary>Window manager that can be used to display other views</summary>
    private IWindowManager windowManager;

    /// <summary>View model for the active page</summary>
    private object activePageViewModel;
    /// <summary>Cached page view models, if caching is enabled</summary>
    private ConcurrentDictionary<TPageEnumeration, object> cachedViewModels;

  }

} // namespace Nuclex.Windows.Forms.ViewModels
