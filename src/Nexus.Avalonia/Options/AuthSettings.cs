using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Configuration.Writable;

namespace Nexus.Options;

[OptionsModel(Id = "Auth", SupportMigration = true, Version = 1)]
public partial class AuthOptions
{
    public const string SectionName = "Auth";

    [Required]
    public string Authority { get; set; } = string.Empty;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string Scopes { get; set; } = string.Empty;

    [Required]
    public string RedirectUrl { get; set; } = string.Empty;

    [Required]
    public string PostLogoutRedirectUrl { get; set; } = string.Empty;

    public bool PushedAuthorization { get; set; }

    [JsonSerializable(typeof(AuthOptions))]
    [JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
    public partial class AuthOptionsSerializerContext : JsonSerializerContext;
}
