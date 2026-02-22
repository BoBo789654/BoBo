using BoBo.Content.Buffs.Good;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace BoBo.Content.Projectiles.Accessories.FightAcc
{
	public class MagicalCircleProj : ModProjectile//AM弹幕 饰品召唤法阵
    {
		public override string Texture => Pictures.FightAccProj + Name;
		public string Texture2 => Pictures.FightAccProj + Name + "2";
		public string Texture3 => Pictures.FightAccProj + Name + "3";
		public override void SetStaticDefaults()
		{
			
		}
		public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 280;//宽高，实际碰撞
            Projectile.DamageType = DamageClass.Generic;//伤害类型
            Projectile.alpha = 0;//透明度，0是完全不透明，255完全透明
            Projectile.timeLeft = 30000;//60帧1秒，注意了，这是倒计时的
            Projectile.penetrate = -1;//穿透次数，-1无限穿
            Projectile.aiStyle = -1;//AI风格，一个数字对应一个原版弹幕，-1自定义
            Projectile.light = 1f;//弹幕发光，白光
            Projectile.tileCollide = false;//穿墙否，true为不穿，false为穿
            Projectile.friendly = true;//友善否
            Projectile.hostile = false;//对玩家造成伤害否
            Projectile.ignoreWater = false;//受水影响否（忽视水）
        }
        Player Player => Main.player[Projectile.owner];
        public override void AI()//自转，然后放弹幕
        {
            if (Player.HasBuff<MagicalCircleBuff>())
                Projectile.timeLeft = 30;
            else
                Projectile.timeLeft = 0;
            Projectile.ai[1]++;
			Lighting.AddLight(Projectile.Center, 1.2f, 1.4f, 2f);
			Projectile.position = Player.position - new Vector2(Projectile.width, Projectile.height) / 2 + new Vector2(Player.width, Player.height) / 2;
            NPC target = null;//目标NPC先默认为空
            if (Player.HasMinionAttackTargetNPC)//右键索敌
            {
                target = Main.npc[Player.MinionAttackTargetNPC];//目标是右键索住的敌人
                float between = Vector2.Distance(target.Center, Projectile.Center);
                if (between < 1500f)//太远的不锁
                {
                    target = null;
                }
            }
            if (target == null || !target.active)//若目标为空或者失活，重新找怪
            {
                int t = Projectile.FindTargetWithLineOfSight(1500);//找这么多像素的
                //这个方法如果在没有敌怪时会返回-1，用来检测是否能找到敌人
                if (t >= 0)
                    target = Main.npc[t];//该NPC为目标
            }
            if (target != null)//若不为空，则执行攻击
            {
				//别攻击，没想好
            }
        }
		public override void OnKill(int timeLeft)
		{
			Lighting.AddLight(Projectile.Center, 0f, 0f, 0f);
		}
		private const float TextureScale = 1f; //贴图放大4倍后，缩放回原尺寸以锐化边缘
		private const float MaxOpacity = 1f;   //最大透明度
		private float rotation;
		private float rotation2;
		private float rotation3;

		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
				SamplerState.PointClamp, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			rotation += 0.0231f;
			rotation2 -= 0.0181f;
			rotation3 -= 0.0451f;
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 origin = texture.Size() / 2f;//旋转中心设为贴图中心
			Vector2 DrawPos = Projectile.Center - Main.screenPosition;
			Color DrawColor = new Color(180, 100, 255) * Projectile.Opacity * MaxOpacity;
			Main.EntitySpriteDraw(texture, DrawPos, null, DrawColor, rotation, 
				origin, Projectile.scale * TextureScale, SpriteEffects.None, 0);
			//Main.EntitySpriteDraw(材质，位置，使用整个贴图，颜色+透明度，旋转角度，旋转中心，缩放，是否翻转，层数)
			Color HighlightColor = new Color(108, 180, 255, (int)(150 * Projectile.Opacity));
			Main.EntitySpriteDraw(texture, DrawPos + new Vector2(0, -2), null, HighlightColor, rotation,
				origin, Projectile.scale * TextureScale * 0.99f, SpriteEffects.None, 0);

			Projectile.rotation += 0.0451f;
			Texture2D texture2 = ModContent.Request<Texture2D>(Texture2).Value;
			Vector2 origin2 = texture2.Size() / 2f;//旋转中心设为贴图中心
			Vector2 DrawPos2 = Projectile.Center - Main.screenPosition;
			Color DrawColor2 = new Color(180, 100, 255) * Projectile.Opacity * MaxOpacity;
			Main.EntitySpriteDraw(texture2, DrawPos2, null, DrawColor2, rotation2,
				origin2, Projectile.scale * TextureScale, SpriteEffects.None, 0);
			//Main.EntitySpriteDraw(材质，位置，使用整个贴图，颜色+透明度，旋转角度，旋转中心，缩放，是否翻转，层数)
			Color HighlightColor2 = new Color(108, 180, 255, (int)(150 * Projectile.Opacity));
			Main.EntitySpriteDraw(texture2, DrawPos2 + new Vector2(0, -2), null, HighlightColor2, rotation2,
				origin2, Projectile.scale * TextureScale * 0.99f, SpriteEffects.None, 0);

			Projectile.rotation += 0.0451f;
			Texture2D texture3 = ModContent.Request<Texture2D>(Texture3).Value;
			Vector2 origin3 = texture3.Size() / 2f;//旋转中心设为贴图中心
			Vector2 DrawPos3 = Projectile.Center - Main.screenPosition;
			Color DrawColor3 = new Color(180, 100, 255) * Projectile.Opacity * MaxOpacity;
			Main.EntitySpriteDraw(texture3, DrawPos3, null, DrawColor3, rotation3,
				origin3, Projectile.scale * TextureScale, SpriteEffects.None, 0);
			//Main.EntitySpriteDraw(材质，位置，使用整个贴图，颜色+透明度，旋转角度，旋转中心，缩放，是否翻转，层数)
			Color HighlightColor3 = new Color(108, 180, 255, (int)(150 * Projectile.Opacity));
			Main.EntitySpriteDraw(texture3, DrawPos3 + new Vector2(0, -2), null, HighlightColor3, rotation3,
				origin3, Projectile.scale * TextureScale * 0.99f, SpriteEffects.None, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
					SamplerState.LinearClamp, DepthStencilState.None,
					RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}
	}
}