using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.Windows.Storage.Pickers;

namespace Foldicon.Tool;

public static class StoragePicker
{
    /// <summary>
    /// 使用 FolderPicker 选择一个文件夹
    /// </summary>
    /// <returns>完整路径</returns>
    public static async Task<string?> PickFolder(WindowId windowId)
    {
        var picker = new FolderPicker(windowId)
        {
            CommitButtonText = "选择此文件夹",
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            ViewMode = PickerViewMode.List,
        };

        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }

    /// <summary>
    /// 使用 FileOpenPicker 选择一个文件
    /// </summary>
    /// <returns>完整路径</returns>
    public static async Task<string?> PickFile(IEnumerable<string> fileTypeFilter,WindowId windowId)
    {
        var picker = new FileOpenPicker(windowId)
        {
            CommitButtonText = "选择此文件",
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            ViewMode = PickerViewMode.List,
        };
        foreach (var fileType in fileTypeFilter) picker.FileTypeFilter.Add(fileType);

        var folder = await picker.PickSingleFileAsync();
        return folder?.Path;
    }
}