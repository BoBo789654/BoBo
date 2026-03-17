using BoBo.Content.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Accessories.CardAcc
{
	public class Card_2 : CardItem // 卡牌：克苏鲁之眼
	{
		public override string Texture => Pictures.CardAcc + Name;
		public override string CardType => "NormalCard";

		public override void SetDefaults()
		{
			Item.width = 48;
			Item.height = 60;
			Item.accessory = true;
			Item.rare = ItemRarityID.Master;
			Item.maxStack = 9999;
			Item.value = Item.sellPrice(999, 0, 0, 0);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
		}
	}
}