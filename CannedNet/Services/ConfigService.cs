namespace CannedNet.Services;

public class ConfigService
{
    public ConfigData Config { get; } = new();
}

public class ConfigData
{
    public bool WhitelistOn { get; set; }
    public List<string> WhitelistedPlatformIds { get; set; } = [];
    public bool EnableAccountCreation { get; set; } = true;
    public List<string> AdminAccountIds { get; set; } = [];
}
