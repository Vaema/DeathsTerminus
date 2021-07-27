using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace DeathsTerminus.Items.Weapons
{
    public class Lazarus_Hellbow : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Lazarus Hellbow");
            Tooltip.SetDefault("Four stages of the game combined into one.");
        }

        public override void SetDefaults()
        {
            item.damage = 150;
            item.ranged = true;
            item.width = 22;
            item.height = 49;
            item.maxStack = 1;
            item.useTime = 2;
            item.useAnimation = 10;
            item.useStyle = 5;
            item.knockBack = 5;
            item.value = 12000;
            item.rare = 11;
            item.UseSound = SoundID.Item5;
            item.noMelee = true;
            item.shoot = 1;
            item.useAmmo = AmmoID.Arrow;
            item.shootSpeed = 20f;
            item.autoReuse = true;
            item.crit = 90;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.HellwingBow);
            recipe.AddIngredient(ItemID.Tsunami);
            recipe.AddIngredient(ItemID.DaedalusStormbow);
            recipe.AddIngredient(ItemID.Phantasm);
            recipe.AddIngredient(ItemID.LunarBar, 50);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}