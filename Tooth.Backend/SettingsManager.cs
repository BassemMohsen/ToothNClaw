using Windows.Storage;
using System.Collections.Generic;
using System;


namespace Tooth.Backend
{
    internal class SettingsManager
    {
        private static readonly ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        // Define all your default values here
        private static readonly Dictionary<string, object> DefaultValues = new()
        {
            { "Contrast", 50.0 },
            { "Brightness", 50.0 },
            { "Gamma", 1.0 },
            { "Hue", 0.0 },
            { "Saturation", 50.0 },
            { "AutoSuspend", 0 },
            { "GoBackToSleep", 0 },
            { "EnhancedSleep", 0 },
        };

        /// <summary>
        /// Ensures all default settings exist.
        /// </summary>
        public static void Initialize()
        {
            foreach (var kvp in DefaultValues)
            {
                if (!localSettings.Values.ContainsKey(kvp.Key))
                {
                    Console.WriteLine($"[SettingsManager] Value is not found for key: {kvp.Key}");
                    localSettings.Values[kvp.Key] = kvp.Value;
                } else
                {
                    Console.WriteLine($"[SettingsManager] Value is found for key: {kvp.Key} and Value is: {kvp.Value}");
                }
            }
        }

        /// <summary>
        /// Get a value from settings, falling back to the default if missing.
        /// </summary>
        public static T Get<T>(string key)
        {
            if (localSettings.Values.TryGetValue(key, out object value) && value is T typed)
            {
                return typed;
            }

            // fallback to default
            if (DefaultValues.TryGetValue(key, out object defaultValue))
            {
                localSettings.Values[key] = defaultValue;
                return (T)defaultValue;
            }

            return default;
        }

        /// <summary>
        /// Set and persist a value.
        /// </summary>
        public static void Set<T>(string key, T value)
        {
            localSettings.Values[key] = value;
        }

        public static void SetObject<T>(string key, T value)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(value);
            localSettings.Values[key] = json;
        }

        public static T GetObject<T>(string key)
        {
            if (localSettings.Values.TryGetValue(key, out object raw) && raw is string json)
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
            }
            return default;
        }

    }
}
