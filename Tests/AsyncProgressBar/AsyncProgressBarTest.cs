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

#if UNITTEST

using System;
using System.IO;
using System.Windows.Forms;

using NUnit.Framework;

using Nuclex.Support;

namespace Nuclex.Windows.Forms {

  /// <summary>Unit Test for the asynchronously updating progress bar</summary>
  [TestFixture, Explicit]
  public class AsyncProgressBarTest {

    /// <summary>
    ///   Verifies that asynchronous progress assignment is working
    /// </summary>
    [Test]
    public void TestProgressAssignment() {
      using(AsyncProgressBar progressBar = new AsyncProgressBar()) {

        // Let the control create its window handle
        progressBar.CreateControl();
        progressBar.Minimum = 0;
        progressBar.Maximum = 100;
        
        Assert.AreEqual(0, progressBar.Value);
        
        // Assign the new value. This will be done asynchronously, so we call
        // Application.DoEvents() to execute the message pump once, guaranteeing
        // that the call will have been executed after Application.DoEvents() returns.
        progressBar.AsyncSetValue(0.33f);
        Application.DoEvents();
        
        Assert.AreEqual(33, progressBar.Value);

        progressBar.AsyncSetValue(0.66f);
        Application.DoEvents();

        Assert.AreEqual(66, progressBar.Value);

      }
    }

  }

} // namespace Nuclex.Windows.Forms

#endif // UNITTEST
