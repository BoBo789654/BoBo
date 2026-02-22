using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Weapons.Magic
{
	public class ExplosionProj2 : ModProjectile
	{
		private const int MaxRadius = 150;//最大爆炸半径
		private const int Lifetime = 30;//弹幕存在时间（帧数）
		public override string Texture => Pictures.MagicProj + Name;

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.timeLeft = Lifetime;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = Lifetime;
		}

		public override void AI()
		{
			//仅在第一帧造成伤害
			if (Projectile.timeLeft == Lifetime)
			{
				SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
				ApplyExplosionDamage();
			}
			//更新视觉效果
			Projectile.scale = 1f + (1f - (float)Projectile.timeLeft / Lifetime) * 3f;
			Projectile.alpha = (int)(255 * (1f - (float)Projectile.timeLeft / Lifetime));
		}
		private void ApplyExplosionDamage()
		{
			float radius = MaxRadius * 1f;
			foreach (NPC npc in Main.npc)
			{
				if (npc.active && !npc.friendly &&
					npc.Distance(Projectile.Center) < radius)
				{
					npc.SimpleStrikeNPC(Projectile.damage, 0);
				}
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			float progress = 1f - (float)Projectile.timeLeft / Lifetime;
			Color BaseColor = new Color(255, 200, 100) * (1f - progress);//橙黄白
			BaseColor.A = 0;
			Color CoreColor = Color.Lerp(Color.Yellow, Color.White, progress);//黄白
			CoreColor.A = 0;
			Color GlowColor = Color.Lerp(Color.Yellow, Color.OrangeRed, progress);//黄橙
			GlowColor.A = 0;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, CoreColor * (1f - progress),
				0f, texture.Size() / 2f, MathHelper.Lerp(0.1f, 2f, progress), SpriteEffects.None, 0);//往外扩，中等范围
			if (progress < 0.7f && progress > 0.2f)
				Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, BaseColor * (0.8f - progress * 0.5f),
				0f, texture.Size() / 2f, MathHelper.Lerp(0.3f, 0.8f, 1f - progress), SpriteEffects.None, 0);//往里收
			if (progress < 1f && progress > 0f)
				Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, GlowColor * (0.6f - progress * 0.7f),
					0f, texture.Size() / 2f, MathHelper.Lerp(1.5f, 3.5f, progress), SpriteEffects.None, 0);//往外扩，大范围
			return false;
		}
	}
}