using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using ChatForgeUI.ViewModels;
using ChatForgeUI.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChatForgeUI;

public partial class App : Application
{
    public static IHost AppHost { get; private set; }
    public static IServiceProvider Services { get; private set; }
    
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {

                services.AddSingleton<HttpClient>();
                services.AddSingleton<SessionContainer>();
                services.AddTransient<Chatroom>();
                foreach (var type in GetTypes("ChatForgeUI.ViewModels",Assembly.GetExecutingAssembly()))
                {
                    //add each of the viewmodels as a singleton
                    Console.WriteLine(type);
                    services.AddSingleton(type);
                }
                foreach (var type in GetTypes("ChatForgeUI.Views",Assembly.GetExecutingAssembly()))
                {
                    //add each of the views as well
                    Console.WriteLine(type);
                    services.AddSingleton(type);
                }
                
                
            })
            .Build();

        Services = AppHost.Services;

    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownRequested += Shutdown;
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            var mainWindow = GetService<MainWindow>();
            mainWindow.DataContext = GetService<MainWindowViewModel>();
            desktop.MainWindow = mainWindow;
            GetService<MainWindowViewModel>().SetContent(GetService<ServerListViewModel>());
            desktop.MainWindow.Show();
        }
        base.OnFrameworkInitializationCompleted();
    }
    
    public void Shutdown(object? sender, ShutdownRequestedEventArgs e)
    {
        GetService<Dialog>().Close();
        AppHost.StopAsync();
        AppHost.Dispose();
        Environment.Exit(0);
    }
    
    private static IEnumerable<Type> GetTypes(string namespaceName, Assembly assembly)
    {

        var types = assembly.GetTypes()
            .Where(t => t.Namespace == namespaceName
                        && t.IsClass
                        && !t.IsNested
                        && !t.IsAbstract
            );

        return types;
        
    }

    public static T GetService<T>() 
    {
        return Services.GetRequiredService<T>();
    }

    public static void ShowDialog(ViewModelBase viewModel)
    {
        var dialog = GetService<Dialog>();
        dialog.Width = 400;
        dialog.Height = 300;
        dialog.Content = viewModel;
        dialog.Show(GetService<MainWindow>());
    }

}