using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Foldicon.Tool;

public static class UriHelper
{
    private static readonly StorageFolder Assets =
        Package.Current.InstalledLocation.GetFolderAsync("Assets").GetAwaiter().GetResult();

    /// <summary>
    /// 获取 WinUI 应用 Assets 下一个文件的路径
    /// </summary>
    /// <param name="filePath">文件相对路径</param>
    /// <returns>绝对路径</returns>
    public static string GetFilePathFromAssets(string filePath)
    {
        return Assets.GetFileAsync(filePath).GetAwaiter().GetResult().Path;
    }

    /// <summary>
    /// 获取 WinUI 应用 Assets 下一个文件夹的路径
    /// </summary>
    /// <param name="folderPath">文件夹相对路径</param>
    /// <returns>绝对路径</returns>
    public static string GetFolderPathFromAssets(string folderPath)
    {
        return Assets.GetFolderAsync(folderPath).GetAwaiter().GetResult().Path;
    }

    /// <summary>
    /// 获取 WinUI 应用 Assets 下一个文件夹内，所有子文件夹的路径
    /// </summary>
    /// <param name="folderPath">文件夹相对路径</param>
    /// <returns>绝对路径</returns>
    public static string[] GetSubFoldersPathFromAssets(string folderPath)
    {
        var path = GetFolderPathFromAssets(folderPath);
        return Directory.GetDirectories(path);
    }
}