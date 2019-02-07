using System;

using NUnit.Framework;

namespace Nuclex.Windows.Forms {

  /// <summary>Unit test for the window manager</summary>
  [TestFixture]
  public class WindowManagerTest {

    /// <summary>Verifies that the window manager provides a default constructor</summary>
    [Test]
    public void HasDefaultConstructor() {
      Assert.DoesNotThrow(
        () => new WindowManager()
      );
    }

  }

} // namespace Nuclex.Windows.Forms
