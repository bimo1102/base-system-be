using Microsoft.Extensions.Configuration;

namespace Configs;

/// <summary>
/// Load toàn bộ key trong <see cref="ConfigSettingEnum"/> từ IConfiguration (appsettings).
/// </summary>
public static class ConfigSetting
{
    public static readonly IDictionary<ConfigSettingEnum, string> Configs =
        new Dictionary<ConfigSettingEnum, string>();

    public static void Init(IConfiguration configuration)
    {
        foreach (var key in Enum.GetValues<ConfigSettingEnum>())
        {
            if (Configs.ContainsKey(key))
            {
                continue;
            }

            // Ưu tiên Display(Name) nếu có (vd: ConnectionStrings:DbConnectionString)
            var keyConfig = key.GetConfigKey();
            var section = configuration.GetSection(keyConfig);
            Configs.Add(key, section.Value ?? string.Empty);
        }
    }
}
