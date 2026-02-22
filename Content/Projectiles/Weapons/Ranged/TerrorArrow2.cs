using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Weapons.Ranged
{
    public class TerrorArrow2 : ModProjectile
    {
        public override string Texture => Pictures.RangedProj + Name;
        private Vector2[] OldPos = new Vector2[8];//拖尾轨迹点数组
        private int FrameTimer = 0;//帧计数器
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 40;
            Projectile.tileCollide = true;
            Projectile.friendly = true;
            Projectile.arrow = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 180;
            Projectile.penetrate = 1;
            Projectile.light = 1f;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Projectile.velocity *= 1.0001f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            NPC target = FindTarget();
            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                Vector2 Direction = toTarget.SafeNormalize(Vector2.Zero);
                float Speed = Projectile.velocity.Length();
                Vector2 newDirection = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), Direction, Strength);
                Projectile.velocity = newDirection * Speed;
                if (Vector2.Dot(Projectile.velocity.SafeNormalize(Vector2.Zero), Direction) < -0.8f)
                {
                    Projectile.velocity = Direction * (Speed * 1.5f);
                }
            }
            if (Projectile.timeLeft > 175)
            {
                Projectile.velocity *= 1.05f;
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
        private const float Range = 600f;
        private const float Strength = 0.2f;
        private NPC FindTarget()
        {
            NPC target = null;
            float minDistance = Range;
            Vector2 Pos = Projectile.Center;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy(Projectile) && npc.Distance(Pos) < minDistance)
                {
                    minDistance = npc.Distance(Pos);
                    target = npc;
                }
            }
            return target;
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