using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using DeathsTerminus.Enums;

namespace DeathsTerminus.NPCs
{
    [AutoloadHead]
    public class AstrumGenesis : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astrum");
            Main.npcFrameCount[npc.type] = 25;
            NPCID.Sets.ExtraFramesCount[npc.type] = 9;
            NPCID.Sets.AttackFrameCount[npc.type] = 4;
            NPCID.Sets.DangerDetectRange[npc.type] = 700;
            NPCID.Sets.AttackType[npc.type] = 0;
            NPCID.Sets.AttackTime[npc.type] = 90;
            NPCID.Sets.AttackAverageChance[npc.type] = 30;
        }

        public override void SetDefaults()
        {
            npc.townNPC = true;
            npc.friendly = true;
            npc.width = 18;
            npc.height = 40;
            drawOffsetY = 2;
            npc.aiStyle = (int)AIStyles.Passive;
            npc.damage = 10;
            npc.defense = 0;
            npc.lifeMax = 250;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.knockBackResist = 0.5f;
            animationType = NPCID.Guide;

            npc.buffImmune[BuffID.Suffocation] = true;
        }

        public override bool CanTownNPCSpawn(int numTownNPCs, int money)
        {

            return true;

        }

        public override string TownNPCName()
        {
            
            return "";
        }


        public override bool CanGoToStatue(bool toKingStatue) => toKingStatue;

        public override void AI()
        {
            npc.breath = 200;
            npc.width = 18;
            npc.height = 40;
        }

        public override string GetChat()
        {
            List<string> dialogue = new List<string>
            {
                "test",
            };


            return Main.rand.Next(dialogue);
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (!Main.LocalPlayer.HasItem(ItemID.RodofDiscord))
            {
                button = Language.GetTextValue("LegacyInterface.28");
                if (NPC.downedMoonlord)
                {
                    button2 = "Challenge";
                }
            }

        }

        public override void OnChatButtonClicked(bool firstButton, ref bool shop)
        {

            if (firstButton)
            {
                shop = true;
            }
            else
            {
                //Spawn Boss Here
                //npc.Transform(mod.NPCType("AstrumBoss"));

                string message = "Don't hold back now, or I'll hold it against you for the rest of your life... which won't be very long anyways.";
                string message2 = "Astrum Genesis has awoken!";

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText(message, 0, 76, 153);
                    Main.NewText(message2, 175, 75, 255);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(message), new Color(0, 76, 153));
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(message2), new Color(175, 75, 255));
                }

                Main.PlaySound(new Terraria.Audio.LegacySoundStyle(SoundID.Zombie, 105));
            }


        }

        public override void SetupShop(Chest shop, ref int nextSlot)
        {
            
        }

        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            npc.life = 0;
            npc.checkDead();
            return true;
        }
    }
}
