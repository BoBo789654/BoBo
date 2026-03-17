using BoBo.Content.Common;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace BoBo.Content.Accessories.CardAcc
{
    public class Card_6 : CardItem//
    {
        public override string Texture => Pictures.CardAcc + Name;
        public override string CardType => "NormalCard";
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 60;
            Item.accessory = true;
            Item.rare = ItemRarityID.Master;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(999, 0, 0, 0);
        }
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<TrapImmunityPlayer>().TrapImmunity = true;
		}
	}

	public class TrapImmunityPlayer : ModPlayer
	{
		public bool TrapImmunity;
		public override void ResetEffects() => TrapImmunity = false;
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (TrapImmunity && IsTrapDamage(modifiers.DamageSource))
			{
				modifiers.FinalDamage *= 0f;//伤害归1
				modifiers.DisableSound();//取消受伤音效
				modifiers.DisableDust();//取消受伤粒子
			}
		}
		private bool IsTrapDamage(PlayerDeathReason source)
		{
			if (
				source.SourceProjectileType == ProjectileID.PoisonDart ||//毒镖	
				source.SourceProjectileType == ProjectileID.PoisonDartTrap ||//超级飞镖机关
				source.SourceProjectileType == ProjectileID.GeyserTrap ||//热喷泉
				source.SourceProjectileType == ProjectileID.VenomDartTrap ||//毒液飞镖
				source.SourceProjectileType == ProjectileID.SpikyBallTrap ||//尖球
				source.SourceProjectileType == ProjectileID.SpearTrap ||//长矛
				source.SourceProjectileType == ProjectileID.FlamesTrap ||//火焰
				source.SourceProjectileType == ProjectileID.FlamethrowerTrap ||//火焰喷射器
				source.SourceProjectileType == ProjectileID.Landmine ||//地雷		
				source.SourceProjectileType == ProjectileID.Explosives ||//炸药
				source.SourceProjectileType == ProjectileID.TNTBarrel ||//TNT桶
				source.SourceProjectileType == ProjectileID.RollingCactus ||//仙人球
				source.SourceProjectileType == ProjectileID.RollingCactusSpike ||//仙人球尖刺
				source.SourceProjectileType == ProjectileID.MiniBoulder ||//For the worthy 世界中的巨石
				source.SourceProjectileType == ProjectileID.BouncyBoulder ||//弹力巨石
				source.SourceProjectileType == ProjectileID.Boulder ||//滚动中的巨石
				source.SourceProjectileType == ProjectileID.LifeCrystalBoulder ||//生命水晶巨石
				source.SourceProjectileType == ProjectileID.MoonBoulder ||//在 Get fixed boi 世界中的月亮领主
				source.SourceProjectileType == ProjectileID.GasTrap ||//毒气机关
				source.SourceProjectileType == ProjectileID.BeeHive ||//蜂巢
				source.SourceProjectileType == ProjectileID.EbonsandBallFalling ||//黑檀沙（下落中）
				source.SourceProjectileType == ProjectileID.PearlSandBallFalling ||//珍珠沙（下落中）
				source.SourceProjectileType == ProjectileID.SiltBall ||//泥沙（下落中）
				source.SourceProjectileType == ProjectileID.SlushBall ||//雪泥（下落中）
				source.SourceProjectileType == ProjectileID.CrimsandBallFalling ||//猩红沙（下落中）
				source.SourceProjectileType == ProjectileID.ShellPileFalling ||//贝壳堆（下落中）
				source.SourceProjectileType == ProjectileID.SandBallFalling ||//沙（掉落中）、蚁狮所发射的沙
				source.SourceProjectileType == ProjectileID.FallingStar//坠落之星
				)
					return true;
			return false;
		}
	}
}