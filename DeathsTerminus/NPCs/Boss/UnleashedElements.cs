using System;
using System.IO;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using DeathsTerminus.Enums;
using SoundType = Terraria.ModLoader.SoundType;

namespace DeathsTerminus.NPCs.Boss
{
    [AutoloadBossHead]
    public class UnleashedElements : ModNPC
    {
        public const int NPC_STATE = 0;
        public const int PAUSE_TIMER = 1;
        public const int COOLDOWN = 2;
        public const int LASER_TIMER = 3;

        public const string PROJECT_TYPE = "Elementdisc";

        public enum States
        {
            FloatTowards,
            DashingFirstTime,
            DashingSecondTime,
            DashingThirdTime,
            //MovingToPlayer,
            //CirclingAroundPlayerFirstTime,
            //CirclingAroundPlayerSecondTime,
            //CirclingAroundPlayerThirdTime,
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Unleased Elements");
        }
        public override void SetDefaults()
        {
            npc.aiStyle = (int)AIStyles.CustomAI;
            npc.lifeMax = 15000;
            npc.damage = 50;
            npc.defense = 30;
            npc.knockBackResist = 0f;
            npc.width = 100;
            npc.height = 100;
            animationType = NPCID.DemonEye;
            Main.npcFrameCount[npc.type] = 2;
            npc.value = Item.buyPrice(0, 40, 75, 45);
            npc.npcSlots = 1f;
            npc.boss = true;
            npc.lavaImmune = true;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.HitSound = SoundID.Item9;
            npc.DeathSound = SoundID.NPCDeath59;
            npc.buffImmune[24] = true;
            music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/Fall");

            npc.ai[NPC_STATE] = (int)States.FloatTowards;
        }

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            npc.lifeMax = (int)(npc.lifeMax * 0.579f * bossLifeScale);
            npc.damage = (int)(npc.damage * 0.6f);
        }
        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.HealingPotion;
        }
        public override void AI()
        {
            switch ((States)npc.ai[NPC_STATE])
            {
                case States.FloatTowards:
                    npc.TargetClosest();
                    FloatTowardsPlayer();
                    break;
                case States.DashingFirstTime:
                    Dash(States.DashingSecondTime);
                    break;
                case States.DashingSecondTime:
                    Dash(States.DashingThirdTime);
                    break;    
                case States.DashingThirdTime:
                    Dash(States.FloatTowards);
                    break;
                //case States.MovingToPlayer:
                //break;
                //case States.CirclingAroundPlayerFirstTime:
                //break;
                //case States.CirclingAroundPlayerSecondTime:
                //break;
                //case States.CirclingAroundPlayerThirdTime:
                //break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            //FloatsTowardsPlayer
            //Dash at the player
            //Dash at the player
            //Dash at the player
            //Start back at the top
            //Shoot lasers the entire time
        }

        private void FloatTowardsPlayer()
        {         
            var player = Main.player[npc.target];
            if (!player.active || player.dead)
            {
                npc.TargetClosest(false);
                player = Main.player[npc.target];
                if (!player.active || player.dead)
                {
                    npc.velocity = new Vector2(0f, 10f);
                    if (npc.timeLeft > 10)
                    {
                        npc.timeLeft = 10;
                    }
                    return;
                }
            }
            npc.ai[COOLDOWN]++;
            const float turnResistance = 100f; //the larger this is, the slower the npc will turn
            const float speed = 20f;

            if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                npc.TargetClosest(true);
            }
            npc.netUpdate = true;

            var moveTo = player.Center;

            var move = moveTo - npc.Center;
            var magnitude = Math.Sqrt(move.X * move.X + move.Y * move.Y);

            if (magnitude > speed)
            {
                move *= (speed / (float)magnitude);
            }

            move = (npc.velocity * turnResistance + move) / (turnResistance + 1f);
            magnitude = Math.Sqrt(move.X * move.X + move.Y * move.Y);

            if (magnitude > speed)
            {
                move *= (speed / (float)magnitude);
            }

            npc.velocity = move;

            FireLaser(player);

            if (magnitude >= 50 || npc.ai[COOLDOWN] <= 180) return;

            npc.ai[NPC_STATE] = (int)States.DashingFirstTime;
            npc.ai[PAUSE_TIMER] = 30;
        }

        private void FireLaser(Player player)
        {
            const float speed = 30f;
            const int damage = 30;
            var terraBladeSoundId = SoundID.Item60;

            npc.ai[LASER_TIMER]++;
            if (npc.ai[LASER_TIMER] < 30) return;

            var npcCenter = new Vector2(npc.position.X + (npc.width * 0.5f), npc.position.Y + (npc.height * 0.5f));
            var playerCenter = new Vector2(player.position.X + (player.width * 0.5f), player.position.Y + (player.height * 0.5f));

            Main.PlaySound(terraBladeSoundId, (int)npc.position.X, (int)npc.position.Y);

            var rotation = (float)Math.Atan2(npcCenter.Y - playerCenter.Y, npcCenter.X - playerCenter.X);
            var verticalSpeed = (float)(Math.Cos(rotation) * speed) * -1;
            var horizontalSpeed = (float)(Math.Sin(rotation) * speed) * -1;
            var type = mod.ProjectileType(PROJECT_TYPE);

            Projectile.NewProjectile(npcCenter.X, npcCenter.Y, verticalSpeed, horizontalSpeed, type, damage, 0f, 0);

            npc.ai[LASER_TIMER] = 0;
        }

        private void Dash(States nextState)
        {
            const float speed = 30f; //Charging is fast.

            npc.ai[PAUSE_TIMER]++;

            if (npc.ai[PAUSE_TIMER] <= 45)
            {
                return; //Wait for 45 ticks
            }

            var player = Main.player[npc.target];
            var moveTo = player.Center; //This player is the same that was retrieved in the targeting section.

            var move = moveTo - npc.Center;
            var magnitude = Math.Sqrt(move.X * move.X + move.Y * move.Y);
            move *= (speed / (float)magnitude);
            npc.velocity = move;
            npc.ai[PAUSE_TIMER] = 0;
            npc.ai[NPC_STATE] = (int)nextState;
            npc.ai[COOLDOWN] = 0;
        }
    }
}