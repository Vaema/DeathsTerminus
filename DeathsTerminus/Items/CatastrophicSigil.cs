using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using DeathsTerminus.NPCs.CataBoss;
using DeathsTerminus.NPCs;

namespace DeathsTerminus.Items
{
    public class CatastrophicSigil : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Catastrophic Sigil");
            Tooltip.SetDefault("Challenges the master mothman");
            Main.RegisterItemAnimation(item.type, new DrawAnimationVertical(6, 4));
            ItemID.Sets.ItemNoGravity[item.type] = true;
        }
        public override void SetDefaults()
        {
            item.width = 66;
            item.height = 96;
            item.maxStack = 1;
            item.value = 0;
            item.rare = ItemRarityID.Expert;
            item.useAnimation = 30;
            item.useTime = 30;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.consumable = false;
            ItemID.Sets.ItemNoGravity[item.type] = true;
        }
        public override bool CanUseItem(Player player)
        {
            if (NPC.downedMoonlord)
            {
                return !NPC.AnyNPCs(mod.NPCType("CataBoss"));
            } else
                return false;
        }
        public override bool UseItem(Player player)
        {
            int cata = NPC.FindFirstNPC(ModContent.NPCType<CataclysmicArmageddon>());

            if (cata > -1 && Main.npc[cata].active)
            {
                string message = "Cataclysmic Armageddon has awoken!";

                Main.npc[cata].Transform(ModContent.NPCType<CataBoss>());

                if (Main.netMode == NetmodeID.SinglePlayer)
                    Main.NewText(message, 175, 75, 255);
                else if (Main.netMode == NetmodeID.Server)
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(message), new Color(175, 75, 255));
            }
            else
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<CataBoss>());
            }

            return true;
        }
    }
}