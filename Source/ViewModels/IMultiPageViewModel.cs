using System;

namespace Nuclex.Windows.Forms.ViewModels {

  /// <summary>Interface for vew models that can switch between different pages</summary>
  public interface IMultiPageViewModel {

    /// <summary>Retrieves (and, if needed, creates) the view model for the active page</summary>
    /// <returns>A view model for the active page on the multi-page view model</returns>
    object GetActivePageViewModel();

  }

} // namespace Nuclex.Windows.Forms.ViewModels
