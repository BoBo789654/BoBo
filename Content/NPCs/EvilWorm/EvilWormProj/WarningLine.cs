using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
namespace BoBo.Content.NPCs.EvilWorm.EvilWormProj
{
	#region 预警线
	public class WarningLine : ModProjectile//激光预警线：蠕虫激光
	{
		public override string Texture => "Terraria/Images/Item_0";
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 4000;//这一项代表弹幕超过屏幕外多少距离以内可以绘制，激光弹幕建议4000左右
			base.SetStaticDefaults();
		}
		public override void SetDefaults()
		{
			Projectile.tileCollide = false; Projectile.ignoreWater = true;//穿墙并且不受水影响
			Projectile.width = Projectile.height = 4;//装饰性弹幕宽高随便写个小一点的数
			Projectile.timeLeft = 90;//预警线持续40帧，是BOSS完成蓄力的时间
		}
		public override bool ShouldUpdatePosition()
		{
			return false;//禁止速度影响弹幕位置
		}
		public override void AI()
		{
			//这个弹幕没有行为
		}
		public override void OnKill(int timeLeft)//弹幕死亡时生成一个激光
		{
			SoundEngine.PlaySound(SoundID.Zombie104, Projectile.Center);
			float laserRotation = Projectile.ai[1];
			int laser = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, laserRotation.ToRotationVector2(), 
				ModContent.ProjectileType<WormRay>(), 25, 1, Main.myPlayer);//创建正向激光
			int laser2 = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, (laserRotation + MathHelper.Pi).ToRotationVector2(), 
				ModContent.ProjectileType<WormRay>(), 25, 1, Main.myPlayer);//创建反向激光
			if (Main.projectile[laser].active) Main.projectile[laser].timeLeft = 90;
			if (Main.projectile[laser2].active) Main.projectile[laser2].timeLeft = 90;

			base.OnKill(timeLeft);
		}
		public override bool PreDraw(ref Color lightColor)//predraw返回false即可禁用原版绘制
		{
			float factor = 1 + (float)MathF.Sin(Projectile.timeLeft / 3f);//制作一个0-2波浪摆动的正弦函数
			factor /= 2f;//除以二，也就是归一化
			Color color = Color.Lerp(Color.Green, Color.Yellow, factor);
			//lerp是线性插值，可以根据第三个空代表的比率(0-1)，选择第一个和第二个参数之间指定比率的中间值
			var tex = TextureAssets.MagicPixel.Value;//这个东西是一个白色像素
			Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 2, 2),
				color, Projectile.velocity.ToRotation()//以速度方向代表预警线方向
				, Vector2.Zero, new Vector2(1500, 1)//X轴拉长1500倍
				, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 2, 2),
				color, Projectile.velocity.ToRotation() + MathHelper.Pi,//反向
				Vector2.Zero, new Vector2(1500, 1), SpriteEffects.None, 0);
			return false;
		}
	}
	#endregion
	public class WormRay : ModProjectile//蠕虫激光弹幕
	{
		public override string Texture => Pictures.EvilWormProj + Name;
		float LaserLength = 0; 

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 4000;
			base.SetStaticDefaults();
		}
		public override void SetDefaults()
		{
			LaserLength = 6000;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 90;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = true;
			base.SetDefaults();
		}
		public override bool ShouldUpdatePosition()
		{
			return false;//位置不随速度变化
		}
		public override void AI()
		{
			//激光伸缩动画
			if (Projectile.localAI[0] < 15 && Projectile.timeLeft > 16)
				Projectile.localAI[0]++;
			if (Projectile.timeLeft < 16)
				Projectile.localAI[0]--;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (Projectile.localAI[0] < 15) return false;//激光未完全伸展时不造成伤害
			Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
			float collisionPoint = 0f;
			//正向激光碰撞检测
			Vector2 forwardStart = Projectile.Center;
			Vector2 forwardEnd = Projectile.Center + direction * LaserLength;
			bool forwardCollision = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), 
				targetHitbox.Size(), forwardStart, forwardEnd, 10, ref collisionPoint);
			if (forwardCollision) return true;
			//反向激光碰撞检测
			Vector2 backwardStart = Projectile.Center;
			Vector2 backwardEnd = Projectile.Center - direction * LaserLength;
			bool backwardCollision = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(),
				targetHitbox.Size(), backwardStart, backwardEnd, 10, ref collisionPoint);
			if (backwardCollision) return true;
			return base.Colliding(projHitbox, targetHitbox);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Color laserColor = new Color(100, 255, 100, 0);
			Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, laserColor, Projectile.velocity.ToRotation(), 
				new Vector2(texture.Width / 2, texture.Height / 2),
				new Vector2(LaserLength / texture.Width, Projectile.localAI[0] / 30f / 10), SpriteEffects.None, 0);//绘制正向激光
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, laserColor, Projectile.velocity.ToRotation(),
				new Vector2(texture.Width / 2, texture.Height / 2),
				new Vector2(LaserLength / texture.Width, Projectile.localAI[0] / 30f / 10), SpriteEffects.None, 0);//绘制反向激光
			return false;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			target.AddBuff(BuffID.Poisoned, 180);//3秒中毒
			target.AddBuff(BuffID.Venom, 120);//2秒剧毒
			SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.Center);//播放击中音效
		}
	}
}