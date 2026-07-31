using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Foldicon.Struct;
using IniFileSharp;
using Microsoft.UI.Xaml.Media.Imaging;
using Vanara.InteropServices;
using Vanara.PInvoke;

namespace Foldicon.Helpers;

/// <summary>
///     System.Drawing.Common 版本必须低于ver9.0
/// </summary>
public static partial class IconHelper // 公开方法
{
    /// <summary>
    ///     设置文件夹图标
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
    ///     获取文件夹的图标
    /// </summary>
    /// <param name="path">文件夹完整路径</param>
    /// <returns>PNG数组；失败返回 null</returns>
    public static byte[]? GetFolderIcon(string path)
    {
        return GetIcon(path, true);
    }

    /// <summary>
    ///     获取文件的图标
    /// </summary>
    /// <param name="path">文件完整路径</param>
    /// <returns>PNG数组；失败返回 null</returns>
    public static byte[]? GetFileIcon(string path)
    {
        return GetIcon(path, false);
    }

    /// <summary>
    ///     获取文件夹下所有exe程序的路径及图标
    /// </summary>
    /// <param name="fullPath">文件夹完整路径</param>
    /// <param name="configureAwait">是否以调用进程返回</param>
    /// <returns>BytesIcon 实例的列表</returns>
    public static async Task<List<BytesIcon>> GetExeIconsAsync(string fullPath, bool configureAwait = true)
    {
        // 创建任务
        List<Task<BytesIcon>> tasks = [];
        var files = Directory.GetFiles(fullPath);
        foreach (var file in files)
        {
            // 扩展名必须为exe
            var extension = Path.GetExtension(file);
            if (!extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)) continue;

            tasks.Add(Task.Run(() => new BytesIcon { FullPath = file, Icon = GetFileIcon(file) }));
        }

        // 获取结果
        await Task.WhenAll(tasks).ConfigureAwait(configureAwait);
        List<BytesIcon> icons = [];
        foreach (var task in tasks)
        {
            var icon = task.Result;
            if (icon.Icon is not null) icons.Add(icon);
        }

        return icons;
    }

    /// <summary>
    ///     获取文件夹下所有子文件夹的路径及图标
    /// </summary>
    /// <param name="fullPath">文件夹完整路径</param>
    /// <param name="configureAwait">是否以调用进程返回</param>
    /// <returns>BytesIcon 实例的列表</returns>
    public static async Task<List<BytesIcon>> GetSubfolderIconsAsync(string fullPath, bool configureAwait = true)
    {
        // 创建任务
        List<Task<BytesIcon>> tasks = [];
        var subFolders = Directory.GetDirectories(fullPath);
        foreach (var subFolder in subFolders)
            tasks.Add(Task.Run(() => new BytesIcon { FullPath = subFolder, Icon = GetFolderIcon(subFolder) }));

        // 获取结果
        List<BytesIcon> icons = [];
        await Task.WhenAll(tasks).ConfigureAwait(configureAwait);
        foreach (var task in tasks)
        {
            var icon = task.Result;
            if (icon.Icon is not null) icons.Add(icon);
        }

        return icons;
    }

    /// <summary>
    ///     获取文件夹自定义图标的路径
    /// </summary>
    /// <param name="folderPath">文件夹完整路径</param>
    /// <param name="encoding">读取Ini文件的编码</param>
    /// <returns>图标路径(失败返回null)</returns>
    public static string? GetFolderCustomIconPath(string folderPath, Encoding encoding)
    {
        var iniPath = Path.Combine(folderPath, "desktop.ini");
        try
        {
            var iniSharp = new IniSharp(iniPath, encoding);
            var resource = iniSharp.GetValue(".ShellClassInfo", "IconResource");
            return resource is null ? null : ConsumeResource(resource);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
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

    /// <summary>
    ///     从数组创建 <see cref="BitmapImage" /> 实例
    /// </summary>
    /// <param name="bytes">图片数组</param>
    /// <exception cref="COMException">没有在 UI 线程调用方法</exception>
    /// <returns><see cref="BitmapImage" /> 实例</returns>
    public static BitmapImage CreateBitmapImage(byte[] bytes)
    {
        var bitmap = new BitmapImage();
        bitmap.SetSource(new MemoryStream(bytes).AsRandomAccessStream());
        return bitmap;
    }
}

public static partial class IconHelper // 私有方法
{
    /// <summary>
    ///     获取文件(夹)的图标位图
    /// </summary>
    /// <param name="path">文件夹完整路径</param>
    /// <param name="isFolder">是否是文件夹</param>
    /// <returns>PNG数组；失败返回 null</returns>
    private static byte[]? GetIcon(string path, bool isFolder)
    {
        if (!TryShell32GetIcon(path, isFolder, out var info) || info.hIcon.IsInvalid) return null;

        // 使用 Icon.FromHandle，防止 Bitmap.FromHIcon 丢失透明度
        using var bitmap = Icon.FromHandle(info.hIcon.DangerousGetHandle()).ToBitmap();
        User32.DestroyIcon(info.hIcon);

        // Bitmap -> MemoryStream -> RandomAccessStream -> BitmapImage
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }

    /// <summary>
    ///     尝试使用 Shell32 获取文件或文件夹的图标信息
    /// </summary>
    /// <para>警告：路径斜杠必须为"\"</para>
    /// <param name="path">完整路径</param>
    /// <param name="isFolder">是否为文件夹</param>
    /// <param name="iconInfo">接收图标信息的 <see cref="Shell32.SHFILEINFO" /></param>
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
