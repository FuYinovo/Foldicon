using System;
using Windows.Storage;

namespace Foldicon.Tool;

public static class UriHelper
{
    /// <summary>
    /// 获取 WinUI 应用 Assets 下一个文件的路径
    /// </summary>
    /// <param name="fileName">文件名称</param>
    /// <returns>绝对路径</returns>
    public static string GetFilePathFromAssets(string fileName)
    {
        var uri = new Uri($"ms-appx:///Assets/{fileName}");
        return (StorageFile.GetFileFromApplicationUriAsync(uri).GetAwaiter().GetResult()).Path;
    }
}