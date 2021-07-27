using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DeathsTerminus.Items.Armor
{
	[AutoloadEquip(EquipType.Body)]
	public class Crystalchestplate : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crystal Chestplate");
			Tooltip.SetDefault("20% Increased damage");
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
			item.defense = 15;
		}

		public override void UpdateEquip(Player player)
		{
			player.allDamage += 0.2f;
		}
		public static void setBonus(Player player)
		{
			player.setBonus = "Greatly increased movement speed";
			player.moveSpeed += 0.50f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.CrystalShard, 30);
			recipe.AddIngredient(ItemID.TitaniumBar, 15);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}