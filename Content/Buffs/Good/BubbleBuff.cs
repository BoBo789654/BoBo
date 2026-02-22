using Terraria;
using Terraria.ModLoader;

namespace BoBo.Content.Buffs.Good
{
	public class BubbleBuff : ModBuff
	{
		public override string Texture => Pictures.Good + Name;
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			player.GetDamage(DamageClass.Generic) += 0.1f;
			player.GetCritChance(DamageClass.Generic) += 10f;
		}
	}
}