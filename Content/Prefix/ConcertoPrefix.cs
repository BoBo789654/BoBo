using Terraria.ModLoader;
using Terraria;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ID;
using BoBo.Content.DamageClasses;

namespace BoBo.Content.Prefix
{
    public abstract class ConcertoPrefix : ModPrefix
    {
        public virtual float DamageBonus => 0f;
        public virtual float CritBonus => 0f;
        public virtual float UseTimeBonus => 0f;
        public virtual float PotionBonus => 0f;
        public override PrefixCategory Category => PrefixCategory.AnyWeapon;
        public override float RollChance(Item item){
            return item.rare switch
            {
                ItemRarityID.Blue => 2.5f,
                ItemRarityID.Red => 1.5f,
                ItemRarityID.Purple => 0.75f,
                ItemRarityID.Gray => 0.05f,
                _ => 1f
            };
        }
        public override bool CanRoll(Item item)
        {
            if (item.DamageType == ModContent.GetInstance<ConcertoDamageClass>())
				return true;
            return false;
        }
        public override void ModifyValue(ref float valueMult)
        {
            valueMult *= 1f + 0.05f * DamageBonus;
        }
        public static LocalizedText Tooltip { get; private set; }
        public LocalizedText AdditionalTooltip => this.GetLocalization(nameof(AdditionalTooltip));
        public override void SetStaticDefaults()
        {
            Tooltip = Mod.GetLocalization($"{LocalizationCategory}.{nameof(Tooltip)}");
            _ = AdditionalTooltip;
        }
        public override void Apply(Item item)
        {
            item.damage = (int)(item.damage * (1f + DamageBonus));
            item.crit += (int)(CritBonus * 100);
            item.useTime = (int)(item.useTime * (1f - UseTimeBonus));
        }
    }
}
