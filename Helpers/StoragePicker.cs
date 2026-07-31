using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.Windows.Storage.Pickers;

namespace Foldicon.Helpers;

public static class StoragePicker
{
    /// <summary>
    ///     使用 FolderPicker 选择一个文件夹
    /// </summary>
    /// <returns>完整路径</returns>
    public static async Task<string?> PickFolder(WindowId windowId)
    {
        var picker = new FolderPicker(windowId)
        {
            CommitButtonText = "选择此文件夹",
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            ViewMode = PickerViewMode.List
        };

        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }

    /// <summary>
    ///     使用 FolderPicker 选择多个文件夹
    /// </summary>
    /// <returns>完整路径</returns>
    public static async Task<string[]> PickFolders(WindowId windowId)
    {
        var picker = new FolderPicker(windowId)
        {
            CommitButtonText = "选择此文件夹",
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            ViewMode = PickerViewMode.List
        };

        var folders = await picker.PickMultipleFoldersAsync();
        if (folders is null) return [];
        return [.. folders.Select(x => x.Path)];
    }

    /// <summary>
    ///     使用 FileOpenPicker 选择一个文件
    /// </summary>
    /// <returns>文件完整路径；未选则返回null</returns>
    public static async Task<string?> PickFile(IEnumerable<string> fileTypeFilter, WindowId windowId)
    {
        var picker = new FileOpenPicker(windowId)
        {
            CommitButtonText = "选择此文件",
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            ViewMode = PickerViewMode.List
        };
        foreach (var fileType in fileTypeFilter) picker.FileTypeFilter.Add(fileType);

        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }

    /// <summary>
    ///     使用 FileOpenPicker 选择多个文件
    /// </summary>
    /// <returns>所有文件的完整路径</returns>
    public static async Task<string[]> PickFiles(IEnumerable<string> fileTypeFilter, WindowId windowId)
    {
        var picker = new FileOpenPicker(windowId)
        {
            CommitButtonText = "选择以上文件",
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            ViewMode = PickerViewMode.List
        };
        foreach (var fileType in fileTypeFilter) picker.FileTypeFilter.Add(fileType);

        var files = await picker.PickMultipleFilesAsync();
        if (files is null) return [];
        return [.. files.Select(x => x.Path)];
    }
}