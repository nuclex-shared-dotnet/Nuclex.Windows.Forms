Nuclex.Windows.Forms Dependencies
================================


To Compile the Library
----------------------

This project is intended to be placed into a source tree using submodules to replicate
the following directory layout:

    root/
        Nuclex.Windows.Forms/       <-- you are here
            ...

        Nuclex.Support/             <-- Git: nuclex-shared-dotnet/Nuclex.Support
            ...

        third-party/
            nunit

You should already have that directory layout in place if you cloned the "foundation package"
repository (with `--recurse-submodules`).

The actual, direct requirements of the code to compile are:

  * Nuclex.Support         (project)
  * nunit                  (NuGet package, optional, if unit tests are built)


To Use this Library as a Binary
-------------------------------

  * Nuclex.Windows.Forms   (project)
  * Nuclex.Support         (project)
