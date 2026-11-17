using System.Text.Json.Serialization;
using Configuration.Writable;
using Nexus.Fluent.Abstractions.Theme;

namespace Nexus.Options;

[OptionsModel(Id = "Application", Version = 1, SupportMigration = true)]
public partial class ApplicationOptions
{
    public const string SectionName = "App";

    public bool IsFirstRun { get; set; }
    public ApplicationTheme Theme { get; set; }

    [JsonSerializable(typeof(ApplicationOptions))]
    [JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
    public partial class ApplicationSerializerContext : JsonSerializerContext;
}
