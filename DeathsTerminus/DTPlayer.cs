using DeathsTerminus.Buffs;
using DeathsTerminus.Items;
using DeathsTerminus.Projectiles;
using DeathsTerminus.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace DeathsTerminus
{
    public class DTPlayer : ModPlayer
    {
        public bool LostStar;

        public override void ResetEffects()
        {
            LostStar = false;
        }

        public override void Hurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active)
                {
                    Main.npc[i].GetGlobalNPC<DTGlobalNPC>().flawless = false;
                }
            }
        }
    }
}