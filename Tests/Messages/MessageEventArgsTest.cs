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

#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using NUnit.Framework;

namespace Nuclex.Windows.Forms.Messages {

  /// <summary>Unit tests for the message box event argument container</summary>
  [TestFixture]
#if NET6_0_OR_GREATER
  [SupportedOSPlatform("windows")]
#endif
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
