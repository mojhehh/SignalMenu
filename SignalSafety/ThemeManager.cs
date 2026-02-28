using System;
using System.Collections.Generic;
using UnityEngine;

namespace SignalMenu.SignalSafety
{
    public static class ThemeManager
    {
        public struct Swatch
        {
            public string Name;
            public Color PanelColor;
            public Color AccentColor;
            public Color AccentGlow;
            public Color ButtonIdle;
            public Color ButtonActive;
            public Color TabIdle;
            public Color TabSelected;
            public Color TextPrimary;
            public Color TextDim;
            public Color PointerColor;
            public Color DividerColor;
        }

        private static readonly List<Swatch> _palettes = new List<Swatch>();
        private static float _rainbowHue;

        public static Swatch CurrentTheme { get; private set; }

        static ThemeManager()
        {
            InitPalettes();
            LoadPalette(0);
        }

        private static void InitPalettes()
        {
            _palettes.Clear();

            _palettes.Add(new Swatch
            {
                Name = "Default Cyan",
                PanelColor = new Color(0.06f, 0.07f, 0.12f, 0.94f),
                AccentColor = new Color(0.2f, 0.75f, 0.95f, 1f),
                AccentGlow = new Color(0.1f, 0.5f, 0.8f, 0.6f),
                ButtonIdle = new Color(0.1f, 0.12f, 0.18f, 0.9f),
                ButtonActive = new Color(0.12f, 0.55f, 0.7f, 0.95f),
                TabIdle = new Color(0.08f, 0.09f, 0.14f, 0.9f),
                TabSelected = new Color(0.12f, 0.55f, 0.7f, 0.95f),
                TextPrimary = new Color(0.92f, 0.94f, 0.96f),
                TextDim = new Color(0.5f, 0.55f, 0.6f),
                PointerColor = new Color(0.3f, 0.85f, 1f, 0.8f),
                DividerColor = new Color(0.2f, 0.6f, 0.85f, 0.3f),
            });

            _palettes.Add(new Swatch
            {
                Name = "Midnight Purple",
                PanelColor = new Color(0.08f, 0.05f, 0.14f, 0.94f),
                AccentColor = new Color(0.65f, 0.3f, 1f, 1f),
                AccentGlow = new Color(0.45f, 0.15f, 0.8f, 0.6f),
                ButtonIdle = new Color(0.12f, 0.08f, 0.2f, 0.9f),
                ButtonActive = new Color(0.4f, 0.15f, 0.7f, 0.95f),
                TabIdle = new Color(0.1f, 0.06f, 0.16f, 0.9f),
                TabSelected = new Color(0.4f, 0.15f, 0.7f, 0.95f),
                TextPrimary = new Color(0.92f, 0.88f, 1f),
                TextDim = new Color(0.55f, 0.45f, 0.65f),
                PointerColor = new Color(0.7f, 0.4f, 1f, 0.8f),
                DividerColor = new Color(0.5f, 0.2f, 0.8f, 0.3f),
            });

            _palettes.Add(new Swatch
            {
                Name = "Blood Red",
                PanelColor = new Color(0.1f, 0.04f, 0.04f, 0.94f),
                AccentColor = new Color(1f, 0.2f, 0.2f, 1f),
                AccentGlow = new Color(0.8f, 0.1f, 0.1f, 0.6f),
                ButtonIdle = new Color(0.15f, 0.06f, 0.06f, 0.9f),
                ButtonActive = new Color(0.6f, 0.1f, 0.1f, 0.95f),
                TabIdle = new Color(0.12f, 0.05f, 0.05f, 0.9f),
                TabSelected = new Color(0.6f, 0.1f, 0.1f, 0.95f),
                TextPrimary = new Color(1f, 0.92f, 0.92f),
                TextDim = new Color(0.6f, 0.4f, 0.4f),
                PointerColor = new Color(1f, 0.3f, 0.3f, 0.8f),
                DividerColor = new Color(0.8f, 0.15f, 0.15f, 0.3f),
            });

            _palettes.Add(new Swatch
            {
                Name = "Emerald Green",
                PanelColor = new Color(0.04f, 0.1f, 0.06f, 0.94f),
                AccentColor = new Color(0.2f, 1f, 0.5f, 1f),
                AccentGlow = new Color(0.1f, 0.7f, 0.3f, 0.6f),
                ButtonIdle = new Color(0.06f, 0.14f, 0.08f, 0.9f),
                ButtonActive = new Color(0.1f, 0.55f, 0.25f, 0.95f),
                TabIdle = new Color(0.05f, 0.12f, 0.07f, 0.9f),
                TabSelected = new Color(0.1f, 0.55f, 0.25f, 0.95f),
                TextPrimary = new Color(0.9f, 1f, 0.94f),
                TextDim = new Color(0.4f, 0.6f, 0.45f),
                PointerColor = new Color(0.3f, 1f, 0.5f, 0.8f),
                DividerColor = new Color(0.15f, 0.7f, 0.35f, 0.3f),
            });

            _palettes.Add(new Swatch
            {
                Name = "Solar Orange",
                PanelColor = new Color(0.12f, 0.07f, 0.03f, 0.94f),
                AccentColor = new Color(1f, 0.6f, 0.1f, 1f),
                AccentGlow = new Color(0.9f, 0.4f, 0.05f, 0.6f),
                ButtonIdle = new Color(0.16f, 0.1f, 0.04f, 0.9f),
                ButtonActive = new Color(0.65f, 0.35f, 0.05f, 0.95f),
                TabIdle = new Color(0.14f, 0.08f, 0.03f, 0.9f),
                TabSelected = new Color(0.65f, 0.35f, 0.05f, 0.95f),
                TextPrimary = new Color(1f, 0.95f, 0.88f),
                TextDim = new Color(0.6f, 0.5f, 0.35f),
                PointerColor = new Color(1f, 0.65f, 0.2f, 0.8f),
                DividerColor = new Color(0.9f, 0.5f, 0.1f, 0.3f),
            });

            _palettes.Add(new Swatch
            {
                Name = "Frost White",
                PanelColor = new Color(0.14f, 0.15f, 0.17f, 0.94f),
                AccentColor = new Color(0.85f, 0.9f, 1f, 1f),
                AccentGlow = new Color(0.6f, 0.7f, 0.85f, 0.6f),
                ButtonIdle = new Color(0.18f, 0.19f, 0.22f, 0.9f),
                ButtonActive = new Color(0.35f, 0.4f, 0.5f, 0.95f),
                TabIdle = new Color(0.16f, 0.17f, 0.2f, 0.9f),
                TabSelected = new Color(0.35f, 0.4f, 0.5f, 0.95f),
                TextPrimary = new Color(0.95f, 0.97f, 1f),
                TextDim = new Color(0.6f, 0.65f, 0.7f),
                PointerColor = new Color(0.9f, 0.95f, 1f, 0.8f),
                DividerColor = new Color(0.7f, 0.75f, 0.85f, 0.3f),
            });

            _palettes.Add(new Swatch
            {
                Name = "Neon Pink",
                PanelColor = new Color(0.1f, 0.04f, 0.08f, 0.94f),
                AccentColor = new Color(1f, 0.2f, 0.7f, 1f),
                AccentGlow = new Color(0.85f, 0.1f, 0.5f, 0.6f),
                ButtonIdle = new Color(0.14f, 0.06f, 0.1f, 0.9f),
                ButtonActive = new Color(0.6f, 0.1f, 0.4f, 0.95f),
                TabIdle = new Color(0.12f, 0.05f, 0.09f, 0.9f),
                TabSelected = new Color(0.6f, 0.1f, 0.4f, 0.95f),
                TextPrimary = new Color(1f, 0.9f, 0.95f),
                TextDim = new Color(0.6f, 0.4f, 0.5f),
                PointerColor = new Color(1f, 0.3f, 0.75f, 0.8f),
                DividerColor = new Color(0.9f, 0.15f, 0.55f, 0.3f),
            });

            _palettes.Add(new Swatch
            {
                Name = "Gold Luxury",
                PanelColor = new Color(0.08f, 0.07f, 0.04f, 0.94f),
                AccentColor = new Color(1f, 0.84f, 0f, 1f),
                AccentGlow = new Color(0.85f, 0.7f, 0f, 0.6f),
                ButtonIdle = new Color(0.12f, 0.1f, 0.05f, 0.9f),
                ButtonActive = new Color(0.5f, 0.42f, 0f, 0.95f),
                TabIdle = new Color(0.1f, 0.09f, 0.04f, 0.9f),
                TabSelected = new Color(0.5f, 0.42f, 0f, 0.95f),
                TextPrimary = new Color(1f, 0.97f, 0.85f),
                TextDim = new Color(0.6f, 0.55f, 0.35f),
                PointerColor = new Color(1f, 0.88f, 0.15f, 0.8f),
                DividerColor = new Color(0.85f, 0.72f, 0f, 0.3f),
            });

            _palettes.Add(new Swatch
            {
                Name = "Stealth Black",
                PanelColor = new Color(0.03f, 0.03f, 0.03f, 0.96f),
                AccentColor = new Color(0.4f, 0.4f, 0.4f, 1f),
                AccentGlow = new Color(0.25f, 0.25f, 0.25f, 0.6f),
                ButtonIdle = new Color(0.06f, 0.06f, 0.06f, 0.9f),
                ButtonActive = new Color(0.2f, 0.2f, 0.2f, 0.95f),
                TabIdle = new Color(0.05f, 0.05f, 0.05f, 0.9f),
                TabSelected = new Color(0.2f, 0.2f, 0.2f, 0.95f),
                TextPrimary = new Color(0.7f, 0.7f, 0.7f),
                TextDim = new Color(0.35f, 0.35f, 0.35f),
                PointerColor = new Color(0.5f, 0.5f, 0.5f, 0.8f),
                DividerColor = new Color(0.3f, 0.3f, 0.3f, 0.3f),
            });

            _palettes.Add(new Swatch
            {
                Name = "Ocean Blue",
                PanelColor = new Color(0.03f, 0.06f, 0.14f, 0.94f),
                AccentColor = new Color(0.1f, 0.5f, 1f, 1f),
                AccentGlow = new Color(0.05f, 0.35f, 0.85f, 0.6f),
                ButtonIdle = new Color(0.05f, 0.08f, 0.2f, 0.9f),
                ButtonActive = new Color(0.06f, 0.3f, 0.65f, 0.95f),
                TabIdle = new Color(0.04f, 0.07f, 0.16f, 0.9f),
                TabSelected = new Color(0.06f, 0.3f, 0.65f, 0.95f),
                TextPrimary = new Color(0.88f, 0.93f, 1f),
                TextDim = new Color(0.4f, 0.5f, 0.65f),
                PointerColor = new Color(0.2f, 0.6f, 1f, 0.8f),
                DividerColor = new Color(0.1f, 0.4f, 0.85f, 0.3f),
            });

            _palettes.Add(new Swatch
            {
                Name = "Toxic Lime",
                PanelColor = new Color(0.05f, 0.08f, 0.03f, 0.94f),
                AccentColor = new Color(0.6f, 1f, 0f, 1f),
                AccentGlow = new Color(0.4f, 0.8f, 0f, 0.6f),
                ButtonIdle = new Color(0.08f, 0.12f, 0.04f, 0.9f),
                ButtonActive = new Color(0.3f, 0.55f, 0f, 0.95f),
                TabIdle = new Color(0.06f, 0.1f, 0.03f, 0.9f),
                TabSelected = new Color(0.3f, 0.55f, 0f, 0.95f),
                TextPrimary = new Color(0.92f, 1f, 0.85f),
                TextDim = new Color(0.5f, 0.6f, 0.35f),
                PointerColor = new Color(0.65f, 1f, 0.1f, 0.8f),
                DividerColor = new Color(0.5f, 0.85f, 0f, 0.3f),
            });

            _palettes.Add(new Swatch
            {
                Name = "Sunset",
                PanelColor = new Color(0.12f, 0.05f, 0.06f, 0.94f),
                AccentColor = new Color(1f, 0.45f, 0.2f, 1f),
                AccentGlow = new Color(0.9f, 0.3f, 0.15f, 0.6f),
                ButtonIdle = new Color(0.15f, 0.07f, 0.08f, 0.9f),
                ButtonActive = new Color(0.6f, 0.2f, 0.1f, 0.95f),
                TabIdle = new Color(0.13f, 0.06f, 0.07f, 0.9f),
                TabSelected = new Color(0.6f, 0.2f, 0.1f, 0.95f),
                TextPrimary = new Color(1f, 0.93f, 0.88f),
                TextDim = new Color(0.6f, 0.45f, 0.4f),
                PointerColor = new Color(1f, 0.5f, 0.25f, 0.8f),
                DividerColor = new Color(0.9f, 0.35f, 0.15f, 0.3f),
            });

            _palettes.Add(new Swatch
            {
                Name = "Matrix",
                PanelColor = new Color(0.01f, 0.04f, 0.01f, 0.96f),
                AccentColor = new Color(0f, 1f, 0f, 1f),
                AccentGlow = new Color(0f, 0.7f, 0f, 0.6f),
                ButtonIdle = new Color(0.02f, 0.08f, 0.02f, 0.9f),
                ButtonActive = new Color(0f, 0.45f, 0f, 0.95f),
                TabIdle = new Color(0.02f, 0.06f, 0.02f, 0.9f),
                TabSelected = new Color(0f, 0.45f, 0f, 0.95f),
                TextPrimary = new Color(0.5f, 1f, 0.5f),
                TextDim = new Color(0.2f, 0.5f, 0.2f),
                PointerColor = new Color(0f, 1f, 0f, 0.8f),
                DividerColor = new Color(0f, 0.8f, 0f, 0.3f),
            });

            _palettes.Add(new Swatch
            {
                Name = "Rainbow",
                PanelColor = new Color(0.05f, 0.05f, 0.08f, 0.94f),
                AccentColor = Color.HSVToRGB(0f, 0.9f, 1f),
                AccentGlow = Color.HSVToRGB(0f, 0.7f, 0.7f),
                ButtonIdle = new Color(0.08f, 0.08f, 0.12f, 0.9f),
                ButtonActive = Color.HSVToRGB(0f, 0.6f, 0.6f),
                TabIdle = new Color(0.06f, 0.06f, 0.1f, 0.9f),
                TabSelected = Color.HSVToRGB(0f, 0.6f, 0.6f),
                TextPrimary = new Color(0.95f, 0.95f, 0.97f),
                TextDim = new Color(0.5f, 0.5f, 0.55f),
                PointerColor = Color.HSVToRGB(0f, 0.85f, 1f),
                DividerColor = Color.HSVToRGB(0f, 0.5f, 0.8f),
            });
        }

        public static int PaletteCount => _palettes.Count;
        public static string PaletteName(int index) => index >= 0 && index < _palettes.Count ? _palettes[index].Name : "Unknown";

        public static bool IsRainbow => SafetyConfig.ThemeIndex == _palettes.Count - 1 && !SafetyConfig.UseCustomTheme;

        public static void TickRainbow()
        {
            if (!IsRainbow) return;
            _rainbowHue += Time.deltaTime * 0.15f;
            if (_rainbowHue > 1f) _rainbowHue -= 1f;

            Color accent = Color.HSVToRGB(_rainbowHue, 0.9f, 1f);
            Color glow = Color.HSVToRGB(_rainbowHue, 0.7f, 0.7f);
            Color active = Color.HSVToRGB(_rainbowHue, 0.6f, 0.6f);

            CurrentTheme = new Swatch
            {
                Name = "Rainbow",
                PanelColor = new Color(0.05f, 0.05f, 0.08f, 0.94f),
                AccentColor = accent,
                AccentGlow = glow,
                ButtonIdle = new Color(0.08f, 0.08f, 0.12f, 0.9f),
                ButtonActive = active,
                TabIdle = new Color(0.06f, 0.06f, 0.1f, 0.9f),
                TabSelected = active,
                TextPrimary = new Color(0.95f, 0.95f, 0.97f),
                TextDim = new Color(0.5f, 0.5f, 0.55f),
                PointerColor = new Color(accent.r, accent.g, accent.b, 0.8f),
                DividerColor = new Color(accent.r * 0.7f, accent.g * 0.7f, accent.b * 0.7f, 0.3f),
            };
        }

        public static void LoadPalette(int index)
        {
            if (index < 0 || index >= _palettes.Count)
                index = 0;

            SafetyConfig.ThemeIndex = index;
            CurrentTheme = _palettes[index];
        }

        public static void StepPalette(bool forward = true)
        {
            int idx = SafetyConfig.ThemeIndex + (forward ? 1 : -1);
            if (idx >= _palettes.Count) idx = 0;
            if (idx < 0) idx = _palettes.Count - 1;
            LoadPalette(idx);
            SafetyConfig.Save();

        }

        public static void LoadUserPalette()
        {
            CurrentTheme = new Swatch
            {
                Name = "Custom",
                PanelColor = SafetyConfig.CustomPanelColor,
                AccentColor = SafetyConfig.CustomAccentColor,
                AccentGlow = new Color(SafetyConfig.CustomAccentColor.r * 0.6f, SafetyConfig.CustomAccentColor.g * 0.6f, SafetyConfig.CustomAccentColor.b * 0.6f, 0.6f),
                ButtonIdle = new Color(SafetyConfig.CustomPanelColor.r + 0.04f, SafetyConfig.CustomPanelColor.g + 0.05f, SafetyConfig.CustomPanelColor.b + 0.06f, 0.9f),
                ButtonActive = new Color(SafetyConfig.CustomAccentColor.r * 0.7f, SafetyConfig.CustomAccentColor.g * 0.7f, SafetyConfig.CustomAccentColor.b * 0.7f, 0.95f),
                TabIdle = new Color(SafetyConfig.CustomPanelColor.r + 0.02f, SafetyConfig.CustomPanelColor.g + 0.02f, SafetyConfig.CustomPanelColor.b + 0.02f, 0.9f),
                TabSelected = new Color(SafetyConfig.CustomAccentColor.r * 0.7f, SafetyConfig.CustomAccentColor.g * 0.7f, SafetyConfig.CustomAccentColor.b * 0.7f, 0.95f),
                TextPrimary = SafetyConfig.CustomTextColor,
                TextDim = new Color(SafetyConfig.CustomTextColor.r * 0.55f, SafetyConfig.CustomTextColor.g * 0.55f, SafetyConfig.CustomTextColor.b * 0.55f),
                PointerColor = new Color(SafetyConfig.CustomAccentColor.r, SafetyConfig.CustomAccentColor.g, SafetyConfig.CustomAccentColor.b, 0.8f),
                DividerColor = new Color(SafetyConfig.CustomAccentColor.r * 0.7f, SafetyConfig.CustomAccentColor.g * 0.7f, SafetyConfig.CustomAccentColor.b * 0.7f, 0.3f),
            };

        }

        public static Swatch ActivePalette()
        {
            return CurrentTheme;
        }
    }
}
