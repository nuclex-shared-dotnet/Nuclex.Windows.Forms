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
using System.Windows.Forms;

#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace Nuclex.Windows.Forms.Messages {

  /// <summary>Uses task dialogs to display message boxes</summary>
#if NET6_0_OR_GREATER
  [SupportedOSPlatform("windows")]
#endif
  public class StandardMessageBoxManager : IMessageService {

    #region class MessageScope

    /// <summary>Triggers the message displayed and acknowledged events</summary>
    private class MessageScope : IDisposable {

      /// <summary>
      ///   Initializes a new message scope, triggering the message displayed event
      /// </summary>
      /// <param name="self">Message service the scope belongs to</param>
      /// <param name="image">Image of the message being displayed</param>
      /// <param name="text">Text contained in the message being displayed</param>
      public MessageScope(
        StandardMessageBoxManager self, MessageBoxIcon image, MessageText text
      ) {
        EventHandler<MessageEventArgs> messageDisplayed = self.MessageDisplaying;
        if(messageDisplayed != null) {
          messageDisplayed(this, new MessageEventArgs(image, text));
        }

        this.self = self;
      }

      /// <summary>Triggers the message acknowledged event</summary>
      public void Dispose() {
        EventHandler messageAcknowledged = self.MessageAcknowledged;
        if(messageAcknowledged != null) {
          messageAcknowledged(this, EventArgs.Empty);
        }
      }

      /// <summary>Message service the scope belongs to</summary>
      private StandardMessageBoxManager self;

    }

    #endregion // class MessageScope

    /// <summary>Delegate for the standard message box show function</summary>
    /// <param name="owner">Window that will modally display the message box</param>
    /// <param name="text">Text that will be presented to the user</param>
    /// <param name="caption">Contents of the message box' title bar</param>
    /// <param name="buttons">Buttons available for the user to choose from</param>
    /// <param name="icon">Icon that will be displayed next to the text</param>
    /// <returns>The choice made by the user if multiple buttons were provided</returns>
    private delegate DialogResult ShowMessageBoxDelegate(
      IWin32Window owner,
      string text,
      string caption,
      MessageBoxButtons buttons,
      MessageBoxIcon icon
    );

    /// <summary>Triggered when a message is displayed to the user</summary>
    public event EventHandler<MessageEventArgs> MessageDisplaying;

    /// <summary>Triggered when the user has acknowledged the current message</summary>
    public event EventHandler MessageAcknowledged;

    /// <summary>Initializes a new task dialog message service</summary>
    public StandardMessageBoxManager() : this(NullActiveWindowTracker.Default) { }

    /// <summary>Initializes a new task dialog message service</summary>
    /// <param name="tracker">
    ///   Active window tracker used to obtain the parent window for message boxes
    /// </param>
    public StandardMessageBoxManager(IActiveWindowTracker tracker) {
      this.tracker = tracker;
      this.showMessageDelegate = new ShowMessageBoxDelegate(MessageBox.Show);
    }

    /// <summary>Asks the user a question that can be answered via several buttons</summary>
    /// <param name="image">Image that will be shown on the message box</param>
    /// <param name="text">Text that will be shown to the user</param>
    /// <param name="buttons">Buttons available for the user to click on</param>
    /// <returns>The button the user has clicked on</returns>
    public DialogResult ShowQuestion(
      MessageBoxIcon image, MessageText text, MessageBoxButtons buttons
    ) {
      using(var scope = new MessageScope(this, image, text)) {
        return showMessageBoxInActiveUiThread(
          text.Message,
          text.Caption,
          buttons,
          image
        );
      }
    }

    /// <summary>Displays a notification to the user</summary>
    /// <param name="image">Image that will be shown on the message bx</param>
    /// <param name="text">Text that will be shown to the user</param>
    public void ShowNotification(MessageBoxIcon image, MessageText text) {
      using(var scope = new MessageScope(this, image, text)) {
        showMessageBoxInActiveUiThread(
          text.Message,
          text.Caption,
          MessageBoxButtons.OK,
          image
        );
      }
    }

    /// <summary>Displays the message box in the active view's thread</summary>
    /// <param name="message">Text that will be presented to the user</param>
    /// <param name="caption">Contents of the message box' title bar</param>
    /// <param name="buttons">Buttons available for the user to choose from</param>
    /// <param name="image">Image that will be displayed next to the text</param>
    /// <returns></returns>
    private DialogResult showMessageBoxInActiveUiThread(
      string message,
      string caption,
      MessageBoxButtons buttons,
      MessageBoxIcon image
    ) {
      Form mainWindow = this.tracker.ActiveWindow;
      if(mainWindow != null) {
        return (DialogResult)mainWindow.Invoke(
          this.showMessageDelegate,
          (IWin32Window)mainWindow, message, caption, buttons, image
        );
      }

      // No window tracker or unknown main window -- just show the message box
      return MessageBox.Show(message, caption, buttons, image);
    }

    /// <summary>Provides the currently active top-level window</summary>
    private IActiveWindowTracker tracker;
    /// <summary>Delegate for the MessageBox.Show() method</summary>
    private ShowMessageBoxDelegate showMessageDelegate;

  }

} // namespace Nuclex.Windows.Forms.Messages
