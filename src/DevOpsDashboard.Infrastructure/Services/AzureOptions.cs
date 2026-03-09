namespace DevOpsDashboard.Infrastructure.Services;

public class AzureDevOpsOptions
{
    public const string Section = "AzureDevOps";

    public string OrganizationUrl { get; set; } = string.Empty;
    public string PersonalAccessToken { get; set; } = string.Empty;
    public List<string> Projects { get; set; } = new();
}

public class AzureMonitorOptions
{
    public const string Section = "AzureMonitor";

    public string SubscriptionId { get; set; } = string.Empty;
    public string ResourceGroupName { get; set; } = string.Empty;
    public string LogAnalyticsWorkspaceId { get; set; } = string.Empty;
    public int ErrorLookbackMinutes { get; set; } = 60;
}