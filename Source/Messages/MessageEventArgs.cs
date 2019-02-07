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

  /// <summary>Provides a displayed message and its severity to event subscribers</summary>
  public class MessageEventArgs : EventArgs {

    /// <summary>Initializes a new message box event argument container</summary>
    /// <param name="image">Image the message box will be displaying</param>
    /// <param name="text">Text that will be displayed in the message box</param>
    public MessageEventArgs(MessageBoxIcon image, MessageText text) {
      this.image = image;
      this.text = text;
    }

    /// <summary>Image that indicates the severity of the message being displayed</summary>
    public MessageBoxIcon Image {
      get { return this.image; }
    }

    /// <summary>Text that is being displayed in the message box</summary>
    public MessageText Text {
      get { return this.text; }
    }

    /// <summary>Image that indicates the severity of the message being displayed</summary>
    private MessageBoxIcon image;
    /// <summary>Text that is being displayed in the message box</summary>
    private MessageText text;

  }

} // namespace Nuclex.Windows.Forms.Messages
