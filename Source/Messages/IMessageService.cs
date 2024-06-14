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

namespace Nuclex.Windows.Forms.Messages {

  /// <summary>Performs simple user interaction</summary>
  /// <remarks>
  ///   Methods provided by this service can be covered using plain old message boxes
  ///   and do not require special dialogs or calls to the task dialog API.
  /// </remarks>
  public interface IMessageService {

    /// <summary>Triggered when a message is about to be displayed to the user</summary>
    event EventHandler<MessageEventArgs> MessageDisplaying;

    /// <summary>Triggered when the user has acknowledged the current message</summary>
    event EventHandler MessageAcknowledged;

    /// <summary>Asks the user a question that can be answered via several buttons</summary>
    /// <param name="image">Image that will be shown on the message box</param>
    /// <param name="text">Text that will be shown to the user</param>
    /// <param name="buttons">Buttons available for the user to click on</param>
    /// <returns>The button the user has clicked on</returns>
    DialogResult ShowQuestion(
      MessageBoxIcon image, MessageText text, MessageBoxButtons buttons
    );

    /// <summary>Displays a notification to the user</summary>
    /// <param name="image">Image that will be shown on the message bx</param>
    /// <param name="text">Text that will be shown to the user</param>
    void ShowNotification(MessageBoxIcon image, MessageText text);

  }

} // namespace Nuclex.Windows.Forms.Messages
