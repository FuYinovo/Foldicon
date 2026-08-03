using System;
using Microsoft.UI.Xaml.Controls;

namespace Foldicon.Contracts;

public interface INavigationService
{
    Frame? Frame { get; set; }
    void Initialize(Frame frame);
    void Navigate(Type target, object? parameter = null);
}