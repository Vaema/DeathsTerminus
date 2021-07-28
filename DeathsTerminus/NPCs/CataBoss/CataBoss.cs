using System;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using DeathsTerminus.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DeathsTerminus.NPCs.CataBoss
{
    [AutoloadBossHead]
    public class CataBoss : ModNPC
    {
        //ai[0] is attack type
        //ai[1] is attack timer
        //ai[2] is ?
        //ai[3] is ?

        private bool canShieldBonk;
        private bool holdingShield;
        private bool onSlimeMount;
        private int iceShieldCooldown;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cataclysmic Armageddon");
            Main.npcFrameCount[npc.type] = 1;
        }

        public override void SetDefaults()
        {
            npc.aiStyle = (int)AIStyles.CustomAI;
            npc.width = 18;
            npc.height = 40;

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
                    //5 secs each
                    SideBlastsAttackHard();
                    break;
                case 10:
                    npc.ai[0] = 0;
                    break;

                    /*
                    case 0:
                        //Spawn animation
                        SpawnAnimation();
                        break;
                    case 1:
                        //Basic side scythes
                        SideScythesAttack();
                        break;
                    case 2:
                        //Spinning side scythes
                        SideScythesAttackSpin();
                        break;
                    case 3:
                        //Scythe blasts
                        SideBlastsAttack();
                        break;
                    case 4:
                        //More scythe blasts
                        SideBlastsAttackHard();
                        break;
                    case 5:
                        //Ice spiral
                        IceSpiralAttack();
                        break;
                    case 6:
                        //Shield bonk
                        ShieldBonk();
                        break;
                    case 7:
                        npc.ai[0] = 1;
                        //Slime bonk
                        //SlimeBonk();
                        break;
                    case 8:
                        npc.ai[0] = 1;
                        //Moths
                        //SlimeBonk();
                        break;
                    case 9:
                        npc.ai[0] = 1;
                        //Sigils
                        //SlimeBonk();
                        break;
                    case 10:
                        npc.ai[0] = 1;
                        //LAMP beams
                        //SlimeBonk();
                        break;
                    case 11:
                        npc.ai[0] = 1;
                        //LAMP beams
                        //SlimeBonk();
                        break;
                    */
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

            npc.ai[1]++;
            if (npc.ai[1] == 60 + shotPeriod * numShots)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        //5 secs
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

                Projectile.NewProjectile(npc.Center, shotVelocity, ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer);
                Projectile.NewProjectile(npc.Center, shotVelocity.RotatedBy(0.15f), ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer);
                Projectile.NewProjectile(npc.Center, shotVelocity.RotatedBy(-0.15f), ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer);
                Projectile.NewProjectile(npc.Center, -shotVelocity, ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer);
                Projectile.NewProjectile(npc.Center, -shotVelocity.RotatedBy(0.15f), ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer);
                Projectile.NewProjectile(npc.Center, -shotVelocity.RotatedBy(-0.15f), ProjectileType<CataBossSuperScythe>(), 80, 0f, Main.myPlayer);
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
                npc.dontTakeDamage = true;

                npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                npc.spriteDirection = npc.direction;
                Vector2 goalPosition = player.Center + (npc.Center - player.Center).SafeNormalize(Vector2.Zero) * 240;

                FlyToPoint(goalPosition, Vector2.Zero, maxXAcc: 0.5f, maxYAcc: 0.5f);
            }
            else
            {
                npc.dontTakeDamage = false;
                npc.velocity = Vector2.Zero;

                if (npc.ai[1] == 60 && Main.netMode != 1)
                {
                    Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<IceShield>(), 80, 0f, Main.myPlayer);
                }

                if (npc.ai[1] == 120 && Main.netMode != 1)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<RotatingIceShards>(), 80, 0f, Main.myPlayer, ai0: i);
                    }
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

                FlyToPoint(goalPosition, player.velocity / 2, 0.8f, 0.8f);
            }
            else
            {
                if (npc.ai[1] == 60)
                {
                    onSlimeMount = true;

                    npc.width = 40;
                    npc.position.X -= 11;
                    npc.height = 64;
                    drawOffsetY = -24;
                }
                else if (npc.ai[1] == 119)
                {
                    npc.velocity.Y = 0;
                    onSlimeMount = false;

                    npc.width = 18;
                    npc.position.X += 11;
                    npc.height = 40;
                    drawOffsetY = 0;
                }
                else
                {
                    npc.velocity.Y += 0.6f;

                    FlyToPoint(player.Center, player.velocity, 0.05f, 0f);
                }
            }

            npc.ai[1]++;
            if (npc.ai[1] == 120)
            {
                npc.ai[1] = 0;
                npc.ai[0]++;
            }
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            return canShieldBonk || onSlimeMount;
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
                npc.velocity.Y = -18f;
            }
        }

        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            damage = 0;
            iceShieldCooldown = 60;
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (onSlimeMount)
            {
                Texture2D mountTexture = ModContent.GetTexture("Terraria/Mount_Slime");
                Rectangle frame = mountTexture.Frame(1, 4, 0, 1);
                SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                Vector2 mountOffset = new Vector2(npc.spriteDirection * 0, 10);
                spriteBatch.Draw(mountTexture, npc.Center - Main.screenPosition + mountOffset, frame, drawColor, 0f, frame.Size() / 2f, 1f, effects, 0f);
            }

            return true;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (holdingShield)
            {
                Texture2D shieldTexture = ModContent.GetTexture("Terraria/Acc_Shield_5");
                Rectangle frame = shieldTexture.Frame(1, 20);
                SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                Vector2 shieldOffset = new Vector2(npc.spriteDirection * 4, -4);

                spriteBatch.Draw(shieldTexture, npc.Center - Main.screenPosition + shieldOffset, frame, drawColor, 0f, frame.Size() / 2f, 1f, effects, 0f);
            }

            if (iceShieldCooldown > 0)
            {
                Texture2D shieldTexture = ModContent.GetTexture("Terraria/Projectile_464");
                Rectangle frame = shieldTexture.Frame();
                SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                float alpha = iceShieldCooldown / 120f;
                Vector2 shieldOffset = new Vector2(0, -2);

                spriteBatch.Draw(shieldTexture, npc.Center - Main.screenPosition + shieldOffset, frame, drawColor * alpha, 0f, frame.Size() / 2f, 1f, effects, 0f);
            }
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

    /*public class CataBossArena : ModProjectile
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
            projectile.scale = 0f;
            projectile.timeLeft = 2;
        }

        public override void AI()
        {
            NPC boss = Main.npc[(int)projectile.ai[0]];
            Player player = Main.player[boss.target];

            if (!boss.active)
            {
                projectile.Kill();
                return;
            }
            else
            {
                projectile.timeLeft = 2;
            }

            //grow to full size
            if (projectile.scale < 1)
            {
                projectile.scale += 1 / 120f;
            }
            else
            {
                projectile.scale = 1f;
                projectile.hostile = true;
            }

            //rotation increment
            projectile.rotation += 0.01f;

            //set radius and center (replace with more dynamic AI later)
            projectile.ai[1] = 1200;
            projectile.velocity = (player.Center - projectile.Center) * GetArenaSpeed(boss);

            if (projectile.scale >= 1)
            {
                if ((Main.LocalPlayer.Center - projectile.Center).Length() > projectile.ai[1])
                {
                    Vector2 normal = (Main.LocalPlayer.Center - projectile.Center).SafeNormalize(Vector2.Zero);
                    Vector2 relativeVelocity = Main.LocalPlayer.velocity - projectile.velocity;

                    Main.LocalPlayer.Center = projectile.Center + normal * projectile.ai[1];

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
        }

        private float GetArenaSpeed(NPC boss)
        {
            switch ((CataBossAttackIDs)boss.ai[0])
            {
                case CataBossAttackIDs.SpawnAnimation:
                    return 1 / 60f;
                case CataBossAttackIDs.SideScythesAttack:
                case CataBossAttackIDs.SideScythesAttackSpin:
                    return 1 / 400f;
                case CataBossAttackIDs.SideDashesAttack:
                case CataBossAttackIDs.SideDashesAttack2:
                    return 1 / 600f;
                case CataBossAttackIDs.ArenaScythesCircle:
                case CataBossAttackIDs.IceSpiralAttack:
                    return 0f;
            }
            return 0f;
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

            for (int i=0; i < sigilCount; i++)
            {
                spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(projectile.ai[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / sigilCount), new Rectangle(0, 96 * projectile.frame, 66, 96), Color.White * projectile.scale, projectile.rotation + i * MathHelper.TwoPi / sigilCount, new Vector2(33, 65), projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }*/

    public class CataBossScythe : ModProjectile
    {
        //demon scythe but no dust and it passes through tiles
        public override string Texture => "Terraria/Projectile_" + ProjectileID.DemonSickle;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Demon Scythe");
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
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height), Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(Main.projectileTexture[projectile.type].Width / 2f, Main.projectileTexture[projectile.type].Height / 2f), projectile.scale, SpriteEffects.None, 0f);

            if (projectile.timeLeft > 130 - 30)
            {
                float telegraphAlpha = (30 + projectile.timeLeft - 130) / 30f;
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.Purple * telegraphAlpha, projectile.velocity.ToRotation(), new Vector2(0, 0.5f), new Vector2(4096, 1), SpriteEffects.None, 0f);
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
                Projectile.NewProjectile(projectile.Center, projectile.velocity.SafeNormalize(Vector2.Zero) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                Projectile.NewProjectile(projectile.Center, projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                Projectile.NewProjectile(projectile.Center, projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.PiOver2) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
            }
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height), Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(Main.projectileTexture[projectile.type].Width / 2f, Main.projectileTexture[projectile.type].Height / 2f), projectile.scale, SpriteEffects.None, 0f);

            if (projectile.timeLeft > 160 - 30)
            {
                float telegraphAlpha = (30 + projectile.timeLeft - 160) / 30f;
                spriteBatch.Draw(mod.GetTexture("NPCs/CataBoss/CataBossScytheTelegraph"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.Purple * telegraphAlpha, projectile.velocity.ToRotation(), new Vector2(0, 0.5f), new Vector2(4096, 1), SpriteEffects.None, 0f);
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
            projectile.timeLeft = 360;

            projectile.hide = true;
        }

        public override void AI()
        {
            float angle = MathHelper.TwoPi * projectile.timeLeft / 360f;

            //set radius and rotation (replace with more dynamic AI later)
            projectile.ai[1] = 600 * (float)Math.Sqrt(2 - 2 * Math.Cos(angle));
            projectile.rotation = projectile.ai[0] * (angle + MathHelper.Pi) / 2;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            for (int i = 0; i < shardCount; i++)
            {
                Vector2 circleCenter = projectile.Center + new Vector2(projectile.ai[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / shardCount);
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

                for (int j = projectile.timeLeft; j >= 0; j--)
                {
                    float angle = MathHelper.TwoPi * j / 360f;
                    float radius = 600 * (float)Math.Sqrt(2 - 2 * Math.Cos(angle));
                    float rotation = projectile.ai[0] * (angle + MathHelper.Pi) / 2;
                    float alphaMultiplier = Math.Max(0, (180 - projectile.timeLeft + j) / 180f);

                    spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(radius * projectile.scale, 0).RotatedBy(rotation + i * MathHelper.TwoPi / shardCount), frame, Color.White * alphaMultiplier * 0.03f, rotation + i * MathHelper.TwoPi / shardCount - MathHelper.PiOver2 + projectile.ai[0] * MathHelper.Pi * (1 + j / 360f), new Vector2(12, 37), projectile.scale, SpriteEffects.None, 0f);
                }

                spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(projectile.ai[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / shardCount), frame, Color.White, projectile.rotation + i * MathHelper.TwoPi / shardCount - MathHelper.PiOver2 + projectile.ai[0] * MathHelper.Pi * (1 + projectile.timeLeft / 360f), new Vector2(12, 37), projectile.scale, SpriteEffects.None, 0f);
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

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
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
}
