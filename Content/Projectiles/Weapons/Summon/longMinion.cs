using BoBo.Content.Buffs.MinionBuff;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Weapons.Summon
{
    public class longMinion : SimulateStarDustDragon
    {
        public override float Offset => 0.58f;
        public override bool ModPlayerMinionBool
        {
            get => Player.GetModPlayer<MinionPlayer>().DragonMinion;
            set => Player.GetModPlayer<MinionPlayer>().DragonMinion = value;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
        }
        public override double ModifyDamageMult(int count)
        {
            return base.ModifyDamageMult(count);
        }
        public override void ActionAI()
        {
            base.ActionAI();
        }
    }
    public abstract class LongMinion : ModProjectile
    {
        public class Head : LongMinion { public override string Texture => Pictures.SummonProj + "longMinionHead"; }
        public class Body : LongMinion { public override string Texture => Pictures.SummonProj + "longMinionBody"; }
        public class Tail : LongMinion { public override string Texture => Pictures.SummonProj + "longMinionTail"; }
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.netImportant = true;
            Projectile.alpha = 0;
            Projectile.light = 255f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SimulateStarDustDragon.DrawSet(Main.spriteBatch, Projectile, ModContent.ProjectileType<Body>(), 2, lightColor, 0);
            return false;
        }
        public override bool? CanDamage()
        {
            return false;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.AddBuff(ModContent.BuffType<longMinionBuff>(), 2);//给玩家上对应buff
            if (Projectile.ai[1] == 1)
            {
                Projectile.netUpdate = true;
                Projectile.ai[1] = 0;
            }
        }
    }
}