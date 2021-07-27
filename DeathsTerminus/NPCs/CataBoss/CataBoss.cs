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
        SideScythesAttackSpin = 4,
        ArenaScythesCircle = 5,
        IceSpiralAttack = 6,
    }

    [AutoloadBossHead]
    public class CataBoss : ModNPC
    {
        //ai[0] is attack type
        //ai[1] is attack timer
        //ai[2] is arena projectile
        //ai[3] is ?

        private Projectile arenaProjectile
        {
            get { return Main.projectile[(int)npc.ai[2]]; }
        }

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
                        npc.Transform(NPCType<NPCs.CataclysmicArmageddon>());
                    }
                    return;
                }
            }

            switch ((CataBossAttackIDs)npc.ai[0])
            {
                case CataBossAttackIDs.SpawnAnimation:
                    //spawn behavior! Just move into place near the player for now, and spawn the arena

                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                    npc.spriteDirection = npc.direction;
                    Vector2 goalPosition = player.Center + new Vector2(-npc.direction, 0) * 240;
                    Vector2 goalVelocity = (goalPosition - npc.Center) / 10;
                    if (goalVelocity.Length() > 4)
                    {
                        npc.velocity += (goalVelocity - npc.velocity) / 10;
                    }

                    if (npc.ai[1] == 0)
                    {
                        if (Main.netMode != 1)
                        {
                            npc.ai[2] = Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<CataBossArena>(), 80, 0f, Main.myPlayer, npc.whoAmI);
                        }
                        npc.netUpdate = true;
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
                            }

                            for (int i = -2; i <= 2; i++)
                            {
                                Projectile.NewProjectile(npc.Center, npc.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 + angleOffset * i) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                                Projectile.NewProjectile(npc.Center, npc.velocity.SafeNormalize(Vector2.Zero).RotatedBy(-MathHelper.PiOver2 + angleOffset * i) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
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
                                }
                            }
                        }

                    npc.ai[1]++;
                    if (npc.ai[1] == 380)
                    {
                        //give it time to get into position
                        npc.ai[1] = -60;
                        npc.ai[0] = (int)CataBossAttackIDs.ArenaScythesCircle;
                    }
                    break;
                case CataBossAttackIDs.ArenaScythesCircle:
                    if (npc.ai[1] < 0)
                    {
                        npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                        npc.spriteDirection = npc.direction;
                        goalPosition = arenaProjectile.Center + new Vector2(-npc.direction, 0) * (arenaProjectile.ai[1] + 120);
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
                        goalPosition = arenaProjectile.Center + new Vector2(-npc.direction, 0).RotatedBy(npc.direction * npc.ai[1] / 10f) * (arenaProjectile.ai[1] + 120);
                        goalVelocity = (goalPosition - npc.Center) / 2;
                        npc.velocity += (goalVelocity - npc.velocity) / 2;

                        if (npc.ai[1] % 4 == 0)
                        {
                            Projectile.NewProjectile(npc.Center, (arenaProjectile.Center - npc.Center).SafeNormalize(Vector2.Zero) * 0.5f, ProjectileType<CataBossScythe>(), 80, 0f, Main.myPlayer);
                        }
                    }

                    npc.ai[1]++;
                    if (npc.ai[1] == 360)
                    {
                        //give it time to get into position
                        npc.ai[1] = -60;
                        npc.ai[0] = (int)CataBossAttackIDs.IceSpiralAttack;
                    }
                    break;
                case CataBossAttackIDs.IceSpiralAttack:
                    npc.direction = player.Center.X > npc.Center.X ? 1 : -1;
                    npc.spriteDirection = npc.direction;
                    goalPosition = arenaProjectile.Center;
                    goalVelocity = (goalPosition - npc.Center) / 10;
                    if (goalVelocity.Length() < 4)
                    {
                        goalVelocity = goalVelocity.SafeNormalize(Vector2.Zero) * 4;
                    }
                    npc.velocity += (goalVelocity - npc.velocity) / 10;

                    if (npc.ai[1] >= 0)
                    {
                        npc.velocity = arenaProjectile.Center - npc.Center;

                        if (npc.ai[1] == 0 && Main.netMode != 1)
                        {
                            for (int i = -1; i <= 1; i++)
                            {
                                Projectile.NewProjectile(arenaProjectile.Center, Vector2.Zero, ProjectileType<RotatingIceShards>(), 80, 0f, Main.myPlayer, ai0: i);
                            }
                        }
                    }

                    npc.ai[1]++;
                    if (npc.ai[1] == 360)
                    {
                        //give it time to get into position
                        npc.ai[1] = -60;
                        npc.ai[0] = (int)CataBossAttackIDs.IceSpiralAttack;
                    }
                    break;
            }
        }

        public override bool CheckDead()
        {
            NPC.NewNPC((int)npc.position.X, (int)npc.position.Y, NPCType<CataclysmicArmageddon>());
            return true;
        }

        public override bool CheckActive()
        {
            return false;
        }
    }

    public class CataBossArena : ModProjectile
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
            projectile.hostile = true;
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
                projectile.scale += 1 / 60f;
            }
            else
            {
                projectile.scale = 1f;
            }

            //rotation increment
            projectile.rotation += 0.01f;

            //set radius and center (replace with more dynamic AI later)
            projectile.ai[1] = 1200;
            projectile.velocity = (player.Center - projectile.Center) * GetArenaSpeed(boss);

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
                    return 1 / 120f;
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

            if (projectile.ai[1] == 0)
            {
                projectile.ai[1] = 1;
                if (Main.netMode != 1)
                {
                    Projectile.NewProjectile(projectile.Center, projectile.velocity, ProjectileType<CataBossScytheTelegraph>(), 0, 0f, Main.myPlayer);
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

    public class RotatingIceShards : ModProjectile
    {
        public override string Texture => "Terraria/Extra_35";

        private static int shardRadius = 12;
        private static int shardCount = 12;

        //the arena!
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ice Shard");
            Main.projFrames[projectile.type] = 1;
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
            projectile.scale = 1f;
            projectile.timeLeft = 360;
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

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];

            for (int i = 0; i < shardCount; i++)
            {
                Rectangle frame = texture.Frame(1, 3);

                spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(projectile.ai[1] * projectile.scale, 0).RotatedBy(projectile.rotation + i * MathHelper.TwoPi / shardCount), frame, Color.White * projectile.scale, projectile.rotation + i * MathHelper.TwoPi / shardCount - MathHelper.PiOver2, new Vector2(12, 37), projectile.scale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
