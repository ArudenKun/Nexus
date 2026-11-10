using System.Text.Json.Serialization;
using Configuration.Writable;
using Fluently.Abstractions.Theme;

namespace Nexus.Settings;

[OptionsModel(Id = "Settings", Version = 1, SupportMigration = true)]
public partial class ApplicationSettings
{
    public const string SectionName = "App";

    public bool IsFirstRun { get; set; }
    public ApplicationTheme Theme { get; set; }

    [JsonSerializable(typeof(ApplicationSettings))]
    [JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
    public partial class ApplicationSettingsJsonSerializerContext : JsonSerializerContext;
}
