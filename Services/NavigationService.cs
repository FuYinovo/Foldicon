using System;
using Foldicon.Contracts;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Services;

public class NavigationService : INavigationService
{
    public Frame? Frame { get; set; }

    public void Initialize(Frame frame)
    {
        Frame = frame ?? throw new ArgumentNullException(nameof(frame));
    }

    public void Navigate(Type target, object? parameter)
    {
        if (Frame is null) throw new Exception("NavigationService 未初始化");
        Frame.Navigate(target, parameter);
    }
}