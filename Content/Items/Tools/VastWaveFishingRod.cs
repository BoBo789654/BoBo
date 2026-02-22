using BoBo.Content.Projectiles.Tools;
using Microsoft.Xna.Framework;
using System.ComponentModel;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace BoBo.Content.Items.Tools
{
	public class VastWaveFishingRodConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("自定义渔力")]
		[Tooltip("设置钓鱼竿的渔力值 (0-500)")]
		[Range(0, 500)]
		[DefaultValue(0)]
		public int CustomFishingPower;

		[Label("鱼线条数")]
		[Tooltip("设置同时发射的鱼线数量 (0-300)")]
		[Range(0, 300)]
		[DefaultValue(0)]
		public int BobberCount;

		[Label("消耗鱼饵")]
		[Tooltip("是否消耗鱼饵")]
		[DefaultValue(true)]
		public bool ConsumeBait;
	}
	/// <summary>
	/// 多功能钓鱼竿
	/// </summary>
	public class VastWaveFishingRod : ModItem
	{
		public override string Texture => Pictures.Tool + Name;
		private const int BaseFishingPower = 51;//基础渔力值
		private const float ShootVelocity = 12f;//浮标发射速度
		private const int MinBobbers = 30;      //最小浮标数
		private const int MaxBobbers = 50;      //最大浮标数
		public override void SetStaticDefaults()
		{
			ItemID.Sets.CanFishInLava[Item.type] = true;//启用熔岩钓鱼特性
		}
		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.GoldenFishingRod);//继承别的钓竿基础属性
			var config = ModContent.GetInstance<VastWaveFishingRodConfig>();//获取配置
			Item.fishingPole = config.CustomFishingPower;
			Item.shootSpeed = ShootVelocity;
			Item.shoot = ModContent.ProjectileType<VastWaveBobber>();//浮标算弹幕
		}
		public override void HoldItem(Player player)//持有触发
		{
			player.sonarPotion = true;      //允许声呐效果：显示鱼钩上的物品名称（原版声呐药水效果）
			player.cratePotion = true;      //允许宝匣效果：增加板条箱上钩的几率（原版宝匣药水效果）
			player.accTackleBox = true;     //允许钓具箱效果：减少鱼饵消耗率
			player.accFishingLine = true;   //高品质钓线：防止钓鱼线断裂（原版渔夫耳环效果）
			player.accLavaFishing = true;   //允许熔岩钓鱼：允许在熔岩中钓鱼（原版防熔岩钓钩效果）
			player.accFishingBobber = true; //提升浮标性能：增加鱼咬钩概率（原版渔夫渔具袋效果）
			player.accFishFinder = true;    //允许鱼获探测：显示钓鱼池信息（原版渔夫信息装备效果）
			//使用配置的渔力值
			var config = ModContent.GetInstance<VastWaveFishingRodConfig>();
			player.fishingSkill += config.CustomFishingPower;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			//多浮漂发射
			var config = ModContent.GetInstance<VastWaveFishingRodConfig>();
			int bobberCount = config.BobberCount;
			//确保数量在合理范围内
			bobberCount = Utils.Clamp(bobberCount, 1, 300);
			float spread = 75f;//浮标散射范围
			for (int i = 0; i < bobberCount; i++)
			{
				Vector2 newVelocity = velocity + new Vector2(Main.rand.NextFloat(-spread, spread) * 0.05f, Main.rand.NextFloat(-spread, spread) * 0.05f);//随机散射
				Projectile.NewProjectile(source, position, newVelocity, type, 0, 0f, player.whoAmI);
			}
			return false;
		}
		public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)//钓线特性
		{
			lineOriginOffset = new Vector2(43, -30);//钓线绘制原点
			if (bobber.ModProjectile is VastWaveBobber custom)
				lineColor = custom.CurrentLineColor;//同步浮标颜色
			else
				lineColor = Main.DiscoColor;//默认彩虹色
		}
		public override bool CanConsumeAmmo(Item ammo, Player player)//重写鱼饵消耗逻辑
		{
			var config = ModContent.GetInstance<VastWaveFishingRodConfig>();//获取配置
			return config.ConsumeBait;
		}
		public override bool CanUseItem(Player player)//允许在没有鱼饵时也能使用钓竿
		{
			var config = ModContent.GetInstance<VastWaveFishingRodConfig>();//获取配置
			if (!config.ConsumeBait)//如果不消耗鱼饵，允许无鱼饵使用
				return true;
			return base.CanUseItem(player);//否则使用默认逻辑（需要鱼饵）
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.GoldenFishingRod, 1);//金钓竿
			recipe.AddIngredient(ItemID.LunarBar, 3);//夜明锭
			recipe.AddTile(TileID.LunarCraftingStation);//制作站：远古操纵机
			recipe.Register();
		}
	}
}