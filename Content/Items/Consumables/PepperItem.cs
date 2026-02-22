using BoBo.Content.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Consumables
{
	internal class PepperItem : ModItem
	{
		public override string Texture => Pictures.Consumables + Name;
		public static readonly int MaxChilis = 20; //最大辣椒数量
		public static readonly int LifePerChili = 20; //每个辣椒增加的生命值
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(LifePerChili, MaxChilis);
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 20;
		}
		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.LifeFruit); //克隆生命果的属性
			Item.rare = ItemRarityID.Lime; //可自定义稀有度
		}
		public override bool CanUseItem(Player player)
		{
			//确保玩家已吃满原版生命水晶和生命果
			return player.ConsumedLifeCrystals == Player.LifeCrystalMax &&
				   player.ConsumedLifeFruit == Player.LifeFruitMax;
		}
		public override bool? UseItem(Player player)
		{
			var modPlayer = player.GetModPlayer<stat1AddPlayer>();
			if (modPlayer.chiliCount >= MaxChilis)
			{
				//超过上限时不消耗物品
				return null;
			}
			//增加玩家生命值
			player.UseHealthMaxIncreasingItem(LifePerChili);
			//增加辣椒计数
			modPlayer.chiliCount++;
			return true;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.SpicyPepper, 1);//辣椒
			recipe.AddTile(TileID.LunarCraftingStation);//制作站：远古操纵机
			recipe.Register();
		}
	}
}