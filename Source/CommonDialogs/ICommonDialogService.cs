#region Apache License 2.0
/*
Nuclex .NET Framework
Copyright (C) 2002-2024 Markus Ewald / Nuclex Development Labs

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion // Apache License 2.0

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
