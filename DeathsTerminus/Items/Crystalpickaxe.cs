using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DeathsTerminus.Items
{
    public class Crystalpickaxe : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystal Pickaxe");
            Tooltip.SetDefault("Light the caverns as you scour for various loot");
        }
        public override void SetDefaults()
        {
            item.damage = 25;
            item.melee = true;
            item.width = 32;
            item.height = 32;
            item.useTime = 3;
            item.useAnimation = 10;
            item.pick = 190;
            item.useStyle = 1;
            item.knockBack = 6;
            item.value = 10;
            item.rare = 5;
            item.UseSound = SoundID.Item1;
            item.autoReuse = true;
            item.useTurn = true;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Pearlwood, 15);
            recipe.AddIngredient(ItemID.CrystalShard, 30);
            recipe.AddIngredient(ItemID.TitaniumBar, 5);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}