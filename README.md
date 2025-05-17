# Window Organizer

A modern, open-source window position manager for Windows 10/11, written in C#/.NET.

## Features
- Move and resize windows using a customizable grid overlay
- Global hotkey (Win+Shift+O by default) to activate the overlay
- Supports multiple monitors (move overlay to any screen with 1-4)
- Configurable grid size per screen (via config.json or tray menu)
- System tray icon with config and exit options
- Dark mode and transparency support for config UI
- Open source (MIT License)

## Usage
1. **Run the app** (build instructions below)
2. Press **Win+Shift+O** to show the grid overlay
3. (Optional) Press 1-4 to move the overlay to another screen
4. Press two grid keys (Q,W,E,R,A,S,D,F for 4x2, or cell coordinates for custom grids) to position the active window
5. Right-click the tray icon for config or exit

## Configuration
- Edit `config.json` to set grid size per screen
- Or use the tray icon → Config... to edit via GUI

## Build & Run
1. Install [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Clone this repo:
   ```sh
   git clone git@github.com:Setlakwe/windoworganizer.git
   cd windoworganizer
   dotnet build
   dotnet run --project WindowOrganizer.csproj
   ```

## License
MIT License (see LICENSE file) 