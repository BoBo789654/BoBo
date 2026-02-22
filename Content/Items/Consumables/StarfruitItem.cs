using BoBo.Content.Players;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Consumables
{
	internal class StarfruitItem : ModItem
	{
		public override string Texture => Pictures.Consumables + Name;
		public static readonly int MaxStarfruits = 15; //最大杨桃数量
		public static readonly int ManaPerStarfruit = 15; //每个杨桃增加的魔力值
		public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ManaPerStarfruit, MaxStarfruits);

		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 15;
		}
		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.ManaCrystal); //克隆魔力水晶属性
			Item.rare = ItemRarityID.Pink; //可自定义稀有度
		}
		public override bool CanUseItem(Player player)
		{
			//确保玩家已吃满原版魔力水晶
			return player.ConsumedManaCrystals == Player.ManaCrystalMax;
		}
		public override bool? UseItem(Player player)
		{
			var modPlayer = player.GetModPlayer<stat1AddPlayer>();
			if (modPlayer.starfruitCount >= MaxStarfruits)
			{
				//超过上限时不消耗物品
				return null;
			}
			//增加玩家魔力值
			player.UseManaMaxIncreasingItem(ManaPerStarfruit);
			//增加杨桃计数
			modPlayer.starfruitCount++;

			return true;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Starfruit, 1);//杨桃
			recipe.AddTile(TileID.LunarCraftingStation);//制作站：远古操纵机
			recipe.Register();
		}
	}
}