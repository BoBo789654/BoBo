using BoBo.Content.Buffs.MinionBuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace BoBo.Content.Projectiles.Weapons.Summon
{
	public class BatB : ModProjectile
	{
		public override string Texture => Pictures.SummonProj + Name;
		Player Player => Main.player[Projectile.owner];
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;//设置为可牺牲召唤物
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;//启用召唤物目标选择特性
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;//对邪教徒免疫
			Main.projPet[Projectile.type] = true;//设置为宠物召唤物
			Main.projFrames[Type] = 4;
		}
		public override void SetDefaults()
		{
			Projectile.width = 44;
			Projectile.height = 32;
			Projectile.scale = 1f;
			Projectile.timeLeft = 60;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
			Projectile.tileCollide = false;
			Projectile.minion = true;
			Projectile.minionSlots = 1;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.netImportant = true;
			Projectile.light = 0.2f;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 120;
		}
		private bool CheckActive(Player owner)//主动去除召唤物的Buff并去除召唤物
		{
			if (owner.dead || !owner.active)
			{
				owner.ClearBuff(ModContent.BuffType<BatSummoningBuffB>());

				return false;
			}
			if (!owner.HasBuff(ModContent.BuffType<BatSummoningBuffB>()))
			{
				Projectile.Kill();
			}
			return true;
		}
		private static List<int> BatOfLightListedTargets = new List<int>();//列表
		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			if (!CheckActive(owner))//主动去除召唤物的Buff并去除召唤物
			{
				return;
			}
			List<int> batOfLightListedTargets = BatOfLightListedTargets;//获取列表引用
			DelegateMethods.v3_1 = BatOfLightGetColor().ToVector3();//设置投射物光照颜色
			Point point = Projectile.Center.ToTileCoordinates();
			DelegateMethods.CastLightOpen(point.X, point.Y);
			if (++Projectile.frameCounter >= 6)//动画帧更新逻辑
			{
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type])
					Projectile.frame = 0;
			}
			int direction = Player.direction;//设置朝向
			if (Projectile.velocity.X != 0f)
				direction = Math.Sign(Projectile.velocity.X);
			Projectile.spriteDirection = direction;
			batOfLightListedTargets.Clear();//清空黑名单并执行主逻辑
			BatOfLightThink(batOfLightListedTargets);
			Projectile.timeLeft = 60;//重置剩余时间保持召唤物存活
		}
		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i <= 10; i++)
				Dust.NewDust(Projectile.Center, 28, 56, DustID.BlueTorch);
		}
		public static Color BatOfLightGetColor()
		{
			return Color.Blue;
		}
		private void BatOfLightThink(List<int> List)
		{
			int AttackTime = 66;
			int BeforeTime = AttackTime - 1;
			int AfterTime = AttackTime + 60;
			_ = AfterTime - 1;
			_ = AttackTime + 1;
			if (Player.active && Vector2.Distance(Player.Center, Projectile.Center) > 2000f)//如果距离玩家过远，重置AI状态
			{
				Projectile.ai[0] = 0f;
				Projectile.ai[1] = 0f;
				Projectile.netUpdate = true;
			}
			if (Projectile.ai[0] == -1f)//返回闲置位置
			{
				AI_GetMyGroupIndexAndFillList(List, out var index, out var totalIndexesInGroup);//获取召唤物在队列中的索引
				BatOfLightGetIdlePosition(index, totalIndexesInGroup, out var idleSpot, out var idleRotation);//计算闲置位置
				Projectile.velocity = Vector2.Zero;//向闲置位置平滑移动
				Projectile.Center = Projectile.Center.MoveTowards(idleSpot, 32f);
				Projectile.rotation = Projectile.rotation.AngleLerp(idleRotation, 0.2f);
				if (Projectile.Distance(idleSpot) < 2f)//到达后切换到状态0
				{
					Projectile.ai[0] = 0f;
					Projectile.netUpdate = true;
				}
				return;
			}
			if (Projectile.ai[0] == 0f)
			{
				AI_GetMyGroupIndexAndFillList(List, out var index2, out var totalIndexesInGroup2);//获取召唤物队列信息
				BatOfLightGetIdlePosition(index2, totalIndexesInGroup2, out var idleSpot2, out var _);//计算闲置位置
				Projectile.velocity = Vector2.Zero;//平滑移动到闲置位置
				Projectile.Center = Vector2.SmoothStep(Projectile.Center, idleSpot2, 0.45f);
				if (Main.rand.NextBool(20))//随机尝试寻找攻击目标
				{
					int targetIndex = BatOfLightTryAttackingNPCs(List);
					if (targetIndex != -1)//找到目标
					{
						BatOfLightStartAttack();//开始攻击
						Projectile.ai[0] = AttackTime;//切换到攻击状态
						Projectile.ai[1] = targetIndex;//记录目标索引
						Projectile.netUpdate = true;
						return;
					}
				}
				return;
			}
			int attackTarget = (int)Projectile.ai[1];//获取目标索引
			if (!Main.npc.IndexInRange(attackTarget))//目标有效性检查
			{
				Projectile.ai[0] = 0f;//目标无效，返回闲置
				Projectile.netUpdate = true;
				return;
			}
			NPC targetNPC = Main.npc[attackTarget];
			if (!targetNPC.CanBeChasedBy(this))//目标可被攻击性检查
			{
				Projectile.ai[0] = 0f;
				Projectile.netUpdate = true;
				return;
			}
			Projectile.ai[0] -= 1f;//减少攻击计时器
			if (Projectile.ai[0] >= (float)BeforeTime)//攻击前准备阶段
			{
				Projectile.velocity *= 0.8f;//减速
				if (Projectile.ai[0] == (float)BeforeTime)//记录起始位置
				{
					Projectile.localAI[0] = Projectile.Center.X;
					Projectile.localAI[1] = Projectile.Center.Y;
				}
				return;
			}
			//攻击移动阶段
			float lerpValue = Utils.GetLerpValue(BeforeTime, 0f, Projectile.ai[0], clamped: true);
			Vector2 startPoint = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
			//后半段攻击从玩家位置开始
			if (lerpValue >= 0.5f)
				startPoint = Player.Center;
			Vector2 targetCenter = targetNPC.Center;
			float angleToTarget = (targetCenter - startPoint).ToRotation();
			//计算弧形攻击路径
			float initialDeflection = (targetCenter.X > startPoint.X) ? (-(float)Math.PI) : ((float)Math.PI);
			float deflectionAngle = initialDeflection + (0f - initialDeflection) * lerpValue * 2f;
			Vector2 spinningPoint = deflectionAngle.ToRotationVector2();
			//添加垂直偏移制造波浪效果
			spinningPoint.Y *= (float)Math.Sin((float)Projectile.identity * 2.3f) * 0.5f;
			spinningPoint = spinningPoint.RotatedBy(angleToTarget);
			//计算弧形路径中心点
			float halfDistance = (targetCenter - startPoint).Length() / 2f;
			Vector2 desiredCenter = Vector2.Lerp(startPoint, targetCenter, 0.5f) + spinningPoint * halfDistance;
			Projectile.Center = desiredCenter;
			//计算速度方向
			Vector2 velocityDir = MathHelper.WrapAngle(angleToTarget + deflectionAngle + 0f).ToRotationVector2() * 10f;
			Projectile.velocity = velocityDir;
			Projectile.position -= Projectile.velocity;
			//攻击结束，寻找新目标
			if (Projectile.ai[0] == 0f)
			{
				int newTarget = BatOfLightTryAttackingNPCs(List);
				if (newTarget != -1)//找到新目标
				{
					Projectile.ai[0] = AttackTime;
					Projectile.ai[1] = newTarget;
					BatOfLightStartAttack();
					Projectile.netUpdate = true;
					return;
				}
				Projectile.ai[1] = 0f;//未找到目标，返回闲置状态
				Projectile.netUpdate = true;
			}
		}
		private void BatOfLightStartAttack()//开始攻击，重置无敌帧
		{
			for (int i = 0; i < Projectile.localNPCImmunity.Length; i++)
				Projectile.localNPCImmunity[i] = 0;
		}
		private int BatOfLightTryAttackingNPCs(List<int> ListedTargets, bool skipBodyCheck = false)//尝试攻击NPC，返回目标索引
		{
			Vector2 center = Player.Center;
			int result = -1;
			float distanceRecord = -1f;
			NPC ownerMinionAttackTarget = Projectile.OwnerMinionAttackTargetNPC;//优先攻击玩家选中的目标
			if (ownerMinionAttackTarget != null && ownerMinionAttackTarget.CanBeChasedBy(this))
			{
				bool canAttack = true;
				if (!ownerMinionAttackTarget.boss && ListedTargets.Contains(ownerMinionAttackTarget.whoAmI))//目标不是BOSS且在已列表目标中
					canAttack = false;
				if (ownerMinionAttackTarget.Distance(center) > 1000f)//目标距离中心点超过1000像素
					canAttack = false;
				if (!skipBodyCheck && !Projectile.CanHitWithOwnBody(ownerMinionAttackTarget))//检查且召唤物无法直接命中目标
					canAttack = false;
				if (canAttack)
					return ownerMinionAttackTarget.whoAmI;
			}
			for (int i = 0; i < 200; i++)//遍历所有NPC寻找最近可攻击目标
			{
				NPC npc = Main.npc[i];
				if (npc.CanBeChasedBy(this) && (npc.boss || !ListedTargets.Contains(i)))
				{
					float distance = npc.Distance(center);
					if (!(distance > 1000f) && (!(distance > distanceRecord) || distanceRecord == -1f) &&
						(skipBodyCheck || Projectile.CanHitWithOwnBody(npc)))
					{
						distanceRecord = distance;
						result = i;
					}
				}
			}
			return result;
		}
		private void BatOfLightGetIdlePosition(int stackedIndex, int totalIndexes, out Vector2 idleSpot, out float idleRotation)//计算召唤物在弧形阵型中的位置
		{
			idleRotation = 0f;
			float middleIndex = ((float)totalIndexes - 1f) / 2f;
			idleSpot = Player.Center + -Vector2.UnitY.RotatedBy(4.3982296f / (float)totalIndexes * ((float)stackedIndex - middleIndex)) * 60f;//弧形排列计算
		}
		private void AI_GetMyGroupIndexAndFillList(List<int> ListedTargets, out int index, out int totalIndexesInGroup)//获取当前召唤物在组中的索引
		{
			ArgumentNullException.ThrowIfNull(ListedTargets);
			index = 0;
			totalIndexesInGroup = 0;
			for (int i = 0; i < 1000; i++)//遍历所有弹幕，统计同类型召唤物
			{
				Projectile projectile = Main.projectile[i];
				if (projectile.active && projectile.owner == Player.whoAmI && projectile.type == Type)
				{
					if (Projectile.whoAmI > i)
						index++;
					totalIndexesInGroup++;
				}
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			int frameHeight = texture.Height / Main.projFrames[Type];
			Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
			Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRect,
				Color.White, Projectile.rotation, origin, Projectile.scale,
				Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			lightColor = Color.White;
			return false;
		}
	}
}