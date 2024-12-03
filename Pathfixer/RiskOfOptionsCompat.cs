using BepInEx.Bootstrap;
using RiskOfOptions;
using System.IO;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using RiskOfOptions.Options;
using System.Linq;

namespace Pathfixer
{
    static class RiskOfOptionsCompat
    {
        public static bool Enabled => Chainloader.PluginInfos.ContainsKey(RiskOfOptions.PluginInfo.PLUGIN_GUID);

        static Sprite _iconSprite;

        const string MOD_GUID = PathfixerPlugin.PluginGUID;
        const string MOD_NAME = PathfixerPlugin.PluginName;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddOptions()
        {
            ModSettingsManager.SetModDescription($"Options for {MOD_NAME}", MOD_GUID, MOD_NAME);

            Sprite icon = tryGetIcon();
            if (icon)
            {
                ModSettingsManager.SetModIcon(icon, MOD_GUID, MOD_NAME);
            }

            if (HalcyonitePathingFix.EnableHalcyonitePathingChanges != null)
            {
                ModSettingsManager.AddOption(new CheckBoxOption(HalcyonitePathingFix.EnableHalcyonitePathingChanges), MOD_GUID, MOD_NAME);
            }
        }

        static Sprite tryGetIcon()
        {
            if (!_iconSprite)
            {
                _iconSprite = tryGenerateIcon();

                if (!_iconSprite)
                {
                    Log.Warning("Failed to get config icon");
                }
            }

            return _iconSprite;
        }

        static Sprite tryGenerateIcon()
        {
            DirectoryInfo searchDir = new DirectoryInfo(Path.GetDirectoryName(PathfixerPlugin.Instance.Info.Location));
            FileInfo iconFile = findIconFileRecursive(searchDir);
            if (iconFile == null)
                return null;

            byte[] imageBytes;
            try
            {
                imageBytes = File.ReadAllBytes(iconFile.FullName);
            }
            catch (Exception e)
            {
                Log.Error_NoCallerPrefix($"Failed to read icon file '{iconFile.FullName}': {e}");
                return null;
            }

            Texture2D iconTexture = new Texture2D(256, 256);
            iconTexture.name = $"tex{PathfixerPlugin.PluginName}Icon";
            if (!iconTexture.LoadImage(imageBytes))
            {
                Log.Error("Failed to load icon into texture");
                return null;
            }

            Sprite icon = Sprite.Create(iconTexture, new Rect(0f, 0f, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
            icon.name = $"{PathfixerPlugin.PluginName}Icon";

            return icon;
        }

        static FileInfo findIconFileRecursive(DirectoryInfo dir)
        {
            if (dir == null)
                return null;

            if (string.Equals(dir.FullName, BepInEx.Paths.PluginPath, StringComparison.OrdinalIgnoreCase))
            {
                Log.Debug($"Icon search reached plugin directory");
                return null;
            }

            Log.Debug($"Searching '{dir.FullName}' for icon");

            FileInfo iconFile = dir.EnumerateFiles("icon.png", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (iconFile != null)
            {
                Log.Debug($"Icon file found at: {iconFile.FullName}");
                return iconFile;
            }

            return findIconFileRecursive(dir.Parent);
        }
    }
}
