using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Weapons.Ranged
{
    public class TerrorArrow : ModProjectile
	{
		public override string Texture => Pictures.RangedProj + Name;
		private Vector2[] OldPos = new Vector2[8];//拖尾轨迹点数组
        private int FrameTimer = 0;//帧计数器
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 60;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.arrow = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 340;
            Projectile.penetrate = -1;
            Projectile.light = 1f;
            Projectile.extraUpdates = 1;
        }
        private int Timer = 0;
        private const int SplitTime = 60;
        public override void AI()
        {
            if (Projectile.timeLeft < 300)
            {
                Projectile.tileCollide = true;
            }
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Timer >= SplitTime)
            {
                SplitArrows();
                Projectile.Kill();
            }
            FrameTimer++;
            if (FrameTimer % 1 == 0)//留下旧位置的频率
            {
                for (int i = OldPos.Length - 1; i > 0; i--)//移动旧位置点
                {
                    OldPos[i] = OldPos[i - 1];
                }
                OldPos[0] = Projectile.Center;//记录当前位置
            }
        }

        private void SplitArrows()
        {
            SoundEngine.PlaySound(SoundID.Item109, Projectile.position);
            int Count = Main.rand.Next(2, 4);
            Vector2 Velocity = Projectile.velocity.SafeNormalize(Vector2.Zero);
            for (int i = 0; i < Count; i++)
            {
                float angle = MathHelper.TwoPi * i / Count;
                Vector2 newVelocity = Velocity.RotatedBy(angle * 0.888f + Main.rand.NextFloat(-0.6f, 0.6f)) * 6f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, newVelocity,
                    ModContent.ProjectileType<TerrorArrow2>(), (int)(Projectile.damage * 1f), Projectile.knockBack, Projectile.owner);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            int frameY = Projectile.frame * frameHeight;
            Rectangle sourceRect = new Rectangle(0, Projectile.frame * frameHeight, tex.Width, frameHeight);
            for (int i = OldPos.Length - 1; i > 0; i--)//拖尾效果
            {
                if (OldPos[i] != Vector2.Zero)
                {
                    float opacity = 1f * (1f - 0.0625f * i);//透明度，逐渐透明
                    float scale = 1f * (1f - 0.0625f * i);//缩放，逐渐变小
                    float rotation = (OldPos[i - 1] - OldPos[i]).ToRotation() + MathHelper.Pi / 2;//计算旋转方向
                    Main.EntitySpriteDraw(tex, OldPos[i] - Main.screenPosition, sourceRect, Color.White * opacity,
                        rotation, new Vector2(tex.Width / 2, frameHeight / 2), scale, SpriteEffects.None, 0);
                }
            }
            return false;
        }
    }
}