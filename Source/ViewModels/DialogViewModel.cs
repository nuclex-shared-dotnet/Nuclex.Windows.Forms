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

using Nuclex.Support;

namespace Nuclex.Windows.Forms.ViewModels {

  /// <summary>Base class for the view model of dialogs (typically modal ones)</summary>
  public class DialogViewModel : Observable {

    /// <summary>Indicates that the view should close with a positive result</summary>
    /// <remarks>
    ///   This event typically corresponds to the 'Ok' button in a dialog.
    /// </remarks>
    public event EventHandler Confirmed;

    /// <summary>Indicates that the view should close with a negative result</summary>
    /// <remarks>
    ///   This event typically corresponds to the 'Cancel' button in a dialog.
    /// </remarks>
    public event EventHandler Canceled;

    /// <summary>Indicates that the view should close</summary>
    /// <remarks>
    ///   This closes the view with a neutral result, used when the view doesn't follow
    ///   an ok/cancel scheme or the result is transmitted in some other way.
    /// </remarks>
    public event EventHandler Submitted;

    /// <summary>
    ///   Indicates that the dialog should be closed with a positive outcome
    /// </summary>
    public virtual void Confirm() {
      if(Confirmed != null) {
        Confirmed(this, EventArgs.Empty);
      }
    }

    /// <summary>
    ///   Indicates that the dialog should be closed with a negative outcome
    /// </summary>
    public virtual void Cancel() {
      if(Canceled != null) {
        Canceled(this, EventArgs.Empty);
      }
    }

    /// <summary>Indicates that the dialog should be closed</summary>
    public virtual void Submit() {
      if(Submitted != null) {
        Submitted(this, EventArgs.Empty);
      }
    }

  }

} // namespace Nuclex.Windows.Forms.ViewModels
