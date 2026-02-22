using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Weapons.Melee
{
	public class XianJian : ModProjectile
	{
		private int TextureType;//贴图类型
		private Vector2 StartPos;//起始位置
		private bool IsMoving;//运动状态
		private const int ParticleCount = 8;//粒子数量
		private readonly Color[] SwordColors =
		[
			new Color(0, 150, 255),//蓝
			new Color(50, 255, 100),//绿
			new Color(255, 50, 50),//红
			new Color(255, 255, 0)//黄
		];
		private NPC TargetNPC => Main.npc[(int)Projectile.ai[0]];//0控目标
		private float Alpha//1控透明度
		{
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}
		private float Angle => Projectile.ai[2];//2控角度
		public override string Texture => $"BoBo/Asset/Projectiles/Weapons/Melee/XianJian{TextureType}";
		private static readonly int[] DustTypes = [15, 61, 90, 204];
		public override void SetDefaults()
		{
			Projectile.width = Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 140;
			Projectile.aiStyle = -1;
			Projectile.penetrate = 1;
		}
		private Vector2 CalculateStartPosition()
		{
			const float Distance = 400f;
			Vector2 basePos = TargetNPC.Center;
			return basePos + new Vector2(Distance * MathF.Cos(Angle), -Distance * MathF.Sin(Angle));
		}
		public override void OnSpawn(IEntitySource source)
		{
			TextureType = Main.rand.Next(0, 4);
			Alpha = 0f;
			StartPos = CalculateStartPosition();
			Projectile.rotation = (TargetNPC.Center - StartPos).ToRotation() + MathHelper.PiOver4;
		}
		public override void AI()
		{
			if (!TargetNPC.active || TargetNPC.life <= 0)//目标失效
			{
				Projectile.Kill();
				return;
			}
			Color swordColor = SwordColors[TextureType];
			if (Projectile.timeLeft > 100)//渐显悬停
			{
				Projectile.Center = StartPos;
				Projectile.rotation = (TargetNPC.Center - StartPos).ToRotation() + MathHelper.PiOver4;
				Alpha = MathHelper.Clamp(Alpha + 0.025f, 0f, 1f);
				if (Main.rand.NextBool(4))
				{
					int DustType = DustTypes[TextureType];
					Dust Dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustType);
					Dust.color = swordColor;
					Dust.noGravity = true;
					Dust.scale = 1.1f;
					Dust.alpha = (int)(200 * (1 - Alpha));
				}
			}
			else if (Projectile.timeLeft > 60)//穿刺运动
			{
				if (!IsMoving)
				{
					IsMoving = true;
					StartPos = Projectile.Center;
				}
				float progress = 1 - (Projectile.timeLeft - 60) / 40f;
				float easedProgress = progress < 0.5f ? 2 * progress * progress : 1 - MathF.Pow(-2 * progress + 2, 2) / 2;
				Projectile.Center = Vector2.Lerp(StartPos, TargetNPC.Center, easedProgress);
				Vector2 dir = (TargetNPC.Center - StartPos).SafeNormalize(Vector2.UnitY);
				Projectile.rotation = dir.ToRotation() + MathHelper.PiOver4;
				if (Main.rand.NextBool(3))
				{
					int DustType = DustTypes[TextureType];
					Dust Dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustType);
					Dust.color = swordColor;
					Dust.noGravity = true;
					Dust.scale = 1.3f;
					Dust.velocity = dir * -2f;
				}
			}
			else//渐隐消失
			{
				Projectile.Center = TargetNPC.Center;
				Alpha = MathHelper.Clamp(Alpha - 0.0167f, 0f, 1f);
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			string texturePath = $"BoBo/Asset/Projectiles/Weapons/Melee/XianJian{TextureType}";
			Texture2D texture = ModContent.Request<Texture2D>(texturePath).Value;
			Vector2 origin = new(texture.Width / 2, texture.Height / 2);
			Vector2 DrawPos = Projectile.Center - Main.screenPosition;
			Main.EntitySpriteDraw(texture, DrawPos, null, new Color(255, 255, 255, 0) * Alpha,
				Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target != TargetNPC) return;
			Projectile.velocity = Vector2.Zero;
			Projectile.damage = 0;
			Projectile.timeLeft = 60;
			if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(4))
			{
				CreateParticles(target);
			}
		}
		private void CreateParticles(NPC target)
		{
			Vector2 swordDirection = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
			Color swordColor = SwordColors[TextureType];
			const int segmentCount = 15;//刀光线段数
			const float totalLength = 80f;//刀光总长度
			float segmentLength = totalLength / segmentCount;
			float speedMultiplier = Main.rand.NextFloat(1.8f, 3f);//动态速度系数
			for (int i = 0; i < segmentCount; i++)
			{
				Vector2 basePos = Projectile.Center + swordDirection * (i * segmentLength);//沿剑方向线性分布粒子
				Vector2 randomOffset = Main.rand.NextVector2Circular(3f, 3f);//随机性
				Vector2 position = basePos + randomOffset;
				float sizeProgress = i / (float)segmentCount;//渐变
				float scale = MathHelper.Lerp(1.8f, 0.6f, sizeProgress);//大->小
				float alpha = MathHelper.Lerp(0.3f, 0.9f, sizeProgress); //多->少
				Dust dust = Dust.NewDustPerfect(position, DustTypes[TextureType], Velocity: swordDirection.RotatedByRandom(0.3f) *
							 Main.rand.NextFloat(3f, 8f) * speedMultiplier, Alpha: (int)(255 * alpha),
					newColor: swordColor, Scale: scale);
				dust.noGravity = true;
				dust.fadeIn = 1.2f;//淡入效果增强动态感
				//剑尖追加高亮粒子
				if (sizeProgress > 0.8f && Main.rand.NextBool(4))
				{
					Dust.NewDustPerfect(position, DustTypes[TextureType], Velocity: swordDirection * 12f + Main.rand.NextVector2Circular(4f, 4f),
						newColor: Color.Lerp(swordColor, Color.White, 0.7f), Scale: 1.5f).noGravity = true;
				}
			}
			for (int i = 0; i < 5; i++)
			{
				Vector2 trailPos = Projectile.Center - swordDirection * (i * 8f);
				Dust.NewDustPerfect(trailPos, DustTypes[TextureType], Velocity: -swordDirection * 4f + Main.rand.NextVector2Circular(3f, 3f),
					newColor: Color.Lerp(swordColor, Color.Cyan, 0.5f), Scale: Main.rand.NextFloat(1.2f, 2f)).noGravity = true;
			}
		}
		public static void SummonSwords(Player player, NPC target)
		{
			float[] angles = [MathHelper.ToRadians(-35f), MathHelper.ToRadians(-15f), MathHelper.ToRadians(15f), MathHelper.ToRadians(35f)];
			for (int i = 0; i < 4; i++)
			{
				Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem), target.Center, Vector2.Zero,
					ModContent.ProjectileType<XianJian>(), player.GetWeaponDamage(player.HeldItem),
					0, player.whoAmI, target.whoAmI, ai2: angles[i]);
			}
		}
		#region 联机同步
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(TextureType);
			writer.WriteVector2(StartPos);
			writer.Write(IsMoving);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			TextureType = reader.ReadInt32();
			StartPos = reader.ReadVector2();
			IsMoving = reader.ReadBoolean();
		}
		#endregion
	}
}

/*心形粒子
private void CreateParticles(NPC target)//撞上之后的粒子
		{
			static Vector2 CalculateHeartPoint(float t, float size)
			{
				float x = 30 * MathF.Pow(MathF.Sin(t), 3);
				float y = -(25 * MathF.Cos(t) - 8 * MathF.Cos(2 * t) - 3 * MathF.Cos(3 * t) - MathF.Cos(4 * t));
				return new Vector2(x, y) * size;
			}
			Color color = SwordColors[TextureType];
			float baseSize = Main.rand.NextBool(4) ? 0.8f : 0.4f;//尺寸放大
			int scaledParticleCount = (int)(ParticleCount * 2.0f);//增加粒子密度填充放大后的爱心轮廓
			for (int i = 0; i < scaledParticleCount; i++)
			{
				float t = MathHelper.TwoPi * i / scaledParticleCount;
				Vector2 heartPoint = CalculateHeartPoint(t, baseSize);
				//扩大随机偏移范围
				Vector2 randomOffset = Main.rand.NextVector2Circular(6f, 6f);
				Vector2 position = Projectile.Center + heartPoint + randomOffset;
				//特殊爱心使用更亮颜色
				Color particleColor = Main.rand.NextBool(4) ? Color.Lerp(color, Color.White, 0.3f) : color;
				int DustType = DustTypes[TextureType];
				Dust Dust = Dust.NewDustPerfect(position, DustType, Vector2.Zero,
					newColor: particleColor, Scale: Main.rand.NextBool(4) ? 2.8f : 2.0f);
				Dust.noGravity = true;//增强粒子运动速度以适应大尺寸
				Dust.velocity = Vector2.UnitY * -Main.rand.NextFloat(2f, 5f) + heartPoint * 0.08f;//加速上浮+扩散
			}
			if (Main.rand.NextBool(4))//特殊爱心中心闪光
			{
				for (int i = 0; i < 8; i++)//增加粒子数量
				{
					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), DustTypes[TextureType], 
						Main.rand.NextVector2Circular(5f, 5f), newColor: Color.Lerp(color, Color.White, 0.7f), Scale: 3.2f).noGravity = true;
				}
				for (int i = 0; i < 12; i++)//爱心轮廓
				{
					float t = MathHelper.TwoPi * i / 12;
					Vector2 EdgePos = Projectile.Center + CalculateHeartPoint(t, baseSize);
					Dust.NewDustPerfect(EdgePos, DustTypes[TextureType], Vector2.Zero, newColor: Color.White, Scale: 3.0f).noGravity = true;
				}
			}
		}*/