using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using DeathsTerminus.Enums;
using DeathsTerminus.NPCs;

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
                "Hey there, if you need anything, let me know, I am the Professional Dumbass.",
                "You circle bosses? I tried that once and was told I circled TOO hard... How do you circle TOO hard!?",
                "Sure, homing is nice, but have you considered aiming?",
                "Not to be racist or anything, but what the hell is oatmeal?",
                "WHAT DO YOU MEAN IM MISSING A SEMICOLON?! I'VE SCOURED THE ENTIRE oh wait, that's where I'm missing it.",

            };

            if (!NPC.downedMechBoss2)
            {
                Mod modFargosSouls = ModLoader.GetMod("FargowiltasSouls");
                if (modFargosSouls != null)
                {
                    dialogue.Add("If you see that metallic worm monstrosity.....Make sure you obliterate it for me!");
                }
            }

            int nurse = NPC.FindFirstNPC(NPCID.Nurse);
            if (nurse != -1)
            {
                
                dialogue.Add($"Hey, I'm gonna chill with {Main.npc[nurse].GivenName}, there might be something between us.");
            }

            int cata = NPC.FindFirstNPC(ModContent.NPCType<CataclysmicArmageddon>());
            if (cata != -1)
            {
                dialogue.Add("That Cata guy is freaking me out, he keeps making me fight stuff while grinning then laughs at me for losing.");
            }

            /*int terry = NPC.FindFirstNPC(ModContent.NPCType<Terry>());
            if (terry != -1)
            {
                dialogue.Add("Terry ask me the time earlier, When I told him it was 22:22 the dude started laughing then exploded");
            }*/

            /*int turing = NPC.FindFirstNPC(ModContent.NPCType<Turing>());
            if (turing != -1)
            {
                dialogue.Add("Don't talk to Turing unless you carry a thesaurus, trust me, I tried.");
            }*/

            if (Main.eclipse)
            {
                dialogue.Add("Where did the sun go? I could've sworn I put it right there.");
            }

            if (NPC.downedMoonlord)
            {
                dialogue.Add("I'm much stronger than I was last time, so don't expect me to go down as easily!");
            }
            else if (NPC.downedPlantBoss)
            {
                dialogue.Add("So I heard you wanted to fight. Talk to me when you're ready.");
            }

            return Main.rand.Next(dialogue);
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            if (!Main.LocalPlayer.HasItem(ItemID.RodofDiscord))
            {
                button = Language.GetTextValue("LegacyInterface.28");
                if (NPC.downedPlantBoss)
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

                string message = "Insert cool dialogue here";
                string message2 = "Astrum Genesis has awoken!";

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText(message, 255, 77, 23);
                    Main.NewText(message2, 175, 75, 255);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(message), new Color(255, 77, 23));
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
