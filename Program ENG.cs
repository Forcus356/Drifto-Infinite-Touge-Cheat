using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DriftoPatcher
{
    class Program
    {
        static string[] defaultPaths = {
            @"E:\SteamLibrary\steamapps\common\Drifto\Drifto_Data\Managed\Drifto.Encryption.dll",
            @"C:\Program Files (x86)\Steam\steamapps\common\Drifto\Drifto_Data\Managed\Drifto.Encryption.dll",
            @"C:\Program Files\Steam\steamapps\common\Drifto\Drifto_Data\Managed\Drifto.Encryption.dll",
            @"D:\SteamLibrary\steamapps\common\Drifto\Drifto_Data\Managed\Drifto.Encryption.dll",
            @"C:\Program Files (x86)\SteamLibrary\steamapps\common\Drifto\Drifto_Data\Managed\Drifto.Encryption.dll"
        };

        static string savePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "AppData", "LocalLow", "UnluckyDuck", "Drifto", "saveDataV3.drifto");

        static string saveBackupPath = savePath + ".bak";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Drifto: Infinite Touge Cheat ===");
            Console.WriteLine("(!) Make sure the game is FULLY CLOSED before continuing!\n");

            string dllPath = null;
            foreach (var p in defaultPaths)
                if (File.Exists(p)) { dllPath = p; break; }

            if (dllPath == null)
            {
                Console.Write("Could not locate Drifto.Encryption.dll automatically.\nEnter the full path manually:\n> ");
                dllPath = Console.ReadLine()?.Trim('"');
                if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
                {
                    Console.WriteLine("File not found.");
                    Console.ReadKey();
                    return;
                }
            }

            string dllBackupPath = dllPath + ".bak";

            Console.WriteLine("Choose an action:");
            Console.WriteLine("1. Add coins (patch DLL + auto-backup save)");
            Console.WriteLine("2. Restore honest methods (revert DLL code)");
            Console.WriteLine("3. Restore save from backup");
            Console.WriteLine("4. Create / update save backup (checkpoint)");
            Console.WriteLine("5. FRESH START (delete saves + revert DLL)");
            Console.Write("> ");
            string choice = Console.ReadLine();

            
            if (choice == "3")
            {
                if (!File.Exists(saveBackupPath))
                {
                    Console.WriteLine("\n[i] No save backup found (.bak). Nothing to restore.");
                    Console.ReadKey();
                    return;
                }
                try
                {
                    File.Copy(saveBackupPath, savePath, true);
                    Console.WriteLine("\n[OK] Save restored from backup!");
                }
                catch (IOException) { Console.WriteLine("\n[ERROR] Save file is locked. Close the game."); }
                catch (UnauthorizedAccessException) { Console.WriteLine("\n[ERROR] Access denied. Close the game or run as Administrator."); }
                catch (Exception ex) { Console.WriteLine($"\n[ERROR] {ex.Message}"); }
                Console.ReadKey();
                return;
            }

            
            if (choice == "4")
            {
                if (!File.Exists(savePath))
                {
                    Console.WriteLine("\n[i] Save file not found - have you played the game yet?");
                    Console.ReadKey();
                    return;
                }
                try
                {
                    File.Copy(savePath, saveBackupPath, true);
                    Console.WriteLine("\n[OK] Save backup created / updated.");
                }
                catch (Exception ex) { Console.WriteLine($"\n[ERROR] {ex.Message}"); }
                Console.ReadKey();
                return;
            }

        
            if (choice == "5")
            {
                Console.WriteLine();
                Console.WriteLine("!!! WARNING !!!");
                Console.WriteLine("Are you sure you want to completely delete your save?");
                Console.WriteLine("This will permanently delete ALL your saves and backups:");
                Console.WriteLine($"  - {savePath}");
                Console.WriteLine($"  - {saveBackupPath}");
                Console.WriteLine("It will also revert the DLL to honest code (standard counter).");
                Console.WriteLine("This action CANNOT be undone!");
                Console.Write("\nType YES to confirm: ");
                string confirm = Console.ReadLine()?.Trim().ToUpper();

                if (confirm != "YES" && confirm != "Y" && confirm != "ДА")
                {
                    Console.WriteLine("\n[i] Cancelled. Nothing was deleted.");
                    Console.ReadKey();
                    return;
                }

                try
                {
                    if (File.Exists(savePath)) { File.Delete(savePath); Console.WriteLine("[DELETED] saveDataV3.drifto"); }
                    else Console.WriteLine("[i] saveDataV3.drifto not found.");

                    if (File.Exists(saveBackupPath)) { File.Delete(saveBackupPath); Console.WriteLine("[DELETED] saveDataV3.drifto.bak"); }
                    else Console.WriteLine("[i] saveDataV3.drifto.bak not found.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[ERROR] Could not delete saves: {ex.Message}");
                    Console.ReadKey();
                    return;
                }

                try
                {
                    EnsureDllBackup(dllPath, dllBackupPath);
                    if (RestoreHonestMethods(dllPath))
                        Console.WriteLine("[OK] DLL reverted to honest code (standard counter).");
                    else
                        Console.WriteLine("[WARN] Could not restore DLL methods (structure changed?).");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] While restoring DLL: {ex.Message}");
                }

                Console.WriteLine("\n[DONE] The game will start fresh on next launch.");
                Console.ReadKey();
                return;
            }

            if (choice != "1" && choice != "2")
            {
                Console.WriteLine("Invalid choice.");
                Console.ReadKey();
                return;
            }

            EnsureDllBackup(dllPath, dllBackupPath);

            if (choice == "1")
            {
                Console.Write("\nHow many coins to set? (default 1000000): ");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) input = "1000000";
                if (!int.TryParse(input, out int val) || val < 0)
                {
                    Console.WriteLine("Invalid amount.");
                    Console.ReadKey();
                    return;
                }

                if (File.Exists(savePath) && !File.Exists(saveBackupPath))
                {
                    try
                    {
                        File.Copy(savePath, saveBackupPath, false);
                        Console.WriteLine("[i] Save backup created (your 'clean' progress).");
                    }
                    catch { Console.WriteLine("[WARN] Could not create save backup."); }
                }

                try
                {
                    if (PatchMoney(dllPath, val))
                    {
                        Console.WriteLine($"\n[DONE] Balance set to {val} coins.");
                        Console.WriteLine("[i] Launch the game, trigger an autosave, then close it.");
                        Console.WriteLine("[i] Then run the patcher again and choose option 2 to restore honest code.");
                    }
                    else Console.WriteLine("\n[ERROR] Required classes/methods not found in DLL.");
                }
                catch (Exception ex) { Console.WriteLine($"\n[ERROR] {ex.Message}"); }
            }
            else
            {
                try
                {
                    if (RestoreHonestMethods(dllPath))
                        Console.WriteLine("\n[DONE] Original methods restored!");
                    else Console.WriteLine("\n[ERROR] Required classes/methods not found in DLL.");
                }
                catch (Exception ex) { Console.WriteLine($"\n[ERROR] {ex.Message}"); }
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