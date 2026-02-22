using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Weapons.Magic
{
	public class DarkStarProj : ModProjectile
	{
		public override string Texture => Pictures.MagicProj + Name;
		private const float MaxSpeed = 18f;       //弹幕最大移动速度
		private bool AttackTriggered = false;     //攻击是否已触发

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 6;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60000;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			if (Main.player[Projectile.owner].channel)
			{
				//获取弹幕持有者
				Player player = Main.player[Projectile.owner];
				//从玩家到达鼠标位置的单位向量
				Vector2 unit = Vector2.Normalize(Main.MouseWorld - player.Center);
				//随机角度
				float rotaion = unit.ToRotation();

				float distance = Vector2.Distance(Projectile.Center, Main.MouseWorld);
				
				//调整玩家转向以及手持物品的转动方向
				player.direction = Main.MouseWorld.X < player.Center.X ? -1 : 1;
				player.itemRotation = (float)Math.Atan2(rotaion.ToRotationVector2().Y * player.direction,
					rotaion.ToRotationVector2().X * player.direction);
				//玩家保持物品使用动画
				player.itemTime = 2;
				player.itemAnimation = 2;
				//从弹幕到达鼠标位置的单位向量
				Vector2 unit2 = Vector2.Normalize(Main.MouseWorld - Projectile.Center);
				//让弹幕缓慢朝鼠标方向移动
				Projectile.velocity = unit2 * distance / 16;
			}
			else
			{
				//如果玩家放弃吟唱就慢慢消失
				if (Projectile.timeLeft > 30)
					Projectile.timeLeft = 30;
				//返回函数这样就不会执行下面的攻击代码
				return;
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D trailTexture = ModContent.Request<Texture2D>(Pictures.MagicProj + Name + "1").Value;
			Texture2D mainTexture = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 drawOrigin = new Vector2(mainTexture.Width / 2, mainTexture.Height / 2);

			// ===== 动态形变参数计算 =====
			float velocityRatio = Math.Clamp(Projectile.velocity.Length() / MaxSpeed, 0f, 1f);

			// 极端形变参数（符合烟雾贴图特性）
			float stretchX = velocityRatio > 0.01f
				? MathHelper.Lerp(0f, 4f, velocityRatio * velocityRatio)
				: 0.2f;  // 静止时轻微收缩

			float squashY = MathHelper.Lerp(
				1.5f,   // 低速时纵向膨胀
				0.3f,   // 高速时剧烈压缩
				velocityRatio
			);

			// ===== 烟雾拖尾绘制 =====
			// 计算拖尾位置（弹幕后方）
			Vector2 trailPosition = Projectile.Center - Main.screenPosition -
								   Vector2.Normalize(Projectile.velocity) * Projectile.width * 0.5f;

			// 动态调整烟雾拖尾尺寸
			Vector2 smokeScale = new Vector2(
				stretchX * Projectile.scale,
				squashY * Projectile.scale
			);

			// 计算动态透明度（速度越快越透明）
			float smokeAlpha = MathHelper.Lerp(0.7f, 0.3f, velocityRatio);

			// 烟雾色调（与速度相关）
			Color smokeColor = new Color(0.8f, 0.8f, 1f, smokeAlpha); // 蓝白色调烟雾

			// 主拖尾层（使用烟雾贴图）
			Main.EntitySpriteDraw(
				trailTexture,
				trailPosition,
				null,
				smokeColor,
				Projectile.velocity.ToRotation(),
				new Vector2(trailTexture.Width * 0.5f, trailTexture.Height * 0.5f), // 烟雾中心点
				smokeScale,
				SpriteEffects.None,
				0
			);

			// 高亮核心区（增强烟雾层次感）
			if (velocityRatio > 0.1f)
			{
				Color coreColor = new Color(1f, 1f, 1f, smokeAlpha * 0.7f);
				Main.EntitySpriteDraw(
					trailTexture,
					trailPosition,
					null,
					coreColor,
					Projectile.velocity.ToRotation(),
					new Vector2(trailTexture.Width * 0.5f, trailTexture.Height * 0.5f),
					smokeScale * 0.5f, // 核心区更小
					SpriteEffects.None,
					0
				);
			}

			// ===== 弹幕本体绘制 =====
			Color drawColor = AttackTriggered ? Color.Gold : Color.White;
			Main.EntitySpriteDraw(
				mainTexture,
				Projectile.Center - Main.screenPosition,
				null,
				drawColor,
				Projectile.rotation,
				drawOrigin,
				Projectile.scale,
				SpriteEffects.None,
				0
			);

			return false;
		}
	}
}