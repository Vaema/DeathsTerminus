using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DeathsTerminus.Projectiles
{

	public class Protojectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Protojectile");
		}

		public override void SetDefaults()
		{
			projectile.width = 24;
			projectile.height = 40;
			projectile.friendly = true;
			projectile.melee = true;
			projectile.tileCollide = true;
			projectile.penetrate = 2;
			projectile.timeLeft = 200;
			projectile.light = 0.75f;
			projectile.extraUpdates = 1;
			projectile.ignoreWater = true;
		}
		public override void AI()
		{
			projectile.rotation = (float)Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.57f;
		}
		public override void OnHitNPC(NPC n, int damage, float knockback, bool crit)
		{
			n.AddBuff(BuffID.OnFire, 180);
		}
	}
}
