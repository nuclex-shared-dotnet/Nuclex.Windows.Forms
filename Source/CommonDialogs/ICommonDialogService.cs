#region CPL License
/*
Nuclex Framework
Copyright (C) 2002-2019 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
IBM Common Public License for more details.

You should have received a copy of the IBM Common Public
License along with this library
*/
#endregion

using System;

namespace Nuclex.Windows.Forms.CommonDialogs {

  /// <summary>Displays common dialogs for selecting files and directories</summary>
  public interface ICommonDialogService {

    /// <summary>Asks the user for a location to save a file under</summary>
    /// <param name="caption">Caption of the dialog</param>
    /// <param name="masks">
    ///   File masks in the form "Description|*.dat" or "Description2|*.da2;*.da3"
    /// </param>
    /// <returns>The full path of the file the user wishes to save as</returns>
    string AskForSaveLocation(string caption, params string[] masks);

    /// <summary>Asks the user to select a file to open</summary>
    /// <param name="caption">Caption of the dialog</param>
    /// <param name="masks">
    ///   File masks in the form "Description|*.dat" or "Description2|*.da2;*.da3"
    /// </param>
    /// <returns>The full path of the file the user selected</returns>
    string AskForFileToOpen(string caption, params string[] masks);

    /// <summary>Asks the user to select one or more files to open</summary>
    /// <param name="caption">Caption of the dialog</param>
    /// <param name="masks">
    ///   File masks in the form "Description|*.dat" or "Description2|*.da2;*.da3"
    /// </param>
    /// <returns>The full path of all files the user selected</returns>
    string[] AskForFilesToOpen(string caption, params string[] masks);

    /// <summary>Asks the user to select a directory</summary>
    /// <returns>The directory the user has selected</returns>
    string AskForDirectory(string caption);

  }

} // namespace Nuclex.Windows.Forms.CommonDialogs
