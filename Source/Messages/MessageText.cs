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

namespace Nuclex.Windows.Forms.Messages {

  /// <summary>Text that will be displayed in a message box</summary>
  public class MessageText {

    /// <summary>Initializs a new message text</summary>
    public MessageText() { }

    /// <summary>Initializes a new message text by copying another instance</summary>
    /// <param name="other">Instance that will be copied</param>
    public MessageText(MessageText other) {
      Caption = other.Caption;
      Message = other.Message;
      Details = other.Details;
      ExpandedDetails = other.ExpandedDetails;
    }

    /// <summary>The caption used when the is displayed in a message box</summary>
    public string Caption { get; set; }
    /// <summary>Main message being displayed to the user</summary>
    public string Message { get; set; }
    /// <summary>Message details shown below the main message</summary>
    public string Details { get; set; }
    /// <summary>
    ///   Additional informations the user can display by expanding
    ///   the message dialog. Can be null, in which case the message dialog
    ///   will not be expandable.
    /// </summary>
    public string ExpandedDetails { get; set; }

  }

} // namespace Nuclex.Windows.Forms.Messages
