using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BoBo.Content.Prefix
{
    public class Melodious : ConcertoPrefix//优美的
	{
        public override float DamageBonus => 1f;
        public override float UseTimeBonus => 0.5f; 
        public override float CritBonus => 0.5f;
        public override PrefixCategory Category => PrefixCategory.AnyWeapon;
        public override void SetStats(ref float damageMult, ref float knockbackMult,
            ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult,
            ref float manaMult, ref int critBonus)
        {
            base.SetStats(ref damageMult, ref knockbackMult, ref useTimeMult,
                ref scaleMult, ref shootSpeedMult, ref manaMult, ref critBonus);
        }
    }
}
