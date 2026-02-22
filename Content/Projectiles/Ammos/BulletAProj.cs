using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Ammos
{
	public class BulletAProj : ModProjectile
	{
		public override string Texture => Pictures.AmmosProj + Name;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}
		public override void SetDefaults()
		{
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.scale = 1f;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.penetrate = 1;
			Projectile.alpha = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.aiStyle = -1;
			Projectile.light = 0.2f;
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			if (Main.rand.NextBool(3))//添加子弹尾迹效果
			{
				for(int i = 0; i < 10; i++)
				{
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
					DustID.Smoke, 0f, 0f, 100, default, 0.5f);
					dust.velocity *= 0.3f;
					dust.noGravity = true;
				}
			}
		}
		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 40; i++)//子弹击中时的粒子效果
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
					DustID.Torch, 0f, 0f, 100, default, 1f);
				dust.velocity *= 1.4f;
				dust.noGravity = true;
			}
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);//播放击中音效
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);//子弹撞墙时的特殊效果
			return true;
		}
	}
}