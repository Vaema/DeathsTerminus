using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace DeathsTerminus.Projectiles
{
    public class LostStar : ModProjectile
    {
        public override void SetDefaults()
        {
            projectile.CloneDefaults(ProjectileID.BabySkeletronHead);
            Main.projFrames[projectile.type] = 4;
            Main.projPet[projectile.type] = true;
            aiType = ProjectileID.BabySkeletronHead;
            projectile.width = 32;
            projectile.height = 32;
        }

        public override bool PreAI()
        {
            Player player = Main.player[projectile.owner];
            player.skeletron = false; // Sets the default cloned pet to false;
            return true;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];
            DTPlayer modPlayer = player.GetModPlayer<DTPlayer>();
            if (player.dead)
            {
                modPlayer.LostStar = false;
            }
            if (modPlayer.LostStar)
            {
                projectile.timeLeft = 2; // Remain at 2 while tutorialPet == true;
            }
        }
    }
}