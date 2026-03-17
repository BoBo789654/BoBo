using BoBo.Content.Projectiles.Weapons.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

public class DarkStarExplosionProj : ModProjectile
{
	public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
	public override void SetStaticDefaults()
	{

	}
	public override void SetDefaults()
	{
		Projectile.width = 1;
		Projectile.height = 1;
		Projectile.aiStyle = -1;
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.DamageType = DamageClass.Magic;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 161;
		Projectile.alpha = 255;
		Projectile.ignoreWater = true;
		Projectile.tileCollide = false;
		Projectile.hide = true;
	}
	public override void AI()
	{
		//ai[0]存储已生成的弹幕数量
		//ai[1]存储计时器
		Projectile.ai[1]++;
		if (Projectile.ai[1] % 20 == 0)
		{
			if (Projectile.ai[0] < 8)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
					Vector2.Zero, ModContent.ProjectileType<DarkStarExplosionProj2>(),
					(int)(Projectile.damage * Main.rand.NextFloat(0.39f, 0.7f)), Projectile.knockBack,
					Projectile.owner, Main.rand.NextFloat(MathHelper.TwoPi), 0f);
				Projectile.ai[0]++;
				if (Projectile.ai[0] >= 8)
					Projectile.Kill();
			}
		}
	}
}