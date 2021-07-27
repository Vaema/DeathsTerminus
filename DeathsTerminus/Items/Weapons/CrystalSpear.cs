using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DeathsTerminus.Items.Weapons
{
    public class CrystalSpear : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystal Spear");
            Tooltip.SetDefault("YEET");
        }
        public override void SetDefaults()
        {
            item.damage = 70;
            item.melee = true;
            item.width = 38;
            item.height = 38;
            item.scale = 1.1f;
            item.maxStack = 1;
            item.useTime = 30;
            item.useAnimation = 30;
            item.knockBack = 4f;
            item.UseSound = SoundID.Item1;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.useTurn = true;
            item.useStyle = 5;
            item.value = 10000;
            item.rare = 5;
            item.shoot = mod.ProjectileType("CrystalSpearjectile");
            item.shootSpeed = 5f;
            item.autoReuse = true;
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[item.shoot] < 1;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.CrystalShard, 14);
            recipe.AddIngredient(ItemID.TitaniumBar, 6);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}