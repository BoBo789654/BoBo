using BoBo.Content.Common;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace BoBo.Content.Accessories.CardAcc
{
    public class Card_22 : CardItem//
    {
        public override string Texture => Pictures.CardAcc + Name;
        public override string CardType => "NormalCard";
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 60;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(999, 0, 0, 0);
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            foreach (int buffID in BaseGoodBuff.GoodBuffIDs)
            {
                if (player.FindBuffIndex(buffID) == -1 ||
                    player.buffTime[player.FindBuffIndex(buffID)] < 600)
                {
                    player.AddBuff(buffID, 3600 * 60);
                }
            }
        }
    }
}