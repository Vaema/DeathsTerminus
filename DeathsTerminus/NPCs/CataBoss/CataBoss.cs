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
    public enum CataBossAttackIDs
    {
        SpawnAnimation = 0,
        SideScythesAttack = 1,
        SideDashesAttack = 2,
        SideDashesAttack2 = 3,
        CircleAttack = 4,
        SideScythesAttackSpin = 5
    }

    [AutoloadBossHead]
    public class CataBoss : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Cataclysmic Armageddon");
            Main.npcFrameCount[npc.type] = 1;
        }

        public override void SetDefaults()
        {
            npc.aiStyle = (int)AIStyles.CustomAI;
            npc.width = 26;
            npc.height = 40;

            npc.defense = 0;
            npc.lifeMax = 250;

            npc.damage = 0;
            npc.knockBackResist = 0f;

            npc.value = Item.buyPrice(platinum: 1);

            npc.npcSlots = 15f;
            npc.boss = true;
            //bossBag = ItemType<CataBossBag>();

            npc.lavaImmune = true;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.dontTakeDamage = true;

            npc.buffImmune[BuffID.Confused] = true;

            music = MusicID.Boss4;
        }

        public override void AI()
        {
            Player player = Main.player[npc.target];
            if (!player.active || player.dead)
            {
                npc.TargetClosest(false);
                player = Main.player[npc.target];
                if (player.dead)
                {
                    if (npc.timeLeft > 10)
                    {
                        npc.timeLeft = 10;
                    }
                    if (npc.timeLeft == 1)
                    {
                        npc.active = false;
                        NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, NPCType<CataclysmicArmageddon>());
                    }
                    return;
                }
            }

            switch ((CataBossAttackIDs)npc.ai[0])
            {
                case CataBossAttackIDs.SpawnAnimation:
                    //spawn behavior! Just move into place near the player for now

                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                    npc.spriteDirection = npc.direction;
                    Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;
                    Vector2 goalVelocity = (goalPosition - npc.Center) / 10;
                    if (goalVelocity.Length() > 4)
                    {
                        npc.velocity += (goalVelocity - npc.velocity) / 10;
                    }

                    npc.ai[1]++;
                    if (npc.ai[1] == 120)
                    {
                        npc.ai[1] = 0;
                        npc.ai[0] = (int)CataBossAttackIDs.SideScythesAttack;
                    }
                    break;
                case CataBossAttackIDs.SideScythesAttack:
                    //hover by the player's side while shooting scythes at them

                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                    npc.spriteDirection = npc.direction;
                    goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;
                    goalVelocity = (goalPosition - npc.Center) / 10;
                    if (goalVelocity.Length() < 4)
                    {
                        goalVelocity = goalVelocity.SafeNormalize(Vector2.Zero) * 4;
                    }
                    npc.velocity += (goalVelocity - npc.velocity) / 10;

                    if (npc.ai[1] >= 0 && npc.ai[1] % 10 == 0 && Main.netMode != 1)
                    {
                        int numShots = 8;
                        for (int i = 0; i < numShots; i++)
                        {
                            Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                            Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots) * 0.5f, ProjectileType<CataBossScytheTelegraph>(), 0, 0f, Main.myPlayer);
                        }
                    }

                    npc.ai[1]++;
                    if (npc.ai[1] == 120)
                    {
                        //give it time to get into position
                        npc.ai[1] = -60;
                        npc.ai[0] = (int)CataBossAttackIDs.SideDashesAttack;
                    }
                    break;
                case CataBossAttackIDs.SideDashesAttack:
                    //dash around the player while shooting scythes

                    int dashPeriod = 40;
                    int postDashPause = 10;
                    int numDashes = 4;
                    int scythePeriod = 6;
                    float dashSpeed = 30;

                    if (npc.ai[1] < 0)
                    {
                        npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                        npc.spriteDirection = npc.direction;
                        goalPosition = player.Center + new Vector2(-npc.direction, 0) * dashSpeed / 2 * dashPeriod;
                        goalVelocity = (goalPosition - npc.Center) / 10;
                        if (goalVelocity.Length() < 4)
                        {
                            goalVelocity = goalVelocity.SafeNormalize(Vector2.Zero) * 4;
                        }
                        npc.velocity += (goalVelocity - npc.velocity) / 10;
                    }
                    else
                    {
                        if (npc.ai[1] % (dashPeriod + postDashPause) == 0)
                        {
                            float angleRatio = 200 / (player.Center - npc.Center).Length();
                            if (angleRatio > 1)
                            {
                                angleRatio = 1;
                            }

                            npc.velocity = (player.Center - npc.Center).SafeNormalize(Vector2.Zero).RotatedBy(Math.Asin(angleRatio)) * dashSpeed;

                            npc.direction = npc.velocity.X > 0 ? 1 : -1;
                            npc.spriteDirection = npc.direction;
                        }

                        if (npc.ai[1] % (dashPeriod + postDashPause) % scythePeriod == 0 && npc.ai[1] % (dashPeriod + postDashPause) < dashPeriod)
                        {
                            Projectile.NewProjectile(npc.Center, npc.velocity.SafeNormalize(Vector2.Zero) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                            Projectile.NewProjectile(npc.Center, npc.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                            Projectile.NewProjectile(npc.Center, npc.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.PiOver2) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);

                            Projectile.NewProjectile(npc.Center, npc.velocity.SafeNormalize(Vector2.Zero), ProjectileType<CataBossScytheTelegraph>(), 0, 0f, Main.myPlayer);
                            Projectile.NewProjectile(npc.Center, npc.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2), ProjectileType<CataBossScytheTelegraph>(), 0, 0f, Main.myPlayer);
                            Projectile.NewProjectile(npc.Center, npc.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.PiOver2), ProjectileType<CataBossScytheTelegraph>(), 0, 0f, Main.myPlayer);
                        }

                        if (npc.ai[1] % (dashPeriod + postDashPause) == dashPeriod)
                        {
                            npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * 4;
                        }
                    }

                    npc.ai[1]++;
                    if (npc.ai[1] == numDashes * (dashPeriod + postDashPause))
                    {
                        npc.ai[1] = -60;
                        npc.ai[0] = (int)CataBossAttackIDs.SideDashesAttack2;
                    }
                    break;
                case CataBossAttackIDs.SideDashesAttack2:
                    //dash around the player while shooting scythes

                    dashPeriod = 50;
                    postDashPause = 10;
                    numDashes = 4;
                    scythePeriod = 8;
                    dashSpeed = 30;

                    if (npc.ai[1] < 0)
                    {
                        npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                        npc.spriteDirection = npc.direction;
                        goalPosition = player.Center + new Vector2(-npc.direction, 0) * dashSpeed / 2 * dashPeriod;
                        goalVelocity = (goalPosition - npc.Center) / 10;
                        if (goalVelocity.Length() < 4)
                        {
                            goalVelocity = goalVelocity.SafeNormalize(Vector2.Zero) * 4;
                        }
                        npc.velocity += (goalVelocity - npc.velocity) / 10;
                    }
                    else
                    {
                        if (npc.ai[1] % (dashPeriod + postDashPause) == 0)
                        {
                            float angleRatio = 200 / (player.Center - npc.Center).Length();
                            if (angleRatio > 1)
                            {
                                angleRatio = 1;
                            }

                            npc.velocity = (player.Center - npc.Center).SafeNormalize(Vector2.Zero).RotatedBy(Math.Asin(angleRatio)) * dashSpeed;

                            npc.direction = npc.velocity.X > 0 ? 1 : -1;
                            npc.spriteDirection = npc.direction;
                        }

                        if (npc.ai[1] % (dashPeriod + postDashPause) % scythePeriod == 0 && npc.ai[1] % (dashPeriod + postDashPause) < dashPeriod)
                        {
                            float angleOffset = MathHelper.TwoPi / 64;

                            for (int i = -2; i <= 2; i++)
                            {
                                Projectile.NewProjectile(npc.Center + npc.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * i * 16, npc.velocity.SafeNormalize(Vector2.Zero) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                                Projectile.NewProjectile(npc.Center + npc.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * i * 16, npc.velocity.SafeNormalize(Vector2.Zero), ProjectileType<CataBossScytheTelegraph>(), 0, 0f, Main.myPlayer);
                            }

                            for (int i = -2; i <= 2; i++)
                            {
                                Projectile.NewProjectile(npc.Center, npc.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 + angleOffset * i) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                                Projectile.NewProjectile(npc.Center, npc.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.PiOver2 + angleOffset * i) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                                Projectile.NewProjectile(npc.Center, npc.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 + angleOffset * i), ProjectileType<CataBossScytheTelegraph>(), 0, 0f, Main.myPlayer);
                                Projectile.NewProjectile(npc.Center, npc.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.PiOver2 + angleOffset * i), ProjectileType<CataBossScytheTelegraph>(), 0, 0f, Main.myPlayer);
                            }
                        }

                        if (npc.ai[1] % (dashPeriod + postDashPause) == dashPeriod)
                        {
                            npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * 4;
                        }
                    }

                    npc.ai[1]++;
                    if (npc.ai[1] == numDashes * (dashPeriod + postDashPause))
                    {
                        //give it time to get into position
                        npc.ai[1] = -60;
                        npc.ai[0] = (int)CataBossAttackIDs.CircleAttack;
                    }
                    break;
                case CataBossAttackIDs.CircleAttack:
                    if (npc.ai[1] < 0)
                    {
                        npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                        npc.spriteDirection = npc.direction;
                        goalPosition = player.Center + new Vector2(-npc.direction, 0) * 480;
                        goalVelocity = (goalPosition - npc.Center) / 10;
                        if (goalVelocity.Length() < 4)
                        {
                            goalVelocity = goalVelocity.SafeNormalize(Vector2.Zero) * 4;
                        }
                        npc.velocity += (goalVelocity - npc.velocity) / 10;
                    }
                    else
                    {
                        npc.spriteDirection = player.Center.X > npc.Center.X ? 1 : -1;
                        goalPosition = player.Center + new Vector2(-npc.direction, 0).RotatedBy(npc.direction * npc.ai[1] / 8f) * 480;
                        goalVelocity = (goalPosition - npc.Center) / 10;
                        npc.velocity += (goalVelocity - npc.velocity) / 10;

                        if (npc.ai[1] % 10 == 0)
                        {
                            Projectile.NewProjectile(npc.Center, (player.Center - npc.Center).SafeNormalize(Vector2.Zero) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                            Projectile.NewProjectile(npc.Center, (player.Center - npc.Center).SafeNormalize(Vector2.Zero) * 0.5f, ProjectileType<CataBossScytheTelegraph>(), 0, 0f, Main.myPlayer);
                        }
                    }

                    npc.ai[1]++;
                    if (npc.ai[1] == 240)
                    {
                        //give it time to get into position
                        npc.ai[1] = -60;
                        npc.ai[0] = (int)CataBossAttackIDs.SideScythesAttackSpin;
                    }
                    break;
                case CataBossAttackIDs.SideScythesAttackSpin:
                    //hover by the player's side while shooting scythes at them that rotate in orientation
                    //first do one rotating downwards, then one rotating upwards

                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                    npc.spriteDirection = npc.direction;
                    goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;
                    goalVelocity = (goalPosition - npc.Center) / 10;
                    if (goalVelocity.Length() < 4)
                    {
                        goalVelocity = goalVelocity.SafeNormalize(Vector2.Zero) * 4;
                    }
                    npc.velocity += (goalVelocity - npc.velocity) / 10;

                    if (npc.ai[1] < 160)
                    {
                        if (npc.ai[1] >= 0 && npc.ai[1] % 10 == 0 && Main.netMode != 1)
                        {
                            int numShots = 8;
                            for (int i = 0; i < numShots; i++)
                            {
                                Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots + npc.direction * npc.ai[1] / 100f) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                                Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots + npc.direction * npc.ai[1] / 100f) * 0.5f, ProjectileType<CataBossScytheTelegraph>(), 0, 0f, Main.myPlayer);
                            }
                        }
                    }
                    else if (npc.ai[1] >= 220)
                        {
                            if (npc.ai[1] >= 0 && npc.ai[1] % 10 == 0 && Main.netMode != 1)
                            {
                                int numShots = 8;
                                for (int i = 0; i < numShots; i++)
                                {
                                    Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots - npc.direction * npc.ai[1] / 100f) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                                    Projectile.NewProjectile(npc.Center, new Vector2(1, 0).RotatedBy(i * MathHelper.TwoPi / numShots - npc.direction * npc.ai[1] / 100f) * 0.5f, ProjectileType<CataBossScytheTelegraph>(), 0, 0f, Main.myPlayer);
                                }
                            }
                        }

                    npc.ai[1]++;
                    if (npc.ai[1] == 380)
                    {
                        //give it time to get into position
                        npc.ai[1] = -60;
                        npc.ai[0] = (int)CataBossAttackIDs.SideScythesAttack;
                    }
                    break;
            }
        }

        public override bool CheckDead()
        {
            NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, NPCType<CataclysmicArmageddon>());
            return true;
        }
    }

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
            projectile.scale = 0.9f;
            projectile.timeLeft = 600;
        }

        public override void AI()
        {
            projectile.rotation += projectile.direction * 0.8f;
            projectile.ai[0] += 1f;
            if (!(projectile.ai[0] < 30f))
            {
                if (projectile.ai[0] < 100f)
                {
                    projectile.velocity *= 1.06f;
                }
                else
                {
                    projectile.ai[0] = 200f;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0, 0, Main.projectileTexture[projectile.type].Width, Main.projectileTexture[projectile.type].Height), Color.White * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(Main.projectileTexture[projectile.type].Width / 2f, Main.projectileTexture[projectile.type].Height / 2f), projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class CataBossScytheTelegraph : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scythe Beam");
        }

        public override void SetDefaults()
        {
            projectile.aiStyle = -1;
            projectile.width = 2;
            projectile.height = 2;

            projectile.timeLeft = 60;

            projectile.penetrate = -1;
            projectile.hostile = false;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.light = 0f;
            projectile.hide = true;
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation();

            projectile.alpha += 8;
            if (projectile.alpha >= 255)
            {
                projectile.Kill();
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            spriteBatch.Draw(Main.projectileTexture[projectile.type], projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.Purple * (1 - projectile.alpha / 255f), projectile.rotation, new Vector2(0, 0.5f), new Vector2(4096, 1), SpriteEffects.None, 0f);

            return false;
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCs.Add(index);
        }
    }
}
