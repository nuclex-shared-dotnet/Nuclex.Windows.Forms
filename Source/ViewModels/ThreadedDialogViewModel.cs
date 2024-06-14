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

namespace Nuclex.Windows.Forms.ViewModels {

  /// <summary>View model for a dialog that can execute tasks in a background thread</summary>
  public abstract class ThreadedDialogViewModel : ThreadedViewModel {

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
    ///		Indicates that the dialog should be closed with a positive outcome
    ///	</summary>
    public virtual void Confirm() {
      if(Confirmed != null) {
        Confirmed(this, EventArgs.Empty);
      }
    }

    /// <summary>
    ///		Indicates that the dialog should be closed with a negative outcome
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
