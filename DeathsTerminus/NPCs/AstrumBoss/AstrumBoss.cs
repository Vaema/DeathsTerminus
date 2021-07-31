using System;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using DeathsTerminus.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace DeathsTerminus.NPCs.AstrumBoss
{
    [AutoloadBossHead]
    public class AstrumBoss : ModNPC
    {

        #region Defaults
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astrum Genesis");
        }

        public override void SetDefaults()
        {
            npc.aiStyle = (int)AIStyles.CustomAI;
            npc.width = 18;
            npc.height = 40;

            npc.defense = 0;
            npc.lifeMax = Main.expertMode ? 75000 : 100000;
            npc.HitSound = SoundID.NPCHit1;

            npc.damage = 160;
            npc.knockBackResist = 0f;

            npc.value = Item.buyPrice(platinum: 1);

            npc.npcSlots = 15f;
            npc.boss = true;
            //bossBag = ItemType<AstrumBossBag>();

            npc.lavaImmune = true;
            npc.noGravity = true;
            npc.noTileCollide = true;

            for (int i = 0; i < Main.maxBuffTypes; i++)
            {
                npc.buffImmune[i] = true;
            }

            music = MusicID.Boss2;

            //Find Music Mod
            Mod modMusic = ModLoader.GetMod("DeathsTerminusMusic");
            if (modMusic != null)
            {
                music = modMusic.GetSoundSlot(SoundType.Music, "Sounds/Music/idolatrizeWorld");
            }
        }

        #endregion
        public override void AI()
        {

            var player = Main.player[npc.target];
            if (!player.active || player.dead)
            {
                npc.TargetClosest(false);
                player = Main.player[npc.target];
                if (!player.active || player.dead)
                {
                    npc.Transform(mod.NPCType("AstrumGenesis"));
                }
            }

            if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead || !Main.player[npc.target].active)
            {
                npc.TargetClosest(true);
            }
            npc.netUpdate = true;


            switch (npc.ai[0])
            {
                case 0:
                    SpawnAnimation();
                    break;
                case 1:
                    SlashFromSide();
                    break;
                case 2:
                    ShootSword();
                    break;
                case 3:
                    npc.ai[0] = 0;
                    break;

            }
        }

        private void FlyToPoint(Vector2 goalPoint, Vector2 goalVelocity, float maxXAcc = 0.5f, float maxYAcc = 0.5f)
        {
            Vector2 goalOffset = goalPoint - goalVelocity - npc.Center;
            Vector2 relativeVelocity = npc.velocity - goalVelocity;

            //compute whether we'll overshoot or undershoot our X goal at our current velocity
            if (relativeVelocity.X * relativeVelocity.X / 2 / maxXAcc > Math.Abs(goalOffset.X) && (goalOffset.X > 0 ^ relativeVelocity.X < 0))
            {
                //overshoot
                npc.velocity.X += maxXAcc * (goalOffset.X > 0 ? -1 : 1);
            }
            else
            {
                //undershoot
                npc.velocity.X += maxXAcc * (goalOffset.X > 0 ? 1 : -1);
            }
            //compute whether we'll overshoot or undershoot our X goal at our current velocity
            if (relativeVelocity.Y * relativeVelocity.Y / 2 / maxYAcc > Math.Abs(goalOffset.Y) && (goalOffset.Y > 0 ^ relativeVelocity.Y < 0))
            {
                //overshoot
                npc.velocity.Y += maxYAcc * (goalOffset.Y > 0 ? -1 : 1);
            }
            else
            {
                //undershoot
                npc.velocity.Y += maxYAcc * (goalOffset.Y > 0 ? 1 : -1);
            }
        }

        //Fly to the side of the player
        private void SpawnAnimation()
        {
            Player player = Main.player[npc.target];
            if ((npc.Center - player.Center).Length() > 1000 && npc.ai[1] == 0)
            {
                npc.Center = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 1000;
            }

            npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;

            FlyToPoint(goalPosition, Vector2.Zero);

            npc.ai[1]++;
            if (npc.ai[1] == 60)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //This attack will make the boss fly to the side of the player and shoot an accelerating projectile 5 times
        private void SlashFromSide()
        {
            Player player = Main.player[npc.target];

            

            npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;

            FlyToPoint(goalPosition, Vector2.Zero);

            if (npc.ai[1] % 24 == 0)
            {
                Projectile.NewProjectile(npc.Center, (player.Center - npc.Center).SafeNormalize(Vector2.Zero) * 2, mod.ProjectileType("SlashWave"), 80, 0f, Main.myPlayer);
                Main.PlaySound(SoundID.Item60, npc.Center);
            }

            

            npc.ai[1]++;
            if (npc.ai[1] == 120)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //This will make swords fly straight up, and then shoot towards the player at an accelerating speed
        private void ShootSword()
        {

            Player player = Main.player[npc.target];


            npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;

            FlyToPoint(goalPosition, Vector2.Zero);

            if (npc.ai[1] % 30 == 0)
            {
                Projectile.NewProjectile(npc.Center, new Vector2(0f, 0f), mod.ProjectileType("PrismaticBlade"), 80, 0f, Main.myPlayer, 0, npc.target);
                Main.PlaySound(SoundID.Item8, npc.Center);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 180)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        #region Projectiles

        //Projectiles

        public class SlashWave : ModProjectile
        {

            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Slash Wave");
                Main.projFrames[projectile.type] = 6;

                ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
                ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            }

            public override void SetDefaults()
            {
                projectile.width = 38;
                projectile.height = 40;
                projectile.magic = true;
                projectile.penetrate = 1;
                projectile.hostile = true;
                projectile.friendly = false;
                projectile.tileCollide = false;
                projectile.ignoreWater = true;
                projectile.timeLeft = 120;
            }

            public override void AI()
            {
                projectile.velocity *= 1.1f;
                if (projectile.velocity.Length() > 50)
                {
                    projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * 50;
                }

                projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi;

                

            }

            public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
            {
                drawCacheProjsBehindNPCs.Add(index);
            }

            public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
            {
                Texture2D texture = Main.projectileTexture[projectile.type];

                Rectangle frame = texture.Frame(1, 6, 0, projectile.frame);
                SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

                for (int i = projectile.oldPos.Length - 1; i >= 0; i--)
                {
                    float alpha = (projectile.oldPos.Length - i) / (float)projectile.oldPos.Length;
                    spriteBatch.Draw(texture, projectile.oldPos[i] + projectile.Center - projectile.position - Main.screenPosition, frame, Color.White * alpha, projectile.oldRot[i], frame.Size() / 2, projectile.scale, effects, 0f);
                }

                return false;
            }

        }

        public class PrismaticBlade : ModProjectile
        {
            public override void SetStaticDefaults()
            {
                DisplayName.SetDefault("Prismatic Blade");
                Main.projFrames[projectile.type] = 1;

                ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
                ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            }

            public override void SetDefaults()
            {
                projectile.width = 94;
                projectile.height = 30;
                projectile.magic = true;
                projectile.penetrate = 1;
                projectile.hostile = true;
                projectile.friendly = false;
                projectile.tileCollide = false;
                projectile.ignoreWater = true;
                projectile.timeLeft = 120;
            }

            public override void AI()
            {
                Player player = Main.player[(int)projectile.ai[1]];

                if (projectile.ai[0] < 30)
                {
                    projectile.velocity = new Vector2(0f, -10f);
                    projectile.rotation = -MathHelper.Pi / 2;
                }
                
                
                
                if (projectile.ai[0] == 30)
                {
                    projectile.rotation = projectile.DirectionTo(player.Center).ToRotation();
                    projectile.velocity = projectile.DirectionTo(player.Center) * 10f;


                }

                if (projectile.ai[0] > 30)
                {
                    projectile.velocity *= 1.1f;

                    if (projectile.velocity.Length() > 50)
                    {
                        projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * 50;
                    }


                }

                projectile.ai[0]++;

            }

            public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
            {
                drawCacheProjsBehindNPCs.Add(index);
            }

            public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
            {
                Texture2D texture = Main.projectileTexture[projectile.type];
                Color color = Main.hslToRgb(projectile.ai[0] / 40f, 1f, 0.8f);

                Rectangle frame = texture.Frame(1, 1, 0, projectile.frame);
                SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

                for (int i = projectile.oldPos.Length - 1; i >= 0; i--)
                {
                    float alpha = (projectile.oldPos.Length - i) / (float)projectile.oldPos.Length;
                    spriteBatch.Draw(texture, projectile.oldPos[i] + projectile.Center - projectile.position - Main.screenPosition, frame, color * alpha, projectile.oldRot[i], frame.Size() / 2, projectile.scale, effects, 0f);
                }


                return false;
            }

        }



        #endregion


        //50% damage resistance
        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            damage /= 2;
            return true;
        }
    }
}