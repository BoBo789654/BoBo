using BoBo.Content.Items.Consumables;
using BoBo.Content.Items.Fishes;
using BoBo.Content.Items.Tools;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
/*
namespace BoBo.Content.Players
{
	/// <summary>
	/// 此类展示了钓鱼相关的扩展功能实现
	/// </summary>
	public class BoBoFishingPlayer : ModPlayer//Example里有很多花活
	{
		//标记玩家是否拥有"示例板条箱"增益
		public bool hasCrateBuff;
		//每帧重置效果状态
		public override void ResetEffects()
		{
			hasCrateBuff = false;
		}
		/// <summary>
		/// 修改钓鱼尝试的相关参数
		/// </summary>
		/// <param name="attempt">当前钓鱼尝试的数据</param>
		public override void ModifyFishingAttempt(ref FishingAttempt attempt)
		{
			//如果玩家拥有"示例板条箱"增益（来自示例板条箱药水）
			//且有10%的额外几率使捕获物变为板条箱
			//注意：板条箱的"等级"取决于稀有度，此处不修改稀有度（详见CatchFish中的说明）
			if (hasCrateBuff && !attempt.crate)
			{
				if (Main.rand.Next(100) < 10)
				{
					attempt.crate = true;//设置为捕获板条箱
				}
			}
		}
		/// <summary>
		/// 捕获鱼获时的处理逻辑
		/// <param name="attempt">钓鱼尝试数据</param>
		/// <param name="itemDrop">捕获的物品类型</param>
		/// <param name="npcSpawn">生成的NPC类型</param>
		/// <param name="sonar">声呐提示框设置</param>
		/// <param name="sonarPosition">声呐提示框位置</param>
		/// </summary>
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
		{
			//检查是否在普通水体（非熔岩/蜂蜜）
			bool inWater = !attempt.inLava && !attempt.inHoney;
			//检查是否在自定义地表生物群落
			//bool inExampleSurfaceBiome = Player.InModBiome<ExampleSurfaceBiome>();//不要，没做
			//条件1：使用示例钓竿 + 在普通水体 + 自定义地表生物群落（取消）
			if (attempt.playerFishingConditions.PoleItemType == ModContent.ItemType<VastWaveFishingRod>() && inWater)
			{
				//在示例地表生物群落的水体中钓起"示例人物"NPC
				//前提：世界中不存在该NPC
				//注意：如果钓竿有多个浮标，则每个浮标都可能生成NPC
				int npc = NPCID.JungleSlime;
				if (!NPC.AnyNPCs(npc))
				{
					//生成NPC时需设置itemDrop = -1，否则只会生成物品
					npcSpawn = npc;
					itemDrop = -1;
					//创建特殊的声呐提示效果
					sonar.Text = "感觉不对劲...";
					sonar.Color = Color.LimeGreen;
					sonar.Velocity = Vector2.Zero;
					sonar.DurationInFrames = 300;
					//在玩家头顶显示提示（而非浮标位置）
					sonarPosition = new Vector2(Player.position.X, Player.position.Y - 64);
					return;//重要：直接返回，避免后续物品生成逻辑
				}
			}
			//条件2：在普通水体 + 捕获的是板条箱 + 自定义地表生物群落（取消）
			if (inWater && attempt.crate)
			{
				//用自定义板条箱替换原版板条箱
				//不替换金/钛金板条箱（最高级板条箱）
				//它们的掉落条件是"非常稀有(veryrare)"或"传说级(legendary)"
				//优先级排序：传说级 > 非常稀有 > 生物群落板条箱（稀有） > 铁/秘银板条箱（不常见） > 木/珍珠板条箱
				//50%几率替换生物群落板条箱（需考虑玩家可能处于多个模组生物群落）
				if (!attempt.veryrare && !attempt.legendary && attempt.rare && Main.rand.NextBool())
				{
					itemDrop = ModContent.ItemType<FishingCrate1>();
					return; //重要：直接返回，避免后续物品生成逻辑
				}
			}

			//设置示例任务鱼的捕获条件
			int QuestFish = ModContent.ItemType<BoBoQuestFish1>();
			//检查今日任务鱼是否匹配示例任务鱼
			if (attempt.questFish == QuestFish)//已测试括号中可以触发
			{
				//示例任务鱼要求倒立时捕获（重力方向为负）
				//普通重力为正，倒立重力为负
				//原版任务鱼通常出现在"不常见(uncommon)"捕获中
				if (Player.gravDir < 0f && attempt.uncommon)
				{
					itemDrop = QuestFish;
					return;//虽然后续没有其他鱼类生成逻辑，但保留return以便未来扩展
				}
			}
		}
		/// <summary>
		/// 检查是否允许消耗鱼饵
		/// </summary>
		/// <param name="bait">要检查的鱼饵</param>
		/// <returns>null表示使用默认逻辑</returns>
		public override bool? CanConsumeBait(Item bait)
		{
			//Player.GetFishingConditions()返回当前最佳钓竿和鱼饵数据
			//包含物品类型、渔力值及模组加成
			//注意：此时获取的条件与CatchFish中的attempt.playerFishingConditions相同
			PlayerFishingConditions conditions = Player.GetFishingConditions();
			//金钓竿永远不会消耗瓢虫类鱼饵
			if ((bait.type == ItemID.LadyBug || bait.type == ItemID.GoldLadyBug) && conditions.Pole.type == ItemID.GoldenFishingRod)
			{
				return false;//禁止消耗
			}
			return null;//使用默认逻辑
		}
		/// <summary>
		/// 修改捕获的鱼获（增加堆叠数量）
		/// </summary>
		/// <param name="fish">捕获的鱼获物品</param>
		public override void ModifyCaughtFish(Item fish)
		{
			//如果使用瓢虫类鱼饵，且捕获的不是任务鱼
			//则增加捕获数量
			if (Player.GetFishingConditions().BaitItemType == ItemID.LadyBug &&
				fish.rare != ItemRarityID.Quest)
			{
				fish.stack += Main.rand.Next(1, 4);//增加1-3条
			}
		}
	}
}*/