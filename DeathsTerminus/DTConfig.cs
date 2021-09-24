using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace DeathsTerminus
{
    public class DTConfig : ModConfig
    {
        public static DTConfig Instance;

        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(true)]
        [Header("Boss-related configurations")]
        [Label("Boss Titles")]
        [Tooltip("Turns on/off boss titles when summoning a boss.")]
        public bool BossTitleText;
    }
}
