using System;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Foldicon.Behavior;

public class DropFileBehavior : Behavior<FrameworkElement>
{
    public event Action<object, DragEventArgs>? FileDropped;

    protected override void OnAttached()
    {
        AssociatedObject.AllowDrop = true;
        AssociatedObject.DragOver += DragOverHandler;
        AssociatedObject.Drop += DropHandler;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.AllowDrop = false;
        AssociatedObject.DragOver -= DragOverHandler;
        AssociatedObject.Drop -= DropHandler;
    }


    private void DropHandler(object sender, DragEventArgs e)
    {
        FileDropped?.Invoke(AssociatedObject, e);
    }

    private static void DragOverHandler(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }
}