﻿using System;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using DeathsTerminus.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace DeathsTerminus.NPCs.CataBoss
{
    [AutoloadBossHead]
    public class CataBoss : ModNPC
    {
        //ai[0] is attack type
        //ai[1] is attack timer
        //ai[2] and ai[3] are memory for goal points

        private bool canShieldBonk;
        private bool holdingShield;
        private bool onSlimeMount;
        private int iceShieldCooldown;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cataclysmic Armageddon");
            Main.npcFrameCount[npc.type] = 5;

            NPCID.Sets.TrailCacheLength[npc.type] = 5;
            NPCID.Sets.TrailingMode[npc.type] = 2;
        }

        public override void SetDefaults()
        {
            npc.aiStyle = (int)AIStyles.CustomAI;
            npc.width = 18;
            npc.height = 40;
            drawOffsetY = -5;

            npc.defense = 0;
            npc.lifeMax = 250;
            npc.chaseable = false;
            npc.HitSound = SoundID.NPCHit5;

            npc.damage = 160;
            npc.knockBackResist = 0f;

            npc.value = Item.buyPrice(platinum: 1);

            npc.npcSlots = 15f;
            npc.boss = true;
            //bossBag = ItemType<CataBossBag>();

            npc.lavaImmune = true;
            npc.noGravity = true;
            npc.noTileCollide = true;

            for (int i=0; i<Main.maxBuffTypes; i++)
            {
                npc.buffImmune[i] = true;
            }

            music = MusicID.Boss4;

            Mod modMusic = ModLoader.GetMod("DeathsTerminusMusic");
            if (modMusic != null)
            {
                music = modMusic.GetSoundSlot(SoundType.Music, "Sounds/Music/nightmare");
            }
        }

        public override void AI()
        {
            Player player = Main.player[npc.target];
            if (!player.active || player.dead)
            {
                npc.TargetClosest(false);
                player = Main.player[npc.target];
                if (!player.active || player.dead)
                {
                    npc.Transform(NPCType<CataclysmicArmageddon>());
                    //NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, NPCType<CataclysmicArmageddon>());
                    //npc.active = false;

                    for (int i=0; i<Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].hostile)
                        {
                            Main.projectile[i].active = false;
                        }
                    }
                    return;
                }
            }

            holdingShield = false;
            if (iceShieldCooldown > 0)
            {
                iceShieldCooldown--;
            }

            npc.life = npc.lifeMax;

            switch (npc.ai[0])
            {
                case 0:
                    //1 sec each
                    SpawnAnimation();
                    break;
                case 1:
                    //3 secs each
                    SideScythesAttack();
                    break;
                case 2:
                case 7:
                    //7.3333 secs each
                    SideScythesAttackSpin();
                    break;
                case 3:
                case 8:
                    //4.3333 secs each
                    SideBlastsAttack();
                    break;
                case 4:
                    //8 secs each
                    IceSpiralAttack();
                    break;
                case 5:
                    //2.8 secs each
                    ShieldBonk();
                    break;
                case 6:
                    //2 secs each
                    SlimeBonk();
                    break;
                case 9:
                    //10 secs each
                    MothsAndLampAttack();
                    break;
                case 10:
                    //10 secs each
                    HeavenPetAttack();
                    break;
                case 11:
                    //1 sec each
                    Phase1To2Animation();
                    break;
                case 12:
                case 19:
                    //7.3333 secs each
                    SideScythesAttackHard();
                    break;
                case 13:
                case 16:
                    //3 secs each
                    SideSuperScythesAttack();
                    break;
                case 14:
                    //5 secs each
                    SideBlastsAttackHard();
                    break;
                case 15:
                    //12 secs each
                    IceSpiralAttackHard();
                    break;
                case 17:
                    //5 secs each
                    ShieldBonkHard();
                    break;
                case 18:
                    //10 secs each
                    SlimeBonkHard();
                    break;
                case 20:
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

        //1 sec
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

        //3 secs
        private void SideScythesAttack()
        {
            Player player = Main.player[npc.target];

            npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;

            FlyToPoint(goalPosition, Vector2.Zero);

            if (npc.ai[1] >= 60 && npc.ai[1] % 10 == 0 && Main.netMode != 1)
            {
                int numShots = 8;
                for (int i = 0; i < numShots; i++)
                {
                    Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                }
            }

            if (npc.ai[1] >= 60 && npc.ai[1] % 10 == 0)
            {
                Main.PlaySound(SoundID.Item8, npc.Center);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 180)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //7 secs 20 ticks
        private void SideScythesAttackSpin()
        {
            Player player = Main.player[npc.target];

            npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;

            FlyToPoint(goalPosition, Vector2.Zero);

            if (npc.ai[1] >= 60 && npc.ai[1] < 220 && npc.ai[1] % 10 == 0 && Main.netMode != 1)
            {
                int numShots = 8;
                for (int i = 0; i < numShots; i++)
                {
                    Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots + npc.direction * (npc.ai[1] - 60) / 100f) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                }
            }
            else if (npc.ai[1] >= 280 && npc.ai[1] < 440 && npc.ai[1] % 10 == 0 && Main.netMode != 1)
            {
                int numShots = 8;
                for (int i = 0; i < numShots; i++)
                {
                    Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots - npc.direction * (npc.ai[1] - 280) / 100f) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                }
            }

            if (((npc.ai[1] >= 60 && npc.ai[1] < 220) || (npc.ai[1] >= 280 && npc.ai[1] < 440)) && npc.ai[1] % 10 == 0)
            {
                Main.PlaySound(SoundID.Item8, npc.Center);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 440)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //4 secs 20 ticks
        private void SideBlastsAttack()
        {
            Player player = Main.player[npc.target];

            npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 480;

            FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.4f, maxYAcc: 0.4f);

            int shotPeriod = 50;
            int numShots = 4;
            float shotSpeed = 0.5f;
            float shotDistanceFromPlayer = 200;

            if (npc.ai[1] >= 60 && npc.ai[1] % shotPeriod == 60 % shotPeriod && Main.netMode != 1)
            {
                float angleRatio = shotDistanceFromPlayer / (player.Center - npc.Center).Length();
                if (angleRatio > 1)
                {
                    angleRatio = 1;
                }

                int direction = npc.ai[1] % (shotPeriod * 2) == 60 % (shotPeriod * 2) ? 1 : -1;
                Vector2 shotVelocity = (player.Center - npc.Center).SafeNormalize(Vector2.Zero).RotatedBy(direction * Math.Asin(angleRatio)) * shotSpeed;

                Projectile.NewProjectile(npc.Center, shotVelocity, ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer);
                Projectile.NewProjectile(npc.Center, -shotVelocity, ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer);
            }

            if (npc.ai[1] >= 90 && npc.ai[1] % shotPeriod == 90 % shotPeriod)
            {
                Main.PlaySound(SoundID.Item71, npc.Center);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 60 + shotPeriod * numShots)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //8 secs
        private void IceSpiralAttack()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 240;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.5f, maxYAcc: 0.5f);
            }
            else
            {
                npc.velocity = Vector2.Zero;

                if (npc.ai[1] == 60 && Main.netMode != 1)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<IceShield>(), 80, 0f, Main.myPlayer);
                    for (int i = -1; i <= 1; i++)
                    {
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<RotatingIceShards>(), 80, 0f, Main.myPlayer, ai0: i);
                    }
                }

                if (npc.ai[1] == 60)
                {
                    Main.PlaySound(29, npc.Center, 88);
                }
                if (npc.ai[1] == 120)
                {
                    Main.PlaySound(SoundID.Item120, npc.Center);
                }

                if (npc.ai[1] % 10 == 0 && npc.ai[1] < 360 && Main.netMode != 1)
                {
                    for (int i=0; i<24; i++)
                    {
                        Vector2 targetPoint = npc.Center + new Vector2(1200, 0).RotatedBy(i * MathHelper.TwoPi / 24);
                        Vector2 launchPoint = targetPoint + new Vector2(2400, 0).RotatedBy(i * MathHelper.TwoPi / 24 + MathHelper.PiOver2);
                        Projectile.NewProjectile(launchPoint, (targetPoint - launchPoint).SafeNormalize(Vector2.Zero) * 24f, ProjectileType<IceShard>(), 80, 0f, Main.myPlayer, ai0: 1);
                        launchPoint = targetPoint - new Vector2(2400, 0).RotatedBy(i * MathHelper.TwoPi / 24 + MathHelper.PiOver2);
                        Projectile.NewProjectile(launchPoint, (targetPoint - launchPoint).SafeNormalize(Vector2.Zero) * 24f, ProjectileType<IceShard>(), 80, 0f, Main.myPlayer, ai0: 1);
                    }
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 480)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //2 secs 48 ticks
        private void ShieldBonk()
        {
            Player player = Main.player[npc.target];

            holdingShield = true;

            if (npc.ai[1] % 84 < 60)
            {
                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 180;

                FlyToPoint(goalPosition, player.velocity, 0.8f, 0.8f);
            }
            else if (npc.ai[1] % 84 == 60)
            {
                canShieldBonk = true;

                npc.width = 40;
                npc.position.X -= 11;

                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                npc.velocity.X += npc.direction * 15;
                npc.velocity.Y /= 2;
            }
            else if (npc.ai[1] % 84 == 83)
            {
                if (canShieldBonk)
                {
                    npc.width = 18;
                    npc.position.X += 11;
                }

                canShieldBonk = false;

                npc.velocity.X -= npc.direction * 15;
            }

            //custom stuff for player EoC shield bonks
            //adapted from how the player detects SoC collision
            if (canShieldBonk)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active && !Main.player[i].dead && Main.player[i].dash == 2 && Main.player[i].eocDash > 0 && Main.player[i].eocHit < 0)
                    {
                        Rectangle shieldHitbox = new Rectangle((int)((double)Main.player[i].position.X + (double)Main.player[i].velocity.X * 0.5 - 4.0), (int)((double)Main.player[i].position.Y + (double)Main.player[i].velocity.Y * 0.5 - 4.0), Main.player[i].width + 8, Main.player[i].height + 8);
                        if (shieldHitbox.Intersects(npc.getRect()))
                        {
                            //custom stuff for player EoC shield bonks
                            //adapted from how the player detects SoC collision
                            npc.width = 18;
                            npc.position.X += 11;

                            npc.direction *= -1;
                            npc.velocity.X += npc.direction * 30;
                            canShieldBonk = false;

                            Main.PlaySound(SoundID.NPCHit4, npc.Center);

                            //redo the player's SoC bounce motion
                            int num40 = Main.player[i].direction;
                            if (Main.player[i].velocity.X < 0f)
                            {
                                num40 = -1;
                            }
                            if (Main.player[i].velocity.X > 0f)
                            {
                                num40 = 1;
                            }
                            Main.player[i].eocDash = 10;
                            Main.player[i].dashDelay = 30;
                            Main.player[i].velocity.X = -num40 * 9;
                            Main.player[i].velocity.Y = -4f;
                            Main.player[i].immune = true;
                            Main.player[i].immuneNoBlink = true;
                            Main.player[i].immuneTime = 4;
                            Main.player[i].eocHit = i;

                            break;
                        }
                    }
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 168)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void SlimeBonk()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(0, -1) * 360;

                FlyToPoint(goalPosition, player.velocity, 3f, 1f);
            }
            else
            {
                if (npc.ai[1] == 60)
                {
                    Main.PlaySound(SoundID.Item81, npc.Center);

                    onSlimeMount = true;

                    if (npc.velocity.Y < 0) npc.velocity.Y /= 2;
                    npc.velocity.X = player.velocity.X;

                    npc.width = 40;
                    npc.position.X -= 11;
                    npc.height = 64;
                    drawOffsetY = -15;
                }
                else if (onSlimeMount)
                {
                    if (npc.ai[1] < 119 && (npc.velocity.Y < 0 || npc.Hitbox.Top - 300 < player.Hitbox.Bottom))
                    {
                        npc.velocity.Y += 0.9f;

                        FlyToPoint(player.Center, player.velocity, 0.05f, 0f);
                    }
                    else
                    {
                        npc.velocity = player.velocity;
                        onSlimeMount = false;

                        npc.width = 18;
                        npc.position.X += 11;
                        npc.height = 40;
                        drawOffsetY = -5;
                    }
                }
                else
                {
                    npc.velocity *= 0.98f;
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 120)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void MothsAndLampAttack()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 1080;

                FlyToPoint(goalPosition, player.velocity, 0.8f, 0.8f);
            }
            else
            {
                if (npc.ai[1] == 60)
                {
                    npc.ai[2] = npc.Center.X;
                    npc.ai[3] = npc.Center.Y;

                    //need to make ray of sunshine cause screenshake
                    Main.PlaySound(SoundID.Zombie, npc.Center + new Vector2(npc.direction, 0) * 1500, 104);
                    Main.LocalPlayer.GetModPlayer<DTPlayer>().screenShakeTime = 60;

                    if (Main.netMode != 1)
                    {
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<SigilArena>(), 80, 0f, Main.myPlayer);
                        Projectile.NewProjectile(npc.Center + new Vector2(npc.direction, 0) * 1500, new Vector2(-npc.direction * 5, 0), ProjectileType<SunLamp>(), 80, 0f, Main.myPlayer);
                    }
                }

                //deepened mothron singing sound, only use for the buffed attack
                /*
                if (npc.ai[1] == 120)
                {
                    Main.PlaySound(SoundID.Zombie, (int)npc.Center.X, (int)npc.Center.Y, 73, volumeScale: 2f, pitchOffset: -1);
                }
                */

                if (Main.netMode != 1 && npc.ai[1] >= 90 && npc.ai[1] <= 480 && npc.ai[1] % 5 == 0)
                {
                    Vector2 arenaCenter = new Vector2(npc.ai[2], npc.ai[3]);

                    //ray X position
                    float relativeRayPosition = npc.direction * 1500 - npc.direction * 5 * (npc.ai[1] + 30 - 60);

                    //angle of the arena still available
                    float availableAngle = (float)Math.Acos(relativeRayPosition / 1200f);
                    if (npc.direction == 1)
                    {
                        availableAngle = MathHelper.Pi - availableAngle;
                    }
                    if (availableAngle > MathHelper.Pi / 2)
                    {
                        availableAngle = MathHelper.Pi / 2;
                    }

                    float availableHeight = (float)Math.Sqrt(1200f * 1200f - relativeRayPosition * relativeRayPosition);

                    float shotAngle = -npc.direction * availableAngle * ((npc.ai[1] / 1.618f / 5 % 1) * 2 - 1);
                    float goalHeight = arenaCenter.Y + -npc.direction * shotAngle / availableAngle * availableHeight;

                    Projectile.NewProjectile(arenaCenter + new Vector2(-1800f * npc.direction, 0f).RotatedBy(shotAngle), new Vector2(32f * npc.direction, 0).RotatedBy(shotAngle), ProjectileType<BabyMothronProjectile>(), 80, 0f, Main.myPlayer, ai0: goalHeight, ai1: npc.direction * 1500 - npc.direction * 5 * (npc.ai[1] - 60) + arenaCenter.X);
                }

                Vector2 goalPosition = new Vector2(npc.ai[2], npc.ai[3]) + new Vector2(-npc.direction, 0) * 1080;

                FlyToPoint(goalPosition, Vector2.Zero, 0.25f, 0.25f);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 600)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void HeavenPetAttack()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] == 0 && Main.netMode != 1)
            {
                Projectile.NewProjectile(npc.Center, new Vector2(0, -6), ProjectileType<HeavenPetProjectile>(), 80, 0f, Main.myPlayer, ai0: npc.whoAmI);
            }

            if (npc.ai[1] < 600)
            {
                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 360;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.5f, maxYAcc: 0.5f);
            }

            if (npc.ai[1] % 15 == 0 && npc.ai[1] > 60)
            {
                Main.PlaySound(SoundID.Item8, npc.Center);

                if (Main.netMode != 1)
                    Projectile.NewProjectile(npc.Center, (player.Center - npc.Center).SafeNormalize(Vector2.Zero) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 600)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //1 sec
        private void Phase1To2Animation()
        {
            Player player = Main.player[npc.target];

            npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 360;

            FlyToPoint(goalPosition, Vector2.Zero);

            npc.ai[1]++;
            if (npc.ai[1] == 60)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //7 secs 20 ticks
        private void SideScythesAttackHard()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 360;

                FlyToPoint(goalPosition, Vector2.Zero);
            }
            else
            {
                npc.spriteDirection = player.Center.X > npc.Center.X ? 1 : -1;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 360;

                FlyToPoint(goalPosition, Vector2.Zero);
            }

            if (npc.ai[1] >= 60 && npc.ai[1] < 440 && npc.ai[1] % 5 == 0 && Main.netMode != 1)
            {
                int numShots = 12;
                for (int i = 0; i < numShots; i++)
                {
                    Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots - npc.direction * MathHelper.Pi / 2f * (Math.Cos((npc.ai[1] - 60) / 380f * MathHelper.TwoPi) - 1) / 2f) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai1: 1);
                }
            }

            if ((npc.ai[1] >= 60 && npc.ai[1] < 440) && npc.ai[1] % 10 == 0)
            {
                Main.PlaySound(SoundID.Item8, npc.Center);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 440)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void SideSuperScythesAttack()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 450;

            FlyToPoint(goalPosition, Vector2.Zero);

            if (npc.ai[1] >= 60 && npc.ai[1] % 15 == 0 && Main.netMode != 1)
            {
                Projectile.NewProjectile(npc.Center, new Vector2(npc.direction, 0) * 0.5f, ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1f);
            }

            if (npc.ai[1] > 60 && npc.ai[1] % 15 == 14)
            {
                Main.PlaySound(SoundID.Item71, npc.Center);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 180)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void SideBlastsAttackHard()
        {
            Player player = Main.player[npc.target];

            npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
            npc.spriteDirection = npc.direction;
            Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 720;

            FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.4f, maxYAcc: 0.4f);

            int shotPeriod = 60;
            int numShots = 4;
            float shotSpeed = 0.5f;
            float shotDistanceFromPlayer = 300;

            if (npc.ai[1] >= 60 && npc.ai[1] % shotPeriod == 60 % shotPeriod && Main.netMode != 1)
            {
                float angleRatio = shotDistanceFromPlayer / (player.Center - npc.Center).Length();
                if (angleRatio > 1)
                {
                    angleRatio = 1;
                }

                int direction = npc.ai[1] % (shotPeriod * 2) == 60 % (shotPeriod * 2) ? 1 : -1;
                Vector2 shotVelocity = (player.Center - npc.Center).SafeNormalize(Vector2.Zero).RotatedBy(direction * Math.Asin(angleRatio)) * shotSpeed;

                Projectile.NewProjectile(npc.Center, shotVelocity, ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1);
                Projectile.NewProjectile(npc.Center, shotVelocity.RotatedBy(0.15f), ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1);
                Projectile.NewProjectile(npc.Center, shotVelocity.RotatedBy(-0.15f), ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1);
                Projectile.NewProjectile(npc.Center, -shotVelocity, ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1);
                Projectile.NewProjectile(npc.Center, -shotVelocity.RotatedBy(0.15f), ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1);
                Projectile.NewProjectile(npc.Center, -shotVelocity.RotatedBy(-0.15f), ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer, ai1: 1);
            }

            if (npc.ai[1] >= 90 && npc.ai[1] % shotPeriod == 90 % shotPeriod)
            {
                Main.PlaySound(SoundID.Item71, npc.Center);
            }

            npc.ai[1]++;
            if (npc.ai[1] == 60 + shotPeriod * numShots)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void IceSpiralAttackHard()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 240;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.5f, maxYAcc: 0.5f);
            }
            else
            {
                npc.velocity = Vector2.Zero;

                if (npc.ai[1] == 60 || npc.ai[1] == 240 && Main.netMode != 1)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<IceShield>(), 80, 0f, Main.myPlayer);
                    for (int i = -1; i <= 1; i++)
                    {
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<RotatingIceShards>(), 80, 0f, Main.myPlayer, ai0: i, ai1: 0f);
                    }
                }

                if (npc.ai[1] > 120 && npc.ai[1] < 600 && npc.ai[1] % 10 == 0)
                {
                    Projectile.NewProjectile(npc.Center, (player.Center - npc.Center).SafeNormalize(Vector2.Zero) * 0.5f, ProjectileType<IceShard>(), 80, 0f, Main.myPlayer, ai0: 1.04f);
                }

                if (npc.ai[1] == 60 || npc.ai[1] == 240)
                {
                    Main.PlaySound(29, npc.Center, 88);
                }
                if (npc.ai[1] == 120 || npc.ai[1] == 300)
                {
                    Main.PlaySound(SoundID.Item120, npc.Center);
                }

                if (npc.ai[1] % 10 == 0 && npc.ai[1] < 600 && Main.netMode != 1)
                {
                    for (int i = 0; i < 24; i++)
                    {
                        Vector2 targetPoint = npc.Center + new Vector2(1200, 0).RotatedBy(i * MathHelper.TwoPi / 24);
                        Vector2 launchPoint = targetPoint + new Vector2(2400, 0).RotatedBy(i * MathHelper.TwoPi / 24 + MathHelper.PiOver2);
                        Projectile.NewProjectile(launchPoint, (targetPoint - launchPoint).SafeNormalize(Vector2.Zero) * 24f, ProjectileType<IceShard>(), 80, 0f, Main.myPlayer, ai0: 1);
                        launchPoint = targetPoint - new Vector2(2400, 0).RotatedBy(i * MathHelper.TwoPi / 24 + MathHelper.PiOver2);
                        Projectile.NewProjectile(launchPoint, (targetPoint - launchPoint).SafeNormalize(Vector2.Zero) * 24f, ProjectileType<IceShard>(), 80, 0f, Main.myPlayer, ai0: 1);
                    }
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 720)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void ShieldBonkHard()
        {
            int dashTime = 24;
            int downTime = 36;
            int numDashes = 4;

            Player player = Main.player[npc.target];

            holdingShield = true;

            if (npc.ai[1] < 60 || (npc.ai[1] - 60) % (dashTime + downTime) >= dashTime)
            {
                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 180;

                FlyToPoint(goalPosition, player.velocity, 4f, 2f);
            }
            else if ((npc.ai[1] - 60) % (dashTime + downTime) == 0)
            {
                canShieldBonk = true;

                npc.width = 40;
                npc.position.X -= 11;

                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                npc.velocity.X += npc.direction * 15;
                npc.velocity.Y = player.velocity.Y / 2;
            }
            else if ((npc.ai[1] - 60) % (dashTime + downTime) == dashTime)
            {
                if (canShieldBonk)
                {
                    npc.width = 18;
                    npc.position.X += 11;
                }

                canShieldBonk = false;

                npc.velocity.X -= npc.direction * 15;
            }

            //custom stuff for player EoC shield bonks
            //adapted from how the player detects SoC collision
            if (canShieldBonk)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active && !Main.player[i].dead && Main.player[i].dash == 2 && Main.player[i].eocDash > 0 && Main.player[i].eocHit < 0)
                    {
                        Rectangle shieldHitbox = new Rectangle((int)((double)Main.player[i].position.X + (double)Main.player[i].velocity.X * 0.5 - 4.0), (int)((double)Main.player[i].position.Y + (double)Main.player[i].velocity.Y * 0.5 - 4.0), Main.player[i].width + 8, Main.player[i].height + 8);
                        if (shieldHitbox.Intersects(npc.getRect()))
                        {
                            //custom stuff for player EoC shield bonks
                            //adapted from how the player detects SoC collision
                            npc.width = 18;
                            npc.position.X += 11;

                            npc.direction *= -1;
                            npc.velocity.X += npc.direction * 30;
                            canShieldBonk = false;

                            Main.PlaySound(SoundID.NPCHit4, npc.Center);

                            //redo the player's SoC bounce motion
                            int num40 = Main.player[i].direction;
                            if (Main.player[i].velocity.X < 0f)
                            {
                                num40 = -1;
                            }
                            if (Main.player[i].velocity.X > 0f)
                            {
                                num40 = 1;
                            }
                            Main.player[i].eocDash = 10;
                            Main.player[i].dashDelay = 30;
                            Main.player[i].velocity.X = -num40 * 9;
                            Main.player[i].velocity.Y = -4f;
                            Main.player[i].immune = true;
                            Main.player[i].immuneNoBlink = true;
                            Main.player[i].immuneTime = 4;
                            Main.player[i].eocHit = i;

                            break;
                        }
                    }
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 60 + numDashes * (dashTime + downTime))
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        private void SlimeBonkHard()
        {
            Player player = Main.player[npc.target];

            if (npc.ai[1] < 60)
            {
                if (Math.Abs(player.Center.X - npc.Center.X) > 8)
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + new Vector2(0, -1) * 360;

                FlyToPoint(goalPosition, player.velocity, 4f, 2f);
            }
            else
            {
                bool justBounced = false;

                if (npc.ai[1] == 60)
                {
                    justBounced = true;

                    Main.PlaySound(SoundID.Item81, npc.Center);

                    onSlimeMount = true;

                    if (npc.velocity.Y < 0) npc.velocity.Y /= 2;
                    npc.velocity.X = player.velocity.X;

                    npc.width = 40;
                    npc.position.X -= 11;
                    npc.height = 64;
                    drawOffsetY = -15;
                }
                else if (onSlimeMount)
                {
                    if (npc.ai[1] == 599)
                    {
                        npc.velocity = player.velocity;
                        onSlimeMount = false;

                        npc.width = 18;
                        npc.position.X += 11;
                        npc.height = 40;
                        drawOffsetY = -5;


                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            if (Main.projectile[i].active && Main.projectile[i].type == ProjectileType<FishronPlatform>() && Main.projectile[i].ai[1] == 0)
                            {
                                Main.projectile[i].ai[1] = 1;

                                break;
                            }
                        }
                    }
                    else if (npc.velocity.Y < 0 || npc.Hitbox.Top - 240 < player.Hitbox.Bottom)
                    {
                        npc.velocity.Y += 0.9f;

                        npc.direction = npc.velocity.X > 0 ? -1 : 1;
                        npc.spriteDirection = -npc.direction;
                    }
                    else
                    {
                        npc.velocity.Y = -36f;
                        npc.velocity.X = player.velocity.X + (player.Center.X - npc.Center.X) / 60f;

                        npc.direction = npc.velocity.X > 0 ? -1 : 1;
                        npc.spriteDirection = -npc.direction;

                        justBounced = true;
                    }
                }
                else
                {
                    npc.velocity *= 0.98f;
                }

                //summon bigger fish
                if (justBounced)
                {
                    for (int i=0; i<Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].type == ProjectileType<FishronPlatform>() && Main.projectile[i].ai[1] == 0)
                        {
                            Main.projectile[i].Kill();
                            break;
                        }
                    }

                    if (npc.ai[1] <= 540 && Main.netMode != 1)
                    {
                        float eta = 5f;
                        float determinant = (npc.velocity.Y - player.velocity.Y) * (npc.velocity.Y - player.velocity.Y) - 4 * 0.45f * (npc.Center.Y - player.Center.Y + 240);
                        if (determinant >= 0)
                            eta = Math.Max(3, (-(npc.velocity.Y - player.velocity.Y) + (float)Math.Sqrt(determinant)) / 0.9f);
                        float speed = 28f;
                        Vector2 targetPoint = new Vector2(npc.Center.X + npc.velocity.X * eta, player.Center.Y + 240 + player.velocity.Y * eta);
                        Vector2 shotPosition = targetPoint + new Vector2(npc.direction * eta * speed, 0);
                        Vector2 shotVelocity = (targetPoint - shotPosition) / eta;

                        Projectile.NewProjectile(shotPosition, shotVelocity, ProjectileType<FishronPlatform>(), 80, 0f, Main.myPlayer, ai0: npc.whoAmI);
                    }
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 600)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return canShieldBonk || (onSlimeMount && npc.velocity.Y > target.velocity.Y);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (canShieldBonk)
            {
                npc.width = 18;
                npc.position.X += 11;

                npc.direction *= -1;
                npc.velocity.X += npc.direction * 30;
                canShieldBonk = false;
            }
            else if (onSlimeMount)
            {
                npc.velocity.Y = -24f;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == ProjectileType<FishronPlatform>() && Main.projectile[i].ai[1] == 0)
                    {
                        Main.projectile[i].ai[1] = 1;

                        if (Main.netMode != 1)
                        {
                            float eta = 5f;
                            float determinant = (npc.velocity.Y - target.velocity.Y) * (npc.velocity.Y - target.velocity.Y) - 4 * 0.45f * (npc.Center.Y - target.Center.Y + 240);
                            if (determinant >= 0)
                                eta = Math.Max(3, (-(npc.velocity.Y - target.velocity.Y) + (float)Math.Sqrt(determinant)) / 0.9f);
                            float speed = 28f;
                            Vector2 targetPoint = new Vector2(npc.Center.X + npc.velocity.X * eta, target.Center.Y + 240 + target.velocity.Y * eta);
                            Vector2 shotPosition = targetPoint + new Vector2(npc.direction * eta * speed, 0);
                            Vector2 shotVelocity = (targetPoint - shotPosition) / eta;

                            Projectile.NewProjectile(shotPosition, shotVelocity, ProjectileType<FishronPlatform>(), 80, 0f, Main.myPlayer, ai0: npc.whoAmI);
                        }

                        break;
                    }
                }
            }
        }

        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            damage = 0;
            iceShieldCooldown = 60;
            return true;
        }

        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            npc.frameCounter++;
            if (npc.frameCounter == 3 * 5)
            {
                npc.frameCounter = 0;
            }
            if (onSlimeMount)
            {
                npc.frameCounter = 3 * 3;
            }

            npc.frame.Y = frameHeight * (((int)npc.frameCounter) / 3);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            int trailLength = 1;
            if (canShieldBonk) trailLength = 5;
            if (onSlimeMount) trailLength = 5;

            for (int i = trailLength - 1; i >= 0; i--)
            {
                if (i == 0 || i % 2 == npc.ai[1] % 2)
                {
                    float alpha = (trailLength - i) / (float)trailLength;
                    Vector2 center = npc.oldPos[i] + new Vector2(npc.width, npc.height) / 2;

                    SpriteEffects effects;

                    if (onSlimeMount)
                    {
                        Texture2D mountTexture = ModContent.GetTexture("Terraria/Mount_Slime");
                        Rectangle frame = mountTexture.Frame(1, 4, 0, 1);
                        effects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                        Vector2 mountOffset = new Vector2(npc.spriteDirection * 0, 10);
                        spriteBatch.Draw(mountTexture, center - Main.screenPosition + mountOffset, frame, Color.White * alpha, 0f, frame.Size() / 2f, 1f, effects, 0f);
                    }

                    Texture2D npcTexture = Main.npcTexture[npc.type];
                    effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    Vector2 npcOffset = new Vector2(-npc.spriteDirection * 3, drawOffsetY);
                    spriteBatch.Draw(npcTexture, center - Main.screenPosition + npcOffset, npc.frame, Color.White * alpha, 0f, npc.frame.Size() / 2f, 1f, effects, 0f);

                    if (holdingShield)
                    {
                        Texture2D shieldTexture = ModContent.GetTexture("Terraria/Acc_Shield_5");
                        Rectangle frame = shieldTexture.Frame(1, 20);
                        effects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                        Vector2 shieldOffset = new Vector2(npc.spriteDirection * 3, -4);

                        spriteBatch.Draw(shieldTexture, center - Main.screenPosition + shieldOffset, frame, Color.White * alpha, 0f, frame.Size() / 2f, 1f, effects, 0f);
                    }

                    if (iceShieldCooldown > 0)
                    {
                        Texture2D shieldTexture = ModContent.GetTexture("Terraria/Projectile_464");
                        Rectangle frame = shieldTexture.Frame();
                        effects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                        float shieldAlpha = iceShieldCooldown / 120f;
                        Vector2 shieldOffset = new Vector2(0, -2);

                        spriteBatch.Draw(shieldTexture, center - Main.screenPosition + shieldOffset, frame, Color.White * shieldAlpha * alpha, 0f, frame.Size() / 2f, 1f, effects, 0f);
                    }
                }
            }

            return false;
        }

        public override bool CheckDead()
        {
            if (false)
            {
                //doesn't actually happen yet
                NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, NPCType<CataclysmicArmageddon>());
                return true;
            }
            npc.life = npc.lifeMax;
            return false;
        }

        public override bool CheckActive()
        {
            return false;
        }
    }

    public class CataBossScythe : ModProjectile
    {
        //demon scythe but no dust and it passes through tiles
        public override string Texture => "Terraria/Projectile_" + ProjectileID.DemonSickle;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Demon Scythe");
            /*
            Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 64, 16, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int j = 0; j < texture.Height; j++)
			{
				for (int i = 0; i < texture.Width; i++)
				{
					float x = i / (float)(texture.Width - 1);
                    float y = j / (float)(texture.Height - 1);

                    int r = 255;
					int g = 255;
					int b = 255;
					int alpha = (int)(255 * (1 - x) * 4 * y * (1 - y));

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "CataBossScytheTelegraph.png", FileMode.Create), texture.Width, texture.Height);*/
        }

        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 48;
            projectile.alpha = 32;
            projectile.light = 0.2f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 0.9f;
            projectile.timeLeft = 130;

            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.rotation += projectile.direction * 0.8f;
            projectile.ai[0] += 1f;
            if (!(projectile.ai[0] < 30f))
            {
                if (projectile.ai[0] < 120f)
                {
                    projectile.velocity *= 1.06f;
                }
            }
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.ai[1] == 0)
            {
                spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height), Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(Main.projectileTexture[projectile.type].Width / 2f, Main.projectileTexture[projectile.type].Height / 2f), projectile.scale, SpriteEffects.None, 0f);

                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 16), Color.Purple * 0.25f, projectile.velocity.ToRotation(), new Vector2(0, 8), new Vector2(projectile.velocity.Length(), projectile.width / 16f), SpriteEffects.None, 0f);
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 16), Color.Purple * 0.25f, (-projectile.velocity).ToRotation(), new Vector2(0, 8), new Vector2(projectile.width / 128f, projectile.width / 16f), SpriteEffects.None, 0f);
            }
            else if (projectile.ai[1] == 1)
            {
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataEclipseScythe"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height), Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(Main.projectileTexture[projectile.type].Width / 2f, Main.projectileTexture[projectile.type].Height / 2f), projectile.scale, SpriteEffects.None, 0f);

                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 16), Color.Orange * 0.25f, projectile.velocity.ToRotation(), new Vector2(0, 8), new Vector2(projectile.velocity.Length(), projectile.width / 16f), SpriteEffects.None, 0f);
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 16), Color.Orange * 0.25f, (-projectile.velocity).ToRotation(), new Vector2(0, 8), new Vector2(projectile.width / 128f, projectile.width / 16f), SpriteEffects.None, 0f);
            }

            return false;
        }
    }

    public class CataBossSuperScythe : ModProjectile
    {
        //demon scythe but no dust and it passes through tiles
        public override string Texture => "Terraria/Projectile_" + ProjectileID.DemonSickle;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Super Scythe");
        }

        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 48;
            projectile.alpha = 32;
            projectile.light = 0.2f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.scale = 0.9f;
            projectile.timeLeft = 160;

            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.rotation += projectile.direction * 0.8f;
            projectile.ai[0] += 1f;
            if (projectile.ai[0] == 30f)
            {
                projectile.velocity *= 60f;
            }

            if (projectile.ai[0] >= 30 && (projectile.ai[0] - 30) % 7 == 0 && Main.netMode != 1)
            {
                Projectile.NewProjectile(projectile.Center, projectile.velocity.SafeNormalize(Vector2.Zero) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai1: projectile.ai[1]);
                Projectile.NewProjectile(projectile.Center, projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai1: projectile.ai[1]);
                Projectile.NewProjectile(projectile.Center, projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.PiOver2) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer, ai1: projectile.ai[1]);
            }
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.ai[1] == 0)
            {
                spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height), Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(Main.projectileTexture[projectile.type].Width / 2f, Main.projectileTexture[projectile.type].Height / 2f), projectile.scale, SpriteEffects.None, 0f);

                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 16), Color.Purple * 0.25f, projectile.velocity.ToRotation(), new Vector2(0, 8), new Vector2(30, projectile.width / 16f), SpriteEffects.None, 0f);
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 16), Color.Purple * 0.25f, (-projectile.velocity).ToRotation(), new Vector2(0, 8), new Vector2(projectile.width / 128f, projectile.width / 16f), SpriteEffects.None, 0f);
            }
            else if (projectile.ai[1] == 1)
            {
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataEclipseScythe"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height), Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(Main.projectileTexture[projectile.type].Width / 2f, Main.projectileTexture[projectile.type].Height / 2f), projectile.scale, SpriteEffects.None, 0f);

                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 16), Color.Orange * 0.25f, projectile.velocity.ToRotation(), new Vector2(0, 8), new Vector2(30, projectile.width / 16f), SpriteEffects.None, 0f);
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 64, 16), Color.Orange * 0.25f, (-projectile.velocity).ToRotation(), new Vector2(0, 8), new Vector2(projectile.width / 128f, projectile.width / 16f), SpriteEffects.None, 0f);
            }

            return false;
        }
    }

    public class RotatingIceShards : ModProjectile
    {
        public override string Texture => "Terraria/Extra_35";

        private static int shardRadius = 12;
        private static int shardCount = 24;

        //ring of ice shards
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ice Shard");
            Main.projFrames[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = 2;
            projectile.height = 2;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 420;

            projectile.hide = true;
        }

        public override void AI()
        {
            if (projectile.timeLeft <= 360)
            {
                float angle = MathHelper.TwoPi * projectile.timeLeft / 360f;

                //set radius and rotation (replace with more dynamic AI later)
                projectile.localAI[1] = 600 * (float)Math.Sqrt(2 - 2 * Math.Cos(angle));
                projectile.rotation = projectile.ai[1] + projectile.ai[0] * (angle + MathHelper.Pi) / 2;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < shardCount; i++)
            {
                Vector2 circleCenter = projectile.Center + new Vector2(projectile.localAI[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / shardCount);
                float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
                float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
                if (new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < shardRadius)
                {
                    return true;
                }
            }
            return false;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            for (int i = 0; i < shardCount; i++)
            {
                Rectangle frame = texture.Frame(1, 3);

                for (int j = Math.Min(projectile.timeLeft, 360); j >= Math.Max(0, projectile.timeLeft - 60); j--)
                {
                    float angle = MathHelper.TwoPi * j / 360f;
                    float radius = 600 * (float)Math.Sqrt(2 - 2 * Math.Cos(angle));
                    float rotation = projectile.ai[1] + projectile.ai[0] * (angle + MathHelper.Pi) / 2;
                    float alphaMultiplier = Math.Max(0, (60 - projectile.timeLeft + j) / 60f);

                    spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(radius * projectile.scale, 0).RotatedBy(rotation + i * MathHelper.TwoPi / shardCount), frame, Color.White * alphaMultiplier * 0.03f, rotation + i * MathHelper.TwoPi / shardCount - MathHelper.PiOver2 + projectile.ai[0] * MathHelper.Pi * (1 + j / 360f), new Vector2(12, 37), projectile.scale, SpriteEffects.None, 0f);
                }

                if (projectile.timeLeft <= 360)
                    spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(projectile.localAI[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / shardCount), frame, Color.White, projectile.rotation + i * MathHelper.TwoPi / shardCount - MathHelper.PiOver2 + projectile.ai[0] * MathHelper.Pi * (1 + projectile.timeLeft / 360f), new Vector2(12, 37), projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }

    public class IceShard : ModProjectile
    {
        public override string Texture => "Terraria/Extra_35";

        private static int shardRadius = 12;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ice Shard");
            Main.projFrames[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = 24;
            projectile.height = 24;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 200;

            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.velocity *= projectile.ai[0];

            projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 circleCenter = projectile.Center;
            float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
            float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
            return new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < shardRadius;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame(1, 3);

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White, projectile.rotation, new Vector2(12, 37), projectile.scale, SpriteEffects.None, 0f);
            
            return false;
        }
    }

    public class IceShield : ModProjectile
    {
        public override string Texture => "Terraria/Projectile_464";

        private static int shardRadius = 12;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ice Shield");
            Main.projFrames[projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            projectile.width = 60;
            projectile.height = 60;
            projectile.alpha = 96;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 60;
        }

        public override void AI()
        {
            projectile.ai[0] += 0.01f;
            projectile.rotation += projectile.ai[0];
            projectile.alpha = projectile.timeLeft * 128 / 60;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 circleCenter = projectile.Center;
            float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
            float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
            return new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < projectile.width / 2;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame(1, 1);

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(46, 51), projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class SigilArena : ModProjectile
    {
        private static int sigilRadius = 27;
        private static int sigilCount = 80;


        //the arena!
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Orbiting Sigil");
            Main.projFrames[projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            projectile.width = 2;
            projectile.height = 2;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = false;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.scale = 2f;
            projectile.timeLeft = 600;
        }

        public override void AI()
        {
            projectile.scale = 1f;
            projectile.hostile = true;

            //rotation increment
            projectile.rotation += 0.01f;

            //set radius and center (replace with more dynamic AI later)
            projectile.ai[1] = 1200 + Math.Max(0, 20 * (60 - projectile.ai[0])) + Math.Max(0, 20 * (60 - projectile.timeLeft));

            if (projectile.scale >= 1)
            {
                if ((Main.LocalPlayer.Center - projectile.Center).Length() > projectile.ai[1] * projectile.scale)
                {
                    Vector2 normal = (Main.LocalPlayer.Center - projectile.Center).SafeNormalize(Vector2.Zero);
                    Vector2 relativeVelocity = Main.LocalPlayer.velocity - projectile.velocity;

                    Main.LocalPlayer.Center = projectile.Center + normal * projectile.ai[1] * projectile.scale;

                    if (relativeVelocity.X * normal.X + relativeVelocity.Y * normal.Y > 0)
                    {
                        Main.LocalPlayer.velocity -= normal * (relativeVelocity.X * normal.X + relativeVelocity.Y * normal.Y);
                    }
                }
            }

            //frame stuff
            projectile.frameCounter++;
            if (projectile.frameCounter == 3)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 4)
                {
                    projectile.frame = 0;
                }
            }

            projectile.ai[0]++;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < sigilCount; i++)
            {
                Vector2 circleCenter = projectile.Center + new Vector2(projectile.ai[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / sigilCount);
                float nearestX = Math.Max(targetHitbox.X, Math.Min(circleCenter.X, targetHitbox.X + targetHitbox.Size().X));
                float nearestY = Math.Max(targetHitbox.Y, Math.Min(circleCenter.Y, targetHitbox.Y + targetHitbox.Size().Y));
                if (new Vector2(circleCenter.X - nearestX, circleCenter.Y - nearestY).Length() < sigilRadius)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            for (int i = 0; i < sigilCount; i++)
            {
                spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(projectile.ai[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / sigilCount), new Rectangle(0, 96 * projectile.frame, 66, 96), Color.White * projectile.scale, projectile.rotation + i * MathHelper.TwoPi / sigilCount, new Vector2(33, 65), projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }

    public class SunLamp : ModProjectile
    {
        //demon scythe but no dust and it passes through tiles
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sun Lamp");
            /*
            Texture2D texture = new Texture2D(Main.spriteBatch.GraphicsDevice, 96, 1, false, SurfaceFormat.Color);
			System.Collections.Generic.List<Color> list = new System.Collections.Generic.List<Color>();
			for (int i = 0; i < texture.Width; i++)
			{
				for (int j = 0; j < texture.Height; j++)
				{
					float x = i / (float)(texture.Width - 1);

                    float index = 4 * x * (1 - x);

                    int r = 255;
					int g = 255 - (int)(64 * (1 - index));
					int b = 255 - (int)(255 * (1 - index));
					int alpha = (int)(255 * index);

					list.Add(new Color((int)(r * alpha / 255f), (int)(g * alpha / 255f), (int)(b * alpha / 255f), alpha));
				}
			}
			texture.SetData(list.ToArray());
			texture.SaveAsPng(new FileStream(Main.SavePath + Path.DirectorySeparatorChar + "SunLamp.png", FileMode.Create), texture.Width, texture.Height);*/
        }

        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 48;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.timeLeft = 540;

            projectile.hide = true;
        }

        public override void AI()
        {
            DelegateMethods.v3_1 = new Vector3(255 / 128f, 220 / 128f, 64 / 128f);
            Utils.PlotTileLine(projectile.Center + new Vector2(0, 2048), projectile.Center + new Vector2(0, -2048), 26, DelegateMethods.CastLight);

            if (projectile.scale < 1f && projectile.timeLeft > 60)
            {
                projectile.scale += 1 / 60f;
            }
            else if (projectile.timeLeft <= 60)
            {
                projectile.scale -= 1 / 60f;
                projectile.velocity.X *= 0.95f;
            }

            projectile.direction = projectile.velocity.X > 0 ? 1 : -1;

            if (projectile.timeLeft > 60)
            {
                if (projectile.Center.X < Main.LocalPlayer.Center.X ^ projectile.direction == 1)
                {
                    Vector2 normal = new Vector2(projectile.direction, 0);
                    Vector2 relativeVelocity = Main.LocalPlayer.velocity - projectile.velocity;

                    Main.LocalPlayer.Center = new Vector2(projectile.Center.X, Main.LocalPlayer.Center.Y);

                    if (relativeVelocity.X * normal.X + relativeVelocity.Y * normal.Y > 0)
                    {
                        Main.LocalPlayer.velocity -= normal * (relativeVelocity.X * normal.X + relativeVelocity.Y * normal.Y);
                    }
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center + new Vector2(0, 2048), projectile.Center + new Vector2(0, -2048), 64 * projectile.scale, ref point);
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        //need to do new visuals for this
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, texture.Frame(), Color.White, projectile.rotation, texture.Frame().Size()/2, new Vector2(projectile.scale, 4096), SpriteEffects.None, 0f);

            return false;
        }
    }

    public class BabyMothronProjectile : ModProjectile
    {
        public override string Texture => "Terraria/NPC_479";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Baby Mothron");
            Main.projFrames[projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            projectile.width = 36;
            projectile.height = 36;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 400;

            projectile.hide = true;
        }

        public override void AI()
        {
            if (projectile.timeLeft > 400 - 33)
            {
                projectile.velocity *= 0.95f;
            }
            else
            {
                projectile.velocity.Y += projectile.velocity.Y * projectile.velocity.Y / (projectile.Center.Y - projectile.ai[0]);

                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * 6;
            }
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.Pi;
            projectile.direction = projectile.velocity.X > 0 ? -1 : 1;
            projectile.spriteDirection = projectile.direction;

            //test for death via ray of sunshine
            projectile.ai[1] += 5 * projectile.direction;
            if (projectile.ai[1] < projectile.Center.X ^ projectile.direction == 1)
            {
                projectile.Kill();
            }

            //frame stuff
            projectile.frameCounter++;
            if (projectile.frameCounter == 4)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 3)
                {
                    projectile.frame = 0;
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            Main.PlaySound(SoundID.NPCHit23, projectile.Center);
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame(1, 3, 0, projectile.frame);
            SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White, projectile.rotation, frame.Size() / 2, projectile.scale, effects, 0f);

            return false;
        }
    }

    public class HeavenPetProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sprocket");
            Main.projFrames[projectile.type] = 7;
        }

        private float cogRotation = 0;

        public override void SetDefaults()
        {
            projectile.width = 18;
            projectile.height = 18;
            projectile.alpha = 0;
            projectile.light = 1f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 660;

            projectile.rotation = MathHelper.PiOver2;

            projectile.hide = true;
        }

        public override void AI()
        {
            if (projectile.timeLeft > 60)
            {
                Player player = Main.player[Main.npc[(int)projectile.ai[0]].target];

                if (projectile.timeLeft <= 600 - 60)
                {
                    projectile.hostile = true;

                    projectile.velocity -= new Vector2(0.25f, 0).RotatedBy(projectile.rotation);
                    projectile.velocity *= 0.95f;

                    if (projectile.timeLeft == 600 - 60)
                    {
                        Main.PlaySound(SoundID.Item122, projectile.Center);
                    }
                    else if (projectile.timeLeft % 60 == 0)
                    {
                        Main.PlaySound(SoundID.Item15, projectile.Center);
                    }
                }
                else
                {
                    projectile.hostile = false;

                    projectile.velocity -= new Vector2(0.1f, 0).RotatedBy(projectile.rotation);
                    projectile.velocity *= 0.95f;
                }

                float maxTurn = 0.015f;

                float rotationOffset = (player.Center - projectile.Center).ToRotation() - projectile.rotation;
                while (rotationOffset > MathHelper.Pi)
                {
                    rotationOffset -= MathHelper.TwoPi;
                }
                while (rotationOffset < -MathHelper.Pi)
                {
                    rotationOffset += MathHelper.TwoPi;
                }
                if (rotationOffset > maxTurn)
                {
                    projectile.rotation += maxTurn;
                }
                else if (rotationOffset < -maxTurn)
                {
                    projectile.rotation -= maxTurn;
                }
                else
                {
                    projectile.rotation = (player.Center - projectile.Center).ToRotation();
                }
            }
            else
            {
                projectile.hostile = false;

                NPC boss = Main.npc[(int)projectile.ai[0]];

                projectile.velocity += (boss.Center + boss.velocity * projectile.timeLeft - projectile.Center - projectile.velocity * projectile.timeLeft) / (projectile.timeLeft * projectile.timeLeft);
            }

            projectile.frameCounter++;
            if (projectile.frameCounter == 3)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 7)
                {
                    projectile.frame = 0;
                }
            }

            cogRotation += projectile.velocity.X * 0.1f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + new Vector2(4096, 0).RotatedBy(projectile.rotation), 22, ref point);
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Rectangle frame;
            SpriteEffects effects;

            //draw the pet
            Texture2D texture = Main.projectileTexture[projectile.type];
            frame = texture.Frame(1, 7, 0, projectile.frame);
            effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            float wingRotation = projectile.velocity.X * 0.1f;

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White, wingRotation, frame.Size() / 2, projectile.scale, effects, 0f);

            Texture2D texture2 = mod.GetTexture("NPCs/CataBoss/HeavenPetProjectile_Cog");
            frame = texture2.Frame();
            effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            spriteBatch.Draw(texture2, projectile.Center - Main.screenPosition, frame, Color.White, cogRotation, frame.Size() / 2, projectile.scale, effects, 0f);


            if (projectile.timeLeft > 60)
            {
                //draw the prism
                //adapted from last prism drawcode
                Texture2D prismTexture = ModContent.GetTexture("Terraria/Projectile_633");
                frame = prismTexture.Frame(1, 5, 0, (projectile.timeLeft / (projectile.timeLeft <= 600 - 60 ? 1 : 3)) % 5);
                effects = SpriteEffects.None;
                Vector2 drawPosition = projectile.Center - Main.screenPosition + new Vector2(20 * projectile.scale, 0).RotatedBy(projectile.rotation);

                spriteBatch.Draw(prismTexture, drawPosition, frame, Color.White, projectile.rotation + MathHelper.PiOver2, frame.Size() / 2, projectile.scale, effects, 0f);

                float scaleFactor2 = (float)Math.Cos(Math.PI * 2f * (projectile.timeLeft / 30f)) * 2f + 2f;
                if (projectile.timeLeft <= 600 - 60)
                {
                    scaleFactor2 = 4f;
                }
                for (float num350 = 0f; num350 < 4f; num350 += 1f)
                {
                    spriteBatch.Draw(prismTexture, drawPosition + new Vector2(0, 1).RotatedBy(num350 * (Math.PI * 2f) / 4f) * scaleFactor2, frame, Color.White.MultiplyRGBA(new Color(255, 255, 255, 0)) * 0.03f, projectile.rotation + MathHelper.PiOver2, frame.Size() / 2, projectile.scale, effects, 0f);
                }

                if (projectile.timeLeft > 600 - 60)
                {
                    //draw the telegraph line
                    float telegraphAlpha = (60 + projectile.timeLeft - 600) / 30f * (600 - projectile.timeLeft) / 30f;
                    spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/HeavenPetProjectileTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * telegraphAlpha, projectile.rotation, new Vector2(0, 0.5f), new Vector2(4096, 1), SpriteEffects.None, 0f);
                }
                else
                {
                    //draw the beam
                    //adapted from last prism drawcode

                    for (int i = 0; i < 6; i++)
                    {
                        //texture
                        Texture2D tex7 = ModContent.GetTexture("Terraria/Projectile_632");
                        //laser length
                        float num528 = 4096;

                        Color value42 = Main.hslToRgb(i / 6f, 1f, 0.5f);
                        value42.A = 0;

                        Vector2 drawOffset = new Vector2(4,0).RotatedBy(projectile.timeLeft * 0.5f + i * MathHelper.TwoPi / 6);

                        //start position
                        Vector2 value45 = projectile.Center.Floor() + drawOffset + new Vector2(36 * projectile.scale, 0).RotatedBy(projectile.rotation);

                        value45 += Vector2.UnitX.RotatedBy(projectile.rotation) * projectile.scale * 10.5f;
                        num528 -= projectile.scale * 14.5f * projectile.scale;
                        Vector2 vector90 = new Vector2(projectile.scale);
                        DelegateMethods.f_1 = 1f;
                        DelegateMethods.c_1 = value42 * 0.75f * projectile.Opacity;
                        _ = projectile.oldPos[0] + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition;
                        Utils.DrawLaser(spriteBatch, tex7, value45 - Main.screenPosition, value45 + Vector2.UnitX.RotatedBy(projectile.rotation) * num528 - Main.screenPosition, vector90, DelegateMethods.RainbowLaserDraw);
                        DelegateMethods.c_1 = new Color(255, 255, 255, 127) * 0.75f * projectile.Opacity;
                        Utils.DrawLaser(spriteBatch, tex7, value45 - Main.screenPosition, value45 + Vector2.UnitX.RotatedBy(projectile.rotation) * num528 - Main.screenPosition, vector90 / 2f, DelegateMethods.RainbowLaserDraw);
                    }
                }
            }

            return false;
        }
    }

    public class FishronPlatform : ModProjectile
    {
        public override string Texture => "Terraria/NPC_370";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sitting Duke");
            Main.projFrames[projectile.type] = 8;

            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 100;
            projectile.height = 100;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 400;

            projectile.hide = true;
        }

        public override void AI()
        {
            if (projectile.ai[1] == 0)
            {
                NPC boss = Main.npc[(int)projectile.ai[0]];
                Player player = Main.player[boss.target];

                float eta = 5f;
                float determinant = (boss.velocity.Y - player.velocity.Y) * (boss.velocity.Y - player.velocity.Y) - 4 * 0.45f * (boss.Center.Y - player.Center.Y + 240);
                if (determinant >= 0)
                    eta = Math.Max(3, (-(boss.velocity.Y - player.velocity.Y) + (float)Math.Sqrt(determinant)) / 0.9f);
                Vector2 targetPoint = new Vector2(boss.Center.X + boss.velocity.X * eta, player.Center.Y + 240 + player.velocity.Y * eta + projectile.height);
                Vector2 targetVelocity = (targetPoint - projectile.Center) / eta;
                projectile.velocity += (targetVelocity - projectile.velocity) / 10f;
            }
            else
            {
                projectile.velocity.Y += 0.15f;
            }
            
            projectile.rotation = projectile.velocity.ToRotation();
            projectile.direction = projectile.velocity.X > 0 ? 1 : -1;
            projectile.spriteDirection = projectile.direction;

            //frame stuff
            projectile.frameCounter++;
            if (projectile.frameCounter == 3)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame == 8)
                {
                    projectile.frame = 0;
                }
            }
        }

        public override void Kill(int timeLeft)
        {
            if (timeLeft > 0)
            {
                Main.PlaySound(SoundID.NPCHit14, projectile.Center);

                for (int i=0; i<8; i++)
                {
                    Projectile.NewProjectile(projectile.Center, new Vector2(4, 0).RotatedBy(i * MathHelper.TwoPi / 8), ProjectileType<FloatingBubble>(), 80, 0f, Main.myPlayer);
                    Projectile.NewProjectile(projectile.Center, new Vector2(2, 0).RotatedBy((i + 0.5f) * MathHelper.TwoPi / 8), ProjectileType<FloatingBubble>(), 80, 0f, Main.myPlayer);
                }
            }
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame(1, 8, 0, projectile.frame);
            SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            for (int i = projectile.oldPos.Length - 1; i >= 0; i--)
            {
                float alpha = (projectile.oldPos.Length - i) / (float)projectile.oldPos.Length;
                spriteBatch.Draw(texture, projectile.oldPos[i] + projectile.Center - projectile.position - Main.screenPosition, frame, Color.White * alpha, projectile.oldRot[i], frame.Size() / 2, projectile.scale, effects, 0f);
            }

            return false;
        }
    }

    public class FloatingBubble : ModProjectile
    {
        public override string Texture => "Terraria/NPC_371";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Death Bubble");
            Main.projFrames[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 48;
            projectile.height = 48;
            projectile.alpha = 0;
            projectile.light = 0f;
            projectile.aiStyle = -1;
            projectile.hostile = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.scale = 1f;
            projectile.timeLeft = 480;

            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.velocity *= 0.99f;
            projectile.velocity.Y -= 0.08f;

            projectile.rotation += projectile.velocity.X * 0.1f;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            Rectangle frame = texture.Frame(1, 2, 0, projectile.frame);
            SpriteEffects effects = projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, Color.White * 0.5f, projectile.rotation, new Vector2(24, 24), projectile.scale, effects, 0f);

            return false;
        }
    }
}
