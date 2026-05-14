namespace Infrastructure;

/// <summary>Settings with float values go here</summary>
public enum SettingFloat
{
    /// <summary>
    /// The poll rate for program data
    /// </summary>
    TickRate,
    DatabaseSaveInterval,
    NetworkReconnectInterval,
    //Notification settings
    /// <summary>
    /// In MB
    /// </summary>
    NoteDBCapacity,
    /// <summary>
    /// In %
    /// </summary>
    NoteIndividualCpuUsage,
    /// <summary>
    /// In MB
    /// </summary>
    NoteIndividualRamUsage,
    /// <summary>
    /// in MB/Sec
    /// </summary>
    NoteIndividualDiskUsage,
    /// <summary>
    /// In MB/Sec
    /// </summary>
    NoteIndividualNetworkUsage,
    /// <summary>
    /// In %
    /// </summary>
    NoteCpuUsage,
    /// <summary>
    /// In MB
    /// </summary>
    NoteRamUsage,
    /// <summary>
    /// in MB/Sec
    /// </summary>
    NoteDiskUsage,
    /// <summary>
    /// In MB/Sec
    /// </summary>
    NoteNetworkUsage,
};
