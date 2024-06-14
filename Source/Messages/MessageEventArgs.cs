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
