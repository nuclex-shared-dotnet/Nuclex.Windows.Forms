using System;

namespace Nuclex.Windows.Forms {

  /// <summary>Constructs views and view model in a scope</summary>
  /// <remarks>
  ///   <para>
  ///     By default the <see cref="WindowManager" /> uses its own
  ///     <see cref="WindowManager.CreateInstance" /> method to construct views
  ///     and view models via <see cref="Activator.CreateInstance(Type)" />,
  ///     which is enough to create forms (which the Windows Forms designer already
  ///     requires to have parameterless constructors) and view models, so long as
  ///     they also have parameterless constructors.
  ///   </para>
  ///   <para>
  ///     To support dependency injection via constructor parameters, you can
  ///     inherit from the <see cref="WindowManager" /> and provide your own override
  ///     of <see cref="WindowManager.CreateInstance" /> that constructs the required
  ///     instance via your dependency injector. This is decent until you have multiple
  ///     view models all accessing the same resource (i.e. a database) via threads.
  ///   </para>
  ///   <para>
  ///     In this final case, &quot;scopes&quot; have become a common solution. Each
  ///     scope has access to singleton services (these exist for the lifetime of
  ///     the entire application), but there are also scoped services which will have
  ///     new instances constructed within each scope. By implementing the
  ///     <see cref="WindowManager.CreateWindowScope" /> method, you can make
  ///     the window manager set up an implicit scope per window or dialog.
  ///   </para>
  /// </remarks>
  public interface IWindowScope : IDisposable {

    /// <summary>Creates an instance of the specified type in the scope</summary>
    /// <param name="type">Type an instance will be created of</param>
    /// <returns>The created instance</returns>
    /// <remarks>
    ///   Use this to wire up your dependency injection container. By default,
    ///   the Activator class will be used to create instances which only works
    ///   if all of your view models are concrete classes.
    /// </remarks>
    object CreateInstance(Type type);

  }

} // namespace Nuclex.Windows.Forms
