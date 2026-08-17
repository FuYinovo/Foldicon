using Foldicon.Enums;

namespace Foldicon.Contracts;

public interface IUserDataService
{
    /// <summary>
    ///     返回指定<see cref="UserData"/>对应的文件(文件夹)路径，若不存在，则创建空的文件(文件夹)并返回路径
    /// </summary>
    string GetPath(UserData data);
}