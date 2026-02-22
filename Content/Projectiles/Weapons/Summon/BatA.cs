using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace BoBo.Content.Projectiles.Weapons.Summon
{
	public class BatA : ModProjectile
	{
		public override string Texture => Pictures.SummonProj + Name;
		Player Player => Main.player[Projectile.owner];
		public bool JustSpawned
		{
			get => Projectile.localAI[0] == 0;
			set => Projectile.localAI[0] = value ? 0 : 1;
		}
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			Main.projFrames[Type] = 5;
		}
		public override void SetDefaults()
		{
			Projectile.width = 44;
			Projectile.height = 160;
			Projectile.scale = 1f;
			Projectile.timeLeft = 60;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.minion = true;
			Projectile.minionSlots = 1;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.netImportant = true;
			Projectile.light = 0.2f;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 120;
		}
		public override void AI()
		{
			DelegateMethods.v3_1 = Color.Purple.ToVector3();
			Point point = Projectile.Center.ToTileCoordinates();
			DelegateMethods.CastLightOpen(point.X, point.Y);
			if (++Projectile.frameCounter >= 6)
			{
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type] - 1)
					Projectile.frame = 0;
			}
			int Dir = Player.direction;
			if (Projectile.velocity.X != 0f)
				Dir = Math.Sign(Projectile.velocity.X);
			Projectile.spriteDirection = Dir;
			BatOfLight();
			Projectile.damage = 100;
			Projectile.timeLeft = 60;
			base.AI();
		}
		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i <= 10; i++)
				Dust.NewDust(Projectile.Center, 28, 56, DustID.PurpleTorch);
		}
		private void BatOfLight()
		{
			int AttackTime = 66;//攻击持续时间
			int BeforeTime = AttackTime - 1;//攻击前摇时间
			int AfterTime = AttackTime + 60;//攻击后摇时间
			_ = AfterTime - 1;//攻击后摇结束时间
			_ = AttackTime + 1;//攻击后摇开始时间
			if (Vector2.Distance(Player.Center, Projectile.Center) > 2000f)//如果离玩家太远，立即重置状态并传送到玩家附近
			{
				Projectile.ai[0] = 0f;//重置AI状态为待机
				Projectile.ai[1] = 0f;//清除目标信息
				Projectile.netUpdate = true;//同步网络数据
			}
			var index = 0;//排序号
			var TotalNumber = 0;//同类总数
			foreach (Projectile Pro in Main.ActiveProjectiles)
			{
				if (Pro.active && Pro.owner == Player.whoAmI && Pro.type == Type)
				{
					if (Projectile.whoAmI > Pro.whoAmI)//比较index确定排序
						index++;
					TotalNumber++;
				}
			}
			float N2 = (TotalNumber - 1f) / 2f;//中间位置的排序号
			var idleRotation = 0f;//待机时的旋转角度
			var idleSpot = Player.Center - Vector2.UnitY.RotatedBy(4.3982296f / TotalNumber * (index - N2)) * 80f;//待机位置计算，围绕玩家旋转排列
			if (Projectile.ai[0] == -1f)//回归待机状态
			{
				Projectile.velocity = Vector2.Zero;//停止移动
				Projectile.Center = Projectile.Center.MoveTowards(idleSpot, 32f);//平滑移动到待机位置
				Projectile.rotation = Projectile.rotation.AngleLerp(idleRotation, 0.2f);//平滑旋转到待机角度
				if (Projectile.Distance(idleSpot) < 2f)//如果已经到达待机位置，切换到待机状态
				{
					Projectile.ai[0] = 0f;
					Projectile.netUpdate = true;
				}
				return;
			}
			if (Projectile.ai[0] == 0f)//待机状态
			{
				Projectile.velocity = Vector2.Zero;
				Projectile.Center = Vector2.SmoothStep(Projectile.Center, idleSpot, 0.45f);//平滑插值移动到待机位置
				if (Main.rand.NextBool(20))//每20帧有5%的概率尝试寻找目标，制造攻击间隔
				{
					NPC Target = TryAttackingNPC();
					if (Target != null)
					{
						Projectile.ResetLocalNPCHitImmunity();//重置攻击冷却
						Projectile.ai[0] = AttackTime;//进入攻击状态
						Projectile.ai[1] = Target.whoAmI;//记录目标
						Projectile.netUpdate = true;
						return;
					}
				}
				return;
			}
			int AttackTarget = (int)Projectile.ai[1];//检查目标有效性
			if (!Main.npc.IndexInRange(AttackTarget))//目标索引是否有效
			{
				Projectile.ai[0] = 0f;
				Projectile.netUpdate = true;
				return;
			}
			NPC TargetNPC = Main.npc[AttackTarget];
			if (!TargetNPC.CanBeChasedBy(this))//目标是否可被攻击
			{
				Projectile.ai[0] = 0f;
				Projectile.netUpdate = true;
				return;
			}
			Projectile.ai[0] -= 1f;//攻击计时器递减
			if (Projectile.ai[0] >= BeforeTime)//攻击前摇阶段
			{
				Projectile.velocity *= 0.8f;//减速效果
				if (Projectile.ai[0] == BeforeTime)//记录攻击起始点，准备开始算一条椭圆轨道（以玩家和敌人为长轴）
				{
					Projectile.localAI[0] = Projectile.Center.X;
					Projectile.localAI[1] = Projectile.Center.Y;
				}
				return;
			}
			float lerpValue = Utils.GetLerpValue(BeforeTime, 0f, Projectile.ai[0], clamped: true);//0到1的插值变量
			Vector2 startPoint = new(Projectile.localAI[0], Projectile.localAI[1]);//攻击起始点
			if (lerpValue >= 0.5f)//半程后修正起始点为玩家位置，防止过远
				startPoint = Player.Center;//攻击起始点修正为玩家位置
			Vector2 targetCenter = TargetNPC.Center;//目标中心点
			Vector2 toTarget = targetCenter - startPoint;//起始点到目标的向量
			float angleToTarget = toTarget.ToRotation();//起始点到目标的角度
			float initialDeflection = targetCenter.X > startPoint.X ? -MathF.PI : MathF.PI;//初始偏转角度
			float deflectionAngle = initialDeflection + (0f - initialDeflection) * lerpValue * 2f;//偏转角度，形成弧线运动
			Vector2 spinningPoint = deflectionAngle.ToRotationVector2();//偏转点
			spinningPoint.Y *= MathF.Sin(Projectile.identity * 2.3f) * 0.5f;//Y轴缩放，形成椭圆轨道
			spinningPoint = spinningPoint.RotatedBy(angleToTarget);//旋转到目标方向
			float halfDistance = toTarget.Length() / 2f;//半程距离
			Vector2 desiredCenter = Vector2.Lerp(startPoint, targetCenter, 0.5f) + spinningPoint * halfDistance;//计算出当前帧的位置
			Projectile.Center = desiredCenter;//设置位置
			Vector2 velocityDir = MathHelper.WrapAngle(angleToTarget + deflectionAngle).ToRotationVector2() * 10f;//计算速度方向，用于旋转朝向
			Projectile.velocity = velocityDir;//设置速度方向
			Projectile.position -= Projectile.velocity;//修正位置，抵消速度影响
			if (Projectile.ai[0] == 0f)//攻击结束后寻找新目标
			{
				NPC Target = TryAttackingNPC();
				if (Target != null)
				{
					Projectile.ai[0] = AttackTime;
					Projectile.ai[1] = Target.whoAmI;
					Projectile.ResetLocalNPCHitImmunity();
					Projectile.netUpdate = true;
					return;
				}

				Projectile.ai[1] = 0f;
				Projectile.netUpdate = true;
			}
		}
		private NPC TryAttackingNPC()//目标选择逻辑（模仿原版优先级系统）
		{
			float DisR = -1f;//距离记录
			NPC ActiveLocking = Projectile.OwnerMinionAttackTargetNPC;
			if (ActiveLocking != null && ActiveLocking.CanBeChasedBy(this) && ActiveLocking.Distance(Player.Center) < 1000f)
				return ActiveLocking;
			NPC Taget = null;
			foreach (NPC n in Main.ActiveNPCs)//寻找最近的可用目标
			{
				if (n.CanBeChasedBy(this) && n.Distance(Player.Center) < 2000f)
				{
					float Dir = n.Distance(Player.Center);
					if (!(Dir > 1000f) && (!(Dir > DisR) || DisR == -1f) && Projectile.CanHitWithOwnBody(n))
					{
						DisR = Dir;
						Taget = n;
					}
				}
			}
			return Taget;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			//Texture2D texture = TextureAssets.Projectile[0].Value;
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Rectangle rectangle = new(0, texture.Height / 5 * Projectile.frame, texture.Width, texture.Height / 5);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, rectangle, Color.White, Projectile.rotation,
				new Vector2(texture.Width / 2, texture.Height / 2 / 5), new Vector2(1, 1),
				Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			lightColor = Color.White;
			return false;
		}
	}
}