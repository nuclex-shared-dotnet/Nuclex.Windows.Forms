using System;

using System.Windows.Forms;
using Nuclex.Windows.Forms.Views;

namespace Nuclex.Windows.Forms.AutoBinding {

	/// <summary>Binds views to their view models</summary>
	public interface IAutoBinder {

		/// <summary>Binds the specified view to an explicitly selected view model</summary>
		/// <typeparam name="TViewModel">
		///   Type of view model the view will be bound to
		/// </typeparam>
		/// <param name="view">View that will be bound to a view model</param>
		/// <param name="viewModel">View model the view will be bound to</param>
		void Bind<TViewModel>(Control view, TViewModel viewModel)
			where TViewModel : class;

		/// <summary>
		///   Binds the specified view to the view model specified in its DataContext
		/// </summary>
		/// <param name="view">View that will be bound</param>
		void Bind(Control view);

	}

} // namespace Nuclex.Windows.Forms.AutoBinding
