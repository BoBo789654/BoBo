using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.NPCs.EvilWorm.EvilWormProj
{
	public class ToxicSplash : ModProjectile
	{
		public override string Texture => Pictures.EvilWormProj + Name;
		private Vector2[] oldPositions = new Vector2[8];//拖尾轨迹点数组
		private int frameTimer;//帧计数器
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 4;
		}
		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 150;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.light = 1.5f;
			for (int i = 0; i < oldPositions.Length; i++) oldPositions[i] = Vector2.Zero;
		}
		public override void AI()
		{
			if (Projectile.timeLeft == 150)
				Projectile.ai[1] = Projectile.velocity.X;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;//旋转效果					
			Projectile.velocity.Y += 0.05f;//重力效果
			Projectile.velocity *= 1.02f;//减速效果
			if (Projectile.velocity.X > 5f * Projectile.ai[1])
				Projectile.velocity.X = Projectile.ai[1];
			if (Main.rand.NextBool(3))//粒子效果
			{
				int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ToxicBubble, 0f, 0f, 100, default, 1f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 0.3f;
			}
			frameTimer++;
			if (frameTimer % 1 == 0)//每1帧记录一次位置
			{
				for (int i = oldPositions.Length - 1; i > 0; i--)
					oldPositions[i] = oldPositions[i - 1];
				oldPositions[0] = Projectile.Center;
			}
			if (++Projectile.frameCounter >= Main.projFrames[Projectile.type] + 1)
			{
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Projectile.type])
					Projectile.frame = 0;
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info)//对玩家施加中毒debuff
		{
			target.AddBuff(BuffID.Poisoned, 300);//5秒中毒
			target.AddBuff(BuffID.Venom, 180);//3秒剧毒
			SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.position);//播放击中音效
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)//友方NPC施加中毒debuff
		{
			target.AddBuff(BuffID.Poisoned, 300);//5秒中毒
			target.AddBuff(BuffID.Venom, 180);//3秒剧毒
			SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.position);//播放击中音效
		}
		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.position);//死亡时播放音效
			for (int i = 0; i < 10; i++)//创建爆炸粒子
			{
				int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ToxicBubble,0f, 0f, 100, default, 1.5f);
				Main.dust[dust].velocity *= 1.4f;
			}
			for (int i = 0; i < 5; i++)//创建毒液溅射
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)//可以创建小的毒液弹幕
				{
					Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
					Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, velocity,
						ModContent.ProjectileType<VenomSplash>(), Projectile.damage / 4, 0f, Projectile.owner);
				}
			}
		}
		public override bool PreDraw(ref Color lightColor)//残影拖尾
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle sourceRectangle = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(texture.Width / 2, frameHeight / 2);
			for (int i = oldPositions.Length - 1; i > 0; i--)
			{
				if (oldPositions[i] != Vector2.Zero)
				{
					float opacity = 1f * (1f - 0.1f * i);//透明度逐渐降低
					float scale = 1f * (1f - 0.1f * i);//缩放逐渐变小
					Color trailColor = Color.White * opacity;
					Main.EntitySpriteDraw(texture, oldPositions[i] - Main.screenPosition, sourceRectangle,
						trailColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0);
				}
			}
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRectangle,
				Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);//绘制本体
			return false;
		}
	}
	//毒液溅射的子弹幕
	public class VenomSplash : ModProjectile
	{
		public override string Texture => Pictures.EvilWormProj + Name;
		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.timeLeft = 600;
			Projectile.tileCollide = true;
			Projectile.alpha = 100;
		}
		public override void AI()
		{
			Projectile.velocity.Y += 0.3f;//更快的下落
			Projectile.velocity *= 0.96f;//更快的减速
			if (Main.rand.NextBool(2))//粒子效果
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ToxicBubble, 0f, 0f, 100, default, 0.8f);
			}
		}
	}
}