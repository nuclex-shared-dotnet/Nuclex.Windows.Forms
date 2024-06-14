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
