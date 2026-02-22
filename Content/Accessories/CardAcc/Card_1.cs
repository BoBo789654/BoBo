using BoBo.Content.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Accessories.CardAcc
{
	public class Card_1 : CardItem//卡牌：史莱姆王（获得所有药水和增益站的效果）
	{
		public override string Texture => Pictures.CardAcc + Name;
		public override string CardType => "NormalCard";//普通品质
		public override void SetDefaults()
		{
			Item.width = 48;
			Item.height = 60;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(999, 0, 0, 0);
			Item.maxStack = 9999;
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<GoodBuffPlayer>().HasCardBuff = true;
		}
	}
	public class GoodBuffPlayer : ModPlayer
	{
		public bool HasCardBuff;

		public override void ResetEffects() => HasCardBuff = false;

		public override void PostUpdate()
		{
			if (HasCardBuff)
			{
				foreach (int buffID in BaseGoodBuff.GoodBuffIDs)
				{
					int index = Player.FindBuffIndex(buffID);
					if (index == -1)
					{
						Player.AddBuff(buffID, 66666);
					}
					else if (Player.buffTime[index] < 66666)
					{
						Player.buffTime[index] = 66666;
					}
				}
			}
			else
			{
				foreach (int buffID in BaseGoodBuff.GoodBuffIDs)
				{
					int index = Player.FindBuffIndex(buffID);
					if (index != -1) Player.DelBuff(index);
				}
			}
		}
	}

}