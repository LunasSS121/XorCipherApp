using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using XorEncryptor.Services;

namespace XorEncryptor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // ── UI-state properties ───────────────────────────────────────────────────

    [ObservableProperty]
    private bool isTextMode = true;

    [ObservableProperty]
    private bool isGuideOpen;

    [ObservableProperty]
    private bool showKey;

    [ObservableProperty]
    private bool isEncryptMode = true;

    [ObservableProperty]
    private string inputText = "Привет, Мир! Это секретное сообщение.";

    [ObservableProperty]
    private string outputText = string.Empty;

    [ObservableProperty]
    private string encryptionKey = "password";

    [ObservableProperty]
    private string selectedFileName = "Файл не выбран";

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private double progressValue;

    [ObservableProperty]
    private bool isBusy;

    // ── Derived booleans ──────────────────────────────────────────────────────

    public bool IsFileMode    => !IsTextMode;
    public bool IsDecryptMode => !IsEncryptMode;
    public bool IsKeyHidden   => !ShowKey;

    // [NotifyCanExecuteChangedFor(nameof(ProcessCommand))] is NOT used here
    // because the source-generator resolves attributes before generating
    // ProcessCommand from [RelayCommand], causing MVVMTK0016.
    // Instead we call NotifyCanExecuteChanged() manually in each partial method.

    partial void OnIsTextModeChanged(bool value)
    {
        OnPropertyChanged(nameof(IsFileMode));
        ProcessCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsEncryptModeChanged(bool value)
    {
        OnPropertyChanged(nameof(IsDecryptMode));
        ProcessCommand.NotifyCanExecuteChanged();
    }

    partial void OnShowKeyChanged(bool value)
        => OnPropertyChanged(nameof(IsKeyHidden));

    partial void OnInputTextChanged(string value)
        => ProcessCommand.NotifyCanExecuteChanged();

    partial void OnEncryptionKeyChanged(string value)
        => ProcessCommand.NotifyCanExecuteChanged();

    partial void OnIsBusyChanged(bool value)
        => ProcessCommand.NotifyCanExecuteChanged();

    // ── Constructor ───────────────────────────────────────────────────────────

    public MainWindowViewModel()
    {
        // Инициализация данных при запуске
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        // Задержка для инициализации UI перед выполнением шифрования
        await Task.Delay(500);

        // Выполняем шифрование примера данных
        if (!string.IsNullOrWhiteSpace(InputText) && !string.IsNullOrWhiteSpace(EncryptionKey))
        {
            await ProcessTextAsync(CancellationToken.None);
        }
    }

    // ── StorageProvider (injected from code-behind) ───────────────────────────

    private IStorageProvider? _storageProvider;
    public void SetStorageProvider(IStorageProvider sp) => _storageProvider = sp;

    // ── Internal file path ────────────────────────────────────────────────────

    private string? _selectedFilePath;

    public void UpdateSelectedFile(string? localPath, string displayName)
    {
        _selectedFilePath = localPath;
        SelectedFileName  = string.IsNullOrWhiteSpace(displayName) ? "Файл не выбран" : displayName;
        ProcessCommand.NotifyCanExecuteChanged();
    }

    // ── Tab / mode switching commands ─────────────────────────────────────────

    [RelayCommand]
    private void ShowTextMode() => IsTextMode = true;

    [RelayCommand]
    private void ShowFileMode() => IsTextMode = false;

    [RelayCommand]
    private void OpenGuide() => IsGuideOpen = true;

    [RelayCommand]
    private void CloseGuide() => IsGuideOpen = false;

    [RelayCommand]
    private void SelectEncryptMode() => IsEncryptMode = true;

    [RelayCommand]
    private void SelectDecryptMode() => IsEncryptMode = false;

    // ── Main processing command ───────────────────────────────────────────────

    private bool CanProcess()
        => !IsBusy
        && !string.IsNullOrWhiteSpace(EncryptionKey)
        && (IsFileMode || !string.IsNullOrWhiteSpace(InputText));

    // CommunityToolkit generates "ProcessCommand" from "ProcessAsync"
    // (strips the "Async" suffix). CancellationToken parameter is
    // supported natively — the toolkit passes it from AsyncRelayCommand.
    [RelayCommand(CanExecute = nameof(CanProcess))]
    private async Task ProcessAsync(CancellationToken ct)
    {
        if (IsTextMode)
            await ProcessTextAsync(ct);
        else
            await ProcessFileAsync(ct);
    }

    // ── Text processing ───────────────────────────────────────────────────────

    private async Task ProcessTextAsync(CancellationToken ct)
    {
        IsBusy        = true;
        StatusMessage = IsEncryptMode ? "Шифрование текста…" : "Дешифрование текста…";

        try
        {
            string result = await Task.Run(() =>
                IsEncryptMode
                    ? XorService.EncryptText(InputText, EncryptionKey)
                    : XorService.DecryptText(InputText, EncryptionKey), ct);

            OutputText    = result;
            StatusMessage = string.Empty;
        }
        catch (FormatException)
        {
            StatusMessage = "❌ Некорректный Base64 — для дешифровки вставьте зашифрованный текст.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "⚠️ Операция отменена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
            Debug.WriteLine($"[XorEncryptor] {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── File processing ───────────────────────────────────────────────────────

    private async Task ProcessFileAsync(CancellationToken ct)
    {
        if (_storageProvider is null)
        {
            StatusMessage = "❌ StorageProvider не установлен.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_selectedFilePath))
        {
            StatusMessage = "⚠️ Сначала выберите файл.";
            return;
        }

        var destFile = await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title             = "Сохранить обработанный файл",
            SuggestedFileName = Path.GetFileName(_selectedFilePath) + ".xor"
        });

        if (destFile is null)
            return;

        IsBusy        = true;
        ProgressValue = 0;
        StatusMessage = $"Обработка: {Path.GetFileName(_selectedFilePath)}…";

        try
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
            var    progress = new Progress<double>(p => ProgressValue = p * 100.0);
            string srcPath  = _selectedFilePath; // capture for lambda

            await Task.Run(async () =>
            {
                await using Stream inputStream  = File.OpenRead(srcPath);
                await using Stream outputStream = await destFile.OpenWriteAsync();
                await XorService.ProcessStreamAsync(inputStream, outputStream, keyBytes, progress, ct);
            }, ct);

            StatusMessage = $"✅ Готово: {destFile.Name}";
            ProgressValue = 100;
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "⚠️ Операция отменена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Ошибка: {ex.Message}";
            Debug.WriteLine($"[XorEncryptor] {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
