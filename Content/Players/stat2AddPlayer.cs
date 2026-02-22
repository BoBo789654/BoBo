using BoBo.Content.Accessories.CardAcc;
using Terraria;
using Terraria.ModLoader;

namespace BoBo.Content.Players
{
	public class stat2AddPlayer : ModPlayer//背包也生效的饰品，主要是懒得拿东西了
    {
        public int Card_1Count = 0;
        public int Card_2Count  = 0;
        public int Card_3Count = 0; 
        public override void ResetEffects()
        {
            Card_1Count = 0;
            Card_2Count = 0;
            Card_3Count = 0;
        }
        public override void UpdateEquips()
        {
            /*//遍历每个格子
            for (int i = 0; i < Player.inventory.Length; i++)
            {
                if (Player.inventory[i].type == ModContent.ItemType<ItemID>())
                {
                    ItemCount++;
                }
            }
            */
            #region +20生命上限/张卡牌1
            if (Player.HasItem(ModContent.ItemType<Card_1>()))
            {
                foreach (Item item in Player.inventory)
                {
                    if (item.type == ModContent.ItemType<Card_1>() && item.stack > 0)
                    {
                        Card_1Count += item.stack;
                    }
                }
                Player.statLifeMax2 += Card_1Count * 20;
            }
            #endregion
            #region +20魔力上限/张卡牌2
            if (Player.HasItem(ModContent.ItemType<Card_2>()))
            {
                foreach (Item item in Player.inventory)
                {
                    if (item.type == ModContent.ItemType<Card_2>() && item.stack > 0)
                    {
                        Card_2Count += item.stack;
                    }
                }
                Player.statManaMax2 += Card_2Count * 20;
            }
            #endregion
            #region +1召唤上限/张卡牌3
            if (Player.HasItem(ModContent.ItemType<Card_3>()))
            {
                foreach (Item item in Player.inventory)
                {
                    if (item.type == ModContent.ItemType<Card_3>() && item.stack > 0)
                    {
                        Card_3Count += item.stack;
                    }
                }
                Player.maxMinions += Card_3Count * 1;
            }
            #endregion
        }
    }
}
