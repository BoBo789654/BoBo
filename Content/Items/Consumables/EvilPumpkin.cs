/*using BoBo.Content.NPCs.EvilPumpking;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Consumables
{
	//此物品用于召唤Boss（本模组中的南瓜王Boss）。对于原版Boss召唤物，请参考SetStaticDefaults中的注释
	public class EvilPumpkin : ModItem
	{
		public override string Texture => Pictures.Consumables + Name;
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;//研究解锁所需数量
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;//标记为Boss召唤物，优化库存排序=
			//如果是原版 Boss（且无默认召唤物），需添加如下代码：
			//NPCID.Sets.MPAllowedEnemies[NPCID.Plantera] = true;
			//否则多人模式中无法通过 UseItem 生成 Boss
		}
		public override void SetDefaults()
		{
			Item.width = 32;//物品宽度
			Item.height = 24;//物品高度
			Item.maxStack = 9999;//最大堆叠数量
			Item.value = 5000;//出售价值
			Item.rare = ItemRarityID.Red;//稀有度
			Item.useAnimation = 30;//使用动画时长
			Item.useTime = 30;//使用间隔
			Item.useStyle = ItemUseStyleID.HoldUp;//使用动作
			Item.consumable = true;//消耗型物品
		}
		//调整创意模式物品栏分类
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
		{
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossSpawners; // 归入 Boss 召唤物分类
		}
		//检查使用条件
		public override bool CanUseItem(Player player)
		{
			//必须包含 !NPC.AnyNPCs(id) 检查（与服务器 MessageID.SpawnBoss 的验证逻辑一致）
			//可组合更多条件，例如：
			//return !Main.IsItDay() && !NPC.AnyNPCs(ModContent.NPCType<Pumpking2>()); //"非白天且无存活的南瓜王"
			return !NPC.AnyNPCs(ModContent.NPCType<EvilPumpking>());//仅当无存活的南瓜王时可用
		}
		//使用物品时触发生成逻辑
		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{	
				//仅客户端执行
				SoundEngine.PlaySound(SoundID.Roar, player.position);//播放咆哮音效
				int type = ModContent.NPCType<EvilPumpking>();//获取Boss的NPC类型ID
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					NPC.SpawnOnPlayer(player.whoAmI, type);//单人模式直接生成
				}
				else
				{
					//多人模式发送生成请求（需Pumpking2中设置NPCID.Sets.MPAllowedEnemies[type] = true）
					NetMessage.SendData(
						MessageID.SpawnBossUseLicenseStartEvent,
						number: player.whoAmI,
						number2: type
					);
				}
			}
			return true;//消耗物品
		}
	}
}*/