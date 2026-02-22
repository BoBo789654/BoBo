using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Accessories.FightAcc
{
    internal class ThunderAccProj : ModProjectile//闪电弹幕
    {
        public override string Texture => "Terraria/Images/Item_0";
        public float rot
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public float length
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public int times = 15;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;

        }
      
        public override void SetDefaults()
        {
            Projectile.width = 5;
            Projectile.height = 5;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.timeLeft = 15;
            Projectile.penetrate = -1;
            Projectile.alpha = 1;
            Projectile.light = 0f;
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 start = Projectile.Center;
            float l = length / times;
            float tK = (Projectile.timeLeft + 1) / 60f;
            for (int i = 1; i <= times; i++)
            {
                Vector2 end = Projectile.Center + (rot + Main.rand.NextFloat(-0.1f, 0.1f)).ToRotationVector2() * l * i;
                if (i == times)
                {
                    end = Projectile.Center + rot.ToRotationVector2() * length;
                }
                DrawLine(Main.spriteBatch, TextureAssets.MagicPixel.Value, new Rectangle(0, 0, 6, 6), new Vector2(7, 7),
                    start - Main.screenPosition, end - Main.screenPosition, 40, tK * (4f - i * 0.2f), tK * (4f - (i + 1) * 0.2f), new Color(160, 60, 255));
                start = end;
            }
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            _ = Main.player[Projectile.owner];
            Vector2 unit = rot.ToRotationVector2();
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
                Projectile.Center + unit * length, 30, ref point);
        }
        private void DrawLine(SpriteBatch sp, Texture2D tex, Rectangle rect, Vector2 origin, Vector2 startPos, Vector2 endPos, int DrawTimes, float startScale, float endScale, Color color)
        {
            Vector2 unit = Vector2.Normalize(endPos - startPos) * 3;
            _ = (int)((endPos - startPos).Length() / unit.Length());
            for (int i = 0; i < DrawTimes; i++)
            {
                float k = i / (float)DrawTimes;
                float size = startScale - (startScale - endScale) * k;
                sp.Draw(tex, Vector2.Lerp(startPos, endPos, k), rect, color, 0, origin, size, SpriteEffects.None, 0);
            }
        }
        public override void AI()
        {
            length -= 1;
            base.AI();
        }
    }
}