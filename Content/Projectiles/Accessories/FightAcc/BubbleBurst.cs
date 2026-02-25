using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
namespace BoBo.Content.Projectiles.Accessories.FightAcc
{
	public class BubbleBurst : ModProjectile
	{
		public override string Texture => Pictures.FightAccProj + Name;
		private const int FrameDelay = 0;							//无延迟（无帧图）
		private int frameCounter = 0;
		private int currentFrame = 0;
		private const float LongAxis = 120f;                        // 长轴长度
		private const float ShortAxis = 40f;                        // 短轴长度
		private const float RotationSpeed = 0.06f;                  // 公转速度
		private float leafAngle1 = 0f;                              // 叶子1在椭圆上的角度
		private float leafAngle2 = MathHelper.PiOver2;              // 叶子2在椭圆上的角度
		private Vector2 leafPosition1;                              // 叶子1的当前位置
		private Vector2 leafPosition2;                              // 叶子2的当前位置
		private float leafScale1 = 1.0f;                            // 叶子1的缩放
		private float leafScale2 = 1.0f;                            // 叶子2的缩放
		private float leafAlpha1 = 1.0f;                            // 叶子1的透明度
		private float leafAlpha2 = 1.0f;                            // 叶子2的透明度
		private Color leafColor1 = Color.LimeGreen;                 // 叶子1的颜色
		private Color leafColor2 = Color.LimeGreen;                 // 叶子2的颜色
		private float ellipseAngle1 = MathHelper.ToRadians(30f);    // 叶子1椭圆角度
		private float ellipseAngle2 = MathHelper.ToRadians(-30f);   // 叶子2椭圆角度
		private float angleTimer = 0f;
		private const float AngleCycleTime = 15f;                   //15秒的轨迹循环
		private bool angleIncreasing = true;
		private Vector2[] leaf1TrailPositions = new Vector2[6];     //叶子1的轨迹点
		private Vector2[] leaf2TrailPositions = new Vector2[6];     //叶子2的轨迹点
		private float trailTimer = 0f;
		private const float TrailUpdateInterval = 0.04f;            //稍微增加更新频率
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 1;					//无帧图
		}
		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 30000;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.hide = false;
			Projectile.aiStyle = -1;
			Projectile.light = 0f;
			Projectile.alpha = 0;
		}
		public override void OnSpawn(IEntitySource source)
		{
			base.OnSpawn(source);
			for (int i = 0; i < leaf1TrailPositions.Length; i++)//初始化两个叶子的轨迹点
			{
				leaf1TrailPositions[i] = Projectile.Center;
				leaf2TrailPositions[i] = Projectile.Center;
			}
			leafAngle1 = MathHelper.ToRadians(Main.rand.Next(360));//随机化初始角度，让两个叶子在不同位置开始
			leafAngle2 = leafAngle1 + MathHelper.ToRadians(90);//相差90度开始
		}
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			if (player.dead || !player.active)
			{
				Projectile.Kill();
				return;
			}
			Projectile.Center = player.Center;
			frameCounter++;
			if (frameCounter >= FrameDelay)
			{
				frameCounter = 0;
				currentFrame = (currentFrame + 1) % Main.projFrames[Projectile.type];
			}
			UpdateEllipseAngles();
			UpdateLeaf1(player);
			UpdateLeaf2(player);
			UpdateTrail();
		}
		private void UpdateEllipseAngles()
		{
			angleTimer += 0.0167f;

			if (angleTimer >= AngleCycleTime)
			{
				angleTimer = 0f;
				angleIncreasing = !angleIncreasing;
			}

			float lerpValue = angleTimer / AngleCycleTime;
			if (!angleIncreasing) lerpValue = 1f - lerpValue;
			//两个椭圆的轨道角度独立变化
			ellipseAngle1 = MathHelper.Lerp(
				MathHelper.ToRadians(15f),
				MathHelper.ToRadians(angleIncreasing ? 15f : 35f),
				lerpValue
			);
			ellipseAngle2 = MathHelper.Lerp(
				MathHelper.ToRadians(-25f),
				MathHelper.ToRadians(angleIncreasing ? -45f : -25f),
				lerpValue
			);
		}
		private void UpdateLeaf1(Player player)// 叶子1在椭圆1上运动
		{
			leafAngle1 += RotationSpeed;
			if (leafAngle1 > MathHelper.TwoPi * 5) leafAngle1 -= MathHelper.TwoPi * 5;
			//计算椭圆上的位置
			float x = LongAxis * 0.5f * (float)Math.Cos(leafAngle1);
			float y = ShortAxis * 0.5f * (float)Math.Sin(leafAngle1);
			Vector2 pos = new Vector2(x, y).RotatedBy(ellipseAngle1);
			leafPosition1 = Projectile.Center + pos;
			UpdateLeafVisuals(ref leafScale1, ref leafAlpha1, ref leafColor1, pos);
		}
		private void UpdateLeaf2(Player player)//叶子在椭圆2上运动
		{
			float leaf2Speed = RotationSpeed * 0.95f;
			leafAngle2 += leaf2Speed;
			if (leafAngle2 > MathHelper.TwoPi * 5) leafAngle2 -= MathHelper.TwoPi * 5;
			//计算椭圆上的位置
			float leaf2LongAxis = LongAxis * 1.05f;
			float leaf2ShortAxis = ShortAxis * 0.95f;
			float x = leaf2LongAxis * 0.5f * (float)Math.Cos(leafAngle2);
			float y = leaf2ShortAxis * 0.5f * (float)Math.Sin(leafAngle2);
			Vector2 pos = new Vector2(x, y).RotatedBy(ellipseAngle2);
			leafPosition2 = Projectile.Center + pos;
			UpdateLeafVisuals(ref leafScale2, ref leafAlpha2, ref leafColor2, pos);
		}
		private void UpdateLeafVisuals(ref float scale, ref float alpha, ref Color color, Vector2 relativePos)//叶子的视觉效果
		{
			float distance = relativePos.Length();
			float depthFactor = MathHelper.Clamp(1f - distance / (LongAxis * 1.5f), 0.4f, 1f);
			bool isBehindPlayer = relativePos.Y < 0;
			scale = isBehindPlayer ?
				MathHelper.Lerp(0.7f, 1.0f, depthFactor) :
				MathHelper.Lerp(1.0f, 1.4f, depthFactor);//缩放
			alpha = isBehindPlayer ?
				MathHelper.Lerp(0.7f, 1.0f, depthFactor) :
				MathHelper.Lerp(1.0f, 1.4f, depthFactor);//透明度
			float greenHue = 0.2f + depthFactor * 0.3f;
			float lightness = isBehindPlayer ?
				MathHelper.Lerp(0.6f, 0.8f, depthFactor) :
				MathHelper.Lerp(1.0f, 1.4f, depthFactor);
			color = ColorFromHSL(greenHue, 0.9f, lightness);//颜色
		}
		private void UpdateTrail()//更新轨迹点
		{
			trailTimer += 0.0167f;
			if (trailTimer >= TrailUpdateInterval)
			{
				trailTimer = 0f;
				for (int i = leaf1TrailPositions.Length - 1; i > 0; i--)//叶子1的轨迹点
					leaf1TrailPositions[i] = Vector2.Lerp(leaf1TrailPositions[i], leaf1TrailPositions[i - 1], 0.4f);
				leaf1TrailPositions[0] = leafPosition1;
				for (int i = leaf2TrailPositions.Length - 1; i > 0; i--)//叶子2的轨迹点
					leaf2TrailPositions[i] = Vector2.Lerp(leaf2TrailPositions[i], leaf2TrailPositions[i - 1], 0.4f);
				leaf2TrailPositions[0] = leafPosition2;
			}
		}
		public override bool PreDraw(ref Color lightColor)//绘制
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle sourceRect = new Rectangle(0, currentFrame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = sourceRect.Size() * 0.5f;
			DrawLeaf(texture, sourceRect, origin, leafPosition1, leafAngle1 + ellipseAngle1, leafScale1, leafColor1, leafAlpha1);
			DrawLeaf(texture, sourceRect, origin, leafPosition2, leafAngle2 + ellipseAngle2, leafScale2, leafColor2, leafAlpha2);
			return false;
		}

		private void DrawLeaf(Texture2D texture, Rectangle sourceRect, Vector2 origin, Vector2 position, float rotation, float scale, Color color, float alpha)//绘制叶子
		{
			Vector2 drawPos = position - Main.screenPosition;
			Color drawColor = color * alpha;
			Main.EntitySpriteDraw(texture, drawPos, sourceRect, color * alpha * 0.8f,
				rotation + MathHelper.PiOver2, origin, scale * 1.2f, SpriteEffects.None, 0);//发光
			Main.EntitySpriteDraw(texture, drawPos, sourceRect, drawColor,
				rotation + MathHelper.PiOver2, origin, scale, SpriteEffects.None, 0);//叶子本体
		}
		private Color ColorFromHSL(float h, float s, float l)//HSL颜色转换辅助函数
		{
			float r, g, b;
			if (s == 0f) return new Color(l, l, l);

			float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
			float p = 2f * l - q;
			r = HueToRGB(p, q, h + 1f / 3f);
			g = HueToRGB(p, q, h);
			b = HueToRGB(p, q, h - 1f / 3f);

			return new Color(r, g, b);
		}
		private float HueToRGB(float p, float q, float t)
		{
			if (t < 0f) t += 1f;
			if (t > 1f) t -= 1f;
			if (t < 1f / 6f) return p + (q - p) * 6f * t;
			if (t < 1f / 2f) return q;
			if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
			return p;
		}
		public override bool? CanDamage() => false;
		public override bool ShouldUpdatePosition() => false;
	}
}