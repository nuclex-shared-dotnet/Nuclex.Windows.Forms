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
using System.ComponentModel;
using System.Windows.Forms;

#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Nuclex.Support;

namespace Nuclex.Windows.Forms.Views {

  /// <summary>
  ///   Base class for MVVM windows that act as views connected to a view model
  /// </summary>
#if NET6_0_OR_GREATER
  [SupportedOSPlatform("windows")]
#endif
  public class ViewForm : Form, IView {

    /// <summary>Initializes a new view control</summary>
    public ViewForm() {
      this.onViewModelPropertyChangedDelegate = OnViewModelPropertyChanged;
    }

#if !NET8_0_OR_GREATER
    /// <summary>Provides the data binding target for the view</summary>
    public object DataContext {
      get { return this.dataContext; }
      set {
        if(value != this.dataContext) {
          object oldDataContext = this.dataContext;
          this.dataContext = value;
          OnDataContextChanged(this, oldDataContext, value);
        }
      }
    }
#endif

    /// <summary>Called when the window's data context is changed</summary>
    /// <param name="sender">Window whose data context was changed</param>
    /// <param name="oldDataContext">Data context that was previously used</param>
    /// <param name="newDataContext">Data context that will be used from now on</param>
    protected virtual void OnDataContextChanged(
      object sender, object oldDataContext, object newDataContext
    ) {
      var oldViewModel = oldDataContext as INotifyPropertyChanged;
      if(oldViewModel != null) {
        oldViewModel.PropertyChanged -= this.onViewModelPropertyChangedDelegate;
      }

      var newViewModel = newDataContext as INotifyPropertyChanged;
      if(newViewModel != null) {
        newViewModel.PropertyChanged += this.onViewModelPropertyChangedDelegate;
        InvalidateAllViewModelProperties();
      }
    }

    /// <summary>Refreshes all properties from the view model</summary>
    protected void InvalidateAllViewModelProperties() {
      OnViewModelPropertyChanged(DataContext, PropertyChangedEventArgsHelper.Wildcard);
    }

    /// <summary>Called when a property of the view model is changed</summary>
    /// <param name="sender">View model in which a property was changed</param>
    /// <param name="arguments">Contains the name of the property that has changed</param>
    protected virtual void OnViewModelPropertyChanged(
      object sender, PropertyChangedEventArgs arguments
    ) { }

#if !NET8_0_OR_GREATER
    /// <summary>Active data binding target, can be null</summary>
    private object dataContext;
#endif
    /// <summary>Delegate for the OnViewModelPropertyChanged() method</summary>
    private PropertyChangedEventHandler onViewModelPropertyChangedDelegate;

  }

} // namespace Nuclex.Windows.Forms.Views
