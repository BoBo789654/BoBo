using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;

namespace BoBo.Content.Common
{
    public class AccessorySlotConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [Header("饰品槽位设置")]
        [Range(0, 7)]
        [DefaultValue(3)]//默认显示7个槽位
        [Slider]
        [Tooltip("饰品槽位设置")]
        public int VisibleSlots; //玩家可自定义的槽位数（0-7）
        public override void OnChanged()
        {
            //强制刷新UI和槽位状态
            Main.playerInventory = false;
            Main.playerInventory = true;
        }
    }
    public static class AccessorySlotHelper
    {
        public static Vector2 GetPosition(int slotIndex)
        {
            return new Vector2(Main.screenWidth - 225 * Main.UIScale, Main.screenHeight * 0.5f + 8 * Main.UIScale + 48 * slotIndex);
        }
    }
    //通用饰品栏基类
    public abstract class BaseAccessorySlot : ModAccessorySlot
    {
        public override bool IsEnabled()
        {
            var config = ModContent.GetInstance<AccessorySlotConfig>();
            return SlotIndex < config.VisibleSlots;
        }//仅显示配置范围内的槽位
        //每个子类必须实现的抽象属性
        public abstract int SlotIndex { get; }
        public abstract int FBTID { get; }
        public abstract int FTID { get; }
        public abstract int VBTID { get; }
        public abstract int VTID { get; }
        public abstract int DBTID { get; }
        public abstract int DTID { get; }

        //位置计算
        public override Vector2? CustomLocation => AccessorySlotHelper.GetPosition(SlotIndex);

        //背景和图标
        public override string FunctionalBackgroundTexture => $"Terraria/Images/Inventory_Back{FBTID}";//配饰背景
        public override string FunctionalTexture => $"Terraria/Images/Item_{FTID}";//背景
        public override string VanityBackgroundTexture => $"Terraria/Images/Item_{VBTID}";//装扮背景
        //public override string VanityBackgroundTexture => $"Terraria/Images/Inventory_Back{VBTID}";//装扮背景
        public override string VanityTexture => $"Terraria/Images/Item_{VTID}";//装扮
        public override string DyeBackgroundTexture => $"Terraria/Images/Item_{DBTID}";//背景
        //public override string DyeBackgroundTexture => $"Terraria/Images/Inventory_Back{DBTID}";//背景
        public override string DyeTexture => $"Terraria/Images/Item_{DTID}";//染料
        public override void OnMouseHover(AccessorySlotType context)
        {
            switch (context)
            {
                case AccessorySlotType.FunctionalSlot:
                    Main.hoverItemName = "饰品";
                    break;
                case AccessorySlotType.VanitySlot:
                    Main.hoverItemName = "装扮";
                    break;
                case AccessorySlotType.DyeSlot:
                    Main.hoverItemName = "染料";
                    break;
            }
        }
    }
    public class ExtraAccessorySlot1 : BaseAccessorySlot
    {
        public override int SlotIndex => 0;
        public override int FBTID => 11;
        public override int FTID => ItemID.HermesBoots;
        public override int VBTID => 0;
        public override int VTID => 0;
        public override int DBTID => 0;
        public override int DTID => 0;
    }
    public class ExtraAccessorySlot2 : BaseAccessorySlot
    {
        public override int SlotIndex => 1;
        public override int FBTID => 12;
        public override int FTID => ItemID.ObsidianShield;
        public override int VBTID => 0;
        public override int VTID => 0;
        public override int DBTID => 0;
        public override int DTID => 0;
    }

    public class WingAccessorySlot : BaseAccessorySlot
    {
        public override int SlotIndex => 2;
        public override int FBTID => 13;
        public override int FTID => ItemID.AngelWings;
        public override int VBTID => 0;
        public override int VTID => 0;
        public override int DBTID => 0;
        public override int DTID => 0;
    }
    public class ExtraAccessorySlot4 : BaseAccessorySlot
    {
        public override int SlotIndex => 3;
        public override int FBTID => 14;
        public override int FTID => ItemID.WarriorEmblem;
        public override int VBTID => 0;
        public override int VTID => 0;
        public override int DBTID => 0;
        public override int DTID => 0;
    }

    public class ExtraAccessorySlot5 : BaseAccessorySlot
    {
        public override int SlotIndex => 4;
        public override int FBTID => 15;
        public override int FTID => ItemID.MiningHelmet;
        public override int VBTID => 0;
        public override int VTID => 0;
        public override int DBTID => 0;
        public override int DTID => 0;
    }

    public class ExtraAccessorySlot6 : BaseAccessorySlot
    {
        public override int SlotIndex => 5;
        public override int FBTID => 8;
        public override int FTID => ItemID.FishingPotion;
        public override int VBTID => 0;
        public override int VTID => 0;
        public override int DBTID => 0;
        public override int DTID => 0;
    }

    public class ExtraAccessorySlot7 : BaseAccessorySlot
    {
        public override int SlotIndex => 6;
        public override int FBTID => 6;
        public override int FTID => ItemID.PygmyNecklace;
        public override int VBTID => 0;
        public override int VTID => 0;
        public override int DBTID => 0;
        public override int DTID => 0;
    }
}
/*
public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
{
    return checkItem.wingSlot > 0;
}

public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo)
{
    return item.wingSlot > 0;
}

public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
{
    return checkItem.accessory && (checkItem.defense > 0 || checkItem.damage > 0 || checkItem.crit > 0);
}
public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
{
    return checkItem.accessory && (checkItem.pick > 0 || checkItem.axe > 0 || checkItem.tileBoost > 0);
}
public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
{
    return checkItem.accessory &&
           (checkItem.fishingPole > 0 ||
            checkItem.bait > 0 );
}
public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
{
    return checkItem.accessory &&
           (checkItem.sentry ||
            checkItem.DD2Summon ||
            checkItem.manaIncrease > 0);
}*/