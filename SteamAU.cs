using Steamworks;

namespace SteamAU
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SteamAU ===");
            Console.WriteLine();

            string? appIdInput;

            if (args.Length > 0)
            {
                appIdInput = args[0];
            }
            else
            {
                Console.WriteLine("Make sure your Steam client is running and you logged in");
                Console.WriteLine("Enter AppID, you can get it at https://steamdb.info");
                appIdInput = Console.ReadLine();
            }
            Console.WriteLine();

            if (appIdInput is null) {
                Console.WriteLine("No AppID provided.");
                Console.WriteLine("Press ENTER to exit.");
                Console.ReadLine();
                return;
            }

            if (!uint.TryParse(appIdInput, out uint appId))
            {
                Console.WriteLine("Invalid AppID provided.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("[*] Initializing SteamAPI...");

            try
            {
                File.WriteAllText("./steam_appid.txt", appIdInput);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] {ex.Message}");
                Console.WriteLine("Press ENTER to exit.");
                Console.ReadLine();
                File.Delete("./steam_appid.txt");
                return;
            }

            if (!SteamAPI.Init())
            {
                Console.WriteLine("[!] SteamAPI.Init() failed!");
                Console.WriteLine("Is Steam running and are you logged in?");
                Console.WriteLine("Is the game in your library?");
                Console.WriteLine("\nPress ENTER to exit.");
                Console.ReadLine();
                File.Delete("./steam_appid.txt");
                return;
            }

            Console.WriteLine("[✓] SteamAPI initialized");
            Console.WriteLine($"AppID: {SteamUtils.GetAppID().m_AppId}");

            Console.WriteLine();
            Console.WriteLine("[*] Loading achievements...");
            SteamUserStats.RequestCurrentStats();

            bool statsReceived = false;
            var callback = Callback<UserStatsReceived_t>.Create((result) =>
            {
                statsReceived = true;
            });

            int attempts = 0;
            while (!statsReceived && attempts < 50)
            {
                SteamAPI.RunCallbacks();
                Thread.Sleep(100);
                attempts++;
            }

            if (!statsReceived)
            {
                Console.WriteLine("[!] Failed to load achievements (timeout)");
                SteamAPI.Shutdown();
                Console.WriteLine("\n    Press ENTER to exit.");
                Console.ReadLine();
                File.Delete("./steam_appid.txt");
                return;
            }

            uint count = SteamUserStats.GetNumAchievements();
            Console.WriteLine($"[✓] Loaded: {count} achievements");
            Console.WriteLine();

            Console.WriteLine("=== Unlocking achievements ===");
            int unlockedCount = 0;
            int alreadyCount = 0;
            int failedCount = 0;

            for (uint i = 0; i < count; i++)
            {
                string apiName = SteamUserStats.GetAchievementName(i);
                string displayName = SteamUserStats.GetAchievementDisplayAttribute(apiName, "name") ?? apiName;

                SteamUserStats.GetAchievement(apiName, out bool already);

                if (already)
                {
                    alreadyCount++;
                    continue;
                }

                if (SteamUserStats.SetAchievement(apiName))
                {
                    unlockedCount++;
                    Console.WriteLine($"  [✓] [{i+1}/{count}] {displayName}");
                }
                else
                {
                    failedCount++;
                    Console.WriteLine($"  [✗] [{i+1}/{count}] {displayName}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("=== Saving changes ===");
            bool stored = SteamUserStats.StoreStats();

            Console.WriteLine();
            Console.WriteLine("=== Result ===");
            Console.WriteLine($"  Already unlocked:      {alreadyCount}");
            Console.WriteLine($"  Unlocked:    {unlockedCount}");
            Console.WriteLine($"  Failed:           {failedCount}");
            Console.WriteLine($"  StoreStats:        {(stored ? "OK ✓" : "FAILED ✗")}");

            SteamAPI.Shutdown();
            File.Delete("./steam_appid.txt");
            Console.WriteLine("\n  Done! Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}
