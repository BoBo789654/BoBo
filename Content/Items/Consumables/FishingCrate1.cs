using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Consumables
{
	/// <summary>
	/// 基础钓鱼板条箱实现
	/// 实际捕获逻辑在单独的 ModPlayer 类中（如 ExampleFishingPlayer）
	/// 放置的物块在单独的 ModTile 类中定义
	/// </summary>
	public class FishingCrate1 : ModItem
	{
		public override string Texture => Pictures.Consumables + Name;
		public override void SetStaticDefaults()
		{
			//以下设置原版仅检查原版物品ID，但为跨模组兼容性，建议模组板条箱也设置
			ItemID.Sets.IsFishingCrate[Type] = true; //标记此物品为钓鱼板条箱类型
													 //ItemID.Sets.IsFishingCrateHardmode[Type] = true; 
													 //若为困难模式板条箱则取消注释（此处模拟困难模式前板条箱，故注释）

			Item.ResearchUnlockCount = 1;//研究解锁需1个样本
		}
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(TileID.FishingCrate); //使用原版板条箱的放置物块
			Item.width = 12;
			Item.height = 12;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 2); 
		}
		//控制物品在创意模式研究界面的分类
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
		{
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.Crates;//归类至"板条箱"分组
		}
		//允许右键直接打开板条箱（无需放置）
		public override bool CanRightClick()
		{
			return true;
		}
		//定义板条箱的掉落物规则
		public override void ModifyItemLoot(ItemLoot itemLoot)
		{
			//主题性掉落：从以下武器/饰品中随机选1个（不随运气值缩放）
			int[] themedDrops = new int[] {
				ItemID.Zenith,//原版最终武器
            };
			itemLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, themedDrops));
			//金币掉落：固定掉落5-13枚金币（权重4）
			itemLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 4, 5, 13));
			//矿石掉落：从以下矿石中随机选1种（权重7），数量30-50
			IItemDropRule[] oreTypes = new IItemDropRule[] {
				ItemDropRule.Common(ItemID.CopperOre, 1, 30, 50),//铜矿
				ItemDropRule.Common(ItemID.TinOre, 1, 30, 50),//锡矿
				ItemDropRule.Common(ItemID.IronOre, 1, 30, 50),//铁矿
				ItemDropRule.Common(ItemID.LeadOre, 1, 30, 50),//铅矿
				ItemDropRule.Common(ItemID.SilverOre, 1, 30, 50),//银矿
				ItemDropRule.Common(ItemID.TungstenOre, 1, 30, 50),//钨矿
				ItemDropRule.Common(ItemID.GoldOre, 1, 30, 50),//金矿
				ItemDropRule.Common(ItemID.PlatinumOre, 1, 30, 50),//铂金矿	
				ItemDropRule.Common(ItemID.DemoniteOre, 1, 30, 50),//魔矿
				ItemDropRule.Common(ItemID.CrimtaneOre, 1, 30, 50),//猩红矿
				ItemDropRule.Common(ItemID.Meteorite, 1, 30, 50),//陨石
				ItemDropRule.Common(ItemID.Hellstone, 1, 30, 50),//狱石
				ItemDropRule.Common(ItemID.CobaltOre, 1, 30, 50),//钴矿
				ItemDropRule.Common(ItemID.MythrilOre, 1, 30, 50),//秘银矿
				ItemDropRule.Common(ItemID.AdamantiteOre, 1, 30, 50),//精金矿
				ItemDropRule.Common(ItemID.PalladiumOre, 1, 30, 50),//钯金矿
				ItemDropRule.Common(ItemID.OrichalcumOre, 1, 30, 50),//精金矿	
				ItemDropRule.Common(ItemID.TitaniumOre, 1, 30, 50),//钛金矿
				ItemDropRule.Common(ItemID.ChlorophyteOre, 1, 30, 50),//叶绿矿
				ItemDropRule.Common(ItemID.LunarOre, 1, 30, 50),//夜明矿
			};
			itemLoot.Add(new OneFromRulesRule(7, oreTypes));
			//金属锭掉落：从以下锭中随机选1种（权重4），数量10-21
			IItemDropRule[] oreBars = new IItemDropRule[] {
				ItemDropRule.Common(ItemID.CopperBar, 1, 30, 50),//铜锭
				ItemDropRule.Common(ItemID.TinBar, 1, 30, 50),//锡锭
				ItemDropRule.Common(ItemID.IronBar, 1, 10, 21),//铁锭
				ItemDropRule.Common(ItemID.LeadBar, 1, 10, 21),//铅锭
				ItemDropRule.Common(ItemID.SilverBar, 1, 10, 21),//银锭
				ItemDropRule.Common(ItemID.TungstenBar, 1, 10, 21),//钨锭
				ItemDropRule.Common(ItemID.GoldBar, 1, 10, 21),//金锭
				ItemDropRule.Common(ItemID.PlatinumBar, 1, 10, 21),//铂金锭
				ItemDropRule.Common(ItemID.DemoniteBar, 1, 10, 21),//魔矿锭
				ItemDropRule.Common(ItemID.CrimtaneBar, 1, 10, 21),//猩红锭	
				ItemDropRule.Common(ItemID.MeteoriteBar, 1, 10, 21),//陨石锭
				ItemDropRule.Common(ItemID.HellstoneBar, 1, 10, 21),//狱石锭
				ItemDropRule.Common(ItemID.CobaltBar, 1, 10, 21),//钴锭
				ItemDropRule.Common(ItemID.MythrilBar, 1, 10, 21),//秘银锭
				ItemDropRule.Common(ItemID.AdamantiteBar, 1, 10, 21),//精金锭
				ItemDropRule.Common(ItemID.PalladiumBar, 1, 10, 21),//钯金锭
				ItemDropRule.Common(ItemID.OrichalcumBar, 1, 10, 21),//山铜锭
				ItemDropRule.Common(ItemID.TitaniumBar, 1, 10, 21),//钛金锭
				ItemDropRule.Common(ItemID.HallowedBar, 1, 10, 21),//神圣锭
				ItemDropRule.Common(ItemID.ChlorophyteBar, 1, 10, 21),//叶绿锭	
				ItemDropRule.Common(ItemID.ShroomiteBar, 1, 10, 21),//蘑菇锭
				ItemDropRule.Common(ItemID.SpectreBar, 1, 10, 21),//幽灵锭
				ItemDropRule.Common(ItemID.LunarBar, 1, 10, 21),//夜明锭
            };
			itemLoot.Add(new OneFromRulesRule(4, oreBars));
			//探索药水：从以下药水中随机选1种（权重4），数量2-5
			IItemDropRule[] explorationPotions = new IItemDropRule[] {
				ItemDropRule.Common(ItemID.ObsidianSkinPotion, 1, 2, 5),
				ItemDropRule.Common(ItemID.SpelunkerPotion, 1, 2, 5),
				ItemDropRule.Common(ItemID.HunterPotion, 1, 2, 5),
				ItemDropRule.Common(ItemID.GravitationPotion, 1, 2, 5),
				ItemDropRule.Common(ItemID.MiningPotion, 1, 2, 5),
				ItemDropRule.Common(ItemID.HeartreachPotion, 1, 2, 5),
			};
			itemLoot.Add(new OneFromRulesRule(4, explorationPotions));
			//基础资源药水：从以下药水中随机选1种（权重2），数量5-18
			IItemDropRule[] resourcePotions = new IItemDropRule[] {
				ItemDropRule.Common(ItemID.SuperHealingPotion, 1, 5, 18),//治疗药水
                ItemDropRule.Common(ItemID.SuperManaPotion, 1, 5, 18),//魔力药水
            };
			itemLoot.Add(new OneFromRulesRule(2, resourcePotions));
			//高级鱼饵：从以下鱼饵中随机选1种（权重2），数量2-7
			IItemDropRule[] highendBait = new IItemDropRule[] {
				ItemDropRule.Common(ItemID.JourneymanBait, 1, 2, 7), //熟手诱饵（30%渔力）
                ItemDropRule.Common(ItemID.MasterBait, 1, 2, 7),     //大师诱饵（50%渔力）
            };
			itemLoot.Add(new OneFromRulesRule(2, highendBait));
		}
	}
}