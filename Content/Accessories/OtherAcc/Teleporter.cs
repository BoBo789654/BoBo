using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Accessories.OtherAcc
{
	public class Teleporter : ModItem
	{
		public override string Texture => Pictures.OtherAcc + Name;
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.accessory = true;
			Item.rare = ItemRarityID.White;
			Item.value = Item.sellPrice(0, 5, 0, 0);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<TeleporterPlayer>().hasAccessory = true;
		}
	}
	public class TeleporterPlayer : ModPlayer
	{
		public bool hasAccessory;
		private int cooldown = 0;//冷却计时器
		private const int CooldownTime = 180;
		public override void ResetEffects() => hasAccessory = false;
		public override void PostUpdate()
		{
			if (cooldown > 0) cooldown--;//冷却倒计时
		}
		public override void OnHurt(Player.HurtInfo info)
		{
			if (!hasAccessory || cooldown > 0) return;
			RandomTeleport();
			cooldown = CooldownTime;//触发冷却
		}

		private void SpawnEffects(Vector2 position, bool isDeparture)
		{
			//基础紫色烟雾
			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustPerfect(position, DustID.PurpleTorch,
							  Main.rand.NextVector2Circular(3f, 3f), 0, default, 2f);
				dust.noGravity = true;
			}
			//环形扩散
			int dustType = isDeparture ? DustID.PurpleTorch : DustID.BlueFairy;
			for (int i = 0; i < 20; i++)
			{
				Vector2 velocity = new Vector2(0, 3f).RotatedBy(MathHelper.TwoPi * i / 20);
				Dust ringDust = Dust.NewDustPerfect(position, dustType, velocity * 2f, 150, default, 2.5f);
				ringDust.noGravity = true;
			}
			//向上飘散
			for (int i = 0; i < 15; i++)
			{
				Vector2 velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(3f, 6f));
				Dust trailDust = Dust.NewDustPerfect(position, DustID.Electric, velocity, 0, default, 1.5f);
				trailDust.noGravity = true;
				trailDust.fadeIn = 1.2f;
			}
		}
		private void RandomTeleport()
		{
			Vector2 targetPos = FindSafePosition();
			if (targetPos != Vector2.Zero)
			{
				//起点特效
				SoundEngine.PlaySound(SoundID.Item8, Player.Center);
				SpawnEffects(Player.Center, true);
				Player.Teleport(targetPos, TeleportationStyleID.TeleportationPotion);
				//终点特效
				SoundEngine.PlaySound(SoundID.Item78, targetPos);
				SpawnEffects(targetPos, false);
			}
		}
		private Vector2 FindSafePosition()
		{
			for (int attempt = 0; attempt < 100; attempt++)//最多尝试100次
			{
				//随机世界坐标
				int randX = Main.rand.Next(100, Main.maxTilesX * 16 - 100);
				int randY = Main.rand.Next((int)Main.worldSurface * 16, Main.maxTilesY * 16 - 300);
				Vector2 candidate = new Vector2(randX, randY);
				//检测周围是否无方块
				if (IsSafe(candidate))
					return candidate;
			}
			return Vector2.Zero;//未找到安全位置
		}
		//位置检测
		private bool IsSafe(Vector2 position)
		{
			int tileX = (int)(position.X / 16);
			int tileY = (int)(position.Y / 16);
			//检查玩家周围3x3区域（中心+8方向）
			for (int x = tileX - 1; x <= tileX + 1; x++)
			{
				for (int y = tileY - 1; y <= tileY + 1; y++)
				{
					//若存在实心块则不安全
					if (x >= 0 && y >= 0 && x < Main.maxTilesX && y < Main.maxTilesY &&
						Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType])
						return false;
				}
			}
			return true;//区域无实心块
		}
	}
}