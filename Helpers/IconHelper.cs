using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Foldicon.Contracts;
using Foldicon.Models.Icon;
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
    ///     设置文件夹图标
    /// </summary>
    /// <param name="folderPath">文件夹路径</param>
    /// <param name="dllPath">DLL 文件路径</param>
    /// <param name="dllIndex">图标在 DLL 文件的索引</param>
    public static void SetFolderIcon(string folderPath, string dllPath, int dllIndex)
    {
        var settings = new Shell32.SHFOLDERCUSTOMSETTINGS
        {
            dwMask = Shell32.FOLDERCUSTOMSETTINGSMASK.FCSM_ICONFILE,
            pszIconFile = new StrPtrAuto(dllPath),
            iIconIndex = dllIndex
        };
        Shell32.SHGetSetFolderCustomSettings(ref settings, folderPath, Shell32.FCS.FCS_FORCEWRITE);
    }

    /// <summary>
    ///     尝试获取文件图标
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <param name="size">图标尺寸</param>
    /// <param name="icon">图标（失败为null）</param>
    /// <returns>是否成功</returns>
    public static bool TryGetFileIcon(string path, Shell32.SHIL size, out FolderFileIcon icon)
    {
        if (!File.Exists(path) ||
            !TryShell32GetIcon(path, false, size, out var hIcon)
            || !TryCreateBitmapImage(hIcon, out var bitmapData))
        {
            icon = null!;
            return false;
        }

        icon = new FolderFileIcon(bitmapData, path);
        return true;
    }

    /// <summary>
    ///     尝试获取文件夹图标
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <param name="size">图标尺寸</param>
    /// <param name="icon">图标（失败为null）</param>
    /// <returns>是否成功</returns>
    public static bool TryGetFolderIcon(string path, Shell32.SHIL size, out IFolderIcon icon)
    {
        // 获取失败、文件夹不存在、创建位图失败
        if (!Directory.Exists(path) ||
            !TryShell32GetIcon(path, true, size, out var hIcon) ||
            !TryCreateBitmapImage(hIcon, out var bitmapData))
        {
            icon = null!;
            return false;
        }

        // 检查 desktop.ini 是否存在
        var desktopIni = Path.Combine(path, "desktop.ini");
        if (!File.Exists(desktopIni))
        {
            // 系统图标
            icon = new FolderSystemIcon(bitmapData);
        }
        else
        {
            // 获取图标路径
            var iniReader = new IniSharp(desktopIni, Encoding.GetEncoding("GBK")); // desktop.ini 可能不是 UTF-8 编码
            if (!TryGetIniValue(iniReader, ".ShellClassInfo", "IconResource", out var value))
            {
                // 读取 desktop.ini 失败
                icon = null!;
                return false;
            }

            var resource = value.Split(",");
            var iconPath = resource[0];
            // 检查图标拓展名
            if (Path.GetExtension(iconPath).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            {
                // Dll 图标
                var iconIndex = int.Parse(resource[1]);
                icon = new FolderDllIcon(bitmapData, iconPath, iconIndex);
            }
            else
            {
                // 文件图标
                icon = new FolderFileIcon(bitmapData, iconPath);
            }
        }

        return true;

        bool TryGetIniValue(IniSharp reader, string section, string key, out string value)
        {
            try
            {
                value = reader.GetValue(section, key);
                return true;
            }
            catch (Exception)
            {
                value = null!;
                return false;
            }
        }
    }

    /// <summary>
    ///     获取一个文件夹内所有文件的图标
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <param name="searchPattern">搜索模式(如"*.exe")</param>
    /// <param name="maxRecursiveDepth">最大递归深度</param>
    /// <param name="size">图标尺寸</param>
    public static List<FolderFileIcon> GetFilesIcon(string path, string searchPattern,
        uint maxRecursiveDepth, Shell32.SHIL size)
    {
        List<FolderFileIcon> icons = [];
        var files = GetFilesRecursive(path, searchPattern, maxRecursiveDepth);
        foreach (var file in files)
        {
            if (!TryGetFileIcon(file, size, out var icon)) continue;
            icons.Add(icon);
        }

        return icons;
    }

    /// <summary>
    ///     创建一个<see cref="BitmapImage"/>
    /// </summary>
    public static BitmapImage CreateBitmap(byte[] bitmapBytes)
    {
        var bitmap = new BitmapImage();
        bitmap.SetSource(new MemoryStream(bitmapBytes).AsRandomAccessStream());
        return bitmap;
    }
}

public static partial class IconHelper // 私有方法
{
    /// <summary>
    ///     尝试从<see cref="Shell32.SHFILEINFO"/>获取图标数据并创建<see cref="BitmapImage"/>
    /// </summary>
    /// <returns>是否成功</returns>
    private static bool TryCreateBitmapImage(User32.SafeHICON hIcon, out byte[] bitmapData)
    {
        if (hIcon.IsInvalid || hIcon.IsNull)
        {
            bitmapData = null!;
            return false;
        }

        // 使用 Icon.FromHandle，防止 Bitmap.FromHIcon 丢失透明度
        using var icon = Icon.FromHandle(hIcon.DangerousGetHandle()).ToBitmap();
        User32.DestroyIcon(hIcon);

        // Bitmap -> MemoryStream -> RandomAccessStream -> BitmapImage
        using var stream = new MemoryStream();
        icon.Save(stream, ImageFormat.Png);
        bitmapData = stream.ToArray();
        return true;
    }

    /// <summary>
    ///     尝试使用 Shell32 获取文件或文件夹的最大图标（256×256）
    /// </summary>
    /// <para>警告：路径斜杠必须为"\"</para>
    /// <param name="path">完整路径</param>
    /// <param name="isFolder">是否为文件夹</param>
    /// <param name="size">图标尺寸</param>
    /// <param name="hIcon">
    ///     图标的 <see cref="User32.SafeHICON" />，调用方负责用 <c>using</c> 释放
    /// </param>
    /// <exception cref="Exception">路径斜杠错误</exception>
    /// <returns>是否成功</returns>
    private static bool TryShell32GetIcon(string path, bool isFolder, Shell32.SHIL size, out User32.SafeHICON hIcon)
    {
        if (path.Contains('/')) throw new Exception("路径斜杠必须为Windows的'\\'分隔符");

        hIcon = null!;
        var info = new Shell32.SHFILEINFO();

        // 获取图标在系统共享图标列表的 index
        var _1 = Shell32.SHGetFileInfo(
            path,
            isFolder ? FileAttributes.Directory : FileAttributes.Normal,
            ref info,
            Marshal.SizeOf(info),
            Shell32.SHGFI.SHGFI_ICON | Shell32.SHGFI.SHGFI_SYSICONINDEX
        );
        if (_1 == IntPtr.Zero) return false;
        User32.DestroyIcon(info.hIcon); // 不需要小图标
        var imageIndex = info.iIcon; // index

        // 获取系统共享图标列表
        var _2 = Shell32.SHGetImageList(
            size, // 指定图标尺寸
            typeof(ComCtl32.IImageList).GUID,
            out var imageListOut
        );
        if (!_2.Succeeded || imageListOut is not ComCtl32.IImageList imageList) return false; // object => IImageList
        try
        {
            // 通过 index 在系统共享图标列表获取 hIcon
            hIcon = imageList.GetIcon(imageIndex, ComCtl32.IMAGELISTDRAWFLAGS.ILD_TRANSPARENT);
            return hIcon is { IsNull: false, IsInvalid: false };
        }
        catch
        {
            return false;
        }
        finally
        {
            Marshal.ReleaseComObject(imageList); // 释放系统共享图标列表的引用
        }
    }


    /// <summary>
    /// 递归获取文件夹中所有文件
    /// </summary>
    /// <param name="path">起始文件夹</param>
    /// <param name="searchPattern">例如「*.exe」只返回exe文件</param>
    /// <param name="maxDepth">最大深度，最小值是1</param>
    private static List<string> GetFilesRecursive(string path, string searchPattern, uint maxDepth)
    {
        if (maxDepth == 0) return [];

        var result = new List<string>();
        GetFiles(path, 1, ref result);
        return result;

        void GetFiles(string currentPath, uint currentDepth, ref List<string> files)
        {
            if (currentDepth > maxDepth) return;
            files.AddRange(SafeDirectoryOperation(() => Directory.GetFiles(currentPath, searchPattern)));
            foreach (var nextPath in SafeDirectoryOperation(() => Directory.GetDirectories(currentPath)))
            {
                GetFiles(nextPath, currentDepth + 1, ref files);
            }
        }

        string[] SafeDirectoryOperation(Func<string[]> operation)
        {
            try
            {
                return operation();
            }
            catch (UnauthorizedAccessException)
            {
                return [];
            }
        }
    }
}