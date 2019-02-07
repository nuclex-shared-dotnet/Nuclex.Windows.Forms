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
