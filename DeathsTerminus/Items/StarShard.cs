using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace DeathsTerminus.Items
{
    public class StarShard : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Star Shar");
            Tooltip.SetDefault("Summons a Lost Star");
        }
        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.BoneKey);  // Clone the defaults of Zephyr Fish
            item.shoot = mod.ProjectileType("LostStar");
            item.buffType = mod.BuffType("LostStarBuff");
        }

        public override void UseStyle(Player player)
        {
            if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
            {
                player.AddBuff(item.buffType, 3600, true);
            }
        }
    }
}