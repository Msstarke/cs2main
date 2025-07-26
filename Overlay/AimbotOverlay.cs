using System;
using System.IO;
using System.Text.Json;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace cs2aim.Overlay
{
    public class AimbotOverlay : ClickableTransparentOverlay.Overlay
    {
        public bool AimbotEnabled = true;
        public bool AimOnTeam = false;
        public float MaxDegreesPerTick = 2.0f;

        // Legit behavior tuning fields
        public float MaxFov = 15.0f;
        public float SmoothAmount = 0.1f;
        public float JitterAmount = 0.1f;

        private const string ConfigFileName = "aimbot_config.json";

        public AimbotOverlay()
        {
            // Initialize config manager and load settings
            ConfigManager.Initialize(ConfigFileName);
            ConfigManager.Load();
            ApplyConfig();
        }

        protected override void Render()
        {
            ImGui.Begin("Aimbot Settings");

            // Core toggles
            ImGui.Checkbox("Enable Aimbot", ref AimbotEnabled);
            ImGui.Checkbox("Target Teammates", ref AimOnTeam);

            ImGui.Separator();
            ImGui.Text("Legit Behavior Tuning:");
            ImGui.SliderFloat("Max FOV", ref MaxFov, 1.0f, 90.0f, "%.1f");
            ImGui.SliderFloat("Smooth Amount", ref SmoothAmount, 0.01f, 1.0f, "%.2f");
            ImGui.SliderFloat("Jitter Amount", ref JitterAmount, 0.0f, 2.0f, "%.2f");

            ImGui.Separator();
            ImGui.SliderFloat("Aim Speed (deg/tick)", ref MaxDegreesPerTick, 0.1f, 20.0f, "%.1f", ImGuiSliderFlags.AlwaysClamp);

            ImGui.Separator();
            if (ImGui.Button("Save Config"))
            {
                UpdateConfig();
                ConfigManager.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button("Load Config"))
            {
                ConfigManager.Load();
                ApplyConfig();
            }

            ImGui.End();
        }

        private void ApplyConfig()
        {
            var s = ConfigManager.Settings;
            AimbotEnabled     = s.AimbotEnabled;
            AimOnTeam         = s.AimOnTeam;
            MaxFov            = s.MaxFov;
            SmoothAmount      = s.SmoothAmount;
            JitterAmount      = s.JitterAmount;
            MaxDegreesPerTick = s.MaxDegreesPerTick;
        }

        private void UpdateConfig()
        {
            var s = ConfigManager.Settings;
            s.AimbotEnabled     = AimbotEnabled;
            s.AimOnTeam         = AimOnTeam;
            s.MaxFov            = MaxFov;
            s.SmoothAmount      = SmoothAmount;
            s.JitterAmount      = JitterAmount;
            s.MaxDegreesPerTick = MaxDegreesPerTick;
        }
    }

    // Configuration model
    public class AimbotConfig
    {
        public bool AimbotEnabled { get; set; } = true;
        public bool AimOnTeam { get; set; } = false;
        public float MaxFov { get; set; } = 15.0f;
        public float SmoothAmount { get; set; } = 0.1f;
        public float JitterAmount { get; set; } = 0.1f;
        public float MaxDegreesPerTick { get; set; } = 2.0f;
    }

    // Simple JSON config manager
    public static class ConfigManager
    {
        private static string _path = string.Empty;
        public static AimbotConfig Settings { get; private set; } = new AimbotConfig();

        public static void Initialize(string fileName)
        {
            _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        public static void Load()
        {
            if (File.Exists(_path))
            {
                try
                {
                    var json = File.ReadAllText(_path);
                    Settings = JsonSerializer.Deserialize<AimbotConfig>(json) ?? new AimbotConfig();
                }
                catch
                {
                    Settings = new AimbotConfig();
                }
            }
            else
            {
                Settings = new AimbotConfig();
                Save();
            }
        }

        public static void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(Settings, options);
                File.WriteAllText(_path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save config: {ex.Message}");
            }
        }
    }
}
