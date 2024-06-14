  - **Status:** Stable and mature. Several projects are using this library,
    and it has received extensive testing on Linux and Windows.

  - **Platforms:** Cross-platform, developed on Linux but also tested and
    working without any known issues on Windows.

Nuclex.Windows.Forms
====================

This is a lightweight MVVM framework for Windows Forms. It is based on
the "convention over configuration" idea and requires zero configuration.
Rather than set up view mappings and view model services, you can simply
ask it to display a view model (`ExampleViewModel`) with its default view
and it will figure out what the correct view is by name (i.e. `ExampleForm`).

There are unit tests for the whole library, so everything is verifiably
working on all platforms tested (Linux, Windows, Raspberry).
