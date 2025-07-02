# UpdateLeaderboard

Automatically updates the Leaderboard tab in `ServerInfo.json` using deposit data from the [DepositBox](https://github.com/Orangemart/DepositBox) plugin. The leaderboard is split into two pages for improved readability and displays the top 40 scrap depositors of the current Rust wipe.

## Features

- Pulls data from `DepositBoxSummary.json`
- Calculates total scrap deposited and percentage per player
- Displays top 40 players in a two-page format
- Injects a new **Leaderboard** tab into `ServerInfo.json`
- Automatically reloads the ServerInfo plugin
- Admin-only `/updateleaderboard` command

## Requirements

- **Oxide/uMod** installed on your Rust server
- [DepositBox Plugin](https://github.com/Orangemart/DepositBox)
- Existing and working `ServerInfo` plugin using `ServerInfo.json` config

## Installation

1. Upload `UpdateLeaderboard.cs` to your server’s `oxide/plugins` directory.
2. Ensure the [DepositBox plugin](https://github.com/Orangemart/DepositBox) is installed and used during the wipe.
3. Ensure `ServerInfo.json` exists under `oxide/config/`.
4. Grant yourself admin permissions on the server.

## Usage

To update the leaderboard:

```rust
/updateleaderboard
```

* This command triggers the DepositBox plugin to generate an up-to-date deposit summary.
* Then, it reads `DepositBoxSummary.json`, calculates top depositors, and updates the `Leaderboard` tab inside `ServerInfo.json` with two pages of ranked player data.
* Finally, it reloads the ServerInfo plugin to apply the changes.

⚠️ Only users with admin rights can execute this command.

## Output Example

The leaderboard will show entries like:

```
🏆 Top Scrap Depositors This Wipe
Total Deposited: 125,000 scrap

1. playerName1 - 25,000 (20.00%)
2. playerName2 - 22,000 (17.60%)
...
20. playerName20 - 2,000 (1.60%)
```

The second page (entries 21–40) is accessible via tabbed navigation in ServerInfo.

## File Locations

* **Reads from:**

  * `oxide/config/ServerInfo.json`
  * `oxide/data/DepositBox/DepositBoxSummary.json`
* **Writes to:**

  * `oxide/config/ServerInfo.json`

## Changelog

### v1.2.0

* Pagination added (two pages of 20 entries each)
* Removed old leaderboard tab before inserting new one
* Now supports full refresh of ServerInfo leaderboard tab

## Credits

Created by **Orangemart**
This plugin integrates closely with the [DepositBox plugin](https://github.com/Orangemart/DepositBox) for dynamic player rankings based on in-game deposits.

## License

MIT License

```

Let me know if you’d like me to generate this as a downloadable file or include a badge, like `Built for Rust` or `Oxide Plugin`.
```
