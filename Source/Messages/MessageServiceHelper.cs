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

  /// <summary>Contains helper methods for the message service</summary>
  public static class MessageServiceHelper {

    /// <summary>Asks the user a question that can be answered with yes or no</summary>
    /// <param name="messageService">
    ///   Message service that will be used to display the question
    /// </param>
    /// <param name="text">Text that will be shown on the message box</param>
    /// <returns>The button the user has clicked on</returns>
    public static DialogResult AskYesNo(
      this IMessageService messageService, MessageText text
    ) {
      return messageService.ShowQuestion(
        MessageBoxIcon.Question, text, MessageBoxButtons.YesNo
      );
    }

    /// <summary>Asks the user a question that can be answered with ok or cancel</summary>
    /// <param name="messageService">
    ///   Message service that will be used to display the question
    /// </param>
    /// <param name="text">Text that will be shown on the message box</param>
    /// <returns>The button the user has clicked on</returns>
    public static DialogResult AskOkCancel(
      this IMessageService messageService, MessageText text
    ) {
      return messageService.ShowQuestion(
        MessageBoxIcon.Question, text, MessageBoxButtons.OKCancel
      );
    }

    /// <summary>
    ///   Asks the user a question that can be answered with yes, no or cancel
    /// </summary>
    /// <param name="messageService">
    ///   Message service that will be used to display the question
    /// </param>
    /// <param name="text">Text that will be shown on the message box</param>
    /// <returns>The button the user has clicked on</returns>
    public static DialogResult AskYesNoCancel(
      this IMessageService messageService, MessageText text
    ) {
      return messageService.ShowQuestion(
        MessageBoxIcon.Question, text, MessageBoxButtons.YesNoCancel
      );
    }

    /// <summary>Displays an informative message</summary>
    /// <param name="messageService">
    ///   Message service that will be used to display the warning
    /// </param>
    /// <param name="text">Text to be displayed on the warning message</param>
    public static void Inform(
      this IMessageService messageService, MessageText text
    ) {
      messageService.ShowNotification(MessageBoxIcon.Information, text);
    }

    /// <summary>Displays a warning</summary>
    /// <param name="messageService">
    ///   Message service that will be used to display the warning
    /// </param>
    /// <param name="text">Text to be displayed on the warning message</param>
    public static void Warn(
      this IMessageService messageService, MessageText text
    ) {
      messageService.ShowNotification(MessageBoxIcon.Warning, text);
    }

    /// <summary>Reports an error</summary>
    /// <param name="messageService">
    ///   Message service that will be used to display the warning
    /// </param>
    /// <param name="text">Text to be displayed on the warning message</param>
    public static void ReportError(
      this IMessageService messageService, MessageText text
    ) {
      messageService.ShowNotification(MessageBoxIcon.Error, text);
    }

  }

} // namespace Nuclex.Windows.Forms.Messages
