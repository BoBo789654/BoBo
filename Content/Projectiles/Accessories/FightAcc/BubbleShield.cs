using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Accessories.FightAcc
{
	public class BubbleShield : ModProjectile
	{
		private float Alpha = 0.7f;           //透明度(0全透明~1不透明)
		private float AlphaPulseSpeed = 1.5f; //透明度脉动速度
		private float MinAlpha = 0.4f;        //最小透明度阈值
		private float RotationSpeed = 0.015f; //旋转速度
		private float FloatAmplitude = 0.8f;  //浮动幅度
		private float FloatFrequency = 0.03f; //浮动频率
		private float FloatTimer;             //浮动计时器
		public override string Texture => Pictures.FightAccProj + Name;
		public override void SetDefaults()
		{
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 60000;
			Projectile.penetrate = -1;
			Projectile.scale = 3f;
			Projectile.Opacity = Alpha;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 4000;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.Kill();
		}
		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			//玩家死亡或不存在时销毁弹幕
			if (!owner.active || owner.dead || owner.ownedProjectileCounts[ModContent.ProjectileType<BubbleShield>()] > 1)
			{
				Projectile.Kill();
				return;
			}
			//泡泡始终跟随玩家
			Projectile.Center = owner.Center;
			//浮动
			FloatTimer += FloatFrequency;
			//简谐运动公式y=A*sin(ωt)
			float VerticalOffset = FloatAmplitude * MathF.Sin(FloatTimer);
			Projectile.position.Y += VerticalOffset;
			//旋转
			Projectile.rotation -= RotationSpeed;
			if (Projectile.rotation > MathHelper.TwoPi)
				Projectile.rotation -= MathHelper.TwoPi;
			//正弦式透明度
			float Pulse = 0.5f * (1 + MathF.Sin(Main.GlobalTimeWrappedHourly * AlphaPulseSpeed));
			Projectile.Opacity = MinAlpha + (Alpha - MinAlpha) * Pulse;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 DrawPosition = Projectile.Center - Main.screenPosition;
			Rectangle SourceRect = texture.Bounds;
			Vector2 origin = SourceRect.Size() / 2f;
			Color BubbleColor = new Color(180, 230, 255) * Projectile.Opacity;//动态透明度
			float LayerDepth = 0.1f;
			Color HighlightColor = new Color(220, 250, 255, (int)(150 * Projectile.Opacity));
			Main.EntitySpriteDraw(texture, DrawPosition, SourceRect, BubbleColor, Projectile.rotation,
				origin, Projectile.scale * 0.53f, SpriteEffects.None, LayerDepth + 0.001f);
			Main.EntitySpriteDraw(texture, DrawPosition + new Vector2(0, -2), SourceRect, HighlightColor, Projectile.rotation,
				origin, Projectile.scale * 0.55f, SpriteEffects.None, LayerDepth + 0.002f);
			return false;
		}
	}
}