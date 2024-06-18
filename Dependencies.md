Nuclex.Windows.Forms Dependencies
================================


To Compile the Library
----------------------

This project is intended to be placed into a source tree using submodules to replicate
the following directory layout:

    root/
        Nuclex.Windows.Forms/       <-- you are here
            ...

        Nuclex.Support.Native/      <-- Git: nuclex-shared-dotnet/Nuclex.Support
            ...

        third-party/
            nunit
            nmock

You should already have that directory layout in place if you cloned the "foundation package"
repository (with `--recurse-submodules`).

The actual, direct requirements of the code to compile are:

  * Nuclex.Support
  * nunit (optional, if unit tests are built)
  * nmock (optional, if unit tests are built)


To Use this Library as a Binary
-------------------------------

  * Nuclex.Windows.Forms.dll
  * Nuclex.Support.dll
