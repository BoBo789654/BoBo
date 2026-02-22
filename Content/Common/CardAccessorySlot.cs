using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Common
{
    public class CardAccessorySlot : ModAccessorySlot
    {
        public override string Name => "CardSlot";
        public override Vector2? CustomLocation => new Vector2(Main.screenWidth / 2, Main.screenHeight - 240 / Main.UIScale );
        public override string FunctionalBackgroundTexture => base.FunctionalBackgroundTexture;
        public override string FunctionalTexture => "Terraria/Images/Item_0";
        public override string VanityBackgroundTexture => "Terraria/Images/Item_0";
        public override string VanityTexture => "Terraria/Images/Item_0";
        public override string DyeBackgroundTexture => "Terraria/Images/Item_0";
        public override string DyeTexture => "Terraria/Images/Item_0";
        /*public override string FunctionalBackgroundTexture => base.FunctionalBackgroundTexture;
        public override string FunctionalTexture => base.FunctionalTexture;
        public override string VanityBackgroundTexture => base.VanityBackgroundTexture;
        public override string VanityTexture => base.VanityTexture;
        public override string DyeBackgroundTexture => base.DyeBackgroundTexture;
        public override string DyeTexture => base.DyeTexture;*/
        //只允许卡片类物品放入
        public override bool CanAcceptItem(Item item, AccessorySlotType context)
        {
            return item.ModItem is CardItem;
        }
        public override void OnMouseHover(AccessorySlotType context)
        {
            Main.HoverItem = new Item();
            switch (context)
            {
                case AccessorySlotType.FunctionalSlot:
                    Main.hoverItemName = "卡牌专属槽位";
                    break;
                case AccessorySlotType.VanitySlot:
                    Main.hoverItemName = "只能放置卡牌饰品";
                    break;
                case AccessorySlotType.DyeSlot:
                    Main.hoverItemName = "不准放染料";
                    break;
            }
        }
        private const float ScaleMultiplier = 1.5f;
        //卡牌框纹理字典（按卡牌类型）
        private static readonly Dictionary<string, string> FrameTextures = new Dictionary<string, string>
        {
            { "NormalCard", "BoBo/Asset/Accessories/CardAcc/CardFrame1" },
            { "BetterCard", "BoBo/Asset/Accessories/CardAcc/CardFrame2" },
            { "BestCard", "BoBo/Asset/Accessories/CardAcc/CardFrame3" },
            { "EvilAngelCard", "BoBo/Asset/Accessories/CardAcc/CardFrame4" },
            { "AngelCard", "BoBo/Asset/Accessories/CardAcc/CardFrame5" },
        };
        public override bool PreDraw(AccessorySlotType context, Item item, Vector2 position, bool isHovered)
        {
            if (context == AccessorySlotType.FunctionalSlot && !item.IsAir)
            {
                Vector2 CardPos = position;
                Texture2D texture = Terraria.GameContent.TextureAssets.Item[item.type].Value;
                float scale = ScaleMultiplier;
                Vector2 origin = texture.Size() / 2f;
                if (item.ModItem is CardItem card)//加框
                {
                    string cardType = card.CardType;
                    if (FrameTextures.TryGetValue(cardType, out string framePath))
                    {
                        try
                        {
                            Texture2D frameTexture = ModContent.Request<Texture2D>(framePath).Value;
                            float frameScale = scale * 1.1f;
                            Vector2 frameOrigin = frameTexture.Size() / 2f;
                            Main.spriteBatch.Draw(frameTexture, CardPos, null, Color.Gold,
                                0f, frameOrigin, frameScale / Main.UIScale * 2, SpriteEffects.None, 0f);
                        }
                        catch
                        {
                            Main.NewText("操作失败", Color.Orange);
                        }
                    }
                }
                Main.spriteBatch.Draw(texture, CardPos, null, Color.White,
                    0f, origin, scale / Main.UIScale * 2, SpriteEffects.None, 0f );
                Lighting.AddLight(CardPos, Color.Gold.ToVector3() * 0.75f);
                return false;
            }
            return true;
        }
    }
    public abstract class CardItem : ModItem//卡牌基类
    {
        public virtual int CardRarity => ItemRarityID.Blue;
        public override string Texture => Pictures.CardAcc + Name;
        public virtual string CardEffectDescription => "卡牌效果";
        public virtual string CardType => "NormalCard";

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 60;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(999, 0, 0, 0);
        }
        //当卡片放入槽位时应用效果
        public virtual void ApplyCardEffect(Player player)
        {
            //在子类中实现具体效果
        }

        //当卡片移除槽位时移除效果
        public virtual void RemoveCardEffect(Player player)
        {
            //在子类中实现效果移除
        }

        //卡片不能放在普通饰品栏
        public override bool CanEquipAccessory(Player player, int slot, bool modded)
        {
            return slot == ModContent.GetInstance<CardAccessorySlot>().Type;
        }
    }
}