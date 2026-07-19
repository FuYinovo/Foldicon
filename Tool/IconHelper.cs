using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Foldicon.Class;
using Foldicon.Struct;
using Microsoft.UI.Xaml.Media.Imaging;
using Vanara.InteropServices;
using Vanara.PInvoke;

namespace Foldicon.Tool;

public static class IconHelper
{
    /// <summary>
    /// 给文件夹设置图标
    /// </summary>
    /// <param name="folderPath">文件夹的完整路径</param>
    /// <param name="iconPath">图标的完整路径</param>
    public static void SetFolderIcon(string folderPath, string iconPath)
    {
        var settings = new Shell32.SHFOLDERCUSTOMSETTINGS
        {
            dwMask = Shell32.FOLDERCUSTOMSETTINGSMASK.FCSM_ICONFILE,
            pszIconFile = new StrPtrAuto(iconPath),
            iIconIndex = 0
        };
        Shell32.SHGetSetFolderCustomSettings(ref settings, folderPath, Shell32.FCS.FCS_FORCEWRITE);
    }


    /// <summary>
    /// 获取文件夹下所有exe程序的图标
    /// </summary>
    /// <param name="path">文件夹完整路径</param>
    /// <returns>「路径+图标」结构体的列表</returns>
    public static List<FileIcon> GetExeIcons(string path)
    {
        List<FileIcon> icons = [];

        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            // 扩展名
            var extension = Path.GetExtension(file);
            if (!extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)) continue; // 必须为exe

            // 图标
            var (iconState, _) = Shell32GetIcon(file, false);
            if (!iconState) continue; // 必须不为null

            // 创建实例
            icons.Add(new FileIcon(file, GetIcon(file, false) ?? throw new Exception($"读取{file}的图标为null")));
        }

        return icons;
    }

    /// <summary>
    /// 获取文件(夹)的图标
    /// <para>警告：System.Drawing.Common 库版本必须低于ver9.0</para>
    /// </summary>
    /// <param name="path">文件夹完整路径</param>
    /// <param name="isFolder">是否是文件夹</param>
    /// <returns>BitmapImage；失败返回 null</returns>
    public static BitmapImage? GetIcon(string path, bool isFolder)
    {
        var (isSucceed, info) = Shell32GetIcon(path, isFolder);
        if (!isSucceed || info.hIcon.IsInvalid) return null;

        // 使用 Icon.FromHandle，防止 Bitmap.FromHIcon 丢失透明度
        var bitmap = Icon.FromHandle(info.hIcon.DangerousGetHandle()).ToBitmap();
        // Bitmap -> MemoryStream -> RandomAccessStream -> BitmapImage
        var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        stream.Seek(0, SeekOrigin.Begin);

        var image = new BitmapImage();
        image.SetSource(stream.AsRandomAccessStream());
        return image;
    }


    /// <summary>
    /// 使用 Shell32 获取文件或文件夹的图标
    /// </summary>
    /// <para>警告：路径斜杠必须为"\"</para>
    /// <param name="path">完整路径</param>
    /// <param name="isFolder">是否为文件夹</param>
    /// <example>路径斜杠错误</example>
    /// <returns>(是否成功, 图标信息)</returns>
    private static (bool isSuccesed, Shell32.SHFILEINFO info) Shell32GetIcon(string path, bool isFolder)
    {
        if (path.Contains('/')) throw new Exception("路径斜杠必须为Windows的'\'分隔符");

        var info = new Shell32.SHFILEINFO();
        var state = Shell32.SHGetFileInfo(
            path,
            isFolder ? FileAttributes.Directory : FileAttributes.Normal,
            ref info,
            Marshal.SizeOf(info),
            // ICON：获取图标 | LARGE_ICON：获取大图标       【必须要有ICON】
            Shell32.SHGFI.SHGFI_LARGEICON | Shell32.SHGFI.SHGFI_ICON
        );
        return (state != IntPtr.Zero, info);
    }
}