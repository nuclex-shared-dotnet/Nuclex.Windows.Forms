using Nuclex.Windows.Forms.Views;
using System;
using System.Windows.Forms;

namespace Nuclex.Windows.Forms.AutoBinding {

  /// <summary>
  ///   Binds a view to its model using a convention-over-configuration approach
  /// </summary>
  public class ConventionBinder : IAutoBinder {

    /// <summary>Binds the specified view to an explicitly selected view model</summary>
    /// <typeparam name="TViewModel">
    ///   Type of view model the view will be bound to
    /// </typeparam>
    /// <param name="view">View that will be bound to a view model</param>
    /// <param name="viewModel">View model the view will be bound to</param>
    public void Bind<TViewModel>(Control view, TViewModel viewModel)
      where TViewModel : class {
      bind(view, viewModel);
    }

    /// <summary>
    ///   Binds the specified view to the view model specified in its DataContext
    /// </summary>
    /// <param name="viewControl">View that will be bound</param>
    public void Bind(Control viewControl) {
      IView viewControlAsView = viewControl as IView;
      if(viewControlAsView == null) {
        throw new InvalidOperationException(
          "The specified view has no view model associated. Either assign your " +
          "view model to the view's data context beforehand or use the overload " +
          "of Bind() that allows you to explicitly specify the view model."
        );
      }

      bind(viewControl, viewControlAsView.DataContext);
    }

    /// <summary>Binds a view to a view model</summary>
    /// <param name="view">View that will be bound</param>
    /// <param name="viewModel">View model the view will be bound to</param>
    private void bind(Control view, object viewModel) {
    }

  }
} // namespace Nuclex.Windows.Forms.AutoBinding
