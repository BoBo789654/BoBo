using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Accessories
{
	public class HolyGuardianProj2 : ModProjectile
	{
		private const float SemiMajorAxis = 160f;//半长轴
		private const float SemiMinorAxis = 120f;//半短轴
		private const int MaxFrames = 20;//特效持续时间
		public override string Texture => Pictures.OtherAccProj + Name;
		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 30;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = MaxFrames;
			Projectile.penetrate = -1;
			Projectile.alpha = 0;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;//获取贴图
			float progress = 1f - (float)Projectile.timeLeft / MaxFrames;//计算进度
			float alpha = 1f - progress;//根据进度计算透明度
			Color baseColor = new Color(50, 200, 50, (int)(alpha * 200));//设置颜色
			Color coreColor = Color.Lerp(Color.LightGreen, Color.White, progress);
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;
			Vector2 origin = texture.Size() / 2f;
			//计算缩放比例，确保爆炸特效与治疗范围匹配，贴图是圆形的，需要将其拉伸为椭圆形
			float textureRadius = texture.Width / 2f;//贴图是圆形
			//计算X和Y方向的缩放因子：使得爆炸特效的X轴半径 = SemiMajorAxis, Y轴半径 = SemiMinorAxis
			float scaleX = SemiMinorAxis / textureRadius;
			float scaleY = SemiMinorAxis / textureRadius;
			float progressScale = MathHelper.Lerp(0.1f, 1.5f, progress);//根据进度调整缩放
			Vector2 scale = new Vector2(scaleX, scaleY) * progressScale;//计算最终的缩放向量
			float rotation = Projectile.ai[0] = Main.rand.NextFloat(MathHelper.TwoPi);//随机旋转角度
			Main.EntitySpriteDraw(texture, drawPosition, null, coreColor * alpha * 0.8f,
				rotation, origin, scale * 1f, SpriteEffects.None, 0);
			Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.7f, 0.1f) * alpha);//添加发光效果
			return false;
		}
	}
}