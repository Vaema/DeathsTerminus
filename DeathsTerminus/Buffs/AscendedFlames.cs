using Terraria;
using Terraria.ModLoader;

namespace DeathsTerminus.Buffs
{
    public class AscendedFlames : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Ascended Flames");
            Description.SetDefault("The power of the godly fire compels you");
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<DTPlayer>().ascendedFlames = true;
            player.onFire = true;
            player.onFire2 = true;
            player.onFrostBurn = true;
            player.burned = true;
            player.bleed = true;
        }
    }
}
