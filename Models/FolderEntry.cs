using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Contracts;
using Foldicon.Models.Icon;
using Microsoft.Extensions.DependencyInjection;

namespace Foldicon.Models;

public partial class FolderEntry : ObservableObject
{
    private readonly string _fullPath;
    [ObservableProperty] public partial IFolderIcon CurrentIcon { get; set; }
    [ObservableProperty] public partial ObservableCollection<IFolderIcon> OptionalIcons { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectedIconChanged))]
    public partial IFolderIcon SelectedIcon { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSelectedIconChanged))]
    public partial IFolderIcon AppliedIcon { get; set; }

    public bool IsSystemIcon => CurrentIcon is FolderSystemIcon;
    public bool IsSelectedIconChanged => !SelectedIcon.Equals(AppliedIcon);
    public string FolderName => Path.GetFileName(_fullPath);

    public FolderEntry(string fullPath, IFolderIcon currentIcon, List<FolderFileIcon> exeIcons)
    {
        _fullPath = fullPath;
        CurrentIcon = currentIcon;
        OptionalIcons = new ObservableCollection<IFolderIcon>(exeIcons);


        // 从 exeIcons 寻找 currentIcon，并作为默认选中的图标
        IFolderIcon? selectionDefault = exeIcons.Where(currentIcon.Equals).FirstOrDefault();
        if (selectionDefault is null)
        {
            // 未找到则将 currentIcon 直接插入并作为默认选中的图标
            OptionalIcons.Add(currentIcon);
            selectionDefault = currentIcon;
        }

        AppliedIcon = selectionDefault;
        SelectedIcon = selectionDefault;

        // 设置选项：自动选择一个图标
        if (currentIcon is FolderSystemIcon &&
            exeIcons.Count >= 2 &&
            App.Services.GetRequiredService<IOptionService>().Options.IsAutoSelectIcon) SelectedIcon = exeIcons[1];
    }

    /// <summary>
    ///     应用当前选中的图标到文件夹
    /// </summary>
    public void Apply()
    {
        if (!IsSelectedIconChanged) return;
        SelectedIcon.ApplyTo(_fullPath);
        AppliedIcon = SelectedIcon;
        CurrentIcon = SelectedIcon;
    }

    /// <summary>
    ///     添加自定义图标并选中
    /// </summary>
    /// <param name="icon">图标</param>
    public void AddCustomIcon(IFolderIcon icon)
    {
        OptionalIcons.Add(icon);
        SelectedIcon = icon;
    }
}