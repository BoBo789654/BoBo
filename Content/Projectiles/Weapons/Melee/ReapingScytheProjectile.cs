using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Projectiles.Weapons.Melee
{
	public class ReapingScytheProjectile : ModProjectile//收割镰刀弹幕：仿照ExampleCustomSwingProjectile
	{
		public override string Texture => Pictures.MeieeProj + Name;
		private enum AttackStage
		{
			Prepare,//准备阶段
			Swing,  //挥动阶段
			Return  //收刀阶段
		}
		private int ComboPhase => (int)Projectile.ai[0];   //当前连击阶段
		private ref float Timer => ref Projectile.ai[1];   //计时器
		private ref float Rotation => ref Projectile.ai[2];//当前旋转角度

		private AttackStage CurrentStage
		{
			get => (AttackStage)Projectile.localAI[0];
			set => Projectile.localAI[0] = (float)value;
		}
		private Player Owner => Main.player[Projectile.owner];
		private Vector2 HandlePosition => Owner.RotatedRelativePoint(Owner.MountedCenter, true);
		public override void SetDefaults()
		{
			Projectile.width = 70;
			Projectile.height = 64;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.ownerHitCheck = true;
			Projectile.hide = true;
			Projectile.scale = 2.5f;
		}
		public override void OnSpawn(IEntitySource source)
		{
			Projectile.spriteDirection = Owner.direction;//根据玩家朝向设置初始方向=
			Rotation = Owner.direction == 1 ? -MathHelper.PiOver2 : MathHelper.PiOver2;//设置初始角度（根据连击阶段）
		}

		public override void AI()
		{
			ParticleOrchestrator.RequestParticleSpawn
				(clientOnly: true, ParticleOrchestraType.BlackLightningHit, new ParticleOrchestraSettings
				{
					PositionInWorld = Projectile.Center,
					MovementVector = Vector2.One
				});
			Owner.heldProj = Projectile.whoAmI;//基础设置
			Owner.itemTime = 2;
			Owner.itemAnimation = 2;
			if (!Owner.active || Owner.dead)//玩家死亡时销毁弹幕
			{
				Projectile.Kill();
				return;
			}
			switch (CurrentStage)//根据当前阶段执行不同逻辑
			{
				case AttackStage.Prepare:
					PrepareAttack();
					break;

				case AttackStage.Swing:
					ExecuteAttack();
					break;

				case AttackStage.Return:
					ReturnAttack();
					break;
			}
			UpdateScythePosition();//更新镰刀位置和旋转
			Timer++;
		}
		private void PrepareAttack()//准备：将镰刀向后拉
		{
			float pullBack = MathHelper.Lerp(0, MathHelper.PiOver4, Timer / 10f);
			Rotation += Owner.direction * pullBack;
			if (Timer > 10f)
			{
				CurrentStage = AttackStage.Swing;
				Timer = 0;
				SoundEngine.PlaySound(SoundID.Item71); //镰刀挥舞音效
			}
		}
		private void ExecuteAttack()//根据连击阶段执行不同攻击模式
		{
			switch (ComboPhase)
			{
				case 0: //第一击：下挥
					ExecuteDownSwing();
					break;

				case 1: //第二击：转两圈
					ExecuteDoubleSpin();
					break;

				case 2: //第三击：上挥
					ExecuteUpSwing();
					break;

				case 3: //第四击：下挥
					ExecuteDownSwing();
					break;
			}
		}
		private void ExecuteDownSwing()//下挥：从后上方向前下方挥动
		{
			float progress = MathHelper.SmoothStep(0, 1, Timer / 15f);
			Rotation = MathHelper.Lerp(Owner.direction * MathHelper.PiOver4, Owner.direction * -MathHelper.PiOver2, progress);
			if (Timer > 15f) TransitionToReturn();
		}
		private void ExecuteUpSwing()//上挥：从前下方向后上方挥动
		{
			float progress = MathHelper.SmoothStep(0, 1, Timer / 15f);
			Rotation = MathHelper.Lerp(Owner.direction * -MathHelper.PiOver2, Owner.direction * MathHelper.PiOver4, progress);
			if (Timer > 15f) TransitionToReturn();
		}
		private void ExecuteDoubleSpin()//旋转：转两圈
		{
			Projectile.spriteDirection = Owner.direction;
			Rotation += Owner.direction * 0.5f;//每帧旋转0.5弧度
			if (Timer == 20) SoundEngine.PlaySound(SoundID.Item71);//中间音效
			if (Timer > 25f) TransitionToReturn();
			if (Timer % 2 == 0)
			{
				for (int i = 0; i < 8; i++)//8个方向均匀分布
				{
					float angle = Rotation + MathHelper.TwoPi * i / 8f;
					Vector2 SpiralPos = Projectile.Center + angle.ToRotationVector2() * 60f;//半径扩大
					Vector2 velocity = angle.ToRotationVector2() * 3f;//径向速度（向外扩散）
					Dust dust = Dust.NewDustPerfect(SpiralPos, DustID.PurpleMoss,
						velocity, 0, Color.Lerp(Color.MediumPurple, Color.HotPink, 0.7f), 1.8f);
					dust.noGravity = true;
					dust.fadeIn = 0.5f;//淡入
				}
			}
		}
		private void TransitionToReturn()
		{
			CurrentStage = AttackStage.Return;
			Timer = 0;
		}
		private void ReturnAttack()//收刀：回初始位置
		{
			float progress = MathHelper.SmoothStep(0, 1, Timer / 10f);
			Rotation = MathHelper.Lerp(Rotation, Owner.direction * MathHelper.PiOver2, progress);
			if (Timer > 10f) Projectile.Kill();
		}
		private void UpdateScythePosition()//算偏移量
		{
			float distance = 50f + (ComboPhase == 1 ? 20f : 0f);
			Vector2 GripToBlade = new Vector2(Owner.direction * 0.7f, -0.4f).RotatedBy(Rotation);//调位置
			Projectile.Center = HandlePosition + GripToBlade * distance;
			Projectile.rotation = Rotation;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 origin = new(texture.Width / 2, texture.Height / 2);
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Main.EntitySpriteDraw(texture, drawPos, null, Color.White, Projectile.rotation, origin, Projectile.scale,
				Owner.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);//玩家朝向不同设置不同贴图朝向
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			for (int i = 0; i < 10; i++)//爆炸粒子
			{
				Dust.NewDustPerfect(target.Center, DustID.PurpleCrystalShard, Main.rand.NextVector2Circular(8, 8), 0,
					new Color(200, 80, 255), Main.rand.NextFloat(1.5f, 2f)).noGravity = true;
			}
			for (int i = 0; i < 16; i++)//环形冲击波
			{
				Dust dust = Dust.NewDustPerfect(target.Center, DustID.Shadowflame, Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 16 * i) * 3,
					100, new Color(170, 50, 230, 0), 2.2f);
				dust.noGravity = true;
				dust.fadeIn = 1.2f;
			}
		}

	}
}