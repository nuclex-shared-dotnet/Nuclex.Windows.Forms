Nuclex.Support Dependencies
===================================


To Compile the Library
----------------------

This project is intended to be placed into a source tree using submodules to replicate
the following directory layout:

    root/
        Nuclex.Windows.Forms/       <-- you are here
            Nuclex.Windows.Forms (net-4.6).csproj

        Nuclex.Support.Native/      <-- Git: nuclex-shared-dotnet/Nuclex.Support
            Nuclex.Support (net-4.6).csproj

        third-party/
            nunit
            nmock

You should already have that directory layout in playe if you cloned the "frame fixer"
repository (with `--recurse-submodules`).

  * Nuclex.Support
  * nunit (optional, if unit tests are built)
  * nmock (optional, if unit tests are built)


To Use this Library as a Binary
-------------------------------

  * Nuclex.Support.dll
