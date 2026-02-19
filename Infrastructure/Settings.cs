namespace Infrastructure;

public partial class ConfigManager
{
    /// <summary>
    /// Provides configuration options for monitoring system resource usage and controlling performance data collection.
    /// </summary>
    /// <remarks>The Settings class enables users to specify which system resources to track, such as CPU,
    /// memory, network, and disk usage. It also allows configuration of the maximum storage limit for collected data
    /// and the interval at which resource usage is sampled. Adjust these settings to balance monitoring detail with
    /// storage and performance considerations.</remarks>
    public class Settings
    {

        public bool TrackCPUUsage { get; set; } = true;
        public bool TrackMemoryUsage { get; set; } = true;
        public bool TrackNetworkUsage { get; set; } = true;
        public bool TrackDiskUsage { get; set; } = true;
        public int StorageLimitMb { get; set; } = 1024;
        public float TickRate { get ; set ; } = 10.0f; // change default value to something reasonable. Not sure what units we're using quite yet

    }

}