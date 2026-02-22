using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Accessories.FightAcc
{
	public class BubbleShieldCrack : ModProjectile
	{
		public override string Texture => Pictures.FightAccProj + Name;
		float ringScale = 2f;//环的大小缩放
		float ringAlpha = 0f;//环的透明度
		Color Color = new Color(30, 144, 255);
		public override void SetDefaults()
		{
			Projectile.width = 256;
			Projectile.height = 256;
			Projectile.timeLeft = 20;
			Projectile.penetrate = 5;
			Projectile.aiStyle = -1;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
		}
		public override bool? CanHitNPC(NPC target)
		{
			return false;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Color drawColor = Color * ringAlpha;
			Main.EntitySpriteDraw(
				texture,
				player.Center - Main.screenPosition,
				new Rectangle(0, 0, texture.Width, texture.Height),
				drawColor,
				Projectile.rotation,
				texture.Size() / 2,
				Projectile.scale * ringScale,
				SpriteEffects.None);
			return false;
		}
		public override void AI()
		{
			if (Projectile.timeLeft == 20)//初始化阶段
			{
				ringAlpha = 1f;
				ringScale = 1f;
				Projectile.ai[0] = 1;//激活膨胀效果 
			}
			if (Projectile.ai[0] == 1)//膨胀效果阶段
			{
				ringScale += 0.2f;//环逐渐变大
				ringAlpha -= 0.05f;//环逐渐变透明
			}
			if (ringScale >= 20)//环过大时消失
			{
				Projectile.Kill();
			}
			Projectile.Center = Main.player[Projectile.owner].Center;//跟随玩家
		}
	}
}