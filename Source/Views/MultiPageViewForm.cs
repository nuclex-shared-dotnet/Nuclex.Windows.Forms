using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Nuclex.Support;
using Nuclex.Windows.Forms.ViewModels;

namespace Nuclex.Windows.Forms.Views {

  /// <summary>Special view form that can display different child views</summary>
  public class MultiPageViewForm : ViewForm {

    #region struct RedrawLockScope

    /// <summary>Prevents controls from redrawing themselves for a while</summary>
    private struct RedrawLockScope : IDisposable {

      /// <summary>Window message that enables or disables control redraw</summary>
      private const int WM_SETREDRAW = 11;

      /// <summary>Sends a window message to the specified window</summary>
      /// <param name="windowHandle">Window a message will be sent to</param>
      /// <param name="messageId">ID of the message that will be sent</param>
      /// <param name="firstArgument">First argument to the window procedure</param>
      /// <param name="secondArgument">Second argument to the window procedure</param>
      /// <returns>The return value of the window procedure</returns>
      [DllImport("user32")]
      public static extern int SendMessage(
        IntPtr windowHandle, int messageId, bool firstArgument, int secondArgument
      );

      /// <summary>Stops redrawing the specified control</summary>
      /// <param name="control">Control to stop redrawing</param>
      public RedrawLockScope(Control control) {
        if(Environment.OSVersion.Platform == PlatformID.Win32NT) {
          SendMessage(control.Handle, WM_SETREDRAW, false, 0);
          this.control = control;
        } else {
          this.control = null;
        }
      }

      /// <summary>Enables redrawing again when the lock scope is disposed</summary>
      public void Dispose() {
        if(this.control != null) {
          SendMessage(this.control.Handle, WM_SETREDRAW, true, 0);
          this.control.Invalidate(true);
        }
      }

      /// <summary>Control that has been stopped from redrawing itself</summary>
      private Control control;

    }

    #endregion // struct RedrawLockScope

    /// <summary>Initializes a new multi page view window</summary>
    /// <param name="windowManager">
    ///   Window manager that is used to set up the child views
    /// </param>
    /// <param name="cachePageViews">Whether page views should be kept alive and reused</param>
    public MultiPageViewForm(IWindowManager windowManager, bool cachePageViews = false) {
      this.windowManager = windowManager;
      this.createViewMethod = typeof(IWindowManager).GetMethod(nameof(IWindowManager.CreateView));

      if(cachePageViews) {
        this.cachedViews = new Dictionary<Type, Control>();
      }
    }

    /// <summary>Called when the control is being disposed</summary>
    /// <param name="calledExplicitly">
    ///   Whether the call was made by user code (vs. the garbage collector)
    /// </param>
    protected override void Dispose(bool calledExplicitly) {
      if(calledExplicitly) {

        // Disable the active view, if any
        if(this.activePageView != null) {
          if(this.childViewContainer != null) {
            this.childViewContainer.Controls.Remove(this.activePageView);
          }
        }

        // If caching is disabled, dispose of the active child view, if any
        if(this.cachedViews == null) {
          if(this.activePageView != null) {
            disposeIfSupported(this.activePageView);
            this.activePageView = null;
          }
        } else { // Caching is enabled, dispose of any cached child views
          foreach(Control childView in this.cachedViews.Values) {
            disposeIfSupported(childView);
          }
          this.cachedViews.Clear();
          this.cachedViews = null;
          this.activePageView = null;
        }
      }

      base.Dispose(calledExplicitly);
    }

    /// <summary>Discovers the container control used to host the child views</summary>
    /// <returns>The container control is which the child views will be hosted</returns>
    /// <remarks>
    ///   This is supposed to be overriden by the user, simply returning the container
    ///   control that should host the page views. If it isn't, however, we use some
    ///   heuristics to figure out the most likely candidate: it should be a container,
    ///   and it should cover most of the window's client area.
    /// </remarks>
    protected virtual Control IdentifyPageContainer() {
      Size halfWindowSize = Size;
      halfWindowSize.Width /= 2;
      halfWindowSize.Height /= 2;

      // First container control we found -- if we find no likely candidate,
      // we simply use the first
      Control firstContainer = null;

      // Check all top-level controls in the window. If there's a container that
      // covers most of the window, it's our best bet
      int controlCount = Controls.Count;
      for(int index = 0; index < controlCount; ++index) {
        Control control = Controls[index];

        // Only check container controls
        if((control is ContainerControl) || (control is Panel)) {
          if(firstContainer == null) {
            firstContainer = control;
          }

          // If this control covers most of the view, it's our candidate!
          Size controlSize = control.Size;
          bool goodCandidate = (
            (controlSize.Width > halfWindowSize.Width) &&
            (controlSize.Height > halfWindowSize.Height)
          );
          if(goodCandidate) {
            return control;
          }
        }
      }

      // If no candidate was found, return the first container control we encountered
      // or create a new UserControl as the container if nothing was found at all.
      if(firstContainer == null) {
        firstContainer = new Panel();
        Controls.Add(firstContainer);
        firstContainer.Dock = DockStyle.Fill;
      }

      return firstContainer;
    }

    /// <summary>Called when the window's data context is changed</summary>
    /// <param name="sender">Window whose data context was changed</param>
    /// <param name="oldDataContext">Data context that was previously used</param>
    /// <param name="newDataContext">Data context that will be used from now on</param>
    protected override void OnDataContextChanged(
      object sender, object oldDataContext, object newDataContext
    ) {

      // Kill the currently active view if there was an old view model.
      if(oldDataContext != null) {
        disableActivePageView();
      }

      base.OnDataContextChanged(sender, oldDataContext, newDataContext);

      // If a valid view model was assigned, create a new view its active page view model
      if(newDataContext != null) {
        var dataContextAsMultiPageViewModel = newDataContext as IMultiPageViewModel;
        if(dataContextAsMultiPageViewModel != null) {
          activatePageView(dataContextAsMultiPageViewModel.GetActivePageViewModel());
        }
      }

    }

    /// <summary>Called when a property of the view model is changed</summary>
    /// <param name="sender">View model in which a property was changed</param>
    /// <param name="arguments">Contains the name of the property that has changed</param>
    protected override void OnViewModelPropertyChanged(
      object sender, PropertyChangedEventArgs arguments
    ) {
      base.OnViewModelPropertyChanged(sender, arguments);

      if(arguments.AreAffecting(nameof(MultiPageViewModel<object>.ActivePage))) {
        var viewModelAsMultiPageviewModel = DataContext as IMultiPageViewModel;
        if(viewModelAsMultiPageviewModel != null) {
          if(InvokeRequired) {
            Invoke(
              new Action<object>(activatePageView),
              viewModelAsMultiPageviewModel.GetActivePageViewModel()
            );
          } else {
            activatePageView(viewModelAsMultiPageviewModel.GetActivePageViewModel());
          }
        }
      }
    }

    /// <summary>Currently active page view control</summary>
    protected Control ActivePageView {
      get { return this.activePageView; }
    }

    /// <summary>The view model running the currently active page</summary>
    protected object ActivePageViewModel {
      get {
        var activePageViewAsView = this.activePageView as IView;
        if(activePageViewAsView == null) {
          return null;
        } else {
          return activePageViewAsView.DataContext;
        }
      }
    }

    /// <summary>Activates the page view for the specified page view model</summary>
    /// <param name="pageViewModel">
    ///   Page view model for which the page view will be activated
    /// </param>
    private void activatePageView(object pageViewModel) {
      object activePageViewModel = null;
      {
        var activePageViewAsView = this.activePageView as IView;
        if(activePageViewAsView != null) {
          activePageViewModel = activePageViewAsView.DataContext;
        }
      }

      // Try from the cheapest to the most expensive way to get to our goal,
      // an activated view suiting the specified view model.

      // If we already have the target view model selected, do nothing
      if(activePageViewModel == pageViewModel) {
        return;
      }

      // If the page view model for the old and the new page are of the same
      // type, we can reuse the currently active page view
      if((activePageViewModel != null) && (pageViewModel != null)) {
        if(pageViewModel.GetType() == this.activePageView.GetType()) {
          var activePageViewAsView = this.activePageView as IView;
          if(activePageViewAsView != null) {
            activePageViewAsView.DataContext = pageViewModel;
          }

          return;
        }
      }

      // Worst, but usual, case: the new page view model might require
      // a different view. Create or look up the new view and put it in the container
      {
        if(pageViewModel == null) {
          disableActivePageView();
        } else {
          Control pageViewContainer = getPageViewContainer();
          using(new RedrawLockScope(pageViewContainer)) {
            disableActivePageView();

            this.activePageView = getOrCreatePageView(pageViewModel);
            pageViewContainer.Controls.Add(this.activePageView);
            this.activePageView.Dock = DockStyle.Fill;
          }
        }
      }
    }

    /// <summary>Gets the cached child view or creates a new one if not cached</summary>
    /// <param name="viewModel">View model for which a child view will be returned</param>
    /// <returns>A child view suitable for the specified view model</returns>
    private Control getOrCreatePageView(object viewModel) {
      Type viewModelType = viewModel.GetType();

      Control view;

      // If caching is enabled, check if we have a cached view
      if(this.cachedViews != null) {
        if(this.cachedViews.TryGetValue(viewModelType, out view)) {
          return view;
        }
      }

      // Otherwise, call the window manager's CreateView() method
      MethodInfo specializedCreateViewMethod = (
        this.createViewMethod.MakeGenericMethod(viewModelType)
      );
      view = (Control)specializedCreateViewMethod.Invoke(
        this.windowManager, new object[1] { viewModel }
      );

      // If caching is enabled, register the view in the cache
      if(this.cachedViews != null) {
        this.cachedViews.Add(viewModelType, view);
      }

      return view;
    }

    /// <summary>Disables the currently active page view control</summary>
    private void disableActivePageView() {
      if(this.activePageView != null) {
        Control container = getPageViewContainer();
        container.Controls.Remove(this.activePageView);

        // If we don't reuse views, kill it now
        if(this.cachedViews == null) {
          disposeIfSupported(this.activePageView);
          this.activePageView = null;
        } else {
          var activePageViewAsView = this.activePageView as IView;
          if(activePageViewAsView != null) {
            activePageViewAsView.DataContext = null;
          }
        }
      }
    }

    /// <summary>Fetches the container that holds the child views</summary>
    /// <returns>The container for the child views</returns>
    private Control getPageViewContainer() {
      if(this.childViewContainer == null) {
        this.childViewContainer = IdentifyPageContainer();
      }

      return this.childViewContainer;
    }

    /// <summary>Disposes the specified object if it is disposable</summary>
    /// <param name="potentiallyDisposable">Object that will be disposed if supported</param>
    private static void disposeIfSupported(object potentiallyDisposable) {
      var disposable = potentiallyDisposable as IDisposable;
      if(disposable != null) {
        disposable.Dispose();
      }
    }

    /// <summary>Window manager through which the child views are created</summary>
    private IWindowManager windowManager;
    /// <summary>Reflection info for the createView() method of the window manager</summary>
    private MethodInfo createViewMethod;

    /// <summary>Container in which the child views will be hosted</summary>
    private Control childViewContainer;
    /// <summary>Cached views that will be reused when the view model activates them</summary>
    private Dictionary<Type, Control> cachedViews;
    /// <summary>The currently active child view</summary>
    private Control activePageView;

  }

} // namespace Nuclex.Windows.Forms.Views
