using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.NPCs.EvilWorm
{	
	///<summary> 蠕虫分段类型枚举， 用于标识每个NPC段在蠕虫中的角色位置</summary>
	public enum WormSegmentType
	{
		///<summary>头部段。负责追踪玩家和控制整体移动方向</summary>
		Head,
		///<summary>身体段。跟随前一段移动，形成连贯的蠕虫身体</summary>
		Body,
		///<summary>尾部段。与身体段具有相同的AI。任何给定蠕虫只有一个"尾部"被视为活动状态</summary>
		Tail
	}
	///<summary>蠕虫敌怪基类，提供蠕虫类敌怪的基础框架和通用功能</summary>
	public abstract class WormBoss : ModNPC
	{
		/*  
		 * ai[] 用法：
		 * ai[0] = "跟随者"段，即跟随此段的段
		 * ai[1] = "被跟随"段，即此段跟随的段
		 * localAI[0] = 同步碰撞检测变化时使用
		 * localAI[1] = 检查Init()是否被调用过
		 * 这种链表结构让蠕虫的各段形成链式连接：头部 → 身体1 → 身体2 → ... → 身体n → 尾部
		 */
		///<summary>此NPC被视为哪种段类型（头部/身体/尾部）</summary>
		public abstract WormSegmentType SegmentType { get; }
		///<summary>NPC的最大速度（像素/帧）</summary>
		public float MoveSpeed { get; set; }
		///<summary> NPC加速的速率（像素/帧²），决定转向和加速的灵敏度</summary>
		public float Acceleration { get; set; }
		///<summary>蠕虫头部段的NPC实例</summary>
		public NPC HeadSegment => Main.npc[NPC.realLife];
		///<summary>跟随此段的NPC实例（ai[1]）。头部段属性始终返回null</summary>
		public NPC FollowingNPC => SegmentType == WormSegmentType.Head ? null : Main.npc[(int)NPC.ai[1]];
		///<summary>跟随此段的NPC实例（ai[0]）。尾部段属性始终返回null</summary>
		public NPC FollowerNPC => SegmentType == WormSegmentType.Tail ? null : Main.npc[(int)NPC.ai[0]];
		///<summary>控制血条的显示：仅头部显示血条，身体和尾部不显示</summary>
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			return SegmentType == WormSegmentType.Head ? null : false;
		}
		private bool startDespawning;//标记是否开始消失过程
		///<summary>主AI入口点，根据段类型分发到不同的AI逻辑</summary>
		public sealed override bool PreAI()
		{
			if (NPC.localAI[1] == 0)//初始化检查：确保Init()方法只被调用一次
			{
				NPC.localAI[1] = 1f;
				Init();//调用子类的初始化方法
			}
			if (SegmentType == WormSegmentType.Head)//根据段类型执行不同的AI逻辑
			{
				HeadAI();//头部负责追踪玩家
				if (!NPC.HasValidTarget)//目标丢失
				{
					NPC.TargetClosest(true);
					if (!NPC.HasValidTarget && NPC.boss)//如果NPC是Boss且没有目标，强制其快速往下
					{
						NPC.velocity.Y += 8f;//钻入地下脱战
						MoveSpeed = 1000f;
						if (!startDespawning)
						{
							startDespawning = true;
							NPC.timeLeft = 90;//如果NPC离得足够远，在90帧后消失
						}
					}
				}
			}
			else
			{
				BodyTailAI();//身体和尾部跟随前一段移动
			}
			return true;
		}
		//内部AI方法，由子类实现具体行为
		internal virtual void HeadAI() { }
		internal virtual void BodyTailAI() { }
		public abstract void Init();
	}
	///<summary>蠕虫敌怪头部段NPC的基类</summary>
	public abstract class WormHead : WormBoss
	{
		public sealed override WormSegmentType SegmentType => WormSegmentType.Head;
		///<summary>身体段NPC的NPCID或ModContent.NPCType，仅当HasCustomBodySegments返回false时使用此属性</summary>
		public abstract int BodyType { get; }
		///<summary>尾部段NPC的NPCID或ModContent.NPCType，仅当HasCustomBodySegments返回false时使用此属性</summary>
		public abstract int TailType { get; }
		///<summary>预期的最小段数，包括头部和尾部段</summary>
		public int MinSegmentLength { get; set; }
		///<summary>预期的最大段数，包括头部和尾部段</summary>
		public int MaxSegmentLength { get; set; }
		///<summary>NPC在尝试"挖掘"通过图块时是否忽略图块碰撞</summary>
		public bool CanFly { get; set; }
		///<summary>如果CanFly返回false，即不能飞行时使用图块碰撞的最大检测距离（像素）</summary>
		public virtual int MaxDistanceForUsingTileCollision => 1200;
		///<summary>NPC是否使用自定义身体段，默认false</summary>
		public virtual bool HasCustomBodySegments => false;
		///<summary>如果不为null，此NPC将瞄准给定的世界位置而不是其玩家目标</summary>
		public Vector2? ForcedTargetPosition { get; set; }
		///<summary>重写此方法以使用自定义身体生成代码，仅当HasCustomBodySegments返回true时运行此方法</summary>
		///<param name="segmentCount">预期生成的身体段数量</param>
		///<returns>最近生成的NPC的whoAmI，即调用NPC.NewNPC的结果</returns>
		public virtual int SpawnBodySegments(int segmentCount)
		{
			return NPC.whoAmI;//默认返回whoAmI，因为尾部段使用返回值作为其跟随的NPC索引
		}
		///<summary>生成一个新的蠕虫段</summary>
		///<param name="source">生成源</param>
		///<param name="type">要生成的段NPC的ID</param>
		///<param name="latestNPC">蠕虫中最近生成的段NPC的whoAmI，包括头部</param>
		///<returns>新生成段的ID</returns>
		protected int SpawnSegment(IEntitySource source, int type, int latestNPC)
		{
			//生成一个新的NPC，将latestNPC设置为较新的NPC，同时使用该变量设置此新NPC的父级
			int oldLatest = latestNPC;
			latestNPC = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, type, NPC.whoAmI, 0, latestNPC);
			//设置双向连接：前一段指向新段，新段指向前一段
			Main.npc[oldLatest].ai[0] = latestNPC;//前段的跟随者=新段
			NPC latest = Main.npc[latestNPC];
			latest.realLife = NPC.whoAmI;//所有段共享相同的血量（指向头部）
			return latestNPC;
		}
		///<summary>头部AI主逻辑流程</summary>
		internal sealed override void HeadAI()
		{
			HeadAI_SpawnSegments();//生成身体段
			bool collision = HeadAI_CheckCollisionForDustSpawns();//检测碰撞
			HeadAI_CheckTargetDistance(ref collision);//检查目标距离
			HeadAI_Movement(collision);//执行移动逻辑
		}
		///<summary>生成蠕虫身体段（只在首次运行时执行）</summary>
		private void HeadAI_SpawnSegments()
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)//只在服务器端执行
			{
				//通过检查NPC.ai[0]（跟随的NPC的whoAmI）是否为0来开始AI
				bool hasFollower = NPC.ai[0] > 0;
				if (!hasFollower)
				{
					NPC.realLife = NPC.whoAmI;//设置realLife为头部ID
					int latestNPC = NPC.whoAmI;
					int randomWormLength = Main.rand.Next(MinSegmentLength, MaxSegmentLength);//随机确定蠕虫的长度
					int distance = randomWormLength - 2;//需要生成的身体段数量
					IEntitySource source = NPC.GetSource_FromAI();
					if (HasCustomBodySegments)//选择生成方式：自定义或标准
					{
						latestNPC = SpawnBodySegments(distance);//调用处理生成身体段的方法
					}
					else
					{
						while (distance > 0)//生成身体段
						{
							latestNPC = SpawnSegment(source, BodyType, latestNPC);
							distance--;
						}
					}
					SpawnSegment(source, TailType, latestNPC);//生成尾部段，即最后一段
					NPC.netUpdate = true;//同步到客户端
					int count = 0;
					foreach (var n in Main.ActiveNPCs)//确保所有段都能生成
					{
						if ((n.type == Type || n.type == BodyType || n.type == TailType) && n.realLife == NPC.whoAmI)
							count++;
					}
					if (count != randomWormLength)//如果段数量不匹配，清理无效的段。检查Bug：如果飞出去太远，尾部消失了，多半是这一段的问题
					{
						foreach (var n in Main.ActiveNPCs)//如果无法生成所有段，杀死蠕虫
						{
							if ((n.type == Type || n.type == BodyType || n.type == TailType) && n.realLife == NPC.whoAmI)
							{
								n.active = false;
								n.netUpdate = true;
							}
						}
					}
					NPC.TargetClosest(true);//锁定玩家为目标
				}
			}
		}
		///<summary>检测与图块的碰撞（用于决定移动模式和生成粒子效果）</summary>
		private bool HeadAI_CheckCollisionForDustSpawns()
		{
			//计算NPC周围的图块检测范围
			int minTilePosX = (int)(NPC.Left.X / 16) - 1;
			int maxTilePosX = (int)(NPC.Right.X / 16) + 2;
			int minTilePosY = (int)(NPC.Top.Y / 16) - 1;
			int maxTilePosY = (int)(NPC.Bottom.Y / 16) + 2;
			//约束检测范围在世界边界内
			if (minTilePosX < 0) minTilePosX = 0;
			if (maxTilePosX > Main.maxTilesX) maxTilePosX = Main.maxTilesX;
			if (minTilePosY < 0) minTilePosY = 0;
			if (maxTilePosY > Main.maxTilesY) maxTilePosY = Main.maxTilesY;
			bool collision = false;
			//遍历检测范围内的所有图块
			for (int i = minTilePosX; i < maxTilePosX; ++i)
			{
				for (int j = minTilePosY; j < maxTilePosY; ++j)
				{
					Tile tile = Main.tile[i, j];
					if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType] && tile.TileFrameY == 0) 
						|| tile.LiquidAmount > 64)//如果图块是可碰撞（实心块、平台或含有大量液体）的
					{
						Vector2 tileWorld = new Point16(i, j).ToWorldCoordinates(0, 0);
						if (NPC.Right.X > tileWorld.X && NPC.Left.X < tileWorld.X + 16 && NPC.Bottom.Y > tileWorld.Y 
							&& NPC.Top.Y < tileWorld.Y + 16)//检查NPC是否与图块重叠
						{
							collision = true;//发现碰撞
							if (Main.rand.NextBool(1))//小概率破坏图块
								WorldGen.KillTile(i, j, fail: true, effectOnly: true, noItem: false);
						}
					}
				}
			}
			return collision;
		}
		///<summary>检查与目标的距离，决定是否触发虚拟碰撞</summary>
		private void HeadAI_CheckTargetDistance(ref bool collision)
		{
			//如果没有与图块碰撞，检查此NPC与其目标之间的距离是否过大，以便能触发碰撞
			if (!collision)//只有在没有实际碰撞时才检查距离
			{
				Rectangle hitbox = NPC.Hitbox;
				int maxDistance = MaxDistanceForUsingTileCollision;
				bool tooFar = true;//目标是否过远
				foreach (var player in Main.ActivePlayers)//检查所有活跃玩家
				{
					Rectangle areaCheck;
					if (ForcedTargetPosition is Vector2 target)//确定检测区域（围绕玩家或强制目标位置）
						areaCheck = new Rectangle((int)target.X - maxDistance, (int)target.Y - maxDistance, maxDistance * 2, maxDistance * 2);
					else if (!player.dead && !player.ghost)
						areaCheck = new Rectangle((int)player.position.X - maxDistance, (int)player.position.Y - maxDistance, maxDistance * 2, maxDistance * 2);
					else
						continue;//不是有效玩家
					if (hitbox.Intersects(areaCheck))//如果NPC在检测区域内，标记为未过远
					{
						tooFar = false;
						break;
					}
				}
				if (tooFar)//如果所有目标都过远，触发虚拟碰撞
					collision = true;
			}
		}
		///<summary>头部段的主要移动逻辑，控制蠕虫BOSS的整体移动行为</summary>
		///<param name="collision">是否检测到碰撞</param>
		private void HeadAI_Movement(bool collision)
		{
			float speed = MoveSpeed;
			float acceleration = Acceleration;
			float targetXPos, targetYPos;
			//获取目标位置（玩家位置或强制目标）
			Player playerTarget = Main.player[NPC.target];
			Vector2 forcedTarget = ForcedTargetPosition ?? playerTarget.Center;//使用强制目标位置或玩家位置作为目标
			(targetXPos, targetYPos) = (forcedTarget.X, forcedTarget.Y);
			Vector2 npcCenter = NPC.Center;
			//将位置对齐到图块网格（16x16像素的网格）
			float targetRoundedPosX = (int)(targetXPos / 16f) * 16;
			float targetRoundedPosY = (int)(targetYPos / 16f) * 16;
			npcCenter.X = (int)(npcCenter.X / 16f) * 16;
			npcCenter.Y = (int)(npcCenter.Y / 16f) * 16;
			//计算到目标的方向向量
			float dirX = targetRoundedPosX - npcCenter.X;
			float dirY = targetRoundedPosY - npcCenter.Y;
			float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);//计算到目标的距离
			if (!collision && !CanFly)
				HeadAI_Movement_HandleFallingFromNoCollision(dirX, speed, acceleration);//如果没有碰撞且不能飞行，使用下落移动逻辑
			else//否则播放挖掘声音并使用正常移动逻辑
			{
				HeadAI_Movement_PlayDigSounds(length);
				HeadAI_Movement_HandleMovement(dirX, dirY, length, speed, acceleration);
			}
			HeadAI_Movement_SetRotation(collision);
		}
		///<summary>处理无碰撞时的下落移动，当蠕虫在空中且没有目标时的行为</summary>
		private void HeadAI_Movement_HandleFallingFromNoCollision(float dirX, float speed, float acceleration)
		{
			NPC.TargetClosest(true);//持续寻找目标
			NPC.velocity.Y += 0.01f;//轻微重力
			if (NPC.velocity.Y > speed)//限制最大下落速度，确保NPC不会下落太快
				NPC.velocity.Y = speed;
			//低速状态
			if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.4f)//如果当前速度较慢，加速向目标方向移动
			{
				if (NPC.velocity.X < 0.0f)//NPC已达到极限速度
					NPC.velocity.X -= acceleration * 1.1f;
				else
					NPC.velocity.X += acceleration * 1.1f;
			}
			//最大下落速度状态
			else if (NPC.velocity.Y == speed)//如果已达到极限下落速度，水平调整位置
			{
				if (NPC.velocity.X < dirX)//NPC已达到极限速度
					NPC.velocity.X += acceleration;
				else if (NPC.velocity.X > dirX)
					NPC.velocity.X -= acceleration;
			}
			//快速下落状态
			else if (NPC.velocity.Y > 4)//如果下落速度较快，减缓水平移动
			{
				if (NPC.velocity.X < 0)
					NPC.velocity.X += acceleration * 1f;
				else
					NPC.velocity.X -= acceleration * 1f;
			}
		}
		///<summary>播放挖掘声音，距离目标越近，声音播放频率越高</summary>
		private void HeadAI_Movement_PlayDigSounds(float length)
		{
			if (NPC.soundDelay == 0)
			{
				float Sound1 = length / 40f;//NPC越接近目标位置，播放声音越快
				//限制声音延迟在10-20帧之间
				if (Sound1 < 10) Sound1 = 10f;
				if (Sound1 > 20) Sound1 = 20f;
				NPC.soundDelay = (int)Sound1;
				SoundEngine.PlaySound(SoundID.WormDig, NPC.position);
			}
		}
		///<summary>
		///主要移动处理逻辑，控制蠕虫向目标移动的精确行为
		///</summary>
		private void HeadAI_Movement_HandleMovement(float dirX, float dirY, float length, float speed, float acceleration)
		{
			float absDirX = Math.Abs(dirX);
			float absDirY = Math.Abs(dirY);
			float newSpeed = speed / length;//将方向向量归一化到最大速度
			dirX *= newSpeed;
			dirY *= newSpeed;
			bool movingTowardTarget = (NPC.velocity.X > 0 && dirX > 0) || (NPC.velocity.X < 0 && dirX < 0) ||
									(NPC.velocity.Y > 0 && dirY > 0) || (NPC.velocity.Y < 0 && dirY < 0);//检查NPC是否正朝目标方向移动
			if (movingTowardTarget)//如果NPC正朝目标位置移动，微调速度使其更接近理想方向
			{
				
				if (NPC.velocity.X < dirX)//水平移动调整
					NPC.velocity.X += acceleration;
				else if (NPC.velocity.X > dirX)
					NPC.velocity.X -= acceleration;
				if (NPC.velocity.Y < dirY)//垂直移动调整
					NPC.velocity.Y += acceleration;
				else if (NPC.velocity.Y > dirY)
					NPC.velocity.Y -= acceleration;
				//智能转向：当需要大幅转向时，通过加强垂直/水平移动来帮助NPC更流畅地转向
				if (Math.Abs(dirY) < speed * 0.2 && (NPC.velocity.X > 0 && dirX < 0 
					|| NPC.velocity.X < 0 && dirX > 0))//预期的Y速度很小且NPC向左移动而目标在NPC右侧，或反之。这有助于蠕虫在需要转弯时调整高度
				{
					if (NPC.velocity.Y > 0)
						NPC.velocity.Y += acceleration * 2f;
					else
						NPC.velocity.Y -= acceleration * 2f;
				}
				
				if (Math.Abs(dirX) < speed * 0.2 && (NPC.velocity.Y > 0 && dirY < 0 
					|| NPC.velocity.Y < 0 && dirY > 0))//预期的X速度很小且NPC向上/下移动而目标在NPC下方/上方。这有助于蠕虫在需要改变高度时调整水平位置
				{
					if (NPC.velocity.X > 0)
						NPC.velocity.X = NPC.velocity.X + acceleration * 2f;
					else
						NPC.velocity.X = NPC.velocity.X - acceleration * 2f;
				}
			}
			else if (absDirX > absDirY)//X轴距离主导，优先水平移动
			{
				if (NPC.velocity.X < dirX)//X距离大于Y距离，强制X轴移动更强
					NPC.velocity.X += acceleration * 1.1f;
				else if (NPC.velocity.X > dirX)
					NPC.velocity.X -= acceleration * 1.1f;
				if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5)//如果总体速度较慢，添加垂直移动
				{
					if (NPC.velocity.Y > 0)
						NPC.velocity.Y += acceleration;
					else
						NPC.velocity.Y -= acceleration;
				}
			}
			else//Y轴距离主导，优先垂直移动
			{
				if (NPC.velocity.Y < dirY)//Y距离大于X距离，强制Y轴移动更强
					NPC.velocity.Y += acceleration * 1.1f;
				else if (NPC.velocity.Y > dirY)
					NPC.velocity.Y -= acceleration * 1.1f;
				if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < speed * 0.5)//如果总体速度较慢，添加水平移动
				{
					if (NPC.velocity.X > 0)
						NPC.velocity.X += acceleration;
					else
						NPC.velocity.X -= acceleration;
				}
			}
		}
		///<summary>设置NPC旋转和网络同步</summary>
		private void HeadAI_Movement_SetRotation(bool collision)
		{
			NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;//设置此NPC的正确旋转，面朝位移方向
			if (collision)//一些网络更新内容（多人游戏兼容性），同步碰撞状态变化网络同步
			{
				if (NPC.localAI[0] != 1)
					NPC.netUpdate = true;
				NPC.localAI[0] = 1f;
			}
			else
			{
				if (NPC.localAI[0] != 0)
					NPC.netUpdate = true;
				NPC.localAI[0] = 0f;
			}
			//如果NPC的速度改变符号且未被玩家击中，则强制网络更新，确保在方向突变时所有客户端都能正确同步
			if ((NPC.velocity.X > 0 && NPC.oldVelocity.X < 0 || NPC.velocity.X < 0 && NPC.oldVelocity.X > 0 ||
				NPC.velocity.Y > 0 && NPC.oldVelocity.Y < 0 || NPC.velocity.Y < 0 && NPC.oldVelocity.Y > 0) && !NPC.justHit)
				NPC.netUpdate = true;
		}
	}
	///<summary> 蠕虫身体段基类，负责跟随前一段移动，形成连贯的蠕虫身体</summary>
	public abstract class WormBody : WormBoss
	{
		public sealed override WormSegmentType SegmentType => WormSegmentType.Body;
		internal override void BodyTailAI()
		{
			CommonAI_BodyTail(this);//使用通用的身体/尾部AI
		}
		///<summary>身体和尾部段的通用AI逻辑，保持与前一段的连接距离，形成连贯的蠕虫身体</summary>
		internal static void CommonAI_BodyTail(WormBoss worm)
		{
			if (!worm.NPC.HasValidTarget)//确保有有效目标
				worm.NPC.TargetClosest(true);
			if (Main.player[worm.NPC.target].dead && worm.NPC.timeLeft > 30000)//如果目标死亡，缩短存在时间
				worm.NPC.timeLeft = 10;
			NPC following = worm.NPC.ai[1] >= Main.maxNPCs ? null : worm.FollowingNPC;//获取此段跟随的前一段
			if (Main.netMode != NetmodeID.MultiplayerClient)//服务器端检查：如果跟随的段无效，移除此段
			{
				if (following is null || !following.active || following.friendly || following.townNPC || following.lifeMax <= 5)
				{
					worm.NPC.life = 0;
					worm.NPC.HitEffect(0, 10);
					worm.NPC.active = false;
				}
			}
			if (following is not null && following.active)//如果有有效的跟随段，调整位置以保持连接
			{
				//计算到跟随段的方向
				float dirX = following.Center.X - worm.NPC.Center.X;
				float dirY = following.Center.Y - worm.NPC.Center.Y;
				worm.NPC.rotation = (float)Math.Atan2(dirY, dirX) + MathHelper.PiOver2;//设置旋转指向跟随段
				//计算距离并调整位置以保持连接																	   
				float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
				/* 距离计算公式：理想距离 = (前段半径 + 本段半径) × 0.9，0.9是重叠系数（调整体节紧密程度的），让段之间有一定重叠，避免出现缝隙 */
				float dist = (length - (following.width / 2f + worm.NPC.width / 2f) * 0.9f) / length;//距离调整比例
				//计算位置调整量
				float posX = dirX * dist;
				float posY = dirY * dist;
				//直接设置位置而不是速度，确保段之间保持连接
				worm.NPC.velocity = Vector2.Zero;
				worm.NPC.position.X += posX;
				worm.NPC.position.Y += posY;;
			}
		}
	}
	///<summary>蠕虫尾部段基类，尾部在行为逻辑上被视为最后一个身体段，仅外观不同，因此直接复用身体段的AI</summary>
	public abstract class WormTail : WormBoss//身体和尾部段共享相同的AI
	{
		public sealed override WormSegmentType SegmentType => WormSegmentType.Tail;//标识本段为蠕虫的尾部
		internal override void BodyTailAI()
		{
			WormBody.CommonAI_BodyTail(this);//复用身体段的通用跟随逻辑
		}
	}
}