using BoBo.Asset.BossBars;
using BoBo.Content.Items.Consumables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.NPCs.EvilWorm
{	
	[AutoloadBossHead]//血条左边的BOSS头部，这个后面要改的，会用自定义血条的
	#region 头
	///<summary>邪恶蠕虫的头部段实现类，负责：玩家追踪、身体段生成、移动控制和Boss行为管理</summary>
	internal class EvilWormHead : WormHead
	{
		public override string Texture => Pictures.EvilWorm + Name;//纹理路径属性
		public override int BodyType => ModContent.NPCType<EvilWormBody>();//定义身体段的类型ID
		public override int TailType => ModContent.NPCType<EvilWormTail>();//定义尾部段的类型ID
		///<summary>设置静态默认值，配置怪物图鉴相关设置，它只需在游戏加载时执行一次，用于定义NPC的全局行为</summary>
		public override void SetStaticDefaults()
		{
			var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()//配置图鉴的绘制参数
			{
				CustomTexturePath = Pictures.EvilWorm + "EvilWorm_Bestiary",//自定义图鉴纹理
				Position = new Vector2(40f, 24f),//在图鉴中的位置偏移
				PortraitPositionXOverride = 0f,//图鉴图像位置X覆盖
				PortraitPositionYOverride = 12f//图鉴图像位置Y覆盖
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
		}
		public override void SetDefaults()
		{
			//基础属性
			NPC.width = 50;//NPC碰撞箱宽度，不是贴图的，贴图后续我们自己绘制
			NPC.height = 74;//NPC碰撞箱高度，不是贴图的，贴图后续我们自己绘制
			NPC.aiStyle = -1;//-1是使用自定义AI，不使用原版AI样式，AI后续我们自己写
			//战斗属性
			NPC.lifeMax = 33000;//最大生命值
			NPC.defense = 30;//防御力
			NPC.damage = 60;//接触伤害
			NPC.knockBackResist = 0f;//击退抗性（0是不可击退）
			//Boss特性
			NPC.boss = true;//标记为Boss，会显示Boss血条
			NPC.noGravity = true;//不受重力影响
			NPC.noTileCollide = true;//忽略图块碰撞
			CanFly = false;//不能飞行（之前基类里自己写的）
			NPC.BossBar = ModContent.GetInstance<EvilWormBossBar>();
			Music = MusicID.OtherworldlyDungeon;
			//旗帜和掉落物
			Banner = Type;//对应旗帜类型
			BannerItem = ModContent.ItemType<EvilBait>();//旗帜物品，掉啥旗帜啊，不如掉召唤物，看后续能换成武器不，还没想好
			ItemID.Sets.KillsToBanner[BannerItem] = 3;//击杀3个获得旗帜
			for (int k = 0; k < NPC.buffImmune.Length; k++)//这是免疫所有Debuff的方法
				NPC.buffImmune[k] = true;
		}
		public float speed = 0f;//基础移动速度
		public float turnSpeed = 0f;//基础移动速度
		///<summary>根据游戏难度设置不同的属性值，不同难度下蠕虫的长度、速度和灵活性都不同，增加游戏趣味性 </summary>
		public override void Init()
		{
			if (Main.getGoodWorld && Main.drunkWorld && Main.tenthAnniversaryWorld && Main.dontStarveWorld 
				&& Main.notTheBeesWorld && Main.remixWorld && Main.noTrapsWorld && Main.zenithWorld)//其余模式更快，身体只给一节，纯整蛊
			{
				MinSegmentLength = 3;
				MaxSegmentLength = MinSegmentLength + 0;
				speed = 11f;
				turnSpeed = 0.25f;
			}
			if (Main.masterMode)//大师模式速度更快
			{
				MinSegmentLength = 32;
				MaxSegmentLength = MinSegmentLength + 1;
				speed = 9f; 
				turnSpeed = 0.15f;
			}
			else if (Main.expertMode)//专家模式速度次之
			{
				MinSegmentLength = 23;
				MaxSegmentLength = MinSegmentLength + 3;
				speed = 6f; 
				turnSpeed = 0.1f;
			}
			else//普通模式速度最慢
			{
				MinSegmentLength = 18;
				MaxSegmentLength = MinSegmentLength + 5;
				speed = 3f; 
				turnSpeed = 0.05f;
			}
			CommonWormInit(this);//调用通用初始化方法设置移动参数
		}
		//战斗阶段管理
		private int currentPhase = 1;//当前阶段
		private float lifeRatio = 1f;//生命值比率
		public float LifeRatio => lifeRatio;
		///<summary>蠕虫通用初始化方法 - 设置所有段的移动参数</summary>
		///<param name="worm">要初始化的蠕虫段</param>
		internal static void CommonWormInit(WormBoss worm)
		{
			if (worm is EvilWormHead head)//如果是头部段，使用Worm1Move的移动参数
			{
				worm.MoveSpeed = head.speed;//设置移动速度
				worm.Acceleration = head.turnSpeed;
			}
			else//身体和尾部段使用默认值，实际不用设，通常头部会覆盖这些值
			{
				worm.MoveSpeed = 0f;
				worm.Acceleration = 0f;
			}
		}
		///<summary>发送额外AI数据，用于多人游戏同步，这是Example教的，确保所有客户端有相同的阶段和参数数据</summary>
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(currentPhase);//同步当前阶段
			writer.Write(lifeRatio);   //同步生命值比率
			writer.Write(speed);       //同步速度参数
			writer.Write(turnSpeed);   //同步转向参数
		}
		public override void ReceiveExtraAI(BinaryReader reader)
		{
			currentPhase = reader.ReadInt32();//读取阶段
			lifeRatio = reader.ReadSingle();  //读取生命比率
			speed = reader.ReadSingle();      //读取速度
			turnSpeed = reader.ReadSingle();  //读取转向速度
		}
		private bool useWorm1MoveAI = true;//是否使用Worm1MoveAI
		public override void AI()//使用Worm1Move的移动
		{
			if (useWorm1MoveAI)
			{
				Worm1MoveAI();//使用Worm1MoveAI
			}
			else
			{
				base.AI();//回退到基类的基础AI
			}
			EvilWormHeadSkill.ExecuteSkills(NPC);//在AI末尾调用技能系统
			lifeRatio = GetLifeRatio();//更新生命值比率
			if (lifeRatio < 0.7f && currentPhase == 1)
				currentPhase = 2;//可以在这里添加第二阶段的行为
			else if (lifeRatio < 0.3f && currentPhase == 2)
				currentPhase = 3;//可以在这里添加第二阶段的行为
			else
				currentPhase = 1;//可以在这里添加第一阶段的行为
		}
		public override bool CheckDead()
		{
			return true;
		}
		public override void OnKill()
		{
			EvilWormHeadSkill.InterruptCurrentSkill();
			base.OnKill();
		}
		public override void BossLoot(ref int potionType)//BOSS掉落
		{
			potionType = ItemID.GreaterHealingPotion;//强效治疗药水
		}
		///<summary>Worm1Move风格的完整AI逻辑，智能追踪，有平滑的曲线移动、动态速度调整</summary>
		private void Worm1MoveAI()//Worm1Move的完整AI逻辑
		{
			//1. 初始化检查：确保有有效的realLife值（用于多段NPC生命共享）
			if (NPC.ai[3] > 0f)
				NPC.realLife = (int)NPC.ai[3];
			//2. 目标验证：检查当前目标是否有效，无效则寻找新目标
			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead)
				NPC.TargetClosest(true);
			//3. 身体段生成：在服务器端生成蠕虫的身体段
			if (Main.netMode != NetmodeID.MultiplayerClient)
				SpawnSegmentsIfNeeded();//按需生成身体和尾部段
			//4. 贴图方向设置：根据水平速度决定面朝方向
			if (NPC.velocity.X < 0f)//方向设置
				NPC.spriteDirection = 1;//向左移动，面朝左
			else if (NPC.velocity.X > 0f)
				NPC.spriteDirection = -1;//向右移动，面朝右
			//5. 目标死亡处理：如果目标死亡，寻找新目标但不强制攻击
			if (Main.player[NPC.target].dead)
				NPC.TargetClosest(false);//寻找目标但不强制攻击
			//6. 透明度渐变效果：实现淡入或其它视觉效果
			NPC.alpha -= 42;//alpha渐变效果
			if (NPC.alpha < 0)
				NPC.alpha = 0;//确保透明度不小于0
			//7. 消失条件检查：距离过远或尾部不存在时消失
			if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 15600f 
				|| !Terraria.NPC.AnyNPCs(TailType))//距离检查和消失逻辑
			{
				NPC.active = false;//标记为不活跃，将被游戏清理
				return;//直接返回，不执行后续逻辑
			}
			//8. 主要移动逻辑：执行Worm1Move风格的智能移动
			Worm1MoveMovement();//Worm1Move的主要移动逻辑
			//9. 状态更新：生命值比率
			lifeRatio = GetLifeRatio();
			 //10. 阶段转换检查：生命值低于30%时进入第二阶段
			if (lifeRatio < 0.7f && currentPhase == 1)
				currentPhase = 2;//可以在这里添加第二阶段的行为
			else if (lifeRatio < 0.3f && currentPhase == 2)
				currentPhase = 3;//可以在这里添加第二阶段的行为
			else
				currentPhase = 1;//可以在这里添加第一阶段的行为
		}
		///<summary>生成身体段逻辑，构建完整的蠕虫身体链，生成顺序：头部 → 身体1 → 身体2 → ... → 身体n → 尾部</summary>
		private void SpawnSegmentsIfNeeded()//生成身体段的逻辑
		{
			bool tailSpawned = NPC.ai[0] != 0f;//检查是否已经生成了尾部
			if (!tailSpawned)
			{
				int previous = NPC.whoAmI;//从头部开始，前一节是头部自身
				for (int i = 0; i < MaxSegmentLength; i++)//循环生成指定数量的段
				{
					int segmentType;//前MinSegmentLength-1节为身体，最后一节为尾部
					if (i < MinSegmentLength - 1) //-1因为头部已经存在
						segmentType = BodyType;//身体段
					else
						segmentType = TailType;//尾部段
					int newSegment = NPC.NewNPC(NPC.GetSource_FromAI(),
						(int)NPC.position.X + (NPC.width / 2),
						(int)NPC.position.Y + (NPC.height / 2),
						segmentType, NPC.whoAmI);//生成新的段NPC
					//设置新段的属性						 
					Main.npc[newSegment].realLife = NPC.whoAmI;//共享头部生命值
					Main.npc[newSegment].ai[2] = NPC.whoAmI;   //记录头部索引
					Main.npc[newSegment].ai[1] = previous;     //新段跟随前一节
					Main.npc[previous].ai[0] = newSegment;     //前一节记录新段为跟随者
					NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, newSegment);//确保所有客户端都知道新段的存在
					previous = newSegment;//更新前一节引用，用于下一次生成
				}
			}
		}
		///<summary>Worm1Move移动逻辑，可以预测移动、速度限制、平滑转向</summary>
		private void Worm1MoveMovement()//Worm1Move的移动逻辑
		{
			float currentSpeed = speed;//当前速度设置
			float currentTurnSpeed = turnSpeed;//当前转向速度
			Vector2 center = new Vector2(NPC.position.X + NPC.width * 0.5f, NPC.position.Y + NPC.height * 0.5f);//BOSS中心位置
			Player targetPlayer = Main.player[NPC.target];//目标玩家
			//计算玩家中心位置
			float targetX = targetPlayer.position.X + targetPlayer.width / 2;
			float targetY = targetPlayer.position.Y + targetPlayer.height / 2;
			if ((targetPlayer.Center - NPC.Center).Length() > 200f)//根据距离动态调整追踪方式
			{
				targetY -= 160;//向上偏移，尝试从上方接近玩家
				if (Math.Abs(NPC.Center.X - targetPlayer.Center.X) < 250f)//水平接近时，预测玩家移动方向
				{
					if (NPC.velocity.X > 0f)
					{
						targetX = targetPlayer.Center.X + 300f;//预测玩家向右移动
					}
					else
					{
						targetX = targetPlayer.Center.X - 300f;//预测玩家向左移动
					}
				}
			}
			//速度限制
			float maxSpeed = currentSpeed * 1.3f;//最大速度限制
			float minSpeed = currentSpeed * 0.7f;//最小速度限制
			float currentVelocityLength = NPC.velocity.Length();
			if (currentVelocityLength > 0f)//速度限制执行
			{
				if (currentVelocityLength > maxSpeed)
				{
					NPC.velocity.Normalize();
					NPC.velocity *= maxSpeed;//限制最大速度
				}
				else if (currentVelocityLength < minSpeed)
				{
					NPC.velocity.Normalize();
					NPC.velocity *= minSpeed;//限制最小速度
				}
			}
			//将坐标对齐到图块网格，提高移动稳定性
			targetX = (int)(targetX / 16f) * 16;
			targetY = (int)(targetY / 16f) * 16;
			center.X = (int)(center.X / 16f) * 16;
			center.Y = (int)(center.Y / 16f) * 16;
			//计算方向向量和距离
			float dirX = targetX - center.X;
			float dirY = targetY - center.Y;
			float distance = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
			float absDirX = Math.Abs(dirX);
			float absDirY = Math.Abs(dirY);
			//根据距离调整移动强度
			float speedRatio = currentSpeed / distance;
			dirX *= speedRatio;
			dirY *= speedRatio;
			//检查NPC是否正在朝目标方向移动
			if ((NPC.velocity.X > 0f && dirX > 0f) || (NPC.velocity.X < 0f && dirX < 0f) ||
				(NPC.velocity.Y > 0f && dirY > 0f) || (NPC.velocity.Y < 0f && dirY < 0f))
			{
				//同向移动，精细调整，平滑追踪
				if (NPC.velocity.X < dirX)//水平速度微调
					NPC.velocity.X += currentTurnSpeed;//加速向右
				else if (NPC.velocity.X > dirX)
					NPC.velocity.X -= currentTurnSpeed;//减速或反向
				if (NPC.velocity.Y < dirY)//垂直速度微调
					NPC.velocity.Y += currentTurnSpeed;//加速向下
				else if (NPC.velocity.Y > dirY)
					NPC.velocity.Y -= currentTurnSpeed;//减速或反向
				
				if (Math.Abs(dirY) < currentSpeed * 0.2f && ((NPC.velocity.X > 0f && dirX < 0f) 
					|| (NPC.velocity.X < 0f && dirX > 0f)))//在需要大幅转向时提供特殊转向
				{
					if (NPC.velocity.Y > 0f)
						NPC.velocity.Y += currentTurnSpeed * 2f;//向下助推
					else
						NPC.velocity.Y -= currentTurnSpeed * 2f;//向上助推
				}
				if (Math.Abs(dirX) < currentSpeed * 0.2f && ((NPC.velocity.Y > 0f && dirY < 0f) 
					|| (NPC.velocity.Y < 0f && dirY > 0f)))//X轴小移动但需要垂直转向：加强水平移动辅助转向
				{
					if (NPC.velocity.X > 0f)
						NPC.velocity.X += currentTurnSpeed * 2f;//向右助推
					else
						NPC.velocity.X -= currentTurnSpeed * 2f;//向左助推
				}
			}
			else//反向移动，需要大幅调整方向
			{
				if (absDirX > absDirY)//X轴距离主导，优先调整水平移动
				{
					if (NPC.velocity.X < dirX)
						NPC.velocity.X += currentTurnSpeed * 1.1f;//强力加速
					else if (NPC.velocity.X > dirX)
						NPC.velocity.X -= currentTurnSpeed * 1.1f;//强力减速

					if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < currentSpeed * 0.5f)//总体速度过慢时添加垂直移动补偿
					{
						if (NPC.velocity.Y > 0f)
							NPC.velocity.Y += currentTurnSpeed;//向下补偿
						else
							NPC.velocity.Y -= currentTurnSpeed;//向上补偿
					}
				}
				else//Y轴距离主导，优先调整垂直移动
				{
					if (NPC.velocity.Y < dirY)
						NPC.velocity.Y += currentTurnSpeed * 1.1f;//强力加速
					else if (NPC.velocity.Y > dirY)
						NPC.velocity.Y -= currentTurnSpeed * 1.1f;//强力减速

					if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < currentSpeed * 0.5f)
					{
						if (NPC.velocity.X > 0f)
							NPC.velocity.X += currentTurnSpeed;//向右补偿
						else
							NPC.velocity.X -= currentTurnSpeed;//向左补偿
					}
				}
			}
			NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + MathHelper.PiOver2;//设置旋转
		}
		///<summary>计算当前生命值比率，用于阶段转换和状态判断</summary>
		private float GetLifeRatio()
		{
			return (float)NPC.life / NPC.lifeMax;
		}
		///<summary>受击效果 - 生成视觉效果和死亡处理</summary>
		public override void HitEffect(NPC.HitInfo hit)
		{
			for (int k = 0; k < 23; k++)//受击时生成粒子
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.CrystalPulse, hit.HitDirection, -1f, 0, default, 1f);
			if (NPC.life <= 0)
				for (int k = 0; k < 33; k++)//死亡时生成粒子
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.CrystalPulse, hit.HitDirection, -1f, 0, default, 1f);
		}
		///<summary>击中玩家效果，常见对玩家施加Debuff</summary>
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)//Worm1Move的玩家击中效果
		{
			target.AddBuff(BuffID.Slow, 121, true);//缓慢
			target.AddBuff(BuffID.Venom, 121, true);//剧毒
		}
		///<summary>活动状态检查，用于处理Boss死亡的逻辑</summary>
		public override bool CheckActive()
		{
			if (NPC.timeLeft < 0 && Main.netMode != NetmodeID.MultiplayerClient)//当Boss战结束且不在客户端时，清理所有身体段
			{
				for (int k = (int)NPC.ai[0]; k > 0; k = (int)Main.npc[k].ai[0])//从头部开始，遍历整个身体链并清理
				{
					if (Main.npc[k].active)
					{
						Main.npc[k].active = false;//标记为不活动
						if (Main.netMode == NetmodeID.Server)//服务器端同步处理
						{
							Main.npc[k].life = 0;//设置生命为0
							Main.npc[k].netSkip = -1;//网络同步标记
							NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, k);//同步状态
						}
					}
				}
			}
			return true;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;//加载并设置绘制参数
			Rectangle sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
			Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);//屏幕空间位置
			Vector2 drawPos = NPC.Center - screenPos;
			Color color = NPC.GetAlpha(drawColor);
			SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;//根据移动方向决定是否水平翻转
			spriteBatch.Draw(texture, drawPos, sourceRect, color, NPC.rotation, origin, NPC.scale, effects, 0f);
			return false;
		}
	}
	#endregion
	#region 身
	internal class EvilWormBody : WormBody
	{
		public override string Texture => Pictures.EvilWorm + Name;
		private int frameCounter = 0;//帧计数器
		private int currentFrame = 0;//当前帧索引
		private const int FrameDelayNormal = 170;//满血时的帧延迟：生命值100%时，每170帧切换一次动画
		private const int FrameDelayLow = 10;//低血时的帧延迟：生命值10%以下时，每10帧切换一次动画
		private const int SpecialFrameDuration = 10;//亮灯持续时间：10帧
		///<summary>设置静态默认值，配置怪物图鉴相关设置，它只需在游戏加载时执行一次，用于定义NPC的全局行为</summary>
		public override void SetStaticDefaults()
		{
			NPCID.Sets.NPCBestiaryDrawModifiers value = new() { Hide = true };//设置身体段在图鉴中的显示方式，此时在图鉴中隐藏
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);//将修饰器添加到图鉴绘制偏移设置中
			NPCID.Sets.RespawnEnemyID[NPC.type] = ModContent.NPCType<EvilWormHead>();//当此NPC死亡时，游戏知道应该生成哪个头部来重生整个蠕虫
		}
		///<summary>配置每个身体段NPC的具体属性，这些设置会在每个身体段NPC生成时应用，决定其基础行为特性</summary>
		public override void SetDefaults()
		{
			//基础属性
			NPC.width = 50;//NPC碰撞箱宽度，不是贴图的，贴图后续我们自己绘制
			NPC.height = 74;//NPC碰撞箱高度，不是贴图的，贴图后续我们自己绘制
			NPC.aiStyle = -1;//-1是使用自定义AI，不使用原版AI样式，AI后续我们自己写
			//战斗属性
			NPC.lifeMax = 33000;//最大生命值
			NPC.defense = 30;//防御力
			NPC.damage = 50;//接触伤害
			NPC.knockBackResist = 0f;//击退抗性（0是不可击退）
			//Boss特性
			NPC.boss = true;//标记为Boss，会显示Boss血条
			NPC.noGravity = true;//不受重力影响
			NPC.noTileCollide = true;//忽略图块碰撞
			//旗帜和掉落物
			Banner = Type;//对应旗帜类型
			BannerItem = ModContent.ItemType<EvilBait>();//旗帜物品，掉啥旗帜啊，不如掉召唤物，看后续能换成武器不，还没想好
			ItemID.Sets.KillsToBanner[BannerItem] = 3;//击杀3个获得旗帜
			for (int k = 0; k < NPC.buffImmune.Length; k++)//这是免疫所有Debuff的方法
				NPC.buffImmune[k] = true;
		}
		///<summary>调用头部的通用初始化逻辑</summary>
		public override void Init()
		{
			EvilWormHead.CommonWormInit(this);//调用头部定义的通用初始化方法，确保身体段与头部使用相同的移动参数
		}
		///<summary>计算生命值比率，蠕虫所有段共享头部的生命值，实现统一的Boss血条</summary>
		private float GetLifeRatio()
		{
			if (NPC.realLife >= 0 && NPC.realLife < Main.maxNPCs)//通过realLife找到头部NPC
			{
				NPC head = Main.npc[NPC.realLife];//通过realLife找到对应的头部NPC实例
				if (head.active)//确认头部存在且处于活动状态
					return (float)head.life / head.lifeMax;//计算头部当前生命值与最大生命值的比率
			}
			return 1f;//默认值
		}
		///<summary> 计算当前帧，根据生命值比率动态调整动画播放速度，生命值越低，动画播放越快</summary>
		///<returns>计算后的帧延迟数值</returns>
		private int CalculateCurrentFrameDelay()
		{
			float lifeRatio = GetLifeRatio();//获取当前生命值比率（0.0到1.0之间的浮点数）
											 //线性插值计算：根据生命值比率在最大和最小延迟之间平滑过渡
											 //公式：延迟 = 正常延迟 + (低血延迟 - 正常延迟) × (1 - 生命比率)
											 //当生命比率为1.0（满血）时：延迟 = 170 + (10-170)×0 = 170
											 //当生命比率为0.1（10%血）时：延迟 = 170 + (10-170)×0.9 = 170 - 144 = 26
			float frameDelay = FrameDelayNormal + (FrameDelayLow - FrameDelayNormal) * (1f - lifeRatio);
			frameDelay = MathHelper.Clamp(frameDelay, FrameDelayLow, FrameDelayNormal);//限制帧延迟在有效范围内，避免计算错误导致异常值
			return (int)frameDelay;//将结果转换为整数返回
		}
		///<summary>查找帧方法，控制身体段的动画帧更新逻辑。每帧调用一次，决定当前应该显示纹理的哪一部分作为动画帧</summary>
		///<param name="frameHeight">每帧的高度（由基类自动计算）</param>
		public override void FindFrame(int frameHeight)
		{
			float lifeRatio = GetLifeRatio();//获取当前生命值比率，用于动态动画速度计算
			int currentFrameDelay = CalculateCurrentFrameDelay();//根据生命值计算当前应该使用的帧延迟
			frameCounter++;//增加帧计数器，记录当前帧已显示的时长
			bool shouldShowSpecialFrame = false;//判断当前是否应该受伤状态帧
			if (frameCounter <= SpecialFrameDuration)//在特定时间窗口内显示受伤状态帧
				shouldShowSpecialFrame = true;
			else if (frameCounter >= currentFrameDelay)//检查是否达到帧切换条件
			{
				frameCounter = 0;//重置帧计数器，开始新的动画周期
				shouldShowSpecialFrame = true;//新周期开始显示受伤状态帧
			}
			if (shouldShowSpecialFrame)//根据标志设置当前帧索引
				currentFrame = 0;//显示第0帧（受伤状态帧）
			else
				currentFrame = 1;//显示第1帧（正常状态帧）
			//更新NPC帧的Y坐标：根据帧索引计算在纹理中的垂直位置
			NPC.frame.Y = currentFrame * (NPC.frame.Height / 2);//它包含2帧垂直排列，每帧高度为纹理总高度的一半
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			int frameHeight = texture.Height / 2;//两帧上下排列，每帧高度为原图的一半
			int currentFrame = NPC.frame.Y / frameHeight;
			Rectangle sourceRect = new Rectangle(0, currentFrame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = new Vector2(texture.Width / 2, frameHeight / 2);//计算绘制位置和旋转
			Vector2 drawPos = NPC.Center - screenPos;
			Color color = NPC.GetAlpha(drawColor);//获取NPC颜色
			SpriteEffects effects = SpriteEffects.None;//根据NPC方向调整旋转
			if (NPC.spriteDirection == -1) effects = SpriteEffects.FlipHorizontally;
			spriteBatch.Draw(texture, drawPos, sourceRect, color, NPC.rotation, origin, NPC.scale, effects, 0f);//绘制NPC
			return false;
		}
	}
	#endregion
	#region 尾
	///<summary>设置尾部段的基本属性，属性值与头部和身体段保持一致，确保行为一致性</summary>
	internal class EvilWormTail : WormTail
	{
		public override string Texture => Pictures.EvilWorm + Name;
		///<summary>设置静态默认值，配置怪物图鉴相关设置，它只需在游戏加载时执行一次，用于定义NPC的全局行为</summary>
		public override void SetStaticDefaults()
		{
			NPCID.Sets.NPCBestiaryDrawModifiers value = new() { Hide = true };//设置身体段在图鉴中的显示方式，此时在图鉴中隐藏
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);//将修饰器添加到图鉴绘制偏移设置中
			NPCID.Sets.RespawnEnemyID[NPC.type] = ModContent.NPCType<EvilWormHead>();//当此NPC死亡时，游戏知道应该生成哪个头部来重生整个蠕虫
		}
		public override void SetDefaults()
		{
			//基础属性
			NPC.width = 50;//NPC碰撞箱宽度，不是贴图的，贴图后续我们自己绘制
			NPC.height = 74;//NPC碰撞箱高度，不是贴图的，贴图后续我们自己绘制
			NPC.aiStyle = -1;//-1是使用自定义AI，不使用原版AI样式，AI后续我们自己写
			//战斗属性
			NPC.lifeMax = 33000;//最大生命值
			NPC.defense = 20;//防御力
			NPC.damage = 40;//接触伤害
			NPC.knockBackResist = 0f;//击退抗性（0是不可击退）
			//Boss特性
			NPC.boss = true;//标记为Boss，会显示Boss血条
			NPC.noGravity = true;//不受重力影响
			NPC.noTileCollide = true;//忽略图块碰撞
			//旗帜和掉落物
			Banner = Type;//对应旗帜类型
			BannerItem = ModContent.ItemType<EvilBait>();//旗帜物品，掉啥旗帜啊，不如掉召唤物，看后续能换成武器不，还没想好
			ItemID.Sets.KillsToBanner[BannerItem] = 3;//击杀3个获得旗帜
			for (int k = 0; k < NPC.buffImmune.Length; k++)//这是免疫所有Debuff的方法
				NPC.buffImmune[k] = true;
		}
		public override void Init()
		{
			EvilWormHead.CommonWormInit(this);
		}
		///<summary>在基类AI基础上添加连接稳定性检查，每帧调用一次，控制尾部段的行为逻辑</summary>
		public override void AI()
		{
			base.AI();//实现基本的跟随移动行为
			if (Main.netMode != NetmodeID.MultiplayerClient)//仅在服务器端执行连接检查
			{
				NPC following = FollowingNPC;//获取当前跟随的段（应该是最后一个身体段）
				if (following == null || !following.active ||
					following.type != ModContent.NPCType<EvilWormBody>())//确保跟随的段存在、活动且类型正确
					if (Main.rand.NextBool(10))//以一定概率尝试重新连接
					{
						TryReconnectToBody();
					}
			}
		}
		///<summary>尝试重新连接到身体段，网络同步问题导致连接丢失；身体段异常消失后的自动修复；加载错误导致的连接关系错误</summary>
		private void TryReconnectToBody()
		{
			foreach (NPC npc in Main.npc)//遍历所有活跃的NPC，寻找合适的身体段进行重新连接
			{
				if (npc.active && npc.type == ModContent.NPCType<EvilWormBody>() && 
					npc.realLife == NPC.realLife)//连接条件检查：NPC处于活动状态；NPC类型为身体段；NPC与当前尾部属于同一个蠕虫
				{
					if (npc.ai[0] == 0)
					{
						//建立双向连接关系
						npc.ai[0] = NPC.whoAmI;//身体段记录尾部为它的跟随者
						NPC.ai[1] = npc.whoAmI;//尾部记录身体段为它的跟随目标
						break;//连接成功后立即退出循环
					}
				}
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Rectangle sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);//尾部只有一帧，使用整个纹理					 
			Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 3);//计算绘制位置和旋转
			Vector2 drawPos = NPC.Center - screenPos;
			Color color = NPC.GetAlpha(drawColor);//获取NPC颜色
			SpriteEffects effects = SpriteEffects.None;//根据NPC方向调整旋转
			if (NPC.spriteDirection == -1) effects = SpriteEffects.FlipHorizontally;
			spriteBatch.Draw(texture, drawPos, sourceRect, color, NPC.rotation, origin, NPC.scale, effects, 0f);
			return false;
		}
	}
	#endregion
}