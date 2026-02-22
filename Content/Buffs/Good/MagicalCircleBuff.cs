using BoBo.Content.Projectiles.Accessories.FightAcc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace BoBo.Content.Buffs.Good
{
    public class MagicalCircleBuff : ModBuff//法阵Buff
    {
		public override string Texture => Pictures.Good + Name;
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;//为true时就是debuff
            Main.buffNoSave[Type] = false;//为true时退出世界时buff消失
            Main.buffNoTimeDisplay[Type] = true;//为true时不显示剩余时间
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;//为debuff不可被护士去除
            BuffID.Sets.LongerExpertDebuff[Type] = false;//为专家模式Debuff持续时间是否延长
        }
        public override void Update(Player player, ref int buffIndex)
        {   
            if (player.ownedProjectileCounts[ModContent.ProjectileType<MagicalCircleProj>()] != 1)//召唤物数量检测，只召唤1个
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


