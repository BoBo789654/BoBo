using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Weapons.Magic
{
	public class LargeDesertTornado : ModProjectile
	{
		public float Timer = 0;						//通用计时器，用于控制各种周期性效果
		public float Alpha = 0;						//整体透明度，用于控制渐入渐出效果
		public float Rot = 0;						//龙卷风纹理的整体旋转角度
		public float T = 0;							//存储弹幕初始的timeLeft，用于计算渐隐开始的时机
		private float DamageTimer = 0;				//专门用于计算对中心区域敌人伤害间隔的计时器
		private const float DamageInterval = 15f;	//每15帧对中心区域的敌人造成一次额外伤害
		public override string Texture => Pictures.MagicProj + Name;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;//设置拖尾缓存长度为10帧，这意味着弹幕会保留最近10帧的位置用于绘制拖尾效果
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;//设置拖尾模式为2。模式2通常用于更复杂的自定义拖尾渲染，与PreDraw中的叠加绘制配合实现体积感。
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 110;//将绘制边界检查的“容差”设置为110像素。这确保即使弹幕的碰撞箱很小，但只要其中心点距离屏幕边缘110像素以内，其巨大的视觉特效（高600像素）仍会被绘制，
		}
		public override void SetDefaults()
		{
			Projectile.width = 32;       
			Projectile.height = 240;     
			Projectile.scale = 4f;        
			Projectile.friendly = true;  
			Projectile.hostile = false;   
			Projectile.penetrate = -1;  
			Projectile.timeLeft = 3600;   
			Projectile.tileCollide = false; 
			Projectile.aiStyle = -1;      
			Projectile.DamageType = DamageClass.Magic;
			Projectile.ignoreWater = true; 
			Projectile.usesLocalNPCImmunity = true; 
			Projectile.localNPCHitCooldown = 5; 
			T = Projectile.timeLeft;
			AIType = 357;
		}
		public override void AI()
		{
			//通用计时器和伤害计时器每帧递增
			Timer++;
			DamageTimer++;
			//获取发射此弹幕的玩家实例
			Player owner = Main.player[Projectile.owner];
			float AttractRadius = 1000f; //吸引半径，与您之前设定的一致
			float AttractStrengthBase = 6f; //基础吸引力大小
			float AttractStrengthGrowth = Timer / 150f;//随时间增长的吸引力
			float totalAttractStrength = AttractStrengthBase + AttractStrengthGrowth;//总吸引力
			foreach (NPC target in Main.npc)
			{
				//条件：活跃、非友好、不是训练假人、可以被追踪
				if (target.active && !target.friendly && target.type != NPCID.TargetDummy && target.CanBeChasedBy())
				{
					//计算从 NPC 指向弹幕中心的向量
					Vector2 toProj = Projectile.Center - target.Center;
					float distanceToTarget = toProj.Length();//获取实际距离
					if (distanceToTarget <= AttractRadius)//如果 NPC 在吸引半径内
					{
						//计算单位方向向量并乘以吸引力强度
						toProj /= distanceToTarget;//等同于 Normalize()，但避免零向量风险
						toProj *= totalAttractStrength;
						//平滑改变 NPC 速度：新速度 = (2 * 吸引力向量 + 1 * 原速度) / 3，这个公式比直接相加更平滑，能保留 NPC 部分原有惯性
						target.velocity = ((toProj * 2f) + target.velocity) / 3f;
						float maxSpeed = 9f;//最大速度限制，防止速度过快
						if (target.velocity.Length() > maxSpeed)
						{
							target.velocity = Vector2.Normalize(target.velocity) * maxSpeed;
						}
					}
					if (distanceToTarget < 50f && DamageTimer >= DamageInterval)//中心区域额外伤害，此伤害判定独立于吸引，仅基于距离和计时器
					{
						int centerDamage = (int)(Projectile.damage * 0.5f);
						target.SimpleStrikeNPC(centerDamage, 0, false, 0f, null, false, 0, true);
						DamageTimer = 0; //重置伤害计时器
					}
				}
			}
			//每10帧且仅在玩家的客户端执行（避免多人模式不同步）
			/*if (Timer % 10 == 0 && Main.myPlayer == Projectile.owner)
			{
				for (int i = 0; i < 1; i++)//每次产生n个小型沙尘弹幕
				{
					//随机生成速度和角度
					float SandSpeed = 3f + Main.rand.NextFloat(2f);
					float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
					Vector2 SandVelocity = randomAngle.ToRotationVector2() * SandSpeed;//将角度转换为方向向量，并乘以速度
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center + Main.rand.NextVector2Circular(30, 30),//半径30像素圆内
						SandVelocity,
						ProjectileID.SandnadoFriendly,
						(int)(Projectile.damage * 0.4f),           
						Projectile.knockBack * 0.5f,               
						Projectile.owner  
					);
				}
			}*/
			if (owner.active && Timer > 30) //在弹幕生成1秒后开始移动，并且其所有者玩家必须存活
			{
				Vector2 TargetMousePos = Main.MouseWorld;//获取当前鼠标的世界坐标
				float MoveSpeed = 5f;//移动的基础速度
				Vector2 desiredVelocity = Vector2.Normalize(TargetMousePos - Projectile.Center) * MoveSpeed;//计算从弹幕当前位置指向鼠标位置的方向向量，并归一化，然后乘以速度
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.05f);//使用线性插值平滑地改变当前速度，使其逐渐趋向目标速度
			}
			if (Timer % 60 == 0)//每秒播放一次声音，音调降低（Pitch = -0.5），音量减小（Volume = 0.3）
				SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown with { Volume = 0.3f, Pitch = -0.5f }, Projectile.Center);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			//定义矩形的宽度和高度
			float rectWidth = 32f;//矩形宽度
			float rectHeight = 1200f;//矩形高度
			//计算矩形的半宽和半高
			float halfWidth = rectWidth / 2f;
			float halfHeight = rectHeight / 2f;
			//获取弹幕中心点
			Vector2 center = Projectile.Center;
			//定义矩形的四个角点
			Vector2 topLeft = new Vector2(center.X - halfWidth, center.Y - halfHeight);
			Vector2 topRight = new Vector2(center.X + halfWidth, center.Y - halfHeight);
			Vector2 bottomLeft = new Vector2(center.X - halfWidth, center.Y + halfHeight);
			Vector2 bottomRight = new Vector2(center.X + halfWidth, center.Y + halfHeight);
			//获取目标矩形的尺寸和位置
			Vector2 aabbPosition = targetHitbox.TopLeft();//目标矩形左上角位置
			Vector2 aabbDimensions = targetHitbox.Size();//目标矩形尺寸
			//现在定义四条边，且检测矩形的四条边与目标矩形是否相交
			if (Collision.CheckAABBvLineCollision2(aabbPosition, aabbDimensions, topLeft, topRight))//上边：从左上到右上
				return true;
			if (Collision.CheckAABBvLineCollision2(aabbPosition, aabbDimensions, topRight, bottomRight))//右边：从右上到右下
				return true;
			if (Collision.CheckAABBvLineCollision2(aabbPosition, aabbDimensions, bottomRight, bottomLeft))//下边：从右下到左下
				return true;
			if (Collision.CheckAABBvLineCollision2(aabbPosition, aabbDimensions, bottomLeft, topLeft))//左边：从左下到左上
				return true;
			//如果没有边相交，检查目标矩形是否完全在自定义矩形内部
			//这是为了处理目标矩形完全被包含在自定义矩形内的情况
			if (targetHitbox.Contains((int)topLeft.X, (int)topLeft.Y) ||
				targetHitbox.Contains((int)topRight.X, (int)topRight.Y) ||
				targetHitbox.Contains((int)bottomLeft.X, (int)bottomLeft.Y) ||
				targetHitbox.Contains((int)bottomRight.X, (int)bottomRight.Y))
			{
				return true;
			}
			if (targetHitbox.Contains((int)center.X, (int)center.Y))//检查自定义矩形的四个顶点是否在目标矩形内
				return true;
			return false;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			var TornadoTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Rot -= MathHelper.PiOver4 / 6f;//每帧减少整体旋转角度，使龙卷风产生逆时针旋转的动画效果
			float VerticalOffset = -600;//绘制起始的垂直偏移。从中心点向上600像素开始画第一层。
			float LayerScale = 2f;//最顶层（第一层）纹理的缩放倍数
			float LayerRotation = 0;//每层纹理独立的旋转角度累加器
			Color TornadoColor = new Color(220, 110, 60);//定义龙卷风的基础颜色
			Point TileCoord = Projectile.Center.ToTileCoordinates();//将弹幕中心的世界坐标转换为图格坐标
			//从中心点垂直向上和向下扩展检测，找出可通行的最大高度和最低地面。
			//参数(15, 15)是向上和向下搜索的最大图格数限制。
			Collision.ExpandVertically(TileCoord.X, TileCoord.Y, out int TopTileY, out int BottomTileY, 15, 15);
			TopTileY++;//微调顶部边界
			BottomTileY--;//微调底部边界
			//将检测到的图格坐标转换回世界坐标。+ new Vector2(8f)是为了取图格中心点。
			Vector2 TopWorldPos = new Vector2(TileCoord.X, TopTileY) * 16f + new Vector2(8f);
			Vector2 BottomWorldPos = new Vector2(TileCoord.X, BottomTileY) * 16f + new Vector2(8f);
			//计算龙卷风占据的空间向量。Y分量是总高度（像素），X分量设为高度的20%，用于控制龙卷风的宽度。
			Vector2 HeightVector = new Vector2(0f, BottomWorldPos.Y - TopWorldPos.Y);
			HeightVector.X = HeightVector.Y * 0.2f;//宽度 = 高度 * 0.2
			float ColorLerpFactor = 0f;//颜色插值因子，在绘制循环中累加，用于控制每层颜色的渐变。
			//在弹幕生命期的前20帧和最后20帧进行透明度变化，中间保持完全显示。
			if (Timer < T - 20)//从生成到消失前20帧：淡入阶段
				if (Alpha < 1) Alpha += 1 / 20f;//用20帧从0线性增加到1
			else //最后20帧：淡出阶段
				if (Alpha > 0) Alpha -= 1 / 20f;//用20帧从1线性减少到0
			//结束当前的材质批处理
			Main.spriteBatch.End();
			//开启一个新的材质批处理，并指定渲染状态。
			Main.spriteBatch.Begin(
				SpriteSortMode.Deferred,	//绘制顺序：延迟排序（效率较高）
				BlendState.Additive,		//叠加混合：颜色值直接相加，使亮部更亮，产生发光、光晕效果。
				Main.DefaultSamplerState,	//采样器状态：默认（线性过滤等）
				DepthStencilState.None,     //无深度/模板状态
				RasterizerState.CullNone,	//光栅化状态：不剔除背面（双面渲染）
				null,                       //无特效
				Main.GameViewMatrix.TransformationMatrix//应用游戏视图的变换矩阵
			);
			for (int LayerIndex = 0; LayerIndex < 300; LayerIndex++)
			{
				//旋转：每层增加一个微小的固定角度，使所有层叠加后形成螺旋纹理。
				LayerRotation += MathHelper.PiOver4 / 64f;
				//位移：每层在垂直方向上下移固定距离，共300层覆盖总高1200像素（-600 到 +600）
				VerticalOffset += 1200f / 300; //1200 / 300 = 每层间隔4像素
				//缩放：每层比上一层略微缩小，模拟龙卷风从顶部到底部逐渐变细的锥形。
				if(LayerScale > 0.01f)
					LayerScale -= 0.02f / 3; //总共缩小 0.02
				//颜色插值：因子每层累加固定值
				ColorLerpFactor += 5f;
				//计算当前层的高度比例（从0到1变化），用于颜色和可能其他属性的插值
				//HeightVector.Y 是计算出的龙卷风实际高度。colorLerpFactor / HeightVector.Y 的值会随着层下移而增大
				float HeightRatio = ColorLerpFactor / HeightVector.Y;
				//计算当前层的颜色。从透明(Color.Transparent)插值到基础色(TornadoColor)
				//HeightRatio * 2f 加快了插值速度，可能使颜色在高度一半时就达到饱和
				//Projectile.GetAlpha() 会考虑弹幕本身的透明度设置（虽然这里没用到）
				Color LayerColor = Projectile.GetAlpha(Color.Lerp(Color.Transparent, TornadoColor, HeightRatio * 2f));
				LayerColor.A = (byte)(LayerColor.A * 0.5f); //将Alpha通道减半，使每一层都半透明，叠加后才有通透感。
				//绘制当前层纹理。
				Main.EntitySpriteDraw(
					TornadoTexture, //使用的纹理
					Projectile.Center + new Vector2(0, VerticalOffset) - Main.screenPosition,//绘制位置（世界坐标转换到屏幕坐标）
					null, //源矩形（null表示绘制整个纹理）
					LayerColor * Alpha, //最终颜色 = 层颜色 * 整体透明度Alpha
					Rot + LayerRotation, //最终旋转 = 整体旋转 + 层独立旋转
					TornadoTexture.Size() / 2f, //旋转和缩放的原点（纹理中心）
					Projectile.scale * LayerScale, //最终缩放 = 弹幕基础缩放 * 层缩放
					SpriteEffects.None, //镜像效果：无
					0 //图层深度
				);
			}
			//结束叠加混合的绘制批处理
			Main.spriteBatch.End();
			//重新开始默认的材质批处理，以免影响后续其他游戏对象的绘制。
			Main.spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend, //切换回Alpha混合。这是标准的透明混合模式。
				Main.DefaultSamplerState,
				DepthStencilState.None,
				RasterizerState.CullNone,
				null,
				Main.GameViewMatrix.TransformationMatrix
			);
			return false;
		}
		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 250; i++) //生成50个粒子
			{
				if (Main.rand.NextBool(2))//有几率在弹幕位置附近生成沙尘粒子
				{
					Dust SandDust = Dust.NewDustDirect(Projectile.position + Main.rand.NextVector2Circular(50, 50),
						Projectile.width, Projectile.height, DustID.Sand, 0f, 0f, 100, default, 1.5f);
					SandDust.noGravity = true; //粒子不受重力影响
					SandDust.velocity = Main.rand.NextVector2Circular(2, 2);//给予随机速度
					if (Main.rand.NextBool(3))//有几率额外生成小石子粒子
					{
						Dust stoneDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
							DustID.Stone, 0f, 0f, 100, default, 1f);
						stoneDust.noGravity = true;
						stoneDust.velocity = Main.rand.NextVector2Circular(3, 3);
					}
				}
				//生成一个在圆形边缘上的随机方向向量，保证粒子向四周均匀散开
				Vector2 ExplosionSpeed = Main.rand.NextVector2CircularEdge(18f, 18f);
				Dust SandExplosion = Dust.NewDustPerfect(Projectile.Center, DustID.Sandstorm,         
					ExplosionSpeed * 2, 100, default, 2.5f);//沙暴粒子
				SandExplosion.noGravity = true;
				Dust SmokeExplosion = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke,             
					ExplosionSpeed * 1.5f, 100, default, 1.5f);//烟雾粒子
				SmokeExplosion.noGravity = true;
			}
			SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f }, Projectile.Center);//播放爆炸音效，音量调整为70%
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC target = Main.npc[i];
				if (target.active && !target.friendly && target.Distance(Projectile.Center) < 200f 
					&& target.type != NPCID.TargetDummy && target.CanBeChasedBy())//target活跃、敌对、不是训练假人、可以被追踪、在爆炸范围内
				{
					Vector2 ExplosionKnockback = target.DirectionFrom(Projectile.Center);//计算击退方向：从爆炸中心指向敌人
					target.velocity += ExplosionKnockback * 8f;//给敌人施加一个瞬间的、较大的速度冲量
					target.AddBuff(BuffID.OnFire, 180);//给敌人添加“着火了！”
				}
			}
		}
	}
}