using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DeathsTerminus.Items.Armor
{
	[AutoloadEquip(EquipType.Head)]
	public class Crystalhelmet : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crystal Helmet");
			Tooltip.SetDefault("15% Increased damage");
		}
		public override bool Autoload(ref string name)
		{
			return base.Autoload(ref name);
		}
		public override void SetDefaults()
		{
			item.width = 18;
			item.height = 18;
			item.value = 10;
			item.rare = 5;
			item.defense = 8;
		}

		public override void UpdateEquip(Player player)
		{
			player.allDamage += 0.15f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.CrystalShard, 12);
			recipe.AddIngredient(ItemID.TitaniumBar, 8);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}