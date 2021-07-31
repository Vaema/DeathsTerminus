using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using DeathsTerminus.NPCs.AstrumBoss;
using DeathsTerminus.NPCs;

namespace DeathsTerminus.Items
{
    public class Genesis : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Genesis");
            Tooltip.SetDefault("Challenges Astrum Genesis");
            ItemID.Sets.ItemNoGravity[item.type] = true;
        }
        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 26;
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
            if (NPC.downedPlantBoss)
            {
                return !NPC.AnyNPCs(mod.NPCType("AstrumBoss"));
            }
            else
                return false;
        }
        public override bool UseItem(Player player)
        {
            int astrum = NPC.FindFirstNPC(ModContent.NPCType<AstrumGenesis>());

            if (astrum > -1 && Main.npc[astrum].active)
            {
                string message = "Astrum Genesis has awoken!";

                Main.npc[astrum].Transform(ModContent.NPCType<AstrumBoss>());

                if (Main.netMode == NetmodeID.SinglePlayer)
                    Main.NewText(message, 175, 75, 255);
                else if (Main.netMode == NetmodeID.Server)
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(message), new Color(175, 75, 255));
            }
            else
            {
                NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<AstrumBoss>());
            }

            Main.PlaySound(new Terraria.Audio.LegacySoundStyle(SoundID.Zombie, 105));

            return true;
        }
    }
}