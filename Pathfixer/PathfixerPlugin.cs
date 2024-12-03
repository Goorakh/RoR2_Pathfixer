using BepInEx;
using System.Diagnostics;

namespace Pathfixer
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class PathfixerPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "Pathfixer";
        public const string PluginVersion = "1.0.0";

        static PathfixerPlugin _instance;
        internal static PathfixerPlugin Instance => _instance;

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            SingletonHelper.Assign(ref _instance, this);

            Log.Init(Logger);

            CharacterFootPositionFix.Init();
            HalcyonitePathingFix.Init(Config);

            if (RiskOfOptionsCompat.Enabled)
            {
                RiskOfOptionsCompat.AddOptions();
            }

            stopwatch.Stop();
            Log.Message_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalMilliseconds:F0}ms");
        }
    }
}
