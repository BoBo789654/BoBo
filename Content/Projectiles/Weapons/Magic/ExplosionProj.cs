using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Weapons.Magic
{
	public class ExplosionProj : ModProjectile
	{
		public override string Texture => Pictures.MagicProj + Name;
		private Vector2[] OldPos = new Vector2[16];//拖尾轨迹点数组
		private int FrameTimer = 0;//帧计数器
		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 9;//图片几帧
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2000;//多远不绘制
			ProjectileID.Sets.TrailCacheLength[Type] = 10;//拖尾长度
			ProjectileID.Sets.TrailingMode[Type] = -1;//不要改这个
		}
		public override void SetDefaults()
		{
			Projectile.width = 60;
			Projectile.height = 46;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 600;
			Projectile.aiStyle = -1;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			for (int i = 0; i < OldPos.Length; i++)//初始化拖尾数组
				OldPos[i] = Vector2.Zero;
		}
		public override void AI()
		{
			#region 帧图绘制
			//Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;//速度指向运动方向
			Projectile.velocity.Y += 0.12f;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= 5)//控制动画速度
			{
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
			}
			#endregion
			#region 拖尾
			FrameTimer++;
			if (FrameTimer % 2 == 0)//留下旧位置的频率
			{
			    for (int i = OldPos.Length - 1; i > 0; i--)//移动旧位置点
				{
			        OldPos[i] = OldPos[i - 1];
			    }
			    OldPos[0] = Projectile.Center;//记录当前位置
			}
			#endregion
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
					float opacity = 1f * (1f - 0.05f * i);//透明度，逐渐透明
					float scale = 1f * (1f - 0.02f * i);//缩放，逐渐变小
					float rotation = (OldPos[i - 1] - OldPos[i]).ToRotation() + MathHelper.Pi / 2;//计算旋转方向
					Main.EntitySpriteDraw(tex,OldPos[i] - Main.screenPosition, sourceRect, Color.White * opacity,
						rotation, new Vector2(tex.Width / 2, frameHeight / 2), scale, SpriteEffects.None, 0);
				}
			}
			return false;
		}
		/*public override void PostDraw(Color lightColor)//PostDraw不要了
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			int frameHeight = tex.Height / Main.projFrames[Type];
			int frameY = Projectile.frame * frameHeight;
			Rectangle sourceRect = new Rectangle(0, Projectile.frame * frameHeight, tex.Width, frameHeight);
			for (int i = OldPos.Length - 1; i > 0; i--)
			{
				if (OldPos[i] != Vector2.Zero)
				{
					Main.spriteBatch.Draw(tex, OldPos[i] - Main.screenPosition, sourceRect, Color.White * 1 * (1 - .05f * i), 
						(OldPos[i - 1] - OldPos[i]).ToRotation() + (float)(1 * MathHelper.Pi), tex.Size() * .5f, 1 * (1 - .02f * i),
						SpriteEffects.None, 0);  //如果贴图不在你所期望的方向上，你应该知道你要做什么
																																																																   //试试修改这里的.05f与.02f，想想它们意味着什么
				}
			}
		}*/
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			ExplosionEffect();
			Explode();
		}
		private void ExplosionEffect()//爆炸生成弹幕与粒子效果
		{
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
				ModContent.ProjectileType<ExplosionProj2>(), (int)(Projectile.damage * 0.5f),
				Projectile.knockBack, Projectile.owner);
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
			for (int i = 0; i < 30; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
					DustID.Smoke, 0f, 0f, 100, default, 3f);
				dust.velocity *= 1.4f;
			}
			for (int i = 0; i < 20; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width,Projectile.height,
					DustID.Torch, 0f, 0f, 100, default, 3.5f);
				dust.noGravity = true;
				dust.velocity *= 7f;
				dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
					DustID.Torch, 0f, 0f, 100, default, 1.5f);
				dust.velocity *= 3f;
			}
		}
		private void Explode()//对周围敌人造成伤害
		{
			float ExplosionRadius = 100f;
			foreach (NPC npc in Main.npc)
			{
				if (npc.active && !npc.friendly && npc.Distance(Projectile.Center) < ExplosionRadius)
				{
					int ExplosionDamage = (int)(Projectile.damage * 0.5f);
					npc.SimpleStrikeNPC(ExplosionDamage, 0);
				}
			}
		}
		public override void Kill(int timeLeft)
		{
			ExplosionEffect();
			Explode();
		}
	}
}