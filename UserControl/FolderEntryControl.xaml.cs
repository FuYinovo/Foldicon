using System.IO;
using System.Linq;
using Foldicon.Class;
using Foldicon.Tool;
using IniFileSharp;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FileIcon = Foldicon.Struct.FileIcon;

namespace Foldicon.UserControl;

public sealed partial class FolderEntryControl
{
    public FolderEntry FolderEntry
    {
        get => (FolderEntry)GetValue(FolderEntryProperty);
        set
        {
            SetValue(FolderEntryProperty, value);
            // ListView 是虚拟化控件，会直接修改 FolderEntry 属性以复用 FolderEntryControl
            // 不会触发 Loaded 委托
            // 因此补充调用 InitIconSelectorIndex()
            InitIconSelectorIndex(null, null);
        }
    }

    private static readonly DependencyProperty FolderEntryProperty = DependencyProperty.Register(
        nameof(FolderEntry),
        typeof(FolderEntry),
        typeof(FolderEntryControl),
        new PropertyMetadata(null)
    );


    public FolderEntryControl()
    {
        InitializeComponent();
        Loaded += InitIconSelectorIndex;
    }


    /// <summary>
    /// 初始化 <see cref="IconSelector"/> 默认选择的图标条目
    /// </summary>
    private void InitIconSelectorIndex(object? sender, RoutedEventArgs? e)
    {
        // 读取 desktop.ini 中指定的文件夹图标
        var iniPath = Path.Combine(FolderEntry.FullPath, "desktop.ini");
        if (!File.Exists(iniPath)) return;

        // desktop.ini 参考:
        // [.ShellClassInfo]
        // IconResource=F:\Folder11-Icon\google.ico,0
        var ini = new IniSharp(iniPath);

        // 1. 节名带上"."
        // 2. IniSharp.GetValue 在键/节不存在且 defaultValue 为 null 时会抛出 ArgumentNullException，
        var iconResource = ini.GetValue(".ShellClassInfo", "IconResource", string.Empty);
        var iconPath = iconResource.Split(",").FirstOrDefault(string.Empty);
        if (string.IsNullOrEmpty(iconPath)) return;

        // 相对路径 -> 绝对路径
        if (!Path.IsPathRooted(iconPath)) iconPath = Path.Combine(FolderEntry.FullPath, iconPath);


        // 忽略 .dll 图标
        // 例如：IconResource=%SystemRoot%\system32\imageres.dll,-189
        if (iconPath.EndsWith(".dll")) return;

        // 尝试在自定义图标可选项里查找
        for (var i = 0; i < FolderEntry.OptionalIcons.Count; i++)
        {
            var path = FolderEntry.OptionalIcons[i].FullPath;
            if (path != iconPath) continue;
            // 找到：直接设为初始选择项
            IconSelector.SelectedIndex = i;
            return;
        }

        // 未找到：创建新的自定义图标可选项
        var icon = IconHelper.GetFileIcon(iconPath);
        if (icon is null) return; // TODO)) 因为 desktop.ini 将中文存储为 GB2312 编码导致读取失败
        FolderEntry.OptionalIcons.Add(new FileIcon(iconPath, icon));
        IconSelector.SelectedIndex = FolderEntry.OptionalIcons.Count - 1;
    }

    /// <summary>
    /// 应用选中的自定义图标
    /// </summary>
    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        FolderEntry.Apply();
    }

    /// <summary>
    /// 使用 FilePicker 选择其他自定义图标
    /// </summary>
    private async void PickOtherIconButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.IsEnabled = false;
        var fullPath =
            await StoragePicker.PickFile([".exe", ".ico"], btn.XamlRoot.ContentIslandEnvironment.AppWindowId);
        btn.IsEnabled = true;

        if (fullPath is null) return;
        var icon = IconHelper.GetFileIcon(fullPath);
        if (icon is null) return;
        FolderEntry.OptionalIcons.Add(new FileIcon(fullPath, icon));
    }
}