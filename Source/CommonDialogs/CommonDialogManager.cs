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
using System.Text;
using System.Windows.Forms;

#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

namespace Nuclex.Windows.Forms.CommonDialogs {

  /// <summary>Displays common dialogs for selecting files and directories</summary>
#if NET6_0_OR_GREATER
  [SupportedOSPlatform("windows")]
#endif
  public class CommonDialogManager : ICommonDialogService {

    /// <summary>Initializes a new task dialog message service</summary>
    public CommonDialogManager() : this(NullActiveWindowTracker.Default) { }

    /// <summary>Initializes a new task dialog message service</summary>
    /// <param name="tracker">
    ///   Active window tracker used to obtain the parent window for message boxes
    /// </param>
    public CommonDialogManager(IActiveWindowTracker tracker) {
      this.tracker = tracker;
    }

    /// <summary>Asks the user for a location to save a file under</summary>
    /// <param name="caption">Caption of the dialog</param>
    /// <param name="masks">
    ///   File masks in the form "Description|*.dat" or "Description2|*.da2;*.da3"
    /// </param>
    /// <returns>The full path of the file the user wishes to save as</returns>
    public string AskForSaveLocation(string caption, params string[] masks) {
      var saveDialog = new SaveFileDialog() {
        Title = caption,
        Filter = combineMasks(masks)
      };

      DialogResult result;
      {
        Form activeWindow = this.tracker.ActiveWindow;
        if(activeWindow == null) {
          result = saveDialog.ShowDialog();
        } else {
          result = saveDialog.ShowDialog(activeWindow);
        }
      }

      if(result == DialogResult.OK) {
        return saveDialog.FileName;
      } else {
        return null;
      }
    }

    /// <summary>Asks the user to select a file to open</summary>
    /// <param name="caption">Caption of the dialog</param>
    /// <param name="masks">
    ///   File masks in the form "Description|*.dat" or "Description2|*.da2;*.da3"
    /// </param>
    /// <returns>The full path of the file the user selected</returns>
    public string AskForFileToOpen(string caption, params string[] masks) {
      var openDialog = new OpenFileDialog() {
        Title = caption,
        Filter = combineMasks(masks),
        CheckFileExists = true,
        CheckPathExists = true,
        Multiselect = false
      };

      DialogResult result;
      {
        Form activeWindow = this.tracker.ActiveWindow;
        if(activeWindow == null) {
          result = openDialog.ShowDialog();
        } else {
          result = openDialog.ShowDialog(activeWindow);
        }
      }

      if(result == DialogResult.OK) {
        return openDialog.FileName;
      } else {
        return null;
      }
    }

    /// <summary>Asks the user to select one or more files to open</summary>
    /// <param name="caption">Caption of the dialog</param>
    /// <param name="masks">
    ///   File masks in the form "Description|*.dat" or "Description2|*.da2;*.da3"
    /// </param>
    /// <returns>The full path of all files the user selected</returns>
    public string[] AskForFilesToOpen(string caption, params string[] masks) {
      var openDialog = new OpenFileDialog() {
        Title = caption,
        Filter = combineMasks(masks),
        CheckFileExists = true,
        CheckPathExists = true,
        Multiselect = true
      };

      DialogResult result;
      {
        Form activeWindow = this.tracker.ActiveWindow;
        if(activeWindow == null) {
          result = openDialog.ShowDialog();
        } else {
          result = openDialog.ShowDialog(activeWindow);
        }
      }

      if(result == DialogResult.OK) {
        return openDialog.FileNames;
      } else {
        return null;
      }
    }

    /// <summary>Asks the user to select a directory</summary>
    /// <returns>The directory the user has selected</returns>
    public string AskForDirectory(string caption) {
      var folderDialog = new System.Windows.Forms.FolderBrowserDialog() {
        Description = caption
      };

      DialogResult result;
      {
        Form activeWindow = this.tracker.ActiveWindow;
        if(activeWindow == null) {
          result = folderDialog.ShowDialog();
        } else {
          result = folderDialog.ShowDialog(activeWindow);
        }
      }

      if(result == DialogResult.OK) {
        return folderDialog.SelectedPath;
      } else {
        return null;
      }
    }

    /// <summary>Combines an array of file masks into a single mask</summary>
    /// <param name="masks">Masks that will be combined</param>
    /// <returns>That combined masks</returns>
    private static string combineMasks(string[] masks) {
      if((masks == null) || (masks.Length == 0)) {
        return null;
      }

      int requiredCapacity = 0;
      for(int index = 0; index < masks.Length; ++index) {
        requiredCapacity += masks[index].Length;
        requiredCapacity += 1;
      }

      var maskBuilder = new StringBuilder(requiredCapacity);
      maskBuilder.Append(masks[0]);
      for(int index = 1; index < masks.Length; ++index) {
        maskBuilder.Append('|');
        maskBuilder.Append(masks[index]);
      }

      return maskBuilder.ToString();
    }

    /// <summary>Provides the currently active top-level window</summary>
    private IActiveWindowTracker tracker;

  }

} // namespace Nuclex.Windows.Forms.CommonDialogs
