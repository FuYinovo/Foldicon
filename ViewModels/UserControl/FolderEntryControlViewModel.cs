using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Contracts;
using CommunityToolkit.Mvvm.Input;
using Foldicon.Helpers;
using Foldicon.Models;
using Foldicon.Views.Dialog;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace Foldicon.ViewModels.UserControl;

public partial class FolderEntryControlViewModel(IDialogService dialogService) : ObservableObject
{
    [ObservableProperty] public partial FolderEntry? FolderEntry { get; set; } // 尚未从依赖属性得到实例时为null

    /// <summary>
    ///     应用选中的自定义图标
    /// </summary>
    [RelayCommand]
    public void Apply()
    {
        FolderEntry?.Apply();
    }

    /// <summary>
    ///     使用 FilePicker 选择其他自定义图标
    /// </summary>
    [RelayCommand]
    public async Task PickIconFromFileAsync()
    {
        var fullPath = await StoragePicker.PickFile([".exe", ".ico"], dialogService.WindowId);
        if (fullPath is null) return;
        FolderEntry?.AddCustomIcon(fullPath);
    }

    /// <summary>
    ///     从图标组选择其他自定义图标
    /// </summary>
    [RelayCommand]
    public async Task PickIconFromGroupAsync(IconGroup group)
    {
        // 弹出选择图标对话框
        var content = new PickIconFromGroupDialog(group);
        var res = await dialogService.ShowDialogAsync(title: $"从\"{group.Name}\" 选择一个图标", content: content);
        if (res == ContentDialogResult.None) return; // 取消操作

        var selectedPath = content.GetSelectedIconPath();
        if (selectedPath is null)
        {
            // 未选择提示
            await dialogService.ShowMessageAsync("添加失败", "未选中任何图标");
            return;
        }

        // 正常选择 -> 添加图标
        FolderEntry?.AddCustomIcon(selectedPath);
    }
}