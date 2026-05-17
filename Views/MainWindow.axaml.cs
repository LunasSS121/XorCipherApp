using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using XorEncryptor.ViewModels;

namespace XorEncryptor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        // Create the VM here so StorageProvider is available immediately after
        // InitializeComponent() — before App.axaml.cs can set DataContext via
        // object-initializer (which runs AFTER the constructor body).
        DataContext = new MainWindowViewModel();
        InitializeComponent();

        // StorageProvider is window-bound; inject it now that the window exists.
        ((MainWindowViewModel)DataContext).SetStorageProvider(StorageProvider);
    }

    // ── File picker (triggered by "Или выберите его" button in XAML) ──────────

    private async void SelectFile_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title         = "Выберите файл для обработки",
            AllowMultiple = false
        });

        if (files is not { Count: > 0 })
            return;

        IStorageFile file = files[0];

        // TryGetLocalPath() returns null for sandboxed/virtual paths;
        // fall back to Name so the display always shows something.
        string? localPath   = file.TryGetLocalPath();
        string  displayName = file.Name;

        vm.UpdateSelectedFile(localPath, displayName);
    }

    // ── Copy output to clipboard ──────────────────────────────────────────────

    private async void CopyOutput_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        if (string.IsNullOrEmpty(vm.OutputText))
            return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
            return;

        try
        {
            await clipboard.SetTextAsync(vm.OutputText);
        }
        catch
        {
            // Clipboard operation failed, ignore
        }
    }
}
