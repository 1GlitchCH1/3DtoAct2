# ThrDtoActTwo - Inscryption Mod

Мод для Inscryption, который заменяет 2D локации второго акта на 3D окружение.

## Возможности

- Карта мира Act 2 остается 2D
- При входе в локацию с карты открывается 3D окружение
- Полная замена визуального представления локаций

## Требования для сборки

1. .NET SDK (для dotnet build)
2. Файл `Assembly-CSharp.dll` из игры Inscryption нужно поместить в папку `lib/`

## Сборка

```bash
dotnet build -c Release
```

Собранный мод будет в `bin/Release/netstandard2.0/ThrDtoActTwo.dll`

## Установка

1. Установите BepInEx для Inscryption
2. Скопируйте `ThrDtoActTwo.dll` в папку `BepInEx/plugins/`
3. Запустите игру

## Структура проекта

- `Plugin.cs` - Основной класс плагина BepInEx
- `Act2LocationPatch.cs` - Harmony патчи для перехвата входов в локации
- `ThreeDEnvironmentManager.cs` - Менеджер создания 3D окружения

## Примечания

Перед сборкой убедитесь, что в папке `lib/` находится `Assembly-CSharp.dll` из игры.
