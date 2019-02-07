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
