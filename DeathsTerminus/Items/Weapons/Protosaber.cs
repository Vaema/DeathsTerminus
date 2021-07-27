using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace DeathsTerminus.Items.Weapons
{
	public class Protosaber : ModItem
	{
		public override void SetStaticDefaults() 
		{
			DisplayName.SetDefault("Protosaber");
			Tooltip.SetDefault("We don't know what we were doing so we are just messing around with crap.");
		}

		public override void SetDefaults() 
		{
			item.damage = 200;
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.useTime = 7;
			item.useAnimation = 7;
			item.useStyle = 1;
			item.knockBack = 6;
			item.value = 10000;
			item.rare = 11;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
		}

		public override void AddRecipes() 
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.FallenStar, 50);
			recipe.AddIngredient(ItemID.FetidBaghnakhs);
			recipe.AddIngredient(ItemID.LunarBar, 100);
			recipe.AddIngredient(ItemID.FragmentSolar, 50);
			recipe.AddIngredient(ItemID.FragmentStardust, 50);
			recipe.AddIngredient(ItemID.FragmentNebula, 50);
			recipe.AddIngredient(ItemID.FragmentVortex, 50);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}