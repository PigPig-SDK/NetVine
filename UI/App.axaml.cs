using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Infrastructure;
using System;
using System.Linq;
using UI.ViewModels;
using UI.Views;


namespace UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            UpdateShutdownStatus();
            desktop.ShutdownRequested += OnShutdownRequested;
        }
        ConfigManager.OnSettingChanged += OnSettingChanged;
        base.OnFrameworkInitializationCompleted();
    }

    public void OnSettingChanged(Enum setting)
    {
        if (setting is SettingInt settingInt)
        {
            if (settingInt != SettingInt.MinimizeOnClose) return;
            UpdateShutdownStatus();
        }
    }

    void UpdateShutdownStatus()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ConfigManager.ReadSettingBool(SettingInt.MinimizeOnClose) ? Avalonia.Controls.ShutdownMode.OnExplicitShutdown : Avalonia.Controls.ShutdownMode.OnMainWindowClose;
        }
    }

    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        ConfigManager.TrySaveToFile();
    }
    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private void ShowWindow(object? sender, System.EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if(desktop.MainWindow is not null)
                desktop.MainWindow.Topmost = true;
            desktop.MainWindow?.Show();
            desktop.MainWindow?.Activate();
        }
    }

    private void QuitWindow(object? sender, System.EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}