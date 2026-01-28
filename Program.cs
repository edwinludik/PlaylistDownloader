using Xtream.Client;
using Xtream.Client.XtreamConnectionInfo;
using Serilog;
using M3UManager;
using M3UManager.Models;
using System.CommandLine;

DateTime startTime = DateTime.UtcNow;
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var serverOption = new Option<string>(
    name: "ServerURL",
    aliases: new[] { "-s", "--server" }
);
var usernameOption = new Option<string>(
    name: "Username",
    aliases: new[] { "-u", "--username" }
);
var passwordOption = new Option<string>(
    name: "Password",
    aliases: new[] { "-p", "--password" }
);
var getAccountInfoOption = new Option<bool>(
    name: "GetAccountInformation",
    aliases: new[] { "--get-account-info" }
);
var getLiveOption = new Option<bool>(
    name:  "DownloadLiveChannels",
    aliases: new[] { "--get-live" }
);
var getEpgOption = new Option<bool>(
    name: "DownloadEPGInformation",
    aliases: new[] { "--get-epg" }
);

var rootCommand = new RootCommand("PlaylistDownloader");
rootCommand.Options.Add(serverOption);
rootCommand.Options.Add(usernameOption);
rootCommand.Options.Add(passwordOption);
rootCommand.Options.Add(getAccountInfoOption);
rootCommand.Options.Add(getLiveOption);
rootCommand.Options.Add(getEpgOption);

// Parse the arguments
var result = rootCommand.Parse(args);
string server = result.GetRequiredValue(serverOption) ?? throw new ArgumentException("Server is required.");
string username = result.GetRequiredValue(usernameOption) ?? throw new ArgumentException("Username is required.");
string password = result.GetRequiredValue(passwordOption) ?? throw new ArgumentException("Password is required.");
bool getAccountInfo = result.GetValue(getAccountInfoOption);
bool getEpg = result.GetValue(getEpgOption);
bool getLive = result.GetValue(getLiveOption);
Log.Debug($"getAccountInfo: {getAccountInfo}");
Log.Debug($"getEpg: {getEpg}");
Log.Debug($"getLive: {getLive}");

var factory = new DefaultHttpClientFactory("PlaylistDownloader/1.0");
using(var xtreamClient = new XtreamClient(factory))
{
    var connectionInfo = new XtBasicConnectionFactory(server, username, password).Create();
   
    // Get Panel Info
    Log.Information("Testing Connection and Getting Account Information");
    var panelInfo = await xtreamClient.GetPanelAsync(connectionInfo, CancellationToken.None);
    Log.Information("Connection Works");
    if (getAccountInfo)
    {
        Log.Information($"Status: {panelInfo.User_info.Status}");
        Log.Information($"Expiration Date: {panelInfo.User_info.ExpirationDate}");
        Log.Information($"Max Connections: {panelInfo.User_info.Max_connections}");
        Log.Information($"Active Connections: {panelInfo.User_info.Active_cons}");
        Log.Information($"Number of Categories: {panelInfo.Categories.Live.Count}");
        Log.Information($"Number of Channels: {panelInfo.Available_Channels.Count}");
    }
    
    if (getLive)
    {
        var categories = panelInfo.Categories.Live.ToDictionary(x=>x.Category_id, x=>x.Category_name);

        Log.Information("Downloading Live Streams");
        List<Channels> liveStreams = await xtreamClient.GetLiveStreamsAsync(connectionInfo, CancellationToken.None);
        int counter = 0;
        int total = liveStreams.Count;
        double progress = 0;
        Console.Write($"Progress: {progress}% ({counter} / {total})");  
    
        M3U playlistFile = new M3U();  
        foreach (Channels liveStream in liveStreams)
        {
            counter++;
            if (counter % 100 == 0)
            {
                progress = (double)counter / total;
                progress = double.Round(progress, MidpointRounding.AwayFromZero);
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Progress: {progress}% ({counter} / {total})");
            }
            String streamUrl = $"{connectionInfo.Server}/live/{connectionInfo.UserName}/{connectionInfo.Password}/{liveStream.StreamId}.ts";
            Channel current = new Channel();
            current.Duration = "-1";
            current.TvgName = liveStream.Name;
            current.Title = liveStream.Name;
            current.GroupTitle = categories.GetValueOrDefault(liveStream.Category_id, "** Unassigned **");
            current.Logo = liveStream.Stream_icon;
            current.MediaUrl = streamUrl;
            current.TvgID = liveStream.Epg_channel_id;
            if (current.TvgID == null)
            {
                current.TvgID = "(no tvg-id)";
            }
            playlistFile.Channels.Add(current);
        }
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.WriteLine($"Progress: 100% ({counter} / {total})");
    
        String fileName = $"{connectionInfo.UserName}.m3u8";
        playlistFile.SaveM3U(fileName,M3UType.TagsType);
        Log.Information($"Saved Playlist file: {fileName}");
    }

    if (getEpg)
    {
        String epgUrl = $"{connectionInfo.Server}/xmltv.php?username={connectionInfo.UserName}&password={connectionInfo.Password}";
        String savePath = $"{connectionInfo.UserName}.xml";
        try
        {
            Log.Information("Downloading EPG information");
            Log.Debug($"From:{epgUrl}");

            using (var httpClient = new HttpClient())
            {
                // Download the EPG XML
                var response = await httpClient.GetAsync(epgUrl);
                response.EnsureSuccessStatusCode();

                var epgContent = await response.Content.ReadAsStringAsync();

                Log.Information("Saving EPG information");
                await File.WriteAllTextAsync(savePath, epgContent);

                Log.Information($"EPG saved to {savePath}");
            }
        }
        catch (HttpRequestException ex)
        {
            Log.Error($"Failed to download EPG: {ex.Message}");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred: {ex.Message}");
        }
    }
}

TimeSpan runDuration = DateTime.UtcNow - startTime;
Log.Information($"Runtime {runDuration.ToString()}");
Log.Information("Done");
Log.CloseAndFlush();