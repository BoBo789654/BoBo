using BoBo.Content.Projectiles.Weapons.Summon;
using Terraria;
using Terraria.ModLoader;

namespace BoBo.Content.Buffs.MinionBuff
{
	public class BatSummoningBuffC : ModBuff
	{
		public override string Texture => Pictures.MinionBuff + Name;
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<BatC>()] > 0)
			{
				player.buffTime[buffIndex] = 18000;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}
