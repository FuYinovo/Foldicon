using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
    /// <param name="icon">图标（失败为null）</param>
    /// <returns>是否成功</returns>
    public static bool TryGetFileIcon(string path, out FolderFileIcon icon)
    {
        if (!File.Exists(path) ||
            !TryShell32GetIcon(path, false, out var info)
            || !TryCreateBitmapImage(info, out var bitmap))
        {
            icon = null!;
            return false;
        }

        icon = new FolderFileIcon(bitmap, path);
        return true;
    }

    /// <summary>
    ///     尝试获取文件夹图标
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <param name="icon">图标（失败为null）</param>
    /// <returns>是否成功</returns>
    public static bool TryGetFolderIcon(string path, out IFolderIcon icon)
    {
        // 获取失败、文件夹不存在、创建位图失败
        if (!Directory.Exists(path) ||
            !TryShell32GetIcon(path, true, out var info) ||
            !TryCreateBitmapImage(info, out var bitmap))
        {
            icon = null!;
            return false;
        }

        // 检查 desktop.ini 是否存在
        var desktopIni = Path.Combine(path, "desktop.ini");
        if (!File.Exists(desktopIni))
        {
            // 系统图标
            icon = new FolderSystemIcon(bitmap);
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
                icon = new FolderDllIcon(bitmap, iconPath, iconIndex);
            }
            else
            {
                // 文件图标
                icon = new FolderFileIcon(bitmap, iconPath);
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
    ///     尝试一个文件夹内所有exe程序的图标
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <param name="maxRecursiveDepth">最大递归深度</param>
    public static async Task<List<FolderFileIcon>> GetExeIconsAsync(string path, uint maxRecursiveDepth)
    {
        List<FolderFileIcon> icons = [];
        var files = GetFilesRecursive(path, "*.exe", maxRecursiveDepth);
        foreach (var file in files)
        {
            if (!TryGetFileIcon(file, out var icon)) continue;
            icons.Add(icon);
        }

        return icons;
    }
}

public static partial class IconHelper // 私有方法
{
    /// <summary>
    ///     尝试从<see cref="Shell32.SHFILEINFO"/>获取图标数据并创建<see cref="BitmapImage"/>
    /// </summary>
    /// <returns>是否成功</returns>
    private static bool TryCreateBitmapImage(Shell32.SHFILEINFO info, out BitmapImage bitmap)
    {
        if (info.hIcon.IsInvalid || info.hIcon.IsNull)
        {
            bitmap = null!;
            return false;
        }

        // 使用 Icon.FromHandle，防止 Bitmap.FromHIcon 丢失透明度
        using var icon = Icon.FromHandle(info.hIcon.DangerousGetHandle()).ToBitmap();
        User32.DestroyIcon(info.hIcon);

        // Bitmap -> MemoryStream -> RandomAccessStream -> BitmapImage
        using var stream = new MemoryStream();
        icon.Save(stream, ImageFormat.Png);
        bitmap = new BitmapImage();
        bitmap.SetSource(new MemoryStream(stream.ToArray()).AsRandomAccessStream());
        return true;
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