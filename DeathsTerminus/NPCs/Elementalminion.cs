using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DeathsTerminus.NPCs
{
    public class Elementalminion : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Elemie");
        }
        public override void SetDefaults()
        {
            npc.width = 50;
            npc.width = 40;
            npc.damage = 30;
            npc.defense = 20;
            npc.lifeMax = 150;
            npc.HitSound = SoundID.Item9;
            npc.DeathSound = SoundID.NPCHit5;
            npc.value = 60f;
            npc.knockBackResist = 0.5f;
            npc.aiStyle = 44;
            Main.npcFrameCount[npc.type] = 3;
            aiType = NPCID.Harpy;
            animationType = NPCID.Harpy;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.spawnTileY < Main.rockLayer && !Main.dayTime ? 0.5f : 0.5f;
        }
        public override void FindFrame(int frameHeight)
        {
            npc.frameCounter -= 0.5f;
            npc.frameCounter %= Main.npcFrameCount[npc.type];
            int frame = (int)npc.frameCounter;
            npc.frame.Y = frame * frameHeight;

            npc.spriteDirection = npc.direction;
        }
        public override void NPCLoot()
        {
            {
                Item.NewItem((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height, mod.ItemType("Elementalessence"));
            }
        }

    }
}