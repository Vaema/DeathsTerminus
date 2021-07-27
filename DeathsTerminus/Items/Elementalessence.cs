using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DeathsTerminus.Items
{
    public class Elementalessence : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Elemental Essence");
            Tooltip.SetDefault("The material that holds the universe together");
        }
        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.maxStack = 999;
            item.value = 100;
            item.rare = ItemRarityID.Expert;
            item.useAnimation = 30;
            item.useTime = 30;
            item.useStyle = 4;
            item.consumable = true;
        }
        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(mod.NPCType("Eymis"));
        }
        public override bool UseItem(Player player)
        {
            NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("UnleashedElements"));
            Main.PlaySound(15, (int)player.position.X, (int)player.position.Y, 0);

            return true;
        }
    }
}