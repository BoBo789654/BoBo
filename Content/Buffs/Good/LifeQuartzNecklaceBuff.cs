using Terraria;
using Terraria.ModLoader;

namespace BoBo.Content.Buffs.Good
{
	public class LifeQuartzNecklaceBuff : ModBuff
	{
		public override string Texture => Pictures.Good + Name;
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;//退出不保存
			Main.persistentBuff[Type] = true;//不可手动取消
			Main.buffNoTimeDisplay[Type] = false;//是否隐藏时间显示
			Main.debuff[Type] = false;//是否为负面Buff
		}
		public override void Update(Player player, ref int buffIndex)
		{
			player.immune = true;
		}
	}
}