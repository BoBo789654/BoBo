using BoBo.Content.Buffs.MinionBuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Weapons.Summon
{
	public class BatC : ModProjectile
	{
		public override string Texture => Pictures.SummonProj + Name;
		Player player => Main.player[Projectile.owner];
		private bool isAttacking = false;
		private int attackIndex = 0;//蝙蝠在攻击队列中的索引
		private int totalAttackers = 0;//总攻击蝙蝠数量
		private Vector2 hoverOffset;//上下浮动偏移
		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 4;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}
		public override void SetDefaults()
		{
			Projectile.width = 80;
			Projectile.height = 184;
			Projectile.friendly = true;
			Projectile.light = 0.5f;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 120;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = true;
			Projectile.minionSlots = 1f;
			Projectile.minion = true;
			Projectile.DamageType = DamageClass.Summon;
		}
		void MoveToTarget(Vector2 targetPos, float maxSpeed = 20f, float smoothFactor = 0.1f)//平滑移动到目标位置
		{
			Vector2 desiredVelocity = (targetPos - Projectile.Center);
			if (desiredVelocity != Vector2.Zero)
			{
				desiredVelocity.Normalize();
				desiredVelocity *= maxSpeed;
			}
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, smoothFactor);
			if (Projectile.velocity.Length() > maxSpeed)//限制最大速度
			{
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpeed;
			}
		}
		void AttackShooting(NPC target)//定期发射弹幕
		{
			Projectile.ai[0]++;
			if (Projectile.ai[0] >= 60)//攻击频率
			{
				Projectile.ai[0] = 0;
				for (int i = 0; i < 3; i++)
				{
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(), Projectile.Center,
						(target.Center - Projectile.Center).RotatedByRandom(0.2f).SafeNormalize(Vector2.Zero) * 10f,
						ProjectileID.Bat, Projectile.damage, Projectile.knockBack, Projectile.owner);
				}
			}
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override void AI()
		{
			if (player.HasBuff<BatSummoningBuffC>())//保持召唤物存活
				Projectile.timeLeft = 2;
			if (++Projectile.frameCounter >= 5)//动画帧更新
			{
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type])
					Projectile.frame = 0;
			}
			float floatOffset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f + Projectile.whoAmI) * 5f;
			hoverOffset = new Vector2(0, floatOffset);//上下浮动效果
			NPC target = null;
			if (player.HasMinionAttackTargetNPC)//优先攻击玩家锁定的目标
			{
				target = Main.npc[player.MinionAttackTargetNPC];
				float between = Vector2.Distance(Projectile.Center, target.Center);
				if (between > 2000f)//如果距离太远，则放弃目标
					target = null;
			}
			if (target == null || !target.active)//如果没有锁定目标，则寻找最近的目标
			{
				int t = Projectile.FindTargetWithLineOfSight(1500);
				if (t >= 0)
					target = Main.npc[t];
			}
			if (target != null && target.active)
			{
				isAttacking = true;
				CalculateAttackPosition(target, out attackIndex, out totalAttackers);//计算攻击队列中的位置
				Projectile.rotation = 0f;//不旋转本体
				if (Vector2.Distance(Projectile.Center, target.Center) > 2000)//如果目标距离超过2000，则回到玩家身边
				{
					Vector2 p = Vector2.Lerp(Projectile.Center, player.Center, 0.1f);
					Projectile.velocity = p - Projectile.Center;
				}
				else//移动到目标附近并攻击
				{
					Vector2 arcPosition = CalculateArcPosition(target, attackIndex, totalAttackers, 200f);//计算弧形排列位置（距离目标200单位）
					arcPosition += hoverOffset;//上下浮动效果
					MoveToTarget(arcPosition, 12f, 0.1f);
					AttackShooting(target);
				}
			}
			else
			{
				isAttacking = false;//标记为非攻击状态
				Vector2 mypos = player.Center;//没有目标时，回到玩家身边
				float dis = Projectile.Distance(mypos);
				if (dis > 1200)
				{
					Vector2 p = Vector2.Lerp(Projectile.Center, player.Center, 0.1f);
					Projectile.velocity = p - Projectile.Center;
				}
				else if (dis > 600)
				{
					MoveToTarget(mypos, 12f, 0.1f);
				}
				else
				{
					StandByMultiRowQueue(player.Center, 50, 50);//水平间距，行间距
				}
				Projectile.rotation = 0f;//待机时不旋转
			}
		}
		void StandByMultiRowQueue(Vector2 center, float horizontalSpacing = 0f, float verticalSpacing = 0f)// 多行排列待机逻辑
		{
			//计算当前蝙蝠在所有同类中的索引和总数
			int totalMinions = 0;
			int myIndex = 0;
			foreach (Projectile proj in Main.projectile)
			{
				if (proj.active && proj.owner == Projectile.owner && proj.type == Projectile.type)
				{
					if (proj.whoAmI < Projectile.whoAmI)
						myIndex++;
					totalMinions++;
				}
			}
			int batsPerRow = 12;//每行最多x个蝙蝠
			int row = myIndex / batsPerRow;//行号（从0开始）
			int col = myIndex % batsPerRow;//列号（从0开始）
			float verticalOffset;//计算垂直偏移
			if (row == 0)//根据行号计算垂直位置
				verticalOffset = 0f;//第1行（基准行）：不偏移
			else if (row % 2 == 1)//奇数行（第2、4、6行等）：在下方
				verticalOffset = ((row + 1) / 2) * verticalSpacing;//第2行：+1行间距，第4行：+2行间距，第6行：+3行间距...
			else//偶数行（第3、5、7行等）：在上方
				verticalOffset = -(row / 2) * verticalSpacing;//第3行：-1行间距，第5行：-2行间距，第7行：-3行间距...
			//初始位置规定：
			//float horizontalOffset = -player.direction * horizontalSpacing * (col - (batsPerRow - 1) / 2f);//以玩家为中心排列
			int arrangeDirection = -player.direction;//排列方向与玩家朝向相反
			float startOffset = 50f;//起始距离
			float horizontalOffset = arrangeDirection * (startOffset + horizontalSpacing * col);//从玩家身后开始线性排列
			Vector2 targetPos = center + new Vector2(horizontalOffset, verticalOffset);//计算目标位置
			if (Projectile.localAI[1] < 3)//平滑移动到目标位置
			{
				Projectile.position = targetPos;
				Projectile.localAI[1]++;
			}
			else
			{
				Vector2 desiredVelocity = (targetPos - Projectile.Center) * 0.1f;
				if (desiredVelocity.Length() > 10f)
					desiredVelocity = desiredVelocity.SafeNormalize(Vector2.Zero) * 10f;
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.2f);
			}
		}
		private Vector2 CalculateArcPosition(NPC target, int index, int total, float radius)//弧形排列位置
		{
			if (total <= 0) return target.Center;
			float arcAngle = MathHelper.Pi * 2f / 3f;//计算弧形角度范围
			float angleStep = total > 1 ? arcAngle / (total - 1) : 0;//计算每个蝙蝠的角度间隔
			float angle = -arcAngle / 2f + angleStep * index;//计算当前蝙蝠的角度
			Vector2 toTarget = target.Center - player.Center;//获取从玩家到目标的向量方向
			if (toTarget == Vector2.Zero)
				toTarget = Vector2.UnitX;
			float baseAngle = toTarget.ToRotation();
			Vector2 arcPos = target.Center + new Vector2((float)Math.Cos(baseAngle + angle), 
				(float)Math.Sin(baseAngle + angle)) * radius;//计算弧形上的位置
			return arcPos;
		}
		private void CalculateAttackPosition(NPC target, out int index, out int total)//计算攻击队列中的位置
		{
			index = 0;
			total = 0;
			foreach (Projectile proj in Main.projectile)//找出所有正在攻击同一目标的同类召唤物
			{
				if (proj.active && proj.owner == Projectile.owner && proj.type == Projectile.type)
				{
					if (proj.ModProjectile is BatC bat && bat.isAttacking)//检查这个召唤物是否也在攻击同一个目标
					{
						NPC projTarget = null;//找到当前攻击同一目标的召唤物
						if (player.HasMinionAttackTargetNPC)
						{
							projTarget = Main.npc[player.MinionAttackTargetNPC];
							float between = Vector2.Distance(proj.Center, projTarget.Center);
							if (between > 2000f)
								projTarget = null;
						}
						if (projTarget == null || !projTarget.active)
						{
							int t = proj.FindTargetWithLineOfSight(1500);
							if (t >= 0)
								projTarget = Main.npc[t];
						}
						if (projTarget != null && projTarget.active && projTarget.whoAmI == target.whoAmI)
						{
							if (proj.whoAmI < Projectile.whoAmI)
								index++;
							total++;
						}
					}
				}
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			int frameHeight = texture.Height / Main.projFrames[Type];
			Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
			Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (!isAttacking)//在玩家身后时，根据位置决定朝向
			{
				if (Projectile.Center.X < player.Center.X)
					spriteEffects = SpriteEffects.FlipHorizontally;//在左侧时朝左
				else
					spriteEffects = SpriteEffects.None;//在右侧时朝右
			}
			else//攻击时，根据与目标的相对位置决定朝向
			{
				NPC target = null;
				if (player.HasMinionAttackTargetNPC)
				{
					target = Main.npc[player.MinionAttackTargetNPC];
					float between = Vector2.Distance(Projectile.Center, target.Center);
					if (between > 2000f)
						target = null;
				}
				if (target == null || !target.active)
				{
					int t = Projectile.FindTargetWithLineOfSight(1500);
					if (t >= 0)
						target = Main.npc[t];
				}
				if (target != null && target.active)//根据蝙蝠在目标的左侧还是右侧决定朝向
				{
					if (Projectile.Center.X < target.Center.X)
						spriteEffects = SpriteEffects.FlipHorizontally;//在目标左侧时朝左
					else
						spriteEffects = SpriteEffects.None;//在目标右侧时朝右
				}
				else//根据移动方向决定
				{
					if (Projectile.velocity.X < 0)
						spriteEffects = SpriteEffects.FlipHorizontally;
					else
						spriteEffects = SpriteEffects.None;
				}
			}
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRect,
				lightColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
			return false;
		}
	}
}