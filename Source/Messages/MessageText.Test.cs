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
using System.Windows;

using NUnit.Framework;

namespace Nuclex.Windows.Forms.Messages {

  /// <summary>Unit tests for the message text container</summary>
  [TestFixture]
  internal class MessageTextTest {

    /// <summary>Ensures that the message text class provides a copy constructor</summary>
    [Test]
    public void HasCopyConstructor() {
      var text = new MessageText() {
        Caption = "Caption",
        Message = "Message",
        Details = "Details",
        ExpandedDetails = "ExpandedDetails"
      };
      var copy = new MessageText(text);

      Assert.AreEqual(text.Caption, copy.Caption);
      Assert.AreEqual(text.Message, copy.Message);
      Assert.AreEqual(text.Details, copy.Details);
      Assert.AreEqual(text.ExpandedDetails, copy.ExpandedDetails);
    }

  }

} // namespace Nuclex.Windows.Forms.Messages
