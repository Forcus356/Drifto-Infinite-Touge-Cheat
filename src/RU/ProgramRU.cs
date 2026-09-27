using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DriftoMoneyPatcher
{
    class Program
    {
        static string[] defaultPaths = {
            @"E:\SteamLibrary\steamapps\common\Drifto\Drifto_Data\Managed\Drifto.Encryption.dll",
            @"C:\Program Files (x86)\Steam\steamapps\common\Drifto\Drifto_Data\Managed\Drifto.Encryption.dll",
            @"C:\Program Files\Steam\steamapps\common\Drifto\Drifto_Data\Managed\Drifto.Encryption.dll",
            @"D:\SteamLibrary\steamapps\common\Drifto\Drifto_Data\Managed\Drifto.Encryption.dll"
        };

        static string savePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "AppData", "LocalLow", "UnluckyDuck", "Drifto", "saveDataV3.drifto");

        static string saveBackupPath = savePath + ".bak";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Drifto: Infinite Touge Cheat ===");
            Console.WriteLine(" УБЕДИТЕСЬ, ЧТО ИГРА ПОЛНОСТЬЮ ЗАКРЫТА!\n");

            string dllPath = null;
            foreach (var p in defaultPaths)
                if (File.Exists(p)) { dllPath = p; break; }

            if (dllPath == null)
            {
                Console.Write("Не удалось найти DLL автоматически. Введите путь вручную:\n> ");
                dllPath = Console.ReadLine()?.Trim('"');
                if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
                {
                    Console.WriteLine("Файл не найден.");
                    Console.ReadKey();
                    return;
                }
            }

            string dllBackupPath = dllPath + ".bak";

            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1. Накрутить деньги (патчит DLL + создаёт бэкап сейва)");
            Console.WriteLine("2. Восстановить честные методы (вернуть код DLL)");
            Console.WriteLine("3. Восстановить СЕЙВ из резервной копии");
            Console.WriteLine("4. Создать / обновить бэкап сейва (чекпоинт)");
            Console.WriteLine("5. НАЧАТЬ С НУЛЯ (удалить сейвы + вернуть честный код)");
            Console.Write("> ");
            string choice = Console.ReadLine();

            
            if (choice == "3")
            {
                if (!File.Exists(saveBackupPath))
                {
                    Console.WriteLine("\n[ℹ️ ИНФО] Бэкапа сейва нет (.bak не найден). Восстанавливать нечего.");
                    Console.ReadKey();
                    return;
                }
                try
                {
                    File.Copy(saveBackupPath, savePath, true);
                    Console.WriteLine("\n[✅ УСПЕХ] Сейв восстановлен из резервной копии!");
                }
                catch (IOException) { Console.WriteLine("\n[❌ ОШИБКА] Файл сейва занят. Закройте игру."); }
                catch (UnauthorizedAccessException) { Console.WriteLine("\n[❌ ОШИБКА] Отказано в доступе. Закройте игру."); }
                catch (Exception ex) { Console.WriteLine($"\n[❌ ОШИБКА] {ex.Message}"); }
                Console.ReadKey();
                return;
            }

            
            if (choice == "4")
            {
                if (!File.Exists(savePath))
                {
                    Console.WriteLine("\n[ℹ️ ИНФО] Файл сейва не найден — вы ещё не играли?");
                    Console.ReadKey();
                    return;
                }
                try
                {
                    File.Copy(savePath, saveBackupPath, true);
                    Console.WriteLine("\n[✅ УСПЕХ] Бэкап сейва создан / обновлён.");
                }
                catch (Exception ex) { Console.WriteLine($"\n[❌ ОШИБКА] {ex.Message}"); }
                Console.ReadKey();
                return;
            }

            
            if (choice == "5")
            {
                Console.WriteLine();
                Console.WriteLine("⚠️⚠️⚠️ ВНИМАНИЕ! ⚠️⚠️⚠️");
                Console.WriteLine("Вы уверены, что хотите полностью удалить ваше сохранение?");
                Console.WriteLine("Это полностью удалит ВСЕ ваши сохранения и бекапы:");
                Console.WriteLine($"  - {savePath}");
                Console.WriteLine($"  - {saveBackupPath}");
                Console.WriteLine("А также вернёт DLL к честному коду (стандартный счётчик).");
                Console.WriteLine("Отменить это действие будет НЕВОЗМОЖНО!");
                Console.Write("\nВведите ДА для подтверждения: ");
                string confirm = Console.ReadLine()?.Trim().ToUpper();

                if (confirm != "ДА" && confirm != "YES" && confirm != "Y")
                {
                    Console.WriteLine("\n[ℹ️ ИНФО] Отменено. Ничего не удалено.");
                    Console.ReadKey();
                    return;
                }

                try
                {
                    if (File.Exists(savePath)) { File.Delete(savePath); Console.WriteLine("[🗑 УДАЛЕНО] saveDataV3.drifto"); }
                    else Console.WriteLine("[ℹ️ ИНФО] saveDataV3.drifto не найден.");

                    if (File.Exists(saveBackupPath)) { File.Delete(saveBackupPath); Console.WriteLine("[🗑 УДАЛЕНО] saveDataV3.drifto.bak"); }
                    else Console.WriteLine("[ℹ️ ИНФО] saveDataV3.drifto.bak не найден.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[❌ ОШИБКА] Не удалось удалить сейвы: {ex.Message}");
                    Console.ReadKey();
                    return;
                }

                
                try
                {
                    EnsureDllBackup(dllPath, dllBackupPath);
                    if (RestoreHonestMethods(dllPath))
                        Console.WriteLine("[🔧 ГОТОВО] DLL возвращена к честному коду (стандартный счётчик).");
                    else
                        Console.WriteLine("[⚠️ ВНИМАНИЕ] Не удалось восстановить методы DLL (структура изменилась?).");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[❌ ОШИБКА] При восстановлении DLL: {ex.Message}");
                }

                Console.WriteLine("\n[🎉 ГОТОВО] При следующем запуске игра начнёт всё с нуля.");
                Console.ReadKey();
                return;
            }

            if (choice != "1" && choice != "2")
            {
                Console.WriteLine("Неверный выбор.");
                Console.ReadKey();
                return;
            }

            
            EnsureDllBackup(dllPath, dllBackupPath);

            if (choice == "1")
            {
                Console.Write("\nКакую сумму установить? (по умолчанию 1000000): ");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) input = "1000000";
                if (!int.TryParse(input, out int val) || val < 0)
                {
                    Console.WriteLine("Неверная сумма.");
                    Console.ReadKey();
                    return;
                }

                if (File.Exists(savePath) && !File.Exists(saveBackupPath))
                {
                    try
                    {
                        File.Copy(savePath, saveBackupPath, false);
                        Console.WriteLine("[💾 ИНФО] Создан бэкап сейва (ваш 'чистый' прогресс сохранён).");
                    }
                    catch { Console.WriteLine("[⚠️ ВНИМАНИЕ] Не удалось создать бэкап сейва."); }
                }

                try
                {
                    if (PatchMoney(dllPath, val))
                    {
                        Console.WriteLine($"\n[🎉 ГОТОВО] Установлено {val} денег.");
                        Console.WriteLine("[ℹ️ ИНФО] Запусти игру, дождись автосохранения, закрой игру.");
                        Console.WriteLine("[ℹ️ ИНФО] Затем выбери опцию 2, чтобы вернуть честный код.");
                    }
                    else Console.WriteLine("\n[❌ ОШИБКА] Не найдены нужные классы/методы в DLL.");
                }
                catch (Exception ex) { Console.WriteLine($"\n[❌ ОШИБКА] {ex.Message}"); }
            }
            else 
            {
                try
                {
                    if (RestoreHonestMethods(dllPath))
                        Console.WriteLine("\n[🎉 ГОТОВО] Оригинальные методы восстановлены!");
                    else Console.WriteLine("\n[❌ ОШИБКА] Не найдены нужные классы/методы в DLL.");
                }
                catch (Exception ex) { Console.WriteLine($"\n[❌ ОШИБКА] {ex.Message}"); }
            }

            Console.ReadKey();
        }

        static void EnsureDllBackup(string dllPath, string dllBackupPath)
        {
            if (!File.Exists(dllBackupPath))
            {
                try { File.Copy(dllPath, dllBackupPath, false); } catch { }
            }
        }

        static DefaultAssemblyResolver MakeResolver(string dllPath)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(dllPath)));
            resolver.AddSearchDirectory(AppDomain.CurrentDomain.BaseDirectory);
            return resolver;
        }

        static bool PatchMoney(string dllPath, int val)
        {
            byte[] originalBytes = File.ReadAllBytes(dllPath);
            using (var inStream = new MemoryStream(originalBytes))
            using (var assembly = AssemblyDefinition.ReadAssembly(inStream, new ReaderParameters
            {
                ReadingMode = ReadingMode.Immediate,
                AssemblyResolver = MakeResolver(dllPath)
            }))
            {
                var type = assembly.MainModule.GetType("Drifto.Encryption.ObfuscatedInt");
                if (type == null) return false;
                var encryptionType = assembly.MainModule.GetType("Drifto.Encryption.XOREncryptorDecryptor");
                var encryptMethod = encryptionType?.Methods.FirstOrDefault(m => m.Name == "IntEncryptDecrypt" && m.Parameters.Count == 1);
                if (encryptMethod == null) return false;

                var getValue = type.Methods.FirstOrDefault(m => m.Name == "GetValue");
                var getObfuscatedValue = type.Methods.FirstOrDefault(m => m.Name == "GetObfuscatedValue");

                if (getValue != null && getValue.HasBody)
                {
                    getValue.Body.Instructions.Clear();
                    var il = getValue.Body.GetILProcessor();
                    il.Append(il.Create(OpCodes.Ldc_I4, val));
                    il.Append(il.Create(OpCodes.Ret));
                }
                if (getObfuscatedValue != null && getObfuscatedValue.HasBody)
                {
                    getObfuscatedValue.Body.Instructions.Clear();
                    var il = getObfuscatedValue.Body.GetILProcessor();
                    il.Append(il.Create(OpCodes.Ldc_I4, val));
                    il.Append(il.Create(OpCodes.Call, encryptMethod));
                    il.Append(il.Create(OpCodes.Ret));
                }

                using (var outStream = new MemoryStream())
                {
                    assembly.Write(outStream);
                    File.WriteAllBytes(dllPath, outStream.ToArray());
                }
                return true;
            }
        }

        static bool RestoreHonestMethods(string dllPath)
        {
            byte[] originalBytes = File.ReadAllBytes(dllPath);
            using (var inStream = new MemoryStream(originalBytes))
            using (var assembly = AssemblyDefinition.ReadAssembly(inStream, new ReaderParameters
            {
                ReadingMode = ReadingMode.Immediate,
                AssemblyResolver = MakeResolver(dllPath)
            }))
            {
                var type = assembly.MainModule.GetType("Drifto.Encryption.ObfuscatedInt");
                if (type == null) return false;
                var field_value = type.Fields.FirstOrDefault(f => f.Name == "_value");
                var encryptionType = assembly.MainModule.GetType("Drifto.Encryption.XOREncryptorDecryptor");
                var encryptMethod = encryptionType?.Methods.FirstOrDefault(m => m.Name == "IntEncryptDecrypt" && m.Parameters.Count == 1);
                if (field_value == null || encryptMethod == null) return false;

                var getValue = type.Methods.FirstOrDefault(m => m.Name == "GetValue");
                var getObfuscatedValue = type.Methods.FirstOrDefault(m => m.Name == "GetObfuscatedValue");

                
                if (getValue != null && getValue.HasBody)
                {
                    getValue.Body.Instructions.Clear();
                    var il = getValue.Body.GetILProcessor();
                    il.Append(il.Create(OpCodes.Ldarg_0));
                    il.Append(il.Create(OpCodes.Ldfld, field_value));
                    il.Append(il.Create(OpCodes.Call, encryptMethod));
                    il.Append(il.Create(OpCodes.Ret));
                }
                
                if (getObfuscatedValue != null && getObfuscatedValue.HasBody)
                {
                    getObfuscatedValue.Body.Instructions.Clear();
                    var il = getObfuscatedValue.Body.GetILProcessor();
                    il.Append(il.Create(OpCodes.Ldarg_0));
                    il.Append(il.Create(OpCodes.Ldfld, field_value));
                    il.Append(il.Create(OpCodes.Ret));
                }

                using (var outStream = new MemoryStream())
                {
                    assembly.Write(outStream);
                    File.WriteAllBytes(dllPath, outStream.ToArray());
                }
                return true;
            }
        }
    }
}