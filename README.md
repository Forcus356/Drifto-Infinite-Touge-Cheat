# 🏎️ Drifto Patcher — coin editor & save manager for Drifto: Infinite Touge / патчер монет и менеджер сохранений

**English first · Русский во второй половине файла**

> ⚠️ **Disclaimer:** Fan-made, unofficial tool for **offline / single-player use only**. Not affiliated with or endorsed by UnluckyDuck. Do not use in multiplayer or leaderboard contexts. Modifying game files may violate the EULA — you are solely responsible. Support the devs if you enjoy the game!
>
> ⚠️ **Дисклеймер:** фанатский неофициальный инструмент **только для оффлайн / одиночного режима**. Не связан с UnluckyDuck. Не используйте в мультиплеере и таблицах лидеров. Изменение файлов игры может нарушать EULA — ответственность на вас. Поддержите разработчиков, если нравится игра!

## 📁 Repository layout / Структура репозитория
```
├── src/
│   ├── EN/   # English console version / англоязычная версия
│   └── RU/   # Russian console version / русскоязычная версия
├── LICENSE   # MIT
└── README.md
```
Prebuilt binaries / готовые exe: see **Releases** (`DriftoPatcher-EN.exe`, `DriftoPatcher-RU.exe`).

---

# 🇬 English

A tiny open-source utility that sets any coin balance in **Drifto: Infinite Touge**, manages save files, and cleanly reverts everything — no Cheat Engine, no hex editors.

## ✨ Features
- 💰 **Set any coin balance** — the game itself encrypts and writes it into your save.
- 🛠 **Restore honest code** — reverts patched methods to original IL; balance counts normally again.
- 💾 **Save backup / restore** — checkpoint progress and roll back anytime.
- 🧹 **Fresh start** — wipes save + backup and reverts the DLL in one confirmed step.
- 📦 **Single self-contained .exe** — no .NET, no runtime, no installation.

## ✅ Requirements
- Windows 10 (1607+) / Windows 11, x64 · A licensed Steam copy of the game. That's it.

## 🔧 How it works
The game stores currency in an XOR-obfuscated wrapper (`Drifto.Encryption.ObfuscatedInt`). The patcher loads `Drifto.Encryption.dll` with **Mono.Cecil** and rewrites the IL of two methods: `GetValue()` → returns your amount (displayed & spent), `GetObfuscatedValue()` → returns `Encrypt(amount)` (written to `saveDataV3.drifto`). After an in-game autosave the balance is persisted; option 2 then rebuilds the original method bodies, so the game runs 100% honestly again — with your new balance in the save.

## 📥 Usage
1. Download `DriftoPatcher-EN.exe` from Releases. 2. **Close the game.** 3. Run and choose:

| # | Action |
|---|--------|
| 1 | Add coins (patches DLL, auto-creates save backup on first run) |
| 2 | Restore honest methods (DLL behaves like stock) |
| 3 | Restore save from backup |
| 4 | Create / update save backup (checkpoint) |
| 5 | Fresh start (deletes save + backup, reverts DLL; type `YES` to confirm) |

## 🧱 Building from source
```bash
cd src/EN   # or src/RU
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 🩺 Troubleshooting
- **SmartScreen / AV warning** — unsigned exe: *More info → Run anyway*.
- **Access denied** — game in `Program Files`: run as Administrator.
- **DLL / save not found** — non-standard install: enter the path manually when asked.
- **Game updated** — the patcher re-reads the current DLL each run; if broken, use option 5 or Steam file verification.

---

# 🇷 Русский

Небольшая open-source утилита: задаёт любой баланс монет в **Drifto: Infinite Touge**, управляет сохранениями и чисто откатывает всё обратно — без Cheat Engine и hex-редакторов.

## ✨ Возможности
- 💰 **Любой баланс монет** — игра сама зашифрует и запишет его в сейв.
- 🛠 **Честный код обратно** — методы возвращаются к исходному IL, баланс снова считается нормально.
- 💾 **Бэкап / восстановление сейва** — чекпоинты и откат в любой момент.
- 🧹 **Полный сброс** — удаляет сейв и бэкап, возвращает DLL одним действием с подтверждением.
- 📦 **Один самодостаточный .exe** — не нужны .NET, рантаймы и установка.

## ✅ Требования
- Windows 10 (1607+) / Windows 11, x64 · Лицензионная копия игры в Steam. Всё.

## 🔧 Как это работает
Валюта хранится в XOR-обфусцированной обёртке (`Drifto.Encryption.ObfuscatedInt`). Патчер загружает `Drifto.Encryption.dll` через **Mono.Cecil** и переписывает IL двух методов: `GetValue()` → возвращает вашу сумму (показ и трата), `GetObfuscatedValue()` → возвращает `Encrypt(сумма)` (запись в `saveDataV3.drifto`). После автосохранения баланс закрепляется в сейве; пункт 2 пересобирает исходные тела методов — игра снова честная на 100%, но с вашим балансом в сохранении.

## 📥 Использование
1. Скачайте `DriftoPatcher-RU.exe` из Releases. 2. **Полностью закройте игру.** 3. Запустите и выберите:

| # | Действие |
|---|----------|
| 1 | Накрутить монеты (патчит DLL, сам создаёт бэкап сейва при первом запуске) |
| 2 | Восстановить честные методы (DLL как оригинальная) |
| 3 | Восстановить сейв из бэкапа |
| 4 | Создать / обновить бэкап сейва (чекпоинт) |
| 5 | Полный сброс (удаляет сейв и бэкап, возвращает DLL; подтверждение — `ДА` или `YES`) |

## 🧱 Сборка из исходников
```bash
cd src/RU   # или src/EN
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 🩺 Возможные проблемы
- **SmartScreen / антивирус** — exe не подписан: *«Подробнее → Всё равно запустить»*.
- **Отказано в доступе** — игра в `Program Files`: запуск от администратора.
- **DLL / сейв не найдены** — нестандартная установка: укажите путь вручную.
- **Игра обновилась** — патчер читает актуальную DLL каждый запуск; если сломалось — пункт 5 или проверка файлов Steam.

---

## 📄 License / Лицензия
[MIT](LICENSE) — use, modify, distribute freely (including commercially), keep the license text / используйте, изменяйте и распространяйте свободно (включая коммерчески), сохраняя текст лицензии.
