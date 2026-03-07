using BoBo.Content.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Weapons.Magic
{
	public class DarkStarProj : ModProjectile
	{
		public override string Texture => Pictures.MagicProj + Name;
		//用于绘制拖尾的数据结构
		public Vector2[] oldPos = new Vector2[60]; //存储历史位置，用于生成拖尾
		public Color[] oldColor = new Color[60];   //存储历史颜色
		public float[] oldRot = new float[60];     //存储历史旋转
		private struct TrailLayer
		{
			public int TextureID;           //材质ID: 195, 196, 197 等
			public float StartWidth;        //起点宽度
			public float EndWidth;          //终点宽度
			public Color BaseColor;         //基础颜色
			public float LengthFactor;      //长度因子 (0.0-1.0)，控制使用多少比例的历史点
			public float Opacity;           //整体不透明度
			public TrailLayer(int texID, float startW, float endW, Color color, float lengthFactor = 1.0f, float opacity = 1.0f)
			{
				TextureID = texID;
				StartWidth = startW;
				EndWidth = endW;
				BaseColor = color;
				LengthFactor = MathHelper.Clamp(lengthFactor, 0.1f, 1.0f);
				Opacity = MathHelper.Clamp(opacity, 0.0f, 1.0f);
			}
		}
		//定义三个独立的材质层，可以在这里调整每个层的属性
		private TrailLayer[] trailLayers = new TrailLayer[]
		{
			new TrailLayer(
				texID: 195,								//材质
				startW: 16f,							//起点宽度
				endW: 5f,								//终点宽度
				color: new Color(128, 0, 255, 150),		//颜色
				lengthFactor: 0.3f,						//使用的历史点，长度
				opacity: 0.7f
			),//第一层: Extra[195]
			new TrailLayer(
				texID: 196,								//材质								
				startW: 60f,							//起点宽度
				endW: 3f,								//终点宽度
				color: new Color(180, 0, 255, 180),		//颜色
				lengthFactor: 0.9f,						//使用的历史点，长度
				opacity: 0.9f
			),//第二层: Extra[196]
			new TrailLayer(	
				texID: 197,								//材质
				startW: 30f,							//起点宽度
				endW: 2f,								//终点宽度
				color: new Color(220, 50, 255, 110),	//颜色
				lengthFactor: 1.0f,						//使用的历史点，长度
				opacity: 1.0f
			)//第三层: Extra[197]
		};
		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 600;
			Projectile.alpha = 50;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.aiStyle = 9;
			Projectile.light = 0.5f;
			for (int i = 0; i < oldPos.Length; i++)//初始化历史位置数组，避免绘制拖尾时出现 Vector2.Zero 导致的断裂
				oldPos[i] = Vector2.Zero;
		}
		public override void AI()
		{
			base.AI();//调用原版AI
			//这部分为拖尾效果准备数据，更新历史位置和旋转数组，实现"队列"效果（最新的位置放在数组开头）
			for (int i = oldPos.Length - 1; i > 0; i--)
			{
				oldPos[i] = oldPos[i - 1];
				oldRot[i] = oldRot[i - 1];
			}
			oldPos[0] = Projectile.Center; //记录当前位置
			oldRot[0] = Projectile.rotation;//记录当前旋转
		}
		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
			Projectile.NewProjectile(
				Projectile.GetSource_FromThis(),					//弹幕来源
				Projectile.Center,									//生成位置（当前弹幕死亡的中心）
				Vector2.Zero,										//速度向量
				ModContent.ProjectileType<DarkStarExplosionProj>(),	//弹幕类型（使用自身类型，生成同种弹幕）
				(int)(Projectile.damage * 0.5f),					//伤害（例如设为原伤害的一半）
				Projectile.knockBack,								//击退
				Projectile.owner,									//所有者（玩家索引）
				0f,													//ai参数1
				0f													//ai参数2
			);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
			//绘制弹幕主体
			Color mainColor = GetCurrentRainbowColor();
			mainColor.A = 200;
			//添加发光效果
			for (int i = 0; i < 3; i++)
			{
				float scale = Projectile.scale * (1.1f + i * 0.1f);
				Color glowColor = mainColor * (0.7f - i * 0.2f);
				glowColor.A = (byte)(mainColor.A * 0.5f);
				Main.EntitySpriteDraw(
					texture,
					Projectile.Center - Main.screenPosition,
					null,
					glowColor,
					Projectile.rotation,
					drawOrigin,
					scale * (Projectile.ai[0] == 0f ? 1.3f : 1f),//手持时更大
					SpriteEffects.None,
					0
				);
			}
			//主体核心
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				mainColor,
				Projectile.rotation,
				drawOrigin,
				Projectile.scale * (Projectile.ai[0] == 0f ? 1.3f : 1f),
				SpriteEffects.None,
				0
			);
			return false;
		}
		//获取当前颜色
		private Color GetCurrentRainbowColor()
		{
			float hue = (Main.GlobalTimeWrappedHourly * 5f + Projectile.whoAmI * 0.1f) % 1f;
			return Main.hslToRgb(hue, 1f, 0.2f);
		}
		public override void PostDraw(Color lightColor)
		{
			//结束原版绘制批次
			Main.spriteBatch.End();
			//开启叠加混合模式的绘制批次
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap,
				DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			//计算实际可用的历史位置总数
			int totalValidLength = 0;
			for (int i = 0; i < oldPos.Length; i++)
			{
				if (oldPos[i] == Vector2.Zero) break;
				totalValidLength++;
			}
			//如果历史点不足2个，无法绘制任何拖尾
			if (totalValidLength < 2)
			{
				//恢复原版绘制状态
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
					Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null,
					Main.GameViewMatrix.TransformationMatrix);
				return;
			}

			// 计算弹幕的当前运动方向，用于确定拖尾起点的方向
			Vector2 projectileDirection = Vector2.Zero;
			if (Projectile.velocity != Vector2.Zero)
			{
				projectileDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
			}
			else if (totalValidLength > 1)
			{
				// 如果弹幕没有速度，使用历史位置计算方向
				projectileDirection = (oldPos[0] - oldPos[1]).SafeNormalize(Vector2.UnitX);
			}
			else
			{
				// 如果没有任何历史位置，使用默认方向
				projectileDirection = Vector2.UnitX;
			}

			//为每个材质层独立生成和绘制拖尾
			foreach (TrailLayer layer in trailLayers)
			{
				//计算该层的实际有效长度（考虑LengthFactor）
				int layerValidLength = (int)(totalValidLength * layer.LengthFactor);
				if (layerValidLength < 2) layerValidLength = 2; //至少需要2个点

				//为该层生成顶点列表
				List<CustomVertexInfo> vertices = new List<CustomVertexInfo>();
				for (int i = 0; i < layerValidLength; i++)
				{
					Vector2 pos = oldPos[i];
					if (pos == Vector2.Zero) break;

					//计算插值因子（0在拖尾起点，1在拖尾末端）
					float factor = i / (float)(layerValidLength - 1);

					//计算该层的颜色（考虑淡出效果和层透明度）
					Color color = layer.BaseColor;
					color *= (1.2f - factor); //末端淡出效果
					color *= layer.Opacity;   //层整体透明度

					//计算该点的法向量方向
					Vector2 dir = Vector2.Zero;
					if (i == 0)
					{
						//起点：使用弹幕的当前运动方向
						dir = projectileDirection;
					}
					else if (i == layerValidLength - 1)
					{
						//终点：用最后一个线段的方向
						int prevIndex = layerValidLength - 2;
						dir = (prevIndex >= 0 && prevIndex < totalValidLength) ?
							(oldPos[prevIndex] - oldPos[layerValidLength - 1]) : Vector2.UnitX;
					}
					else
					{
						//中间点：前后方向的平均值
						int prevIndex = i - 1;
						int nextIndex = i + 1;
						if (nextIndex < totalValidLength)
						{
							dir = (oldPos[prevIndex] - oldPos[nextIndex]) / 2f;
						}
						else
						{
							dir = (oldPos[prevIndex] - oldPos[i]) / 2f;
						}
					}

					//归一化方向向量并计算法向量
					if (dir != Vector2.Zero) dir.Normalize();
					Vector2 normal = new Vector2(-dir.Y, dir.X);

					//计算该层的宽度（使用该层独立的宽度参数）
					// 对起点特殊处理：让起点宽度更小，这样拖尾会从弹幕中心"长出"
					float width = MathHelper.Lerp(layer.StartWidth, layer.EndWidth, factor) * Projectile.scale;

					// 对起点(i==0)使用更小的宽度，使其从弹幕中心开始
					if (i == 0)
					{
						width = MathHelper.Lerp(0.5f, layer.EndWidth, factor) * Projectile.scale;
					}

					//世界坐标转屏幕坐标
					Vector2 screenPos = pos - Main.screenPosition;
					float texCoordX = factor;
					float widthFactor = MathHelper.Lerp(1f, 0.1f, factor);

					//为该点添加左右两个顶点
					vertices.Add(new CustomVertexInfo(screenPos + normal * width, color, new Vector3(texCoordX, 1, widthFactor))); //右侧
					vertices.Add(new CustomVertexInfo(screenPos - normal * width, color, new Vector3(texCoordX, 0, widthFactor))); //左侧
				}

				//为该层连接顶点形成三角形列表
				List<CustomVertexInfo> triangleList = new List<CustomVertexInfo>();
				if (vertices.Count >= 4)
				{
					for (int i = 0; i < vertices.Count - 2; i += 2)
					{
						//构成一个四边形（两个三角形）
						triangleList.Add(vertices[i]);
						triangleList.Add(vertices[i + 1]);
						triangleList.Add(vertices[i + 2]);

						triangleList.Add(vertices[i + 2]);
						triangleList.Add(vertices[i + 1]);
						triangleList.Add(vertices[i + 3]);
					}
				}

				//绘制该材质层
				if (triangleList.Count > 0)
				{
					//设置该层对应的材质
					Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.Extra[layer.TextureID].Value;
					Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;

					//绘制该层
					Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList,
						triangleList.ToArray(), 0, triangleList.Count / 3);
				}
			}

			//恢复原版绘制状态
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null,
				Main.GameViewMatrix.TransformationMatrix);
		}
	}
}