using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Foldicon.Class;
using Foldicon.Tool;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace Foldicon;

public sealed partial class MainWindow : INotifyPropertyChanged
{
    private string _parentFolder = "";

    public string ParentFolder
    {
        get => _parentFolder;
        set => SetField(ref _parentFolder, value);
    }

    public ObservableCollection<FolderEntry> SubFolders { get; } = [];

    public MainWindow()
    {
        InitializeComponent();
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
    /// 「刷新」按钮
    /// </summary>
    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshSubfolders();
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