using BoBo.Content.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Weapons.Melee
{
	public class XianJian : ModProjectile
	{
		//弹幕状态变量
		private int TextureType;                //贴图类型 (0-3)
		private Vector2 StartPos;               //起始位置
		private float OrbitAngle;               //当前圆轨迹角度
		private float OrbitRadius = 500f;       //圆轨迹半径
		private float OrbitSpeed;               //转圈速度
		private int OrbitDirection;             //转圈方向，1=逆时针，-1=顺时针
		private int ChargePhaseTime = 150;       //冲锋阶段持续时间
		private static readonly Color[] SwordColors = [//颜色数组
			new Color(0, 150, 255),				//蓝色
            new Color(50, 255, 100),			//绿色
            new Color(255, 50, 50),				//红色
            new Color(255, 255, 0)				//黄色
        ];
		private static readonly int[] DustTypes = [15, 61, 90, 204];//粒子数组
		//拖尾参数
		private float TrailWidth = 20f;//拖尾基础宽度
		//拖尾颜色
		private Color TrailColor1 => SwordColors[TextureType];
		private Color TrailColor2 => new Color(TrailColor1.R, TrailColor1.G, TrailColor1.B, 0);
		private NPC TargetNPC => Main.npc[(int)Projectile.ai[0]];//目标NPC
		private float Alpha//透明度
		{
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}
		private float InitialAngle => Projectile.ai[2];//初始角度
		public override string Texture => $"BoBo/Asset/Projectiles/Weapons/Melee/XianJian{TextureType}";
		public override void SetDefaults()
		{
			Projectile.width = Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 300;
			Projectile.aiStyle = -1;
			Projectile.penetrate = 1;
			Projectile.oldPos = new Vector2[20];
			Projectile.oldRot = new float[20];
			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				Projectile.oldPos[i] = Projectile.position;
				Projectile.oldRot[i] = Projectile.rotation;
			}
		}
		public override void OnSpawn(IEntitySource source)
		{
			if (TargetNPC == null || !TargetNPC.active)
			{
				Projectile.Kill();//无效目标则立即销毁
				return;
			}
			TextureType = Main.rand.Next(0, 4);//随机选择剑的类型 (0-3)
			Alpha = 0f;//设置初始透明度
			OrbitAngle = InitialAngle;//初始化圆轨迹角度
			OrbitSpeed = Main.rand.NextFloat(1.0f, 4.0f);//随机转圈速度
			OrbitDirection = Main.rand.Next(2) == 0 ? 1 : -1;//随机转圈方向
			Vector2 direction = new Vector2(//计算圆轨迹上的起始位置
				(float)Math.Cos(OrbitAngle),
				(float)Math.Sin(OrbitAngle)
			);//公式：目标中心 + 半径 * (cos(角度), sin(角度))
			StartPos = TargetNPC.Center + direction * OrbitRadius;
			Projectile.Center = StartPos;//设置初始位置和旋转
			Projectile.rotation = (TargetNPC.Center - StartPos).ToRotation() + MathHelper.PiOver4;
		}
		public override void AI()
		{
			if (!TargetNPC.active || TargetNPC.life <= 0)
			{
				Projectile.Kill();
				return;
			}
			UpdateTrailHistory();//更新拖尾历史位置
			//分阶段
			if (Projectile.timeLeft > 180)//沿圆轨迹运动
				HandleOrbitPhase();
			else if (Projectile.timeLeft > 150)//冲向目标
			{
				HandleChargePhase(); 
				ChargePhaseTime--;
			}
			else//渐隐消失
				HandleFadePhase();
			SpawnVisualParticles();//生成视觉特效粒子
		}
		private void UpdateTrailHistory()
		{
			if (Projectile.oldPos == null || Projectile.oldPos.Length == 0) return;
			for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
				Projectile.oldPos[i] = Projectile.oldPos[i - 1];
			Projectile.oldPos[0] = Projectile.Center;
			for (int i = Projectile.oldRot.Length - 1; i > 0; i--)
				Projectile.oldRot[i] = Projectile.oldRot[i - 1];
			Projectile.oldRot[0] = Projectile.rotation;
		}
		private void HandleOrbitPhase()//圆轨迹阶段
		{
			Alpha = Math.Min(Alpha + 0.02f, 1f);//增加透明度
			OrbitAngle += MathHelper.ToRadians(OrbitSpeed) * OrbitDirection;//使用随机方向和随机速度增加圆轨迹角度
			Vector2 direction = new Vector2((float)Math.Cos(OrbitAngle), (float)Math.Sin(OrbitAngle));//计算圆轨迹上的新位置
			Vector2 newPosition = TargetNPC.Center + direction * OrbitRadius;
			Projectile.Center = Vector2.Lerp(Projectile.Center, newPosition, 0.1f);//平滑移动到新位置
			Vector2 toTarget = TargetNPC.Center - Projectile.Center;//设置旋转方向指向目标
			Projectile.rotation = toTarget.ToRotation() + MathHelper.PiOver4;
			OrbitRadius = MathHelper.Lerp(OrbitRadius, OrbitRadius - 200, 0.005f);//减小圆轨迹半径，逐渐靠近目标
			//根据转圈速度调整拖尾宽度：转得越快，拖尾越长
			float speedFactor = OrbitSpeed / 4.0f;//归一化速度
			float targetTrailWidth = 20f + 10f * speedFactor;//速度增加
			TrailWidth = MathHelper.Lerp(TrailWidth, targetTrailWidth, 0.05f);
		}
		private void HandleChargePhase()//冲锋阶段
		{
			float timeRatio = ChargePhaseTime / 150f;
			float waveProgress = (float)Math.Sin(timeRatio * MathHelper.Pi * 2f) * 0.5f + 1f;//计算冲锋进度
			Projectile.Center = Vector2.Lerp(Projectile.Center, TargetNPC.Center, waveProgress * 0.12f);//使用缓动函数使运动更平滑
																										//float progress = 1f - (ChargePhaseTime - 60) / 60f;//计算冲锋进度
																										//float EasedProgress = 1f - (float)Math.Pow(1f - progress, 3);//使用缓动函数使运动更平滑
																										//Projectile.Center = Vector2.Lerp(Projectile.Center, TargetNPC.Center, EasedProgress * 0.04f);//向目标位置移动
			Vector2 toTarget = TargetNPC.Center - Projectile.Center;//保持指向目标的旋转
			Projectile.rotation = toTarget.ToRotation() + MathHelper.PiOver4;
			TrailWidth = MathHelper.Lerp(TrailWidth, 40f, 0.1f);//增加拖尾宽度
		}
		private void HandleFadePhase()//渐隐阶段
		{
			Projectile.Center = TargetNPC.Center;//停留在目标位置
			Alpha = Math.Max(Alpha - 0.02f, 0f);//减少透明度
			TrailWidth = MathHelper.Lerp(TrailWidth, 5f, 0.1f);//减少拖尾宽度
		}
		
		private void SpawnVisualParticles()//生成粒子
		{
			if (Main.rand.NextBool(5))//概率生成粒子
			{
				int dustType = DustTypes[TextureType];
				Color color = SwordColors[TextureType];
				
				Vector2 velocityDirection = (TargetNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero);//根据转圈方向调整粒子速度方向
				
				if (OrbitDirection == -1)//如果是顺时针转圈，粒子方向也顺时针偏转
					velocityDirection = velocityDirection.RotatedBy(MathHelper.PiOver2 * 0.3f);
				else//如果是逆时针转圈，粒子方向也逆时针偏转
					velocityDirection = velocityDirection.RotatedBy(-MathHelper.PiOver2 * 0.3f);
				Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
					dustType, velocityDirection * -2f * (OrbitSpeed / 2.5f),//匹配，速度越快，粒子速度越快
					100, color,  Main.rand.NextFloat(0.8f, 1.5f)
				);
				dust.noGravity = true;
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			DrawTrail();//绘制拖尾
			DrawProjectileBody();//绘制弹幕本体
			return false;
		}
		private void DrawTrail()//绘制拖尾特效
		{
			if (Alpha <= 0.01f) return;
			if (Projectile.oldPos == null || Projectile.oldPos.Length < 2) return;
			Texture2D texture = ModContent.Request<Texture2D>($"BoBo/Asset/Projectiles/Weapons/Melee/XianJian{TextureType}",
				ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			if (texture == null || texture.IsDisposed) return;
			//创建顶点列表
			List<CustomVertexInfo> vertices = new List<CustomVertexInfo>();
			//遍历历史位置创建拖尾
			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] == Vector2.Zero) break;
				//计算拖尾参数
				float factor = i / (float)Projectile.oldPos.Length;
				float alpha = Alpha * (1f - factor) * 0.7f;
				//根据转圈速度调整拖尾长度：速度越快，拖尾越长
				float speedFactor = OrbitSpeed / 4.0f;
				float width = TrailWidth * (1f - factor) * Projectile.scale * (1.0f + speedFactor * 0.5f);
				//计算颜色
				Color color = Color.Lerp(TrailColor1, TrailColor2, factor);
				color *= alpha;
				//获取当前位置和旋转
				Vector2 currentPos = Projectile.oldPos[i];
				float currentRotation = Projectile.rotation;
				if (i < Projectile.oldRot.Length)
					currentRotation = Projectile.oldRot[i];
				//计算拖尾的两个边缘点
				float cos = (float)Math.Cos(currentRotation + MathHelper.PiOver2);
				float sin = (float)Math.Sin(currentRotation + MathHelper.PiOver2);
				Vector2 perpendicular = new Vector2(cos, sin);
				Vector2 pos1 = currentPos + perpendicular * width;
				Vector2 pos2 = currentPos - perpendicular * width;
				//转换为屏幕坐标并添加顶点
				vertices.Add(new CustomVertexInfo(pos1 - Main.screenPosition, color, new Vector3(factor, 1, 1)));
				vertices.Add(new CustomVertexInfo(pos2 - Main.screenPosition, color, new Vector3(factor, 0, 1)));
			}
			if (vertices.Count >= 4)//如果有足够顶点则绘制
				DrawTrailPrimitives(vertices, texture);
		}
		private void DrawTrailPrimitives(List<CustomVertexInfo> vertices, Texture2D texture)//绘制拖尾基元
		{
			Main.spriteBatch.End();//保存原始SpriteBatch状态
			Main.spriteBatch.Begin(
				SpriteSortMode.Immediate,
				BlendState.Additive,
				SamplerState.LinearClamp,
				DepthStencilState.None,
				RasterizerState.CullNone,
				null,
				Main.GameViewMatrix.TransformationMatrix
			);//使用加法混合绘制拖尾
			try
			{
				var shader = Terraria.Graphics.Effects.Filters.Scene["MagicMissile"]?.GetShader();//应用MagicMissile着色器
				if (shader != null)
				{
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
					var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));

					shader.Shader.Parameters["transformMatrix"]?.SetValue(model * projection);
					shader.Apply();
				}
				if (Main.graphics.GraphicsDevice != null && !Main.graphics.GraphicsDevice.IsDisposed)//绘制拖尾
				{
					Main.graphics.GraphicsDevice.Textures[0] = texture;
					Main.graphics.GraphicsDevice.DrawUserPrimitives(
						PrimitiveType.TriangleStrip,
						vertices.ToArray(),
						0,
						Math.Max(0, vertices.Count - 2)
					);
				}
			}
			finally
			{
				//恢复原始SpriteBatch状态
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(
					SpriteSortMode.Deferred,
					BlendState.AlphaBlend,
					Main.DefaultSamplerState,
					DepthStencilState.None,
					Main.Rasterizer,
					null,
					Main.GameViewMatrix.TransformationMatrix
				);
			}
		}
		private void DrawProjectileBody()//绘制弹幕本体
		{
			Texture2D texture = ModContent.Request<Texture2D>($"BoBo/Asset/Projectiles/Weapons/Melee/XianJian{TextureType}").Value;
			if (texture == null || texture.IsDisposed) return;
			Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;
			Main.EntitySpriteDraw(texture, drawPosition, null, Color.White * Alpha, 
				Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target != TargetNPC) return;//只对目标NPC有效
			Projectile.velocity = Vector2.Zero;//停止移动
			Projectile.damage = 0;//避免多次伤害
			if (Main.netMode != NetmodeID.Server)//如果不在服务器端，生成击中粒子
				CreateHitParticles(target);
		}
		private void CreateHitParticles(NPC target)//创建击中粒子特效
		{
			Color swordColor = SwordColors[TextureType];
			int dustType = DustTypes[TextureType];
			for (int i = 0; i < 12; i++)//生成爆炸粒子
			{
				Dust dust = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(20f, 20f),
					dustType, Main.rand.NextVector2Circular(6f, 6f), 100, swordColor, Main.rand.NextFloat(1.2f, 2f));
				dust.noGravity = true;
			}
			Vector2 direction = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
			for (int i = 0; i < 8; i++)//生成轨迹粒子
			{
				Dust.NewDustPerfect(target.Center - direction * (i * 10f), dustType, -direction * 5f + Main.rand.NextVector2Circular(3f, 3f),
					newColor: Color.Lerp(swordColor, Color.Cyan, 0.5f), Scale: Main.rand.NextFloat(1f, 1.5f)).noGravity = true;
			}
		}
		#region 联机同步
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(TextureType);
			writer.WriteVector2(StartPos);
			writer.Write(OrbitAngle);
			writer.Write(OrbitSpeed);
			writer.Write(OrbitDirection);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			TextureType = reader.ReadInt32();
			StartPos = reader.ReadVector2();
			OrbitAngle = reader.ReadSingle();
			OrbitSpeed = reader.ReadSingle();
			OrbitDirection = reader.ReadInt32();
		}
		#endregion
	}
}