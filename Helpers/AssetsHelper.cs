using System;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Foldicon.Helpers;

public static class AssetsHelper
{
    private static readonly StorageFolder Assets =
        Package.Current.InstalledLocation.GetFolderAsync("Assets").GetAwaiter().GetResult();

    /// <summary>
    ///     获取 WinUI 应用 Assets 下一个文件的路径
    /// </summary>
    /// <param name="filePath">文件相对路径</param>
    /// <returns>绝对路径</returns>
    public static string GetFilePathFromAssets(string filePath)
    {
        return Assets.GetFileAsync(filePath).GetAwaiter().GetResult().Path;
    }
}