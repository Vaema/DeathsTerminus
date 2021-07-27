using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DeathsTerminus.Items.Armor
{
    [AutoloadEquip(EquipType.Legs)]
    public class Crystalleggings : ModItem
    {
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crystal Leggings");
			Tooltip.SetDefault("20% Increased movement speed\n10% Increased damage");
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
			item.defense = 10;
		}

		public override void UpdateEquip(Player player)
		{
			player.moveSpeed += 0.2f;
			player.allDamage += 0.1f;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.CrystalShard, 15);
			recipe.AddIngredient(ItemID.TitaniumBar, 10);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}