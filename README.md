# 🛰️ Space Flight: Орбитальный симулятор Земля-Луна-Спутник

[![C#](https://img.shields.io/badge/C%23-.NET%2010-512BD4)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/UI-WPF%20%2B%20HelixToolkit-007ACC)](https://helix-toolkit.org/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)
[![Release](https://img.shields.io/github/v/release/verdenkaa/space_flight)](https://github.com/verdenkaa/space_flight/releases)

## ✨ Особенности

- **Физическая модель**: Расчёт гравитационных сил по закону Ньютона для трёх тел
- **Численное интегрирование**: Метод Эйлера с обработкой сингулярности при r → 0
- **Валидация**: Мониторинг сохранения полной механической энергии (E_total ≈ const)
- **Интерактивность**: Свободная навигация камеры, отслеживание объектов, параметризация начальных условий
- **Визуализация**: 3D-графика через HelixToolkit + динамические графики через WinForms Chart

> Интерактивный 3D-симулятор орбитальной механики с физической валидацией. Моделирует движение спутника в системе Земля-Луна с мониторингом сохранения полной механической энергии.

## 🎬 Демонстрация
<img width="1150" height="669" alt="image" src="https://github.com/user-attachments/assets/02d8ff0a-6f86-4e94-acdb-564efae565fb" />

<img width="1147" height="667" alt="image" src="https://github.com/user-attachments/assets/b1c53450-c937-4d77-850b-3059b969fe53" />

<img width="1152" height="669" alt="image" src="https://github.com/user-attachments/assets/66a28692-6cd1-4ed3-86dc-24024e7cab7a" />

## 🚀 Быстрый старт
[Релиз v1.0 (exe + зависимости)](https://github.com/verdenkaa/space_flight/releases/download/release/Release.zip)

### Требования
- .NET 10 SDK или выше
- Windows 10/11 (WPF)

### Сборка и запуск
```bash
# Клонируйте репозиторий
git clone https://github.com/verdenkaa/space_flight.git
cd space_flight

# Откройте решение в Visual Studio
Space Flight Code/Space_Flight_Code.sln

# Или соберите через CLI
dotnet build
dotnet run --project "Space Flight Code/Space_Flight_Code.csproj"
```


