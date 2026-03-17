using BoBo.Content.NPCs.EvilWorm;
using BoBo.Content.UIs.Dialogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Consumables
{
	public class EvilBait : ModItem
	{
		public override string Texture => Pictures.Consumables + Name;

		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;//研究解锁所需数量
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;//标记为Boss召唤物，优化库存排序
			NPCID.Sets.MPAllowedEnemies[ModContent.NPCType<EvilWormHead>()] = true;//为蠕虫BOSS添加多人模式支持
		}

		public override void SetDefaults()
		{
			Item.width = 32;//物品宽度
			Item.height = 24;//物品高度
			Item.maxStack = 9999;//最大堆叠数量
			Item.value = 5000;//出售价值
			Item.rare = ItemRarityID.Lime;//稀有度
			Item.useAnimation = 30;//使用动画时长
			Item.useTime = 30;//使用间隔
			Item.useStyle = ItemUseStyleID.HoldUp;//使用动作
			Item.consumable = true;//消耗型物品
		}

		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
		{
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossSpawners;//归入Boss召唤物分类
		}

		public override bool CanUseItem(Player player)
		{
			UISystem.DialogueInstance.StartDialogue(3);
			return !NPC.AnyNPCs(ModContent.NPCType<EvilWormHead>());//检查是否已经有蠕虫BOSS存在
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)//仅客户端执行
			{
				SoundEngine.PlaySound(SoundID.Roar, player.position);//播放咆哮音效
				int type = ModContent.NPCType<EvilWormHead>();//生成蠕虫的头部
				if (Main.netMode != NetmodeID.MultiplayerClient)//单人模式直接生成
				{
					Vector2 spawnPos = FindSuitableSpawnPosition(player);
					int npcIndex = NPC.NewNPC(Item.GetSource_FromThis(), (int)spawnPos.X, (int)spawnPos.Y, type);
					if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
					{
						Main.npc[npcIndex].target = player.whoAmI;//设置蠕虫的目标为玩家
						if (Main.netMode == NetmodeID.SinglePlayer)//显示生成位置信息
						{
							Vector2 relativePos = spawnPos - player.Center;
							string direction = relativePos.X < 0 ? "左侧" : "右侧";
							Main.NewText($"邪恶蠕虫从玩家{direction}生成，距离: {relativePos.Length():F1}像素", Color.LightGreen);
						}
					}
				}
				else//多人模式发送生成请求
					NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
			}

			return true;//消耗物品
		}

		private Vector2 FindSuitableSpawnPosition(Player player)
		{
			
			int maxAttempts = 40;//尝试次数
			int leftCount = 0, rightCount = 0, upCount = 0, downCount = 0;//方向统计
			for (int attempt = 0; attempt < maxAttempts; attempt++)//使用均匀的角度分布生成方向向量
			{
				float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);//0-2π间随机角度
				float distance = Main.rand.NextFloat(300f, 500f);//生成距离稍远一些
				Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
				//方向统计
				if (direction.X < 0) leftCount++;
				else if (direction.X > 0) rightCount++;
				if (direction.Y < 0) upCount++;
				else if (direction.Y > 0) downCount++;
				Vector2 candidatePos = player.Center + direction * distance;//计算候选位置
				candidatePos.X = MathHelper.Clamp(candidatePos.X, 200f, Main.maxTilesX * 16f - 200f);//确保位置在世界边界内
				candidatePos.Y = MathHelper.Clamp(candidatePos.Y, 200f, Main.maxTilesY * 16f - 200f);//确保位置在世界边界内
				
				Rectangle screenArea = new Rectangle((int)Main.screenPosition.X - 100, (int)Main.screenPosition.Y - 100,
					Main.screenWidth + 200, Main.screenHeight + 200);//避免生成在屏幕内
				if (screenArea.Contains((int)candidatePos.X, (int)candidatePos.Y))//如果在屏幕内，调整到屏幕外
					candidatePos = player.Center + direction * (distance + 300f);
				if (Main.rand.NextBool(4))//1/4的概率在地下生成
				{
					candidatePos.Y = player.Center.Y + Main.rand.NextFloat(200f, 400f);//保持X轴位置，Y轴移到地下
					candidatePos.X = MathHelper.Clamp(candidatePos.X, 300f, Main.maxTilesX * 16f - 300f);//确保在地下生成时，X轴位置不超出世界边界
				}
				Vector2? spawnPos = FindValidSpawnFromPosition(candidatePos, player, attempt);//尝试查找有效生成位置
				if (spawnPos.HasValue)
				{
					if (Main.netMode == NetmodeID.SinglePlayer && Main.rand.NextBool(10))
					{
						Vector2 relativePos = spawnPos.Value - player.Center;
						string dir = relativePos.X < 0 ? "左侧" : "右侧";
						string vertical = relativePos.Y < 0 ? "上方" : "下方";
						Main.NewText($"生成位置: 玩家{dir}{vertical}，水平距离: {Math.Abs(relativePos.X):F0}像素", Color.LightBlue);
					}
					return spawnPos.Value;
				}
			}
			if (Main.netMode == NetmodeID.SinglePlayer)//输出方向统计
				Main.NewText($"方向统计: 左{leftCount} 右{rightCount} 上{upCount} 下{downCount}", Color.Yellow);
			Main.NewText("使用备用生成位置", Color.Orange);//如果所有尝试都失败，使用备用位置
			return GetFallbackSpawnPosition(player);
		}
		private Vector2? FindValidSpawnFromPosition(Vector2 startPos, Player player, int attempt)
		{
			int startX = (int)(startPos.X / 16f);
			int startY = (int)(startPos.Y / 16f);
			//搜索半径和深度
			int searchRadius = 25;
			int maxVerticalSearch = 40;
			//根据尝试次数调整搜索策略，增加多样性
			int spiralRadius = (attempt % 5) + 1; //1-5的螺旋半径
			//使用螺旋搜索算法，从中心向外搜索
			for (int r = 0; r <= searchRadius; r += spiralRadius)
			{
				for (int angleStep = 0; angleStep < 360; angleStep += 30)
				{
					double rad = angleStep * Math.PI / 180.0;
					int checkX = startX + (int)(r * Math.Cos(rad));
					int checkY = startY + (int)(r * Math.Sin(rad));
					if (checkX < 0 || checkX >= Main.maxTilesX || checkY < 0 || checkY >= Main.maxTilesY)//确保坐标有效
						continue;
					if (CheckTileForSpawn(checkX, checkY, player))//检查当前位置
					{
						Vector2 spawnPos = new Vector2(checkX * 16f + 8f, (checkY - 2) * 16f);
						Tile tile = Main.tile[checkX, checkY];//确保生成点不在墙内
						if (tile.WallType == 0 || !Main.wallHouse[tile.WallType])
							return spawnPos;
					}
					for (int dy = 1; dy <= 5; dy++)//向下搜索几格
					{
						int downY = checkY + dy;
						if (downY >= Main.maxTilesY) break;
						if (CheckTileForSpawn(checkX, downY, player))
						{
							Vector2 spawnPos = new Vector2(checkX * 16f + 8f, (downY - 2) * 16f);
							Tile tile = Main.tile[checkX, downY];
							if (tile.WallType == 0 || !Main.wallHouse[tile.WallType])
								return spawnPos;
						}
					}
				}
			}
			return null;//未找到合适位置
		}
		private bool CheckTileForSpawn(int tileX, int tileY, Player player)
		{
			if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)//检查当前方块是否有效
				return false;
			Tile tile = Main.tile[tileX, tileY];
			bool isSolidOrPlatform = tile.HasTile && (Main.tileSolid[tile.TileType] ||
				 TileID.Sets.Platforms[tile.TileType] ||  Main.tileSolidTop[tile.TileType]);//检查是否是固体方块或平台 
			if (!isSolidOrPlatform)
				return false; 
			//检查上方是否有足够空间
			int spaceNeeded = 6;
			bool hasEnoughSpace = true;
			for (int i = 1; i <= spaceNeeded; i++)
			{
				int spaceY = tileY - i;
				if (spaceY < 0)
				{
					hasEnoughSpace = false;
					break;
				}
				Tile spaceTile = Main.tile[tileX, spaceY];
				if (spaceTile.HasTile && Main.tileSolid[spaceTile.TileType])
				{
					hasEnoughSpace = false;
					break;
				}
			}
			if (!hasEnoughSpace)
				return false;
			Vector2 tilePos = new Vector2(tileX * 16f, tileY * 16f);//检查是否在玩家过近的位置
			if (Vector2.Distance(tilePos, player.Center) < 200f)
				return false;
			Rectangle screenArea = new Rectangle((int)Main.screenPosition.X - 50, (int)Main.screenPosition.Y - 50,
				Main.screenWidth + 100, Main.screenHeight + 100);//检查是否在屏幕内
			if (screenArea.Contains((int)tilePos.X, (int)tilePos.Y))
				return false;
			return true;
		}
		private Vector2 GetFallbackSpawnPosition(Player player)
		{
			float direction = Main.rand.NextBool() ? -1f : 1f; //随机选择左右
			float distance = 400f; //固定距离
			Vector2 spawnPos = player.Center + new Vector2(direction * distance, -100f);
			spawnPos.X = MathHelper.Clamp(spawnPos.X, 200f, Main.maxTilesX * 16f - 200f);//确保在世界边界内
			spawnPos.Y = MathHelper.Clamp(spawnPos.Y, 200f, Main.maxTilesY * 16f - 200f);//确保在世界边界内
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				string side = direction < 0 ? "左侧" : "右侧";
				Main.NewText($"使用备用位置: 玩家{side}，距离: {distance}像素", Color.Orange);
			}

			return spawnPos;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe(); //创建一个配方
			recipe.AddIngredient(ItemID.RottenChunk, 5); //腐肉
			recipe.AddIngredient(ItemID.ShadowScale, 10); //暗影鳞片
			recipe.AddIngredient(ItemID.SoulBottleSight, 10); //视域之魂
			recipe.AddTile(TileID.DemonAltar); //制作站：恶魔祭坛
			recipe.Register();
		}
	}
}