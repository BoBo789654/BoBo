using BoBo.Content.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Weapons.Magic
{
	public class ThunderflameProj : ModProjectile
	{
		public override string Texture => Pictures.MagicProj + Name + "1";
		public class LightningSegment
		{
			public Vector2 Start;		//线段起点
			public Vector2 End;			//线段终点
			public float Width;			//线段的绘制宽度
			public Color Color;			//线段的颜色
			public float Alpha;			//线段的整体透明度
			public int Life;			//线段剩余的生命值，为0时消失
			public bool IsMainBranch;	//标记此线段是否为主干分支
			public LightningSegment(Vector2 start, Vector2 end, float width, Color color, float alpha, int life, bool isMain = true)//LightningSegment的构造函数
			{
				Start = start;
				End = end;
				Width = width;
				Color = color;
				Alpha = alpha;
				Life = life;
				IsMainBranch = isMain;
			}
		}
		//存储当前弹幕生成的所有闪电线段的列表
		private List<LightningSegment> segments = new List<LightningSegment>();
		//当前弹幕的蓄力等级，从 Projectile.ai[0] 读取，决定闪电的规模、颜色和效果
		private int chargeLevel = 1;
		//闪电主路径的最大曲折（分段）次数蓄力等级越高，次数越多
		private int maxBends = 5;
		//闪电第一段（起始段）的长度
		private float firstSegmentLength = 100f;
		//闪电的基础颜色，随蓄力等级变化（浅蓝 -> 蓝 -> 紫）
		private Color lightningColor = Color.LightBlue;
		//每个闪电线段的总存活时间（帧数）高级蓄力存活更久
		private int totalLife = 60;
		//控制“全屏黑屏”效果透明度的变量从 0.5 线性递减至 0，实现屏幕渐亮效果
		private float dark = 0.5f;
		//标记闪电路径是否已生成完毕并准备好被绘制
		private bool isReadyToDraw = false;
		//用于控制初始化或状态切换的通用计时器
		private int setupTimer = 0;
		//随机数生成器实例，用于生成闪电分支的角度、长度等随机属性
		private Random rand = new Random();
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 3000;//设置此弹幕的绘制边界检查绒毛大小，扩大此值可使大型闪电在屏幕边缘外也能被正确绘制
		}
		//设置弹幕的实例属性每当一个此类弹幕被创建时调用
		public override void SetDefaults()
		{
			Projectile.width = 4;
			Projectile.height = 4;
			Projectile.friendly = true;      
			Projectile.hostile = false;     
			Projectile.DamageType = DamageClass.Magic; 
			Projectile.penetrate = -1;       
			Projectile.timeLeft = 300;       
			Projectile.tileCollide = false;  
			Projectile.ignoreWater = true;  
			Projectile.alpha = 255;         
			Projectile.extraUpdates = 2;
		}
		//在弹幕生成到世界中时立即调用用于初始化基于蓄力等级的参数并生成闪电
		public override void OnSpawn(IEntitySource source)
		{
			chargeLevel = (int)Projectile.ai[0];//ai[0]: 蓄力等级 (1, 2, 3)
			int index = (int)Projectile.ai[1];//ai[1]: 弹幕索引（用于区分同一次发射的多个弹幕）
			switch (chargeLevel)//根据蓄力等级，配置闪电的各种参数
			{
				case 1: //第一档
					maxBends = 5;
					firstSegmentLength = 100f;
					lightningColor = Color.LightBlue;
					totalLife = 20;
					break;
				case 2: //第二档
					maxBends = 9;
					firstSegmentLength = 100f;
					lightningColor = Color.DarkCyan;
					totalLife = 60;
					break;
				case 3: //第三档
					maxBends = 12;
					firstSegmentLength = 100f;
					lightningColor = Color.Purple;
					totalLife = 240;
					break;
			}
			//初始化随机数生成器使用弹幕位置和索引等生成，确保每次发射的闪电形状在一定范围内可重复
			rand = new Random((int)Projectile.position.X + (int)Projectile.position.Y + index * 1000 + Main.rand.Next(1, 20));
			GenerateLightningPath();//生成闪电路径
			DealDamageImmediately();//在闪电生成的同一帧，立即对路径上的所有敌人造成伤害
		}
		//生成闪电的主干路径此方法会清空现有线段列表，并调用递归函数 GenerateBranch 开始构建
		private void GenerateLightningPath()
		{
			segments.Clear();//确保列表为空
			Vector2 direction = Projectile.velocity;//决定初始方向如果弹幕速度几乎为0，则随机选择一个方向
			if (direction.LengthSquared() < 0.1f)
				direction = Vector2.UnitX.RotatedBy(rand.NextDouble() * MathHelper.TwoPi);
			direction.Normalize();//确保是单位向量
			Vector2 currentPos = Projectile.Center;//从弹幕中心开始，生成第一段分支
			GenerateBranch(currentPos, direction, firstSegmentLength, maxBends, 0, true, 1f);
		}
		private void GenerateBranch(Vector2 startPos, Vector2 direction, float segmentLength, int remainingBends,
								   int depth, bool isMainBranch, float alpha)
		{
			if (remainingBends <= 0 || segmentLength <= 5f || depth > 3)//没有剩余曲折次数；线段太短；递归深度过深
				return;
			Vector2 endPos = startPos + direction * segmentLength;//计算当前线段的终点
			float width = isMainBranch ? 2f : 1f;//决定线段宽度：主干最宽，一级分支次之，更深级分支最细
			if (depth > 1) width = 0.5f;
			segments.Add(new LightningSegment(startPos, endPos, width, lightningColor, alpha, totalLife, isMainBranch));//创建线段对象并加入总列表
			if (remainingBends > 1)//如果还有剩余曲折次数，继续生成后续线段和可能的侧枝
			{
				//根据蓄力等级，应用不同的生成规则和特效
				if (Projectile.ai[0] == 1) //第一档蓄力
				{
					SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.2f, Pitch = 0.5f }, Projectile.Center);//播放基础的闪电音效，并调整音高
					int j = rand.Next(0, 2);
					SoundEngine.PlaySound(new SoundStyle($"Terraria/Sounds/Thunder_{j}")
					{
						Volume = 0.2f,		//音量 (0.0-1.0)
						PitchVariance = 1f,	//音高随机变化范围 (-1.0 到 1.0)
						MaxInstances = 2,	//同时播放的最大实例数
						Pitch = 0.5f,		//音高校正 (-1.0 到 1.0)
						IsLooped = false	//是否循环播放
					});
					float angleVariation = MathHelper.ToRadians(20f);//在当前方向基础上添加一个随机偏移角度
					Vector2 newDirection = direction.RotatedBy(rand.NextDouble() * angleVariation * 2 - angleVariation);
					float newSegmentLength = segmentLength - 7f;//新线段比前一段稍短
					GenerateBranch(endPos, newDirection, newSegmentLength, remainingBends - 1, depth, isMainBranch, alpha);//递归生成下一段主干
					if (rand.Next(2) == 0)//50% 概率生成侧枝
					{
						int numBranches = isMainBranch ? 1 : rand.Next(0, 3);//主干可生成1个侧枝，非主干分支可能生成0-2个侧枝
						for (int i = 0; i < numBranches; i++)
						{
							float branchAngle = MathHelper.ToRadians(20f + rand.Next(0, 20));//侧枝角度随机，方向随机
							if (rand.Next(2) == 0) branchAngle = -branchAngle;
							Vector2 branchDirection = direction.RotatedBy(branchAngle);
							float branchLength = segmentLength * 0.5f;//侧枝长度
							float branchAlpha = alpha * 0.8f;//侧枝透明度
							GenerateBranch(endPos, branchDirection, branchLength, 2, depth + 1, false, branchAlpha);//递归生成侧枝，其 remainingBends 固定为2，深度+1，标记为非主干
						}
					}
				}
				else if (Projectile.ai[0] == 2) //第二档蓄力
				{
					SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.5f, Pitch = 0.5f }, Projectile.Center);
					int j = rand.Next(2, 4);
					SoundEngine.PlaySound(new SoundStyle($"Terraria/Sounds/Thunder_{j}")
					{ Volume = 0.5f, PitchVariance = 1f, MaxInstances = 2, Pitch = 0.5f, IsLooped = false});
					PunchCameraModifier shaking = new PunchCameraModifier(
						Projectile.Center,												//震动源点
						(Main.rand.NextFloat() * MathHelper.TwoPi).ToRotationVector2(), //随机方向
						10f,															//强度
						3f,																//震动频率
						10,																//持续时间（帧）
						100f,															//衰减距离
						"Rock"															//标识符
					);
					CameraModifierPlayer.ShakeStack.Add(shaking);//将震动效果添加到全局的震动堆栈中
					//主分支延续（逻辑与第一档类似）
					float angleVariation = MathHelper.ToRadians(20f);
					Vector2 newDirection = direction.RotatedBy(rand.NextDouble() * angleVariation * 2 - angleVariation);
					float newSegmentLength = segmentLength - 6f; 
					GenerateBranch(endPos, newDirection, newSegmentLength, remainingBends - 1, depth, isMainBranch, alpha);
					if (rand.Next(2) == 0)//生成侧枝（逻辑与第一档类似）
					{
						int numBranches = isMainBranch ? rand.Next(1, 3) : rand.Next(0, 3);
						for (int i = 0; i < numBranches; i++)
						{
							float branchAngle = MathHelper.ToRadians(20f + rand.Next(0, 20));
							if (rand.Next(2) == 0) branchAngle = -branchAngle;
							Vector2 branchDirection = direction.RotatedBy(branchAngle);
							float branchLength = segmentLength * 0.7f;
							float branchAlpha = alpha * 0.8f;
							GenerateBranch(endPos, branchDirection, branchLength, 2, depth + 1, false, branchAlpha);
						}
					}
				}
				else if (Projectile.ai[0] == 3) //第三档蓄力（逻辑与第一档类似）
				{
					int j = rand.Next(4, 5);
					SoundEngine.PlaySound(new SoundStyle($"Terraria/Sounds/Thunder_{j}")
					{ Volume = 0.9f, PitchVariance = 1f, MaxInstances = 2, Pitch = 0.5f, IsLooped = false });
					PunchCameraModifier shaking = new PunchCameraModifier(Projectile.Center,
						(Main.rand.NextFloat() * MathHelper.TwoPi).ToRotationVector2(),
						10f, 3f, 20, 1000f, "Rock");
					CameraModifierPlayer.ShakeStack.Add(shaking);
					float angleVariation = MathHelper.ToRadians(20f);
					Vector2 newDirection = direction.RotatedBy(rand.NextDouble() * angleVariation * 2 - angleVariation);
					float newSegmentLength = segmentLength - 5f;
					GenerateBranch(endPos, newDirection, newSegmentLength, remainingBends - 1, depth, isMainBranch, alpha);
					int numBranches = isMainBranch ? 2 : rand.Next(0, 3);
					for (int i = 0; i < numBranches; i++)
					{
						float branchAngle = MathHelper.ToRadians(10f + rand.Next(0, 50));
						if (rand.Next(2) == 0) branchAngle = -branchAngle;
						Vector2 branchDirection = direction.RotatedBy(branchAngle);
						float branchLength = segmentLength * 0.9f;
						float branchAlpha = alpha * 0.8f;
						GenerateBranch(endPos, branchDirection, branchLength, 2, depth + 1, false, branchAlpha);
					}
				}
			}
		}
		private void DealDamageImmediately()//在闪电生成后立即执行，对闪电路径附近的敌人造成伤害，此方法遍历所有线段，将每条线段细分为多个检测点，并检查每个点附近的敌人，使用 HashSet 记录已击中的敌人，避免同一敌人在同一帧内受到多次伤害
		{
			Player player = Main.player[Projectile.owner];//获取发射此弹幕的玩家
			HashSet<int> hitNPCs = new HashSet<int>();//用于存储本帧内已被伤害的NPC的ID
			
			foreach (var segment in segments)//遍历所有闪电线段
			{
				Vector2 segmentVector = segment.End - segment.Start;
				float segmentLength = segmentVector.Length();
				if (segmentLength <= 0.1f) continue;//忽略无效线段
				Vector2 segmentDirection = segmentVector / segmentLength;//线段单位方向向量
				int steps = Math.Max(1, (int)(segmentLength / 20f));//将线段划分为多个步进点进行检测，步长为20像素
				for (int i = 0; i <= steps; i++)
				{
					Vector2 checkPoint = segment.Start + segmentDirection * (segmentLength * i / steps);//计算当前检测点在线段上的位置
					foreach (NPC npc in Main.npc)//遍历场景中所有NPC
					{
						if (npc.active && !npc.friendly && npc.life > 0 && !npc.dontTakeDamage)//检查NPC是否可被攻击
						{
							if (hitNPCs.Contains(npc.whoAmI))//如果此NPC在本帧已被此闪电伤害过，则跳过
								continue;
							float distance = Vector2.Distance(checkPoint, npc.Center);//计算检测点到NPC中心的距离
							float hitRadius = Math.Max(npc.width, npc.height) * 0.5f + 15f;//计算NPC的大致碰撞半径，并加上一个固定阈值
							if (distance < hitRadius)//如果NPC在伤害范围内
							{
								int damage = Projectile.damage;//获取弹幕的基础伤害值
								hitNPCs.Add(npc.whoAmI);//标记此NPC为已击中
								if (npc.boss)//对BOSS类敌人造成一定比例伤害，对普通敌人造成全额伤害
									player.ApplyDamageToNPC(npc, (int)(Projectile.damage / 3 * Main.rand.NextFloat(0.85f, 1.15f)), Projectile.knockBack, -npc.direction, true);
								else
									player.ApplyDamageToNPC(npc, (int)(Projectile.damage * Main.rand.NextFloat(0.85f, 1.15f)), Projectile.knockBack, -npc.direction, true);
								if (rand.Next(3) == 0)
									npc.AddBuff(BuffID.Electrified, 60);
								CreateHitEffect(npc.Center);//在命中点创建视觉特效
								break;//找到一个敌人后，跳出对当前检测点的NPC循环，继续检查下一个检测点
							}
						}
					}
				}
			}
		}
		private void CreateHitEffect(Vector2 position)//在指定位置创建闪电命中敌人的视觉特效
		{
			for (int i = 0; i < 5; i++)
			{
				Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.Electric, 0f, 0f, 100, default, 1.5f);
				dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
				dust.noGravity = true;
			}
		}
		public override void AI()
		{
			dark -= 0.003f;//更新黑屏变量
			if (dark <= 0)
				dark = 0;//确保值不为负
			if (setupTimer > 0)//计时器
			{
				setupTimer--;
				if (setupTimer == 0)
					isReadyToDraw = true;//计时器归零时标记可以绘制
			}
			for (int i = segments.Count - 1; i >= 0; i--)//更新所有闪电线段，并移除结束的线段
			{
				segments[i].Life -= 1;//每帧生命值减1
				if (segments[i].Life <= 0)
					segments.RemoveAt(i);//生命值为0时从列表中移除
			}
			if (segments.Count == 0 && isReadyToDraw)//如果所有线段都已消失，并且已经准备好被绘制，则销毁弹幕
			{
				Projectile.Kill();//调用弹幕的销毁逻辑
				return;
			}
			//if (Main.rand.Next(3) == 0)//有概率在随机线段位置上生成一个闪烁的粒子效果
			//{
			//	Vector2 randomPos = GetRandomSegmentPosition();//获取随机线段上的一个点
			//	int dustType = chargeLevel >= 3 ? DustID.PurpleTorch :
			//				   chargeLevel >= 2 ? DustID.BlueTorch : DustID.Electric;//根据蓄力等级决定粒子类型和颜色
			//	Dust dust = Dust.NewDustDirect(randomPos, 0, 0, dustType, 0f, 0f, 100, default, 1f);
			//	dust.velocity = Vector2.Zero;//粒子静止
			//	dust.noGravity = true;
			//}
			Projectile.velocity = Vector2.Zero;//使弹幕固定在发射时的中心位置，不移动
			Projectile.Center = Main.player[Projectile.owner].Center;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Projectile.damage = (int)(Projectile.damage * Main.rand.NextFloat(0.85f, 1.15f));
		}
		private Vector2 GetRandomSegmentPosition()//从segments列表中随机选择一个线段，并返回该线段上的一个随机位置，用于生成闪烁粒子等随机位置特效
		{
			if (segments.Count == 0) return Projectile.Center;//没有线段时返回弹幕中心
			LightningSegment segment = segments[Main.rand.Next(segments.Count)];//随机选一个线段
			float t = (float)Main.rand.NextDouble();//生成一个0到1的随机比例
			return Vector2.Lerp(segment.Start, segment.End, t);//根据比例t，在线段的起点和终点之间线性插值，得到随机位置
		}
		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.ai[0] == 3)//只有第三档蓄力才触发黑屏效果
			{
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(
					SpriteSortMode.Deferred,							//绘制排序模式
					BlendState.AlphaBlend,								//透明度混合
					Main.DefaultSamplerState,							//纹理采样器
					DepthStencilState.None,								//深度模板状态
					RasterizerState.CullNone,							//光栅化状态
					null,												//着色器效果
					Main.GameViewMatrix.TransformationMatrix			//视口变换矩阵
				);
				Texture2D texture = ModContent.Request<Texture2D>(Pictures.MagicProj + "ThunderflameProj1").Value;//黑屏在这处理
				Main.EntitySpriteDraw(
					texture,											//纹理
					Main.screenPosition,								//绘制起点（屏幕左上角的世界坐标）
					null,												//源矩形（null表示整个纹理）
					Color.Black * (dark / 8f),							//颜色与透明度
					0,													//旋转
					texture.Size() / 2,									//旋转原点
					new Vector2(Main.screenWidth, Main.screenHeight),	//缩放至整个屏幕大小
					SpriteEffects.None,									//镜像效果
					0													//图层深度
				);
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,//恢复为标准混合模式
					Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}
			return base.PreDraw(ref lightColor);
		}
		public override void PostDraw(Color lightColor)//在游戏绘制弹幕自身之后调用此处用于绘制发光的闪电线段，使用Additive混合实现自发光效果
		{
			if (segments.Count == 0)//如果没有要绘制的线段，直接返回避免操作 SpriteBatch 导致状态错误
				return;
			var originalBlendState = Main.graphics.GraphicsDevice.BlendState;//保存图形设备当前的混合状态
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,//叠加混合模式
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			Texture2D lightningTexture = ModContent.Request<Texture2D>(Pictures.MagicProj + "ThunderflameProj2").Value;
			if (lightningTexture == null || lightningTexture.IsDisposed)//如果自定义纹理加载失败，则使用原版雷云法杖的闪电纹理作为后备
				lightningTexture = TextureAssets.Projectile[ProjectileID.ThunderStaffShot].Value;
			foreach (var segment in segments)//遍历并绘制所有存活的闪电线段
			{
				if (segment.Life <= 0) continue;//跳过已消亡的线段
				DrawLightningSegment(segment, lightningTexture);
			}
			Main.spriteBatch.End();
			Main.graphics.GraphicsDevice.BlendState = originalBlendState;//恢复图形设备的混合状态
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,//恢复为标准混合模式
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}
		private void DrawLightningSegment(LightningSegment segment, Texture2D texture)//绘制单个闪电线段将一张纹理拉伸、旋转以匹配线段的起点、终点和宽度
		{
			Vector2 segmentVector = segment.End - segment.Start;//计算线段的向量、长度和方向
			float length = segmentVector.Length();
			if (length <= 0.1f) return;//忽略极短的线段
			float rotation = segmentVector.ToRotation() - MathHelper.PiOver2;//计算纹理的旋转角度由于纹理默认是垂直的，需要旋转到与线段方向对齐，再减Pi/2
			Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);//定义旋转原点为纹理中心
			//X轴缩放 = 线段期望宽度 / 纹理实际宽度 * 2 (加宽)
			//Y轴缩放 = 线段实际长度 / 纹理实际高度 * 2 (加长)
			Vector2 scale = new Vector2(segment.Width / texture.Width * 2, length / texture.Height * 2);
			float lifeRatio = segment.Life / (float)totalLife;//计算线段当前的生存比例（0到1），用于控制随生命衰减的透明度
			Color drawColor = segment.Color * (segment.Alpha * lifeRatio) * 2f;//计算最终绘制颜色：基础颜色 * 线段固有透明度 * 生存比例 * 2
			Main.EntitySpriteDraw(											
				texture,													//纹理
				segment.Start - Main.screenPosition + segmentVector * 0.5f, //绘制位置：线段中点转换到屏幕坐标
				null,														//源矩形
				drawColor,													//颜色
				rotation,													//旋转
				origin,														//旋转原点
				scale,														//缩放
				SpriteEffects.None,											//镜像效果
				0															//图层深度
			);
		}
		//public override void OnKill(int timeLeft)
		//{
		//	for (int i = 0; i < 10; i++)
		//	{
		//		Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
		//			DustID.Electric, 0f, 0f, 100, default, 1.5f);
		//		dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
		//		dust.noGravity = true;
		//	}
		//	base.OnKill(timeLeft);
		//}
	}
}