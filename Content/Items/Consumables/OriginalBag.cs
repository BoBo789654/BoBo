using BoBo.Content.Common;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Consumables
{
	public class OriginalBag : ModItem
	{
		public override string Texture => Pictures.Consumables + Name;
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;//研究解锁需1个样本
		}
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.rare = ItemRarityID.Expert;
			Item.value = Item.sellPrice(9999);
			Item.maxStack = 9999;
		}
		//物品在创意模式研究界面的分类
		public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
		{
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossBags;
		}
		//允许右键直接打开板条箱（无需放置）
		public override bool CanRightClick()
		{
			return true;
		}
		//我要测试档的所有初始物品
		public override void ModifyItemLoot(ItemLoot itemLoot)//使用Common，第一个参数是物品ID，第二个参数是概率，第三个参数是最大值，第四个参数是最小值
		{
			//药水掉落，每种30瓶，100%掉落
			foreach (int potionIds in BasePotions.PotionId)//药水
				itemLoot.Add(ItemDropRule.Common(potionIds, minimumDropped: 30, maximumDropped: 30));
			foreach (int potionIds in BasePotions.HealingPotionId)//药水
				itemLoot.Add(ItemDropRule.Common(potionIds, minimumDropped: 9999, maximumDropped: 9999));

			//箱子、手机
			itemLoot.Add(ItemDropRule.Common(ItemID.MoneyTrough, minimumDropped: 1, maximumDropped: 1));//钱币槽
			itemLoot.Add(ItemDropRule.Common(ItemID.Safe, minimumDropped: 1, maximumDropped: 1));//保险箱
			itemLoot.Add(ItemDropRule.Common(ItemID.DefendersForge, minimumDropped: 1, maximumDropped: 1));//护卫熔炉
			itemLoot.Add(ItemDropRule.Common(ItemID.Shellphone, minimumDropped: 1, maximumDropped: 1));//贝壳电话（家）

			//加上限
			itemLoot.Add(ItemDropRule.Common(ItemID.LifeCrystal, minimumDropped: 15, maximumDropped: 15));//生命水晶
			itemLoot.Add(ItemDropRule.Common(ItemID.ManaCrystal, minimumDropped: 19, maximumDropped: 19));//魔力水晶
			itemLoot.Add(ItemDropRule.Common(ItemID.LifeFruit, minimumDropped: 20, maximumDropped: 20));//生命果

			//在旁边有效
			itemLoot.Add(ItemDropRule.Common(ItemID.UltraBrightCampfire, minimumDropped: 9999, maximumDropped: 9999));//超亮篝火
			itemLoot.Add(ItemDropRule.Common(ItemID.Sunflower, minimumDropped: 9999, maximumDropped: 9999));//向日葵
			itemLoot.Add(ItemDropRule.Common(ItemID.HeartLantern, minimumDropped: 9999, maximumDropped: 9999));//红心灯笼
			itemLoot.Add(ItemDropRule.Common(ItemID.PeaceCandle, minimumDropped: 9999, maximumDropped: 9999));//和平蜡烛
			itemLoot.Add(ItemDropRule.Common(ItemID.WaterCandle, minimumDropped: 9999, maximumDropped: 9999));//水蜡烛
			itemLoot.Add(ItemDropRule.Common(ItemID.StarinaBottle, minimumDropped: 9999, maximumDropped: 9999));//星星瓶
			itemLoot.Add(ItemDropRule.Common(ItemID.CatBast, minimumDropped: 9999, maximumDropped: 9999));//巴斯特雕像

			//右键有效
			itemLoot.Add(ItemDropRule.Common(ItemID.AmmoBox, minimumDropped: 9999, maximumDropped: 9999));//弹药箱
			itemLoot.Add(ItemDropRule.Common(ItemID.BewitchingTable, minimumDropped: 9999, maximumDropped: 9999));//施法桌
			itemLoot.Add(ItemDropRule.Common(ItemID.CrystalBall, minimumDropped: 9999, maximumDropped: 9999));//水晶球
			itemLoot.Add(ItemDropRule.Common(ItemID.SharpeningStation, minimumDropped: 9999, maximumDropped: 9999));//利器站
			itemLoot.Add(ItemDropRule.Common(ItemID.WarTable, minimumDropped: 9999, maximumDropped: 9999));//战争桌
			itemLoot.Add(ItemDropRule.Common(ItemID.SliceOfCake, minimumDropped: 9999, maximumDropped: 9999));//蛋糕块

			//无尽桶
			itemLoot.Add(ItemDropRule.Common(ItemID.EmptyBucket, minimumDropped: 9999, maximumDropped: 9999));//空桶
			itemLoot.Add(ItemDropRule.Common(ItemID.BottomlessBucket, minimumDropped: 1, maximumDropped: 1));//无底水桶
			itemLoot.Add(ItemDropRule.Common(ItemID.BottomlessLavaBucket, minimumDropped: 1, maximumDropped: 1));//无底熔岩桶
			itemLoot.Add(ItemDropRule.Common(ItemID.BottomlessHoneyBucket, minimumDropped: 1, maximumDropped: 1));//无底蜂蜜桶
			itemLoot.Add(ItemDropRule.Common(ItemID.BottomlessShimmerBucket, minimumDropped: 1, maximumDropped: 1));//无底微光桶

			//微光嬗变
			itemLoot.Add(ItemDropRule.Common(ItemID.DontHurtComboBook, minimumDropped: 1, maximumDropped: 1));//和平共处指南
			itemLoot.Add(ItemDropRule.Common(ItemID.ArtisanLoaf, minimumDropped: 1, maximumDropped: 1));//工匠面包
			itemLoot.Add(ItemDropRule.Common(ItemID.CombatBook, minimumDropped: 1, maximumDropped: 1));//先进战斗技术
			itemLoot.Add(ItemDropRule.Common(ItemID.CombatBookVolumeTwo, minimumDropped: 1, maximumDropped: 1));//先进战斗技术：卷二
			itemLoot.Add(ItemDropRule.Common(ItemID.AegisCrystal, minimumDropped: 1, maximumDropped: 1));//活力水晶
			itemLoot.Add(ItemDropRule.Common(ItemID.AegisFruit, minimumDropped: 1, maximumDropped: 1));//神盾果
			itemLoot.Add(ItemDropRule.Common(ItemID.ArcaneCrystal, minimumDropped: 1, maximumDropped: 1));//奥术水晶
			itemLoot.Add(ItemDropRule.Common(ItemID.GalaxyPearl, minimumDropped: 1, maximumDropped: 1));//星系珍珠
			itemLoot.Add(ItemDropRule.Common(ItemID.GummyWorm, minimumDropped: 1, maximumDropped: 1));//黏性蠕虫
			itemLoot.Add(ItemDropRule.Common(ItemID.Ambrosia, minimumDropped: 1, maximumDropped: 1));//仙馔密酒
			itemLoot.Add(ItemDropRule.Common(ItemID.PeddlersSatchel, minimumDropped: 1, maximumDropped: 1));//商贩背包

			//工具
			itemLoot.Add(ItemDropRule.Common(ItemID.RodOfHarmony, minimumDropped: 1, maximumDropped: 1));//和谐传送杖
			itemLoot.Add(ItemDropRule.Common(ItemID.Clentaminator2, minimumDropped: 1, maximumDropped: 1));//泰拉改造枪
			itemLoot.Add(ItemDropRule.Common(ItemID.NebulaPickaxe, minimumDropped: 1, maximumDropped: 1));//星云镐
			itemLoot.Add(ItemDropRule.Common(ItemID.LunarHamaxeNebula, minimumDropped: 1, maximumDropped: 1));//星云锤斧

			//武器
			//itemLoot.Add(ItemDropRule.Common(ItemID., minimumDropped: 1, maximumDropped: 1));//
			//itemLoot.Add(ItemDropRule.Common(ItemID., minimumDropped: 1, maximumDropped: 1));//
			//itemLoot.Add(ItemDropRule.Common(ItemID., minimumDropped: 1, maximumDropped: 1));//
			//itemLoot.Add(ItemDropRule.Common(ItemID., minimumDropped: 1, maximumDropped: 1));//

			//制作站
			//itemLoot.Add(ItemDropRule.Common(ItemID.WorkBench, minimumDropped: 1, maximumDropped: 1));//工作台
			//itemLoot.Add(ItemDropRule.Common(ItemID.MythrilAnvil, minimumDropped: 1, maximumDropped: 1));//秘银砧
			//itemLoot.Add(ItemDropRule.Common(ItemID.AdamantiteForge, minimumDropped: 1, maximumDropped: 1));//精金熔炉
			//itemLoot.Add(ItemDropRule.Common(ItemID.AlchemyTable, minimumDropped: 1, maximumDropped: 1));//炼药桌
			//itemLoot.Add(ItemDropRule.Common(ItemID.Sawmill, minimumDropped: 1, maximumDropped: 1));//锯木机
			//itemLoot.Add(ItemDropRule.Common(ItemID.Loom, minimumDropped: 1, maximumDropped: 1));//织布机
			//itemLoot.Add(ItemDropRule.Common(ItemID.CookingPot, minimumDropped: 1, maximumDropped: 1));//烹饪锅
			//itemLoot.Add(ItemDropRule.Common(ItemID.TinkerersWorkshop, minimumDropped: 1, maximumDropped: 1));//工匠作坊
			//itemLoot.Add(ItemDropRule.Common(ItemID.ImbuingStation, minimumDropped: 1, maximumDropped: 1));//灌注站
			//itemLoot.Add(ItemDropRule.Common(ItemID.DyeVat, minimumDropped: 1, maximumDropped: 1));//染缸
			//itemLoot.Add(ItemDropRule.Common(ItemID.HeavyWorkBench, minimumDropped: 1, maximumDropped: 1));//重型工作台
			//itemLoot.Add(ItemDropRule.Common(ItemID.Bookcase, minimumDropped: 1, maximumDropped: 1));//书架
			//itemLoot.Add(ItemDropRule.Common(ItemID.Autohammer, minimumDropped: 1, maximumDropped: 1));//自动锤炼机
			//itemLoot.Add(ItemDropRule.Common(ItemID.LunarCraftingStation, minimumDropped: 1, maximumDropped: 1));//远古操纵机
			//itemLoot.Add(ItemDropRule.Common(ItemID.Keg, minimumDropped: 1, maximumDropped: 1));//酒桶
			//itemLoot.Add(ItemDropRule.Common(ItemID.TeaKettle, minimumDropped: 1, maximumDropped: 1));//茶壶
			//itemLoot.Add(ItemDropRule.Common(ItemID.BlendOMatic, minimumDropped: 1, maximumDropped: 1));//搅拌机
			//itemLoot.Add(ItemDropRule.Common(ItemID.MeatGrinder, minimumDropped: 1, maximumDropped: 1));//绞肉机
			//itemLoot.Add(ItemDropRule.Common(ItemID.BoneWelder, minimumDropped: 1, maximumDropped: 1));//骨头焊机
			//itemLoot.Add(ItemDropRule.Common(ItemID.GlassKiln, minimumDropped: 1, maximumDropped: 1));//玻璃窑
			//itemLoot.Add(ItemDropRule.Common(ItemID.HoneyDispenser, minimumDropped: 1, maximumDropped: 1));//蜂蜜分配机
			//itemLoot.Add(ItemDropRule.Common(ItemID.IceMachine, minimumDropped: 1, maximumDropped: 1));//冰雪机
			//itemLoot.Add(ItemDropRule.Common(ItemID.LivingLoom, minimumDropped: 1, maximumDropped: 1));//生命木织机
			//itemLoot.Add(ItemDropRule.Common(ItemID.SkyMill, minimumDropped: 1, maximumDropped: 1));//天磨
			//itemLoot.Add(ItemDropRule.Common(ItemID.Solidifier, minimumDropped: 1, maximumDropped: 1));//固化机
			//itemLoot.Add(ItemDropRule.Common(ItemID.LesionStation, minimumDropped: 1, maximumDropped: 1));//腐变室
			//itemLoot.Add(ItemDropRule.Common(ItemID.FleshCloningVaat, minimumDropped: 1, maximumDropped: 1));//血肉克隆台
			//itemLoot.Add(ItemDropRule.Common(ItemID.SteampunkBoiler, minimumDropped: 1, maximumDropped: 1));//蒸汽朋克锅炉
			//itemLoot.Add(ItemDropRule.Common(ItemID.LihzahrdFurnace, minimumDropped: 1, maximumDropped: 1));//丛林蜥蜴熔炉
			//itemLoot.Add(ItemDropRule.Common(ItemID.Extractinator, minimumDropped: 1, maximumDropped: 1));//提炼机
			//itemLoot.Add(ItemDropRule.Common(ItemID.ChlorophyteExtractinator, minimumDropped: 1, maximumDropped: 1));//叶绿提炼机
			//itemLoot.Add(ItemDropRule.Common(ItemID.Toilet, minimumDropped: 1, maximumDropped: 1));//马桶
			//itemLoot.Add(ItemDropRule.Common(ItemID.BombStatue, minimumDropped: 9999, maximumDropped: 9999));//炸弹雕像
			//itemLoot.Add(ItemDropRule.Common(ItemID.HeartStatue, minimumDropped: 9999, maximumDropped: 9999));//心形雕像
			//itemLoot.Add(ItemDropRule.Common(ItemID.StarStatue, minimumDropped: 9999, maximumDropped: 9999));//星星雕像
		}
	}
}