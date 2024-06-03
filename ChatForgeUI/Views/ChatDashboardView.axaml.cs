using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ChatForgeUI.ViewModels;

namespace ChatForgeUI.Views;

public partial class ChatDashboardView : UserControl
{
    public ChatDashboardView()
    {
        InitializeComponent();
        ScrollToBottom();
        if (DataContext is ChatDashboardViewModel vm)
        {
            vm.NewMessage += ScrollToBottom;
        }
    }

    private void ScrollToBottom()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
            MessageViewer.ScrollToEnd(),
            DispatcherPriority.Background
            
        );
        Console.WriteLine("Hi");
    }
}