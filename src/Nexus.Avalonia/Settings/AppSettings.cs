using Configuration.Writable;

namespace Nexus.Settings;

[OptionsModel(Id = "Settings", Version = 1, SupportMigration = true)]
public partial class AppSettings
{
    public const string SectionName = "App";
    public bool IsFirstRun { get; set; }
}
