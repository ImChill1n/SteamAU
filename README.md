# SteamAU

A simple command-line utility that unlocks all Steam achievements for a selected game using the Steamworks API.

## Features

- Unlocks all achievements for any Steam game
- Works by entering a Steam AppID
- Supports command-line arguments
- Self-contained executable (no .NET installation required)

## Requirements

- Windows x64
- Steam client running
- Logged into your Steam account
- The game must be owned by the current Steam account

## Usage

### Interactive

Run the program:

```text
SteamAU.exe
```

Enter the AppID when prompted.

You can find AppIDs on:

https://steamdb.info

Example:

```text
480
```

### Command line

```text
SteamAU.exe 480
```

## License

MIT
