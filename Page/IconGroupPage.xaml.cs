using Foldicon.Service;
using Microsoft.UI.Xaml;

namespace Foldicon.Page;

public sealed partial class IconGroupPage
{
    private readonly IconGroupService _viewModel = IconGroupService.Instance;

    public IconGroupPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 点击「移除图标组」按键
    /// </summary>
    private void RemoveGroup_Click(object sender, RoutedEventArgs e)
    {
        // TODO))
    }

    /// <summary>
    /// 点击「编辑图标组信息」按键
    /// </summary>
    private void EditGroupInfo_Click(object sender, RoutedEventArgs e)
    {
        // TODO))
    }

    /// <summary>
    /// 点击「导入图标」按键
    /// </summary>
    private void ImportIcon_Click(object sender, RoutedEventArgs e)
    {
        // TODO))
    }

    /// <summary>
    /// 点击「导入图标组」按钮
    /// </summary>
    private void ImportGroup_Click(object sender, RoutedEventArgs e)
    {
        // TODO))
    }

    /// <summary>
    /// 点击「创建图标组」按钮
    /// </summary>
    private void CreateGroup_Click(object sender, RoutedEventArgs e)
    {
        // TODO))
    }
}