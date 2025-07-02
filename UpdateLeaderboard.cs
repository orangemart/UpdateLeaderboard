// UpdateLeaderboard.cs (with pagination)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("UpdateLeaderboard", "Orangemart", "1.2.0")]
    [Description("Updates the Leaderboard tab in ServerInfo.json with 2-page paginated data.")]
    public class UpdateLeaderboard : CovalencePlugin
    {
        [PluginReference] private Plugin DepositBox;

        private const string ServerInfoPath = "oxide/config/ServerInfo.json";
        private const string SummaryPath = "oxide/data/DepositBox/DepositBoxSummary.json";

        [Command("updateleaderboard")]
        private void CmdUpdateLeaderboard(IPlayer player, string command, string[] args)
        {
            if (player?.IsAdmin != true)
            {
                player?.Reply("You must be an admin to use this command.");
                return;
            }

            var rustPlayer = player.Object as BasePlayer;
            DepositBox?.Call("GenerateDepositSummary", rustPlayer, 100000);
            Puts("📊 Called GenerateDepositSummary from DepositBox plugin.");

            if (!File.Exists(ServerInfoPath))
            {
                player.Reply("❌ ServerInfo.json not found.");
                return;
            }

            if (!File.Exists(SummaryPath))
            {
                player.Reply("❌ DepositBoxSummary.json not found.");
                return;
            }

            try
            {
                var summaryRaw = File.ReadAllText(SummaryPath);
                Puts("📂 DepositBoxSummary.json contents:");
                Puts(summaryRaw);

                var summary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(summaryRaw);
                long totalDeposited = summary.Values.Sum(v => Convert.ToInt64(v["total_deposited"]));
                Puts($"💰 Total deposited across all players: {totalDeposited}");

                var sortedPlayers = summary
                    .OrderByDescending(kv => Convert.ToInt64(kv.Value["total_deposited"]))
                    .Take(40)
                    .ToList();

                Puts($"📈 Found {sortedPlayers.Count} top players with deposits.");

                // Build all lines first
                var allLines = new List<string>
                {
                    "🏆 Top Scrap Depositors This Wipe",
                    $"Total Deposited: {totalDeposited:N0} scrap",
                    ""
                };

                for (int i = 0; i < sortedPlayers.Count; i++)
                {
                    var kv = sortedPlayers[i];
                    var steamId = kv.Key;
                    var name = covalence.Players.FindPlayerById(steamId)?.Name ?? steamId;
                    var deposited = Convert.ToInt64(kv.Value["total_deposited"]);
                    double percentage = totalDeposited > 0 ? (double)deposited / totalDeposited * 100 : 0;
                    allLines.Add($"{i + 1}. {name} - {deposited:N0} ({percentage:F2}%)");
                }

                // Split into 2 pages of 20 entries each (plus title on page 1)
                var page1Lines = allLines.Take(23).ToList(); // 3 header lines + 20 players
                var page2Lines = allLines.Skip(23).ToList();

                var leaderboardTab = new JObject
                {
                    ["ButtonText"] = "Leaderboard",
                    ["HeaderText"] = "Top Scrap Depositors",
                    ["Pages"] = new JArray
                    {
                        new JObject
                        {
                            ["TextLines"] = JArray.FromObject(page1Lines),
                            ["ImageSettings"] = new JArray()
                        },
                        new JObject
                        {
                            ["TextLines"] = JArray.FromObject(page2Lines),
                            ["ImageSettings"] = new JArray()
                        }
                    },
                    ["TabButtonAnchor"] = 4,
                    ["TabButtonFontSize"] = 16,
                    ["HeaderAnchor"] = 0,
                    ["HeaderFontSize"] = 32,
                    ["TextFontSize"] = 16,
                    ["TextAnchor"] = 3,
                    ["OxideGroup"] = ""
                };

                var serverInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(ServerInfoPath));
                var settingsToken = serverInfo["settings"];
                if (settingsToken == null || !(settingsToken is JObject settings))
                {
                    player.Reply("❌ 'settings' section missing from ServerInfo.json.");
                    return;
                }

                var tabsToken = settings["Tabs"];
                if (tabsToken == null || !(tabsToken is JArray tabs))
                {
                    player.Reply("❌ 'Tabs' section missing or malformed in ServerInfo.json.");
                    return;
                }

                // Remove old leaderboard
                for (int i = tabs.Count - 1; i >= 0; i--)
                {
                    if (tabs[i]["ButtonText"]?.ToString() == "Leaderboard")
                    {
                        tabs.RemoveAt(i);
                        Puts($"🧹 Removed existing Leaderboard tab at index {i}.");
                    }
                }

                tabs.Add(leaderboardTab);
                File.WriteAllText(ServerInfoPath, JsonConvert.SerializeObject(serverInfo, Formatting.Indented));
                Puts("✅ ServerInfo.json updated with 2-page leaderboard.");

                server.Command("oxide.reload ServerInfo");
                Puts("🔄 ServerInfo plugin reloaded.");
                player.Reply("✅ Leaderboard updated and ServerInfo reloaded!");
            }
            catch (Exception ex)
            {
                player.Reply($"❌ Error: {ex.Message}");
                Puts($"❌ Error updating leaderboard: {ex.Message}");
            }
        }
    }
}
