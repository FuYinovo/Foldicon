using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Foldicon.Class;
using Foldicon.Tool;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Page;

public sealed partial class IconEditor : INotifyPropertyChanged
{
    public IconEditor()
    {
        InitializeComponent();
    }

    private string _parentFolder = string.Empty;

    public string ParentFolder
    {
        get => _parentFolder;
        set => SetField(ref _parentFolder, value);
    }

    public ObservableCollection<FolderEntry> SubFolders { get; } = []; // 数据源
    public ObservableCollection<FolderEntry> FilteredSubFolders { get; } = []; // UI 显示
    public string FolderNameFilter { get; set; } = string.Empty;
    public bool IsSubFoldersLoaded { get; set; }

    /// <summary>
    /// 搜索框改变时应用筛选
    /// </summary>
    private void FolderNameFilter_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(sender.Text)) return; // 忽略空格
        ApplyFilter();
    }


    /// <summary>
    /// 「选择文件夹」按钮
    /// </summary>
    private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        btn.IsEnabled = false;
        var fullPath = await StoragePicker.PickFolder(btn.XamlRoot.ContentIslandEnvironment.AppWindowId);
        btn.IsEnabled = true;

        if (fullPath is null) return;
        ParentFolder = fullPath;
        IsSubFoldersLoaded = true;
        RefreshSubfolders();
    }

    /// <summary>
    /// 「应用所有图标」按钮
    /// </summary>
    private void ApplyAllButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var entry in SubFolders) entry.Apply();
    }


    /// <summary>
    /// 刷新子文件夹
    /// </summary>
    private void RefreshSubfolders()
    {
        if (!Directory.Exists(ParentFolder)) return;
        SubFolders.Clear();
        foreach (var path in Directory.GetDirectories(ParentFolder))
        {
            try
            {
                var entry = new FolderEntry(path);
                SubFolders.Add(entry);
            }
            // 跳过「无访问权限」的文件夹
            catch (UnauthorizedAccessException accessException)
            {
                Console.WriteLine(accessException.Message);
            }
        }

        ApplyFilter();
    }

    /// <summary>
    /// 应用文件夹筛选
    /// </summary>
    private void ApplyFilter()
    {
        FilteredSubFolders.Clear();
        foreach (var folder in SubFolders)
        {
            // 名称筛选
            if (!folder.FolderName.Contains(FolderNameFilter)) continue;

            // TODO)) 类型筛选

            // 通过筛选
            FilteredSubFolders.Add(folder);
        }
    }


    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }

    #endregion
}