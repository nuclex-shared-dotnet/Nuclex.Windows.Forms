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

namespace Nuclex.Windows.Forms {

  /// <summary>
  ///   Proxy stand-in to delay checking for the main window until it has been created
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The issue: when the view model for the main window is created, the main window
  ///     may only exist as a .NET object, without the underlying operating system window
  ///     (done in <see cref="System.Windows.Forms.Control.CreateControl" /> checkable via
  ///     <see cref="System.Windows.Forms.Control.IsHandleCreated" />). Not only will things
  ///     like <see cref="System.Windows.Forms.Control.Invoke(Delegate)" /> fail, we can't
  ///     even locate the main window at that stage.
  ///   </para>
  ///   <para>
  ///     Thus, if the main window cannot be found at the time a view model is created,
  ///     this late-checking synchronizer will jump into its place and re-check for
  ///     the main window only when something needs to be executed in the UI thread.
  ///   </para>
  /// </remarks>
  class LateCheckedSynchronizer : ISynchronizeInvoke {

    /// <summary>Initializes a new late-checked main window synchronizer</summary>
    /// <param name="uiContextFoundCallback"></param>
    public LateCheckedSynchronizer(Action<ISynchronizeInvoke> uiContextFoundCallback) {
      this.uiContextFoundCallback = uiContextFoundCallback;
    }

    /// <summary>Finds the application's main window</summary>
    /// <returns>Main window of the application or null if none has been created</returns>
    /// <remarks>
    ///   The application's main window, if it has been created yet
    /// </remarks>
    public static Form GetMainWindow() {
      IntPtr mainWindowHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

      // We can get two things: a list of all open windows and the handle of
      // the window that the process has registered as main window. Use the latter
      // to pick the correct window from the former.
      FormCollection openForms = Application.OpenForms;
      int openFormCount = openForms.Count;
      for(int index = 0; index < openFormCount; ++index) {
        Form form = openForms[index];

        IntPtr handle;
        if(form.InvokeRequired) {
          handle = (IntPtr)form.Invoke(new Func<Form, IntPtr>(getWindowHandle), form);
        } else {
          handle = getWindowHandle(form);
        }
        if(handle != IntPtr.Zero) {
          if(handle == mainWindowHandle) {
            return form;
          }
        }
      }

      // No matching main window found: use the first one in good faith or fail.
      if(openFormCount > 0) {
        return openForms[0];
      } else {
        return null;
      }
    }

    /// <summary>Checks whether the calling thread needs to use Invoke()</summary>
    public bool InvokeRequired {
      get { return getMainWindowOrFail().InvokeRequired; }
    }

    /// <summary>Schedules a method to be run by the main UI thread</summary>
    /// <param name="method">Method that will be scheduled to run</param>
    /// <param name="args">Arguments that will be passed to the method</param>
    /// <returns>An asynchronous result handle that can be used to track the call</returns>
    public IAsyncResult BeginInvoke(Delegate method, object[] args) {
      return getMainWindowOrFail().BeginInvoke(method, args);
    }

    /// <summary>Waits for a call scheduled on the main UI thread to complete</summary>
    /// <param name="result">Asynchronous result handle returned by BeginInvoke()</param>
    /// <returns>The value returned by the method ran in the main UI thread</returns>
    public object EndInvoke(IAsyncResult result) {
      return getMainWindowOrFail().EndInvoke(result);
    }

    /// <summary>Executes a method on the main UI thread and waits for it to complete</summary>
    /// <param name="method">Method that will be run by the main UI thread</param>
    /// <param name="arguments">Arguments that will be passed to the method</param>
    /// <returns>The value returned by the method</returns>
    public object Invoke(Delegate method, object[] arguments) {
      return getMainWindowOrFail().Invoke(method, arguments);
    }

    /// <summary>Retrieves the application's current main window</summary>
    /// <returns>The application's current main window</returns>
    /// <remarks>
    ///   If there is no main window, an exception will be thrown
    /// </remarks>
    private Form getMainWindowOrFail() {
      Form mainWindow = GetMainWindow();
      if(mainWindow == null) {
        throw new InvalidOperationException(
          "Could not schedule work for the UI thread because no WinForms UI main window " +
          "was found. Create a main window first or specify the UI synchronization context " +
          "explicitly to the view model."
        );
      }

      if(this.uiContextFoundCallback != null) {
        this.uiContextFoundCallback(mainWindow);
        this.uiContextFoundCallback = null;
      }

      return mainWindow;
    }

    /// <summary>Returns a Form's window handle without forcing its creation</summary>
    /// <param name="form">Form whose window handle will be returned</param>
    /// <returns>The form's window handle of IntPtr.Zero if it has none</returns>
    private static IntPtr getWindowHandle(Form form) {
      if(form.IsHandleCreated) {
        return form.Handle;
      } else {
        return IntPtr.Zero;
      }
    }

    /// <summary>Called when the late-checked synchronizer finds the main window</summary>
    private Action<ISynchronizeInvoke> uiContextFoundCallback;

  }

} // namespace Nuclex.Windows.Forms
