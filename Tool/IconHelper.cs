using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using Foldicon.Struct;
using IniFileSharp;
using Microsoft.UI.Xaml.Media.Imaging;
using Vanara.InteropServices;
using Vanara.PInvoke;

namespace Foldicon.Tool;

/// <summary>
/// System.Drawing.Common 版本必须低于ver9.0
/// </summary>
public static partial class IconHelper // 公开方法
{
    /// <summary>
    /// 设置文件夹图标
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
    /// 获取文件夹的图标
    /// </summary>
    /// <param name="path">文件夹完整路径</param>
    /// <returns>BitmapImage；失败返回 null</returns>
    public static BitmapImage? GetFolderIcon(string path)
    {
        return GetIcon(path, true);
    }

    /// <summary>
    /// 获取文件的图标
    /// </summary>
    /// <param name="path">文件完整路径</param>
    /// <returns>BitmapImage；失败返回 null</returns>
    public static BitmapImage? GetFileIcon(string path)
    {
        return GetIcon(path, false);
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
            var icon = GetFileIcon(file);
            if (icon is null) continue; // 必须不为null

            // 创建实例
            icons.Add(new FileIcon(file, icon));
        }

        return icons;
    }

    /// <summary>
    /// 获取文件夹自定义图标的路径
    /// </summary>
    /// <param name="folderPath">文件夹完整路径</param>
    /// <param name="encoding">读取Ini文件的编码</param>
    /// <returns>图标路径(失败返回null)</returns>
    public static string? GetFolderCustomIconPath(string folderPath, Encoding encoding)
    {
        var iniPath = Path.Combine(folderPath, "desktop.ini");
        var iniSharp = new IniSharp(iniPath, encoding);
        try
        {
            var resource = iniSharp.GetValue(".ShellClassInfo", "IconResource");
            return resource is null ? null : ConsumeResource(resource);
        }
        catch (ArgumentNullException)
        {
            // IniSharp.GetValue 在键/节不存在且 defaultValue 为 null 时会抛出 ArgumentNullException
            return null;
        }

        string? ConsumeResource(string resource)
        {
            var path = resource.Split(",").FirstOrDefault(string.Empty);
            if (string.IsNullOrEmpty(path)) return null;

            // 忽略 .dll 图标
            if (path.EndsWith(".dll")) return null;

            // 相对路径 -> 绝对路径
            if (!Path.IsPathRooted(path)) path = Path.Combine(folderPath, path);

            return path;
        }
    }
}

public static partial class IconHelper // 私有方法
{
    /// <summary>
    /// 获取文件(夹)的图标位图
    /// </summary>
    /// <param name="path">文件夹完整路径</param>
    /// <param name="isFolder">是否是文件夹</param>
    /// <returns>BitmapImage；失败返回 null</returns>
    private static BitmapImage? GetIcon(string path, bool isFolder)
    {
        if (!TryShell32GetIcon(path, isFolder, out var info) || info.hIcon.IsInvalid) return null;

        // 使用 Icon.FromHandle，防止 Bitmap.FromHIcon 丢失透明度
        var bitmap = Icon.FromHandle(info.hIcon.DangerousGetHandle()).ToBitmap();
        User32.DestroyIcon(info.hIcon);

        // Bitmap -> MemoryStream -> RandomAccessStream -> BitmapImage
        var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        stream.Seek(0, SeekOrigin.Begin);

        var image = new BitmapImage();
        image.SetSource(stream.AsRandomAccessStream());
        return image;
    }

    /// <summary>
    /// 尝试使用 Shell32 获取文件或文件夹的图标信息
    /// </summary>
    /// <para>警告：路径斜杠必须为"\"</para>
    /// <param name="path">完整路径</param>
    /// <param name="isFolder">是否为文件夹</param>
    /// <param name="iconInfo">接收图标信息的 <see cref="Shell32.SHFILEINFO"/></param>
    /// <example>路径斜杠错误</example>
    /// <returns>是否成功</returns>
    private static bool TryShell32GetIcon(string path, bool isFolder, out Shell32.SHFILEINFO iconInfo)
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
        iconInfo = info;
        return state != IntPtr.Zero;
    }
}