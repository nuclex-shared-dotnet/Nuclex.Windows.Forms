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
using System.ComponentModel;
using System.Windows.Forms;

using Nuclex.Support;

namespace Nuclex.Windows.Forms.Views {

  /// <summary>
  ///   Base class for MVVM windows that act as views connected to a view model
  /// </summary>
  public class ViewForm : Form, IView {

    /// <summary>Initializes a new view control</summary>
    public ViewForm() {
      this.onViewModelPropertyChangedDelegate = OnViewModelPropertyChanged;
    }

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
      OnViewModelPropertyChanged(this.dataContext, PropertyChangedEventArgsHelper.Wildcard);
    }

    /// <summary>Called when a property of the view model is changed</summary>
    /// <param name="sender">View model in which a property was changed</param>
    /// <param name="arguments">Contains the name of the property that has changed</param>
    protected virtual void OnViewModelPropertyChanged(
      object sender, PropertyChangedEventArgs arguments
    ) { }

    /// <summary>Provides the data binding target for the view</summary>
    public object DataContext {
      get { return this.dataContext; }
      set {
        if(value != this.dataContext) {
          this.dataContext = value;
        }
      }
    }

    /// <summary>Active data binding target, can be null</summary>
    private object dataContext;
    /// <summary>Delegate for the OnViewModelPropertyChanged() method</summary>
    private PropertyChangedEventHandler onViewModelPropertyChangedDelegate;

  }

} // namespace Nuclex.Windows.Forms.Views
