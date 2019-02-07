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

using NUnit.Framework;

namespace Nuclex.Windows.Forms.Messages {

  /// <summary>Unit tests for the message box event argument container</summary>
  [TestFixture]
  internal class MessageEventArgsTest {

    /// <summary>Verifies that the image associated with the message gets stored</summary>
    [Test]
    public void ImageIsStored() {
      var arguments = new MessageEventArgs(MessageBoxIcon.Exclamation, null);
      Assert.AreEqual(MessageBoxIcon.Exclamation, arguments.Image);
    }

    /// <summary>Verifies that the text associated with the message gets stored</summary>
    [Test]
    public void TextIsStored() {
      var text = new MessageText();
      var arguments = new MessageEventArgs(MessageBoxIcon.None, text);
      Assert.AreSame(text, arguments.Text);
    }

  }

} // namespace Nuclex.Windows.Forms.Messages
