using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XorEncryptor.Services;

namespace XorEncryptor;

/// <summary>
/// Console entry point for XOR Encryptor (no GUI dependencies)
/// </summary>
public static class ConsoleProgram
{
    // Box drawing characters for Unicode-enabled consoles
    private static class BoxUnicode
    {
        public const string TopLeft = "╔";
        public const string TopRight = "╗";
        public const string BottomLeft = "╚";
        public const string BottomRight = "╝";
        public const string Horizontal = "═";
        public const string Vertical = "║";
        public const string Cross = "╬";
    }

    // ASCII alternatives for Windows console
    private static class BoxAscii
    {
        public const string TopLeft = "+";
        public const string TopRight = "+";
        public const string BottomLeft = "+";
        public const string BottomRight = "+";
        public const string Horizontal = "-";
        public const string Vertical = "|";
        public const string Cross = "+";
    }

    private static bool UseUnicode { get; set; }

    public static async Task Main(string[] args)
    {
        try
        {
            // Try to enable UTF8 encoding
            Console.OutputEncoding = Encoding.UTF8;
            UseUnicode = true;
        }
        catch
        {
            // Fall back to ASCII if UTF8 fails
            UseUnicode = false;
        }

        await RunConsoleInteractive();
    }

    private static string Box(string unicode, string ascii) => UseUnicode ? unicode : ascii;

    private static async Task RunConsoleInteractive()
    {
        while (true)
        {
            Console.Clear();
            string topLine = $"{Box(BoxUnicode.TopLeft, BoxAscii.TopLeft)}{new string(char.Parse(Box(BoxUnicode.Horizontal, BoxAscii.Horizontal)), 38)}{Box(BoxUnicode.TopRight, BoxAscii.TopRight)}";
            string midLine = $"{Box(BoxUnicode.Vertical, BoxAscii.Vertical)}     XOR Encryptor - Консольная версия   {Box(BoxUnicode.Vertical, BoxAscii.Vertical)}";
            string botLine = $"{Box(BoxUnicode.BottomLeft, BoxAscii.BottomLeft)}{new string(char.Parse(Box(BoxUnicode.Horizontal, BoxAscii.Horizontal)), 38)}{Box(BoxUnicode.BottomRight, BoxAscii.BottomRight)}";
            
            Console.WriteLine(topLine);
            Console.WriteLine(midLine);
            Console.WriteLine(botLine);
            Console.WriteLine();

            Console.WriteLine("Выберите режим:");
            Console.WriteLine("  1. Текстовое шифрование");
            Console.WriteLine("  2. Шифрование файла");
            Console.WriteLine("  3. Выход");
            Console.Write("\nВыбор (1-3): ");

            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine("❌ Некорректный ввод!");
                await Task.Delay(1500);
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 1:
                        await HandleTextMode();
                        break;
                    case 2:
                        await HandleFileMode();
                        break;
                    case 3:
                        Console.WriteLine("\nДо свидания!");
                        return;
                    default:
                        Console.WriteLine("❌ Неизвестный выбор!");
                        await Task.Delay(1500);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Ошибка: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }
    }

    private static async Task HandleTextMode()
    {
        Console.Clear();
        string topLine = $"{Box(BoxUnicode.TopLeft, BoxAscii.TopLeft)}{new string(char.Parse(Box(BoxUnicode.Horizontal, BoxAscii.Horizontal)), 38)}{Box(BoxUnicode.TopRight, BoxAscii.TopRight)}";
        string midLine = $"{Box(BoxUnicode.Vertical, BoxAscii.Vertical)}        ТЕКСТОВОЕ ШИФРОВАНИЕ            {Box(BoxUnicode.Vertical, BoxAscii.Vertical)}";
        string botLine = $"{Box(BoxUnicode.BottomLeft, BoxAscii.BottomLeft)}{new string(char.Parse(Box(BoxUnicode.Horizontal, BoxAscii.Horizontal)), 38)}{Box(BoxUnicode.BottomRight, BoxAscii.BottomRight)}";
        
        Console.WriteLine(topLine);
        Console.WriteLine(midLine);
        Console.WriteLine(botLine);
        Console.WriteLine();

        Console.WriteLine("Выберите операцию:");
        Console.WriteLine("  1. Зашифровать");
        Console.WriteLine("  2. Расшифровать");
        Console.Write("\nВыбор (1-2): ");

        if (!int.TryParse(Console.ReadLine(), out int mode))
        {
            Console.WriteLine("❌ Некорректный ввод!");
            await Task.Delay(1500);
            return;
        }

        if (mode < 1 || mode > 2)
        {
            Console.WriteLine("❌ Неизвестный выбор!");
            await Task.Delay(1500);
            return;
        }

        bool isEncrypting = mode == 1;
        string operationName = isEncrypting ? "зашифровать" : "расшифровать";

        Console.Write($"\nВведите текст для {operationName}: ");
        string text = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(text))
        {
            Console.WriteLine("❌ Текст не может быть пустым!");
            await Task.Delay(1500);
            return;
        }

        Console.Write("Введите ключ: ");
        string key = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(key))
        {
            Console.WriteLine("❌ Ключ не может быть пустым!");
            await Task.Delay(1500);
            return;
        }

        try
        {
            string result = isEncrypting
                ? XorService.EncryptText(text, key)
                : XorService.DecryptText(text, key);

            Console.WriteLine("\n✅ Успешно!");
            Console.WriteLine(UseUnicode ? new string('─', 40) : new string('-', 40));
            Console.WriteLine($"Результат:\n{result}");
            Console.WriteLine(UseUnicode ? new string('─', 40) : new string('-', 40));

            Console.WriteLine("\nХотите скопировать результат в буфер обмена? (y/n): ");
            if (Console.ReadLine()?.ToLower() == "y")
            {
                try
                {
                    // Для Windows используем команду clip
                    if (OperatingSystem.IsWindows())
                    {
                        var process = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "clip",
                                UseShellExecute = false,
                                RedirectStandardInput = true,
                                CreateNoWindow = true
                            }
                        };
                        process.Start();
                        await process.StandardInput.WriteLineAsync(result);
                        process.StandardInput.Close();
                        process.WaitForExit();
                        Console.WriteLine("✅ Скопировано в буфер обмена!");
                    }
                }
                catch
                {
                    Console.WriteLine("⚠️  Не удалось скопировать в буфер обмена");
                }
            }
        }
        catch (FormatException)
        {
            Console.WriteLine("❌ Ошибка: Некорректный формат Base64 для расшифровки!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка: {ex.Message}");
        }

        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    private static async Task HandleFileMode()
    {
        Console.Clear();
        string topLine = $"{Box(BoxUnicode.TopLeft, BoxAscii.TopLeft)}{new string(char.Parse(Box(BoxUnicode.Horizontal, BoxAscii.Horizontal)), 38)}{Box(BoxUnicode.TopRight, BoxAscii.TopRight)}";
        string midLine = $"{Box(BoxUnicode.Vertical, BoxAscii.Vertical)}        ШИФРОВАНИЕ ФАЙЛА                {Box(BoxUnicode.Vertical, BoxAscii.Vertical)}";
        string botLine = $"{Box(BoxUnicode.BottomLeft, BoxAscii.BottomLeft)}{new string(char.Parse(Box(BoxUnicode.Horizontal, BoxAscii.Horizontal)), 38)}{Box(BoxUnicode.BottomRight, BoxAscii.BottomRight)}";
        
        Console.WriteLine(topLine);
        Console.WriteLine(midLine);
        Console.WriteLine(botLine);
        Console.WriteLine();

        Console.Write("Введите полный путь к исходному файлу: ");
        string inputPath = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(inputPath))
        {
            Console.WriteLine("❌ Путь не может быть пустым!");
            await Task.Delay(1500);
            return;
        }

        if (!File.Exists(inputPath))
        {
            Console.WriteLine("❌ Файл не найден!");
            await Task.Delay(1500);
            return;
        }

        Console.Write("Введите путь для сохранения результата: ");
        string outputPath = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            Console.WriteLine("❌ Путь не может быть пустым!");
            await Task.Delay(1500);
            return;
        }

        Console.Write("Введите ключ: ");
        string key = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(key))
        {
            Console.WriteLine("❌ Ключ не может быть пустым!");
            await Task.Delay(1500);
            return;
        }

        try
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            var progress = new Progress<double>(p =>
            {
                int percent = (int)(p * 100);
                Console.Write($"\rПрогресс: {percent}% ");
                DrawProgressBar(percent);
            });

            using (var inputStream = File.OpenRead(inputPath))
            using (var outputStream = File.Create(outputPath))
            {
                await XorService.ProcessStreamAsync(inputStream, outputStream, keyBytes, progress);
            }

            Console.WriteLine("\n\n✅ Файл успешно обработан!");
            Console.WriteLine($"📁 Результат сохранён: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Ошибка обработки файла: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    private static void DrawProgressBar(int percent, int width = 20)
    {
        Console.Write("[");
        int filled = (percent * width) / 100;
        string filledChar = UseUnicode ? "█" : "#";
        string emptyChar = UseUnicode ? "░" : "-";
        for (int i = 0; i < width; i++)
        {
            Console.Write(i < filled ? filledChar : emptyChar);
        }
        Console.Write("]");
    }
}
