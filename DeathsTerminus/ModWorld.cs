using DeathsTerminus.Items;
using DeathsTerminus.NPCs;
using DeathsTerminus.Tiles;
using DeathsTerminus.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;

namespace DeathsTerminus
{
    public class DTWorld : ModWorld
    {
        private const int saveVersion = 0;

        public static bool bossFlawless;

        public override void Initialize()
        {
            bossFlawless = false;
        }

        public override TagCompound Save()
        {
            var flawless = new List<string>();
            if (bossFlawless)
            {
                flawless.Add("true");
            }


            return new TagCompound
            {
                ["flawless"] = flawless
            };
        }

        public override void Load(TagCompound tag)
        {
            var flawless = tag.GetList<string>("flawless");
            bossFlawless = flawless.Contains("true");
        }

        public override void LoadLegacy(BinaryReader reader)
        {
            int loadVersion = reader.ReadInt32();
            if (loadVersion == 0)
            {
                BitsByte flags = reader.ReadByte();
                bossFlawless = flags[0];
            }
            else
            {
                mod.Logger.WarnFormat("DeathsTerminus: Unknown loadVersion: {0}", loadVersion);
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            var flags = new BitsByte();
            flags[0] = bossFlawless;
            writer.Write(flags);
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            bossFlawless = flags[0];
            
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight)
        {
            int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
            if (ShiniesIndex == 1)
            {
                return;
            }
            tasks.Insert(ShiniesIndex + 1, new PassLegacy("Custom Mod Ores", delegate (GenerationProgress progress)
             {
                 progress.Message = "Custom Mod Ores";

                 for (int k = 0; k < (int)((double)(Main.maxTilesX * Main.maxTilesY) * 6E-05); k++)
                 {
                     WorldGen.TileRunner(WorldGen.genRand.Next(0, Main.maxTilesX), WorldGen.genRand.Next((int)WorldGen.worldSurfaceLow, Main.maxTilesY), (double)WorldGen.genRand.Next(3, 6), WorldGen.genRand.Next(2, 6), mod.TileType("BadassiumTile"), false, 0f, 0f, false, true);
                 }
             }));
        }
    }
}