using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Accessories
{
	public class HolyGuardianProj : ModProjectile
	{
		public override string Texture => Pictures.OtherAccProj + Name;
		private const float SemiMajorAxis = 160f;	//半长轴
		private const float SemiMinorAxis = 120f;	//半短轴
		private const float Gravity = 0.25f;		//重力
		private const int HealAmount = 25;          //治疗量
		private bool hasTriggeredHeal = false;      //是否已经触发过治疗效果
		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 30;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 360;
			Projectile.penetrate = 1;
			Projectile.light = 0.5f;
		}
		public override void AI()
		{
			Projectile.velocity.Y += Gravity;//应用重力
			Projectile.rotation += 0.08f;//减缓旋转速度
			Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.8f, 0.1f));//添加发光效果
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			TriggerHolyHealEffect();
			return true;
		}
		public override void Kill(int timeLeft)
		{
			if (!hasTriggeredHeal)//只在弹幕自然死亡且尚未治疗时触发
				TriggerHolyHealEffect();
			SoundEngine.PlaySound(SoundID.Item68, Projectile.position);
			TriggerHolyHealEffect();
		}
		private void TriggerHolyHealEffect()
		{
			if (hasTriggeredHeal) return;//防止重复治疗
			hasTriggeredHeal = true;
			Vector2 center = Projectile.Center;
			for (int i = 0; i < Main.maxPlayers; i++)//治疗范围内的玩家
			{
				Player player = Main.player[i];
				if (player.active && !player.dead && IsInEllipseArea(player.Center, center))
				{
					int healAmount = Math.Min(HealAmount, player.statLifeMax2 - player.statLife);
					if (healAmount > 0)
					{
						player.statLife += healAmount;
						player.HealEffect(healAmount, true);
						if (Main.netMode == NetmodeID.MultiplayerClient)
							NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, i, healAmount);
					}
				}
			}
			for (int i = 0; i < Main.maxNPCs; i++)//治疗范围内的友好NPC
			{
				NPC npc = Main.npc[i];
				if (npc.active && npc.friendly && !npc.immortal && IsInEllipseArea(npc.Center, center))
				{
					int healAmount = Math.Min(HealAmount, npc.lifeMax - npc.life);
					if (healAmount > 0)
					{
						npc.life += healAmount;
						CombatText.NewText(npc.getRect(), Color.LightSeaGreen, healAmount);
					}
				}
			}
			CreateExplosionEffect(center);//创建爆炸视觉效果
		}
		private bool IsInEllipseArea(Vector2 point, Vector2 center)//判断点是否在椭圆范围内，椭圆方程: (dx/a)^2 + (dy/b)^2 <= 1
		{
			float dx = point.X - center.X;
			float dy = point.Y - center.Y;
			float aSquared = SemiMajorAxis * SemiMajorAxis;
			float bSquared = SemiMinorAxis * SemiMinorAxis;
			return (dx * dx) / aSquared + (dy * dy) / bSquared <= 1f;
		}
		private void CreateExplosionEffect(Vector2 center)//创建爆炸视觉效果
		{
			//爆炸特效弹幕
			Projectile.NewProjectile(Projectile.GetSource_Death(), center, Vector2.Zero,
				ModContent.ProjectileType<HolyGuardianProj2>(), 0, 0, Projectile.owner);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)//检测平台碰撞的手段
		{
			//检测当前位置下方的方块类型
			int x = (int)(Projectile.position.X / 16f);
			int y = (int)((Projectile.position.Y + Projectile.height) / 16f);
			if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY)
			{
				Tile tile = Main.tile[x, y];
				if (tile != null && tile.HasTile)
				{
					//检查是否是平台
					if (Main.tileSolidTop[tile.TileType])
					{
						fallThrough = false;//是平台，不让弹幕穿过
					}
					else if (Main.tileSolid[tile.TileType])
					{
						fallThrough = false;//是实心方块，不让弹幕穿过
					}
				}
			}
			width = 10;//缩小碰撞箱，更容易触发碰撞
			height = 10;
			return true;
		}
	}
}