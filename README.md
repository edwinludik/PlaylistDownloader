# PlaylistDownloader

A **cross-platform CLI tool** built with **C# .NET** to interact with Xtream Codes-compatible IPTV servers. Download M3U8 playlists, EPG data, and retrieve account information — all from the command line.

## Features

- **Account Information**: View your subscription status, expiration date, and connection limits.
- **Playlist Download**: Generate a `.m3u8` playlist file with all available live channels, including logos and group categories.
- **EPG Download**: Download the Electronic Program Guide (XMLTV) for your subscription.
- **Cross-platform**: Built with .NET 9, runs on Linux, Windows, and macOS.

## Installation

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Clone and Build

```bash
git clone https://github.com/edwinludik/PlaylistDownloader.git
cd PlaylistDownloader
dotnet build -c Release
```

## Usage

Run the application using `dotnet run` or the compiled executable.

### Options

| Option | Alias | Description | Required |
|--------|-------|-------------|-------|
| `--server` | `-s` | Xtream Codes Server URL | Yes   |
| `--username` | `-u` | Your username | Yes   |
| `--password` | `-p` | Your password | Yes   |
| `--get-account-info` | | Display account information | No    |
| `--get-live` | | Download live channels to M3U8 | No    |
| `--get-epg` | | Download EPG information to XML | No    |

Note: If no --get-xxx option is provided, the tool will just test the connection to the server

### Examples

**Download playlist and EPG:**

```bash
./PlaylistDownloader -s "http://example.com:8080" -u "user" -p "pass" --get-live --get-epg
```

**Check account info:**

```bash
./PlaylistDownloader -s "http://example.com:8080" -u "user" -p "pass" --get-account-info
```

## Development

### Publish 

To publish a self-contained executable for Linux x64:

```bash
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true  
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
