using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Configs;

public static class ConfigSettingExtensions
{
    extension(ConfigSettingEnum enumValue)
    {
        /// <summary>
        /// Key dùng để đọc IConfiguration: Display.Name nếu có, ngược lại tên enum.
        /// </summary>
        public string GetConfigKey()
        {
            var displayName = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName();

            return string.IsNullOrEmpty(displayName) ? enumValue.ToString() : displayName;
        }

        public string GetConfig()
        {
            return ConfigSetting.Configs.TryGetValue(enumValue, out var value)
                ? value
                : string.Empty;
        }
    }
}
