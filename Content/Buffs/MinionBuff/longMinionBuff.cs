using BoBo.Content.Projectiles;
using BoBo.Content.Projectiles.Weapons.Summon;
using Terraria;
using Terraria.ModLoader;
namespace BoBo.Content.Buffs.MinionBuff
{
    public class longMinionBuff : ModBuff
    {
        public override string Texture => Pictures.MinionBuff + Name;
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            MinionPlayer modPlayer = player.GetModPlayer<MinionPlayer>();
            if (player.ownedProjectileCounts[ModContent.ProjectileType<LongMinion.Body>()] > 0)
            {
                modPlayer.DragonMinion = true;
            }
            if(!modPlayer.DragonMinion)
            {
                player.DelBuff(buffIndex--);
            }
            else
            {
                player.buffTime[buffIndex] = 18000;
            }
        }
    }
}