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
using System.Text;
using System.Windows.Forms;

namespace Nuclex.Windows.Forms.CommonDialogs {

  /// <summary>Displays common dialogs for selecting files and directories</summary>
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
