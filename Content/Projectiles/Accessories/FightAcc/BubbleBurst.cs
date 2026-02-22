using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace BoBo.Content.Projectiles.Accessories.FightAcc
{
	public class BubbleBurst : ModProjectile
	{
		public override string Texture => Pictures.FightAccProj + Name;
		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = true;
			Projectile.timeLeft = 30;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.scale = 0.8f;
		}
		public override void AI()
		{
			Projectile.Opacity -= 0.04f;//透明度衰减
			Projectile.scale -= 0.01f;
			if (Projectile.Opacity <= 0) Projectile.Kill();
			Projectile.velocity *= 0.96f;//减速
		}
		public override Color? GetAlpha(Color lightColor)
			=> new Color(200, 220, 255) * Projectile.Opacity;
	}
}