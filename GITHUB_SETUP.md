# 🚀 Инструкция по выложению на GitHub

## Шаг 1: Создай репозиторий на GitHub

1. Перейди на https://github.com/new
2. Введи имя: `xorencryptor` (или другое по желанию)
3. Выбери "Public" если хочешь открыть код, или "Private" для закрытого доступа
4. Нажми "Create repository"

## Шаг 2: Добавь remote и отправь код

Скопируй и выполни эти команды в PowerShell:

```powershell
cd "c:\хуета финал\XorEncryptor"

# Замени USERNAME и REPO на свои значения
git remote add origin https://github.com/USERNAME/REPO.git

# Переименуй ветку на main (если требуется GitHub)
git branch -M main

# Отправь код на GitHub
git push -u origin main
```

## Что было сделано локально:

### ✅ Инициализирован Git репозиторий
```
Initialized empty Git repository
```

### ✅ Создана структура проекта:

**Commit 1 - Initial commit (724245f)**
- Добавлены все файлы проекта
- 20 файлов, 2024 строки кода
- GUI приложение (Avalonia)
- Console приложение
- MVVM архитектура
- XorService для шифрования

**Commit 2 - Version update (f1d9cfc)**
- Добавлена версия 1.0.0
- Метаданные проектов
- Информация об авторах

### 📁 Созданные файлы:
- `.gitignore` - Исключение файлов из git
- `CHANGELOG.md` - История изменений версии

### 📊 Git статус:

```
HEAD -> master (2 commits)
```

Используй команды выше с заменой USERNAME и REPO на свои учётные данные GitHub!

Подробнее: https://docs.github.com/en/repositories/creating-and-managing-repositories
