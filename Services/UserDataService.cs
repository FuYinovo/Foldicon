using System;
using System.IO;
using Foldicon.Contracts;
using Foldicon.Enums;
using Windows.Storage;

namespace Foldicon.Services;

public class UserDataService : IUserDataService
{
    // 沙箱的 RoamingState 文件夹
    private static readonly string RootPath = Path.Combine(ApplicationData.Current.RoamingFolder.Path);

    public UserDataService()
    {
        if (!Directory.Exists(RootPath))
            Directory.CreateDirectory(RootPath);
    }


    public string GetPath(UserData data)
    {
        return data switch
        {
            UserData.SettingsFile => ReturnPathSafely(Path.Combine(RootPath, "settings.json")),
            UserData.IconGroupsFolder => ReturnPathSafely(Path.Combine(RootPath, "IconGroups")),
            _ => throw new ArgumentOutOfRangeException(nameof(data), data, null)
        };
    }

    /// <summary>
    ///     返回指定路径，若路径不存在则创建空文件/文件夹
    /// </summary>
    private static string ReturnPathSafely(string path)
    {
        var isDirectory = string.IsNullOrWhiteSpace(Path.GetExtension(path));

        // Existence Check
        switch (isDirectory)
        {
            // Directory
            case true when !Directory.Exists(path):
                Directory.CreateDirectory(path);
                break;
            // File
            case false when !File.Exists(path):
            {
                var parentFolder = Path.GetDirectoryName(path);
                if (!Directory.Exists(parentFolder))
                    Directory.CreateDirectory(parentFolder);
                File.Create(path).Close();
                break;
            }
        }

        return path;
    }
}