using BoBo.Content.NPCs.EvilWorm.EvilWormProj;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace BoBo.Content.NPCs.EvilWorm
{
	///<summary>
	///邪恶蠕虫Boss技能系统，专门处理技能逻辑
	///我写这个主要是想着，那边移动逻辑已经写了很长了，再往下写就不合适了
	///</summary>
	public static class EvilWormHeadSkill
	{
		#region 准备工作
		//技能状态变量
		private static int skillCooldown = 0;//技能冷却
		private static int currentSkill = 0;//当前技能
		private static int skillDuration = 0;//技能持续时间
		private static Vector2 chargeDirection = Vector2.Zero;
		private static int startDelayTimer = 180; // 3秒 = 180帧（60帧/秒）
		private enum Skills//技能枚举
		{
			None = 0,
			ToxicSplash = 1, //毒液喷射
			MinionSummon = 2,//召唤小怪
			ChargeAttack = 3,//冲锋攻击
			WromLaser = 4    //蠕虫激光
		}
		///<summary>主要技能执行入口，可以在EvilWorm的AI中调用</summary>
		public static void ExecuteSkills(NPC npc)
		{
			if (npc.ModNPC is not WormBoss wormBoss) return;//只有头部执行技能逻辑
			if (startDelayTimer > 0)
			{
				startDelayTimer--;
				return; // 延迟期间不执行任何技能
			}
			float lifeRatio = GetLifeRatio(npc);//获取生命值比率
			int currentPhase = GetCurrentPhase(lifeRatio);//获取阶段信息
			UpdateCooldowns();//更新技能冷却
			Skills selectedSkill = SelectSkill(lifeRatio, currentPhase);//根据阶段选择技能
			if (selectedSkill != Skills.None && skillCooldown <= 0)//执行选中的技能
				ExecuteSelectedSkill(selectedSkill, npc, lifeRatio, currentPhase);
			ProcessActiveSkill(npc);//处理当前激活技能的持续效果
		}
		public static void ResetStartDelay()
		{
			startDelayTimer = 180; // 重置为3秒
		}
		///<summary>获取NPC的生命值比率</summary>
		private static float GetLifeRatio(NPC npc)
		{
			return (float)npc.life / npc.lifeMax;
		}
		///<summary>根据生命值比率判断当前阶段</summary>
		private static int GetCurrentPhase(float lifeRatio)
		{
			if (lifeRatio > 0.7f) return 1;
			if (lifeRatio > 0.3f) return 2;
			return 3;
		}
		///<summary>更新技能冷却时间</summary>
		private static void UpdateCooldowns()
		{
			if (skillCooldown > 0) skillCooldown--;
			if (skillDuration > 0) skillDuration--;
		}
		///<summary>根据生命值和阶段选择技能，可用Main.rand.Next()来分配权重，也可用if来写技能组循环</summary>
		private static Skills SelectSkill(float lifeRatio, int currentPhase)
		{
			if (currentPhase == 1)//阶段1
			{
				int rand = Main.rand.Next(500);
				if (rand < 250) return Skills.ToxicSplash;
				else if (rand < 350) return Skills.MinionSummon;
				else if (rand < 450) return Skills.ChargeAttack;
				else return Skills.WromLaser;
			}
			else if (currentPhase == 2)//阶段2
			{
				int rand = Main.rand.Next(500);
				if (rand < 100) return Skills.ToxicSplash;
				else if (rand < 250) return Skills.MinionSummon;
				else if (rand < 400) return Skills.ChargeAttack;
				else return Skills.WromLaser;
			}
			else//阶段3
			{
				int rand = Main.rand.Next(500);
				if (rand < 50) return Skills.ToxicSplash;
				else if (rand < 150) return Skills.MinionSummon;
				else if (rand < 300) return Skills.ChargeAttack;
				else return Skills.WromLaser;
			}
		}
		///<summary>执行选中的技能</summary>
		private static void ExecuteSelectedSkill(Skills skill, NPC npc, float lifeRatio, int currentPhase)
		{
			currentSkill = (int)skill;
			switch (skill)
			{
				case Skills.ToxicSplash:
					StartToxicSplash(npc, lifeRatio, currentPhase);
					break;
				case Skills.MinionSummon:
					StartMinionSummon(npc, lifeRatio, currentPhase);
					break;
				case Skills.ChargeAttack:
					StartChargeAttack(npc, lifeRatio, currentPhase);
					break;
				case Skills.WromLaser:
					StartWromLaser(npc, lifeRatio, currentPhase);
					break;
			}
		}
		///<summary>处理当前激活技能的持续效果</summary>
		private static void ProcessActiveSkill(NPC npc)
		{
			switch ((Skills)currentSkill)
			{
				case Skills.ToxicSplash:
					if (skillDuration > 0) ContinueToxicSplash(npc);
					break;
				case Skills.ChargeAttack:
					if (skillDuration > 0) ContinueChargeAttack(npc);
					break;
				case Skills.WromLaser:
					if (skillDuration > 0) ContinueWromLaser(npc);
					break;
			}
		}
		#endregion
		#region 毒液喷射
		private static void StartToxicSplash(NPC npc, float lifeRatio, int currentPhase)
		{
			skillDuration = 60;//持续60帧（1秒）
			skillCooldown = 180;//3秒冷却
			if (currentPhase >= 2)//根据阶段调整技能强度
			{
				skillDuration += 20;//阶段2以上持续时间更长
			}
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			SoundEngine.PlaySound(SoundID.Item85, npc.Center);//播放技能音效
		}
		private static void ContinueToxicSplash(NPC npc)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			if (skillDuration % 10 == 0)//每10帧发射一次毒液
			{
				Player target = Main.player[npc.target];
				Vector2 direction = Vector2.Normalize(target.Center - npc.Center);//扇形发射多个毒液弹幕
				int projectileCount = 3 + (skillDuration / 20);//随时间增加弹幕数量
				float spreadAngle = 30f + (60 - skillDuration) * 0.6f;//随时间扩大扇形角度
				for (int i = 0; i < projectileCount; i++)
				{
					float angle = MathHelper.ToRadians(-spreadAngle / 2 + (spreadAngle * i / (projectileCount - 1)));
					Vector2 spreadDirection = direction.RotatedBy(angle) * 1.5f;
					int j = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, spreadDirection * 7f,
						ModContent.ProjectileType<ToxicSplash>(), npc.damage / 10, 1f, Main.myPlayer);
					Main.projectile[j].hostile = true;
				}
			}
		}
		#endregion
		#region 召唤小怪技能
		private static void StartMinionSummon(NPC npc, float lifeRatio, int currentPhase)
		{
			skillCooldown = 600; //10秒冷却
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			int minionCount = currentPhase;//根据阶段调整召唤数量，阶段1召唤1个，阶段2召唤2个，阶段3召唤3个
			Player target = Main.player[npc.target];//在玩家周围召唤小怪，但排除600像素范围内
			int attempts = 0;
			int maxAttempts = 20;//防止无限循环
			for (int i = 0; i < minionCount; i++)
			{
				Vector2 spawnPos;
				bool validPosition = false;
				attempts = 0;
				while (!validPosition && attempts < maxAttempts)//尝试找到有效位置
				{
					attempts++;
					float distance = Main.rand.Next(600, 800);//生成在600-800像素距离的位置
					float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
					spawnPos = target.Center + new Vector2(distance, 0).RotatedBy(angle);
					if (!WorldGen.SolidTile((int)spawnPos.X / 16, (int)spawnPos.Y / 16) &&//检查位置是否有效（不在实心方块内，且距离玩家足够远）
						Vector2.Distance(spawnPos, target.Center) >= 600)
					{
						validPosition = true;
						NPC.NewNPC(npc.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, NPCID.HornetLeafy);//召唤小怪
					}
				}
				if (!validPosition)//如果尝试多次都找不到有效位置，在更远距离生成
				{
					float distance = Main.rand.Next(800, 1000);
					float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
					spawnPos = target.Center + new Vector2(distance, 0).RotatedBy(angle);
					NPC.NewNPC(npc.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, NPCID.HornetLeafy);//召唤小怪
				}
			}

			SoundEngine.PlaySound(SoundID.Item44, npc.Center);
		}
		#endregion
		#region 冲锋攻击技能
		private static void StartChargeAttack(NPC npc, float lifeRatio, int currentPhase)
		{
			skillDuration = 90;//冲锋持续90帧
			skillCooldown = 300;//5秒冷却
			Player target = Main.player[npc.target];
			chargeDirection = Vector2.Normalize(target.Center - npc.Center);
			//降低冲锋速度，让玩家有反应时间
			float speedMultiplier = 1.2f + currentPhase * 0.3f;//阶段1:1.5倍, 阶段2:1.8倍, 阶段3:2.1倍
			npc.velocity = chargeDirection * (npc.ModNPC as WormBoss).MoveSpeed * speedMultiplier;
			SoundEngine.PlaySound(SoundID.Item74, npc.Center);
		}
		private static void ContinueChargeAttack(NPC npc)
		{
			//保持冲锋方向，但允许轻微调整
			Player target = Main.player[npc.target];
			Vector2 idealDirection = Vector2.Normalize(target.Center - npc.Center);
			chargeDirection = Vector2.Lerp(chargeDirection, idealDirection, 0.05f);
			chargeDirection.Normalize();
			//使用相同的降低后的速度倍数
			float baseSpeed = (npc.ModNPC as WormBoss).MoveSpeed;
			float speedMultiplier = 1.2f + GetCurrentPhase(GetLifeRatio(npc)) * 0.3f;
			npc.velocity = chargeDirection * baseSpeed * speedMultiplier;
			//冲锋路径上产生效果
			if (Main.rand.NextBool(2))
				Dust.NewDust(npc.position, npc.width, npc.height, DustID.Corruption, 0f, 0f, 0, default, 1.5f);
			//技能结束
			if (skillDuration == 1)
				currentSkill = (int)Skills.None;
		}
		#endregion
		#region 蠕虫激光
		private static void StartWromLaser(NPC npc, float lifeRatio, int currentPhase)
		{
			skillDuration = 180;//预警线60帧+激光90帧+缓冲时间
			skillCooldown = 180;//6秒冷却
			SoundEngine.PlaySound(SoundID.Item120, npc.Center);
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			Player target = Main.player[npc.target];
			for (int i = 0; i < 15; i++)//在玩家半径x像素远处发射15条预警线
			{
				float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);//随机角度
				Vector2 circlePos = target.Center + new Vector2(1200, 0).RotatedBy(angle);
				Vector2 targetPos = target.Center + new Vector2(
					Main.rand.NextFloat(-80f, 80f),
					Main.rand.NextFloat(-80f, 80f)
				);//目标位置为玩家附近（随机偏移±y像素）
				Vector2 direction = Vector2.Normalize(circlePos - targetPos);
				int warningProj = Projectile.NewProjectile(npc.GetSource_FromAI(), targetPos, -direction, ModContent.ProjectileType<WarningLine>(),
					0, 0f, Main.myPlayer, ai0: npc.whoAmI, ai1: direction.ToRotation());
			}
		}
		private static void ContinueWromLaser(NPC npc)
		{
			//预警线阶段（前40帧），Boss缓慢移动
			if (skillDuration > 0)
			{
				Player target = Main.player[npc.target];
				Vector2 direction = Vector2.Normalize(target.Center - npc.Center);
				npc.velocity = direction * (npc.ModNPC as WormBoss).MoveSpeed * 0.3f;
			}
			//预警线结束，开始激光阶段（第140帧）
			if (skillDuration == 120)
			{
				SoundEngine.PlaySound(SoundID.Item125, npc.Center);
				if (Main.netMode == NetmodeID.MultiplayerClient) return;
				for (int i = 0; i < 10; i++)//激光特效
					Dust.NewDust(npc.Center, npc.width, npc.height, DustID.PurpleTorch, 0f, 0f, 100, default, 1.5f);
			}
			if (skillDuration <= 140 && skillDuration > 50)//激光持续阶段
				if (Main.rand.NextBool(3))//持续阶段特效
					Dust.NewDust(npc.Center, npc.width, npc.height, DustID.PurpleTorch, 0f, 0f, 100, default, 1f);
		}
		#endregion
		///<summary>
		///强制中断当前技能（用于Boss被击败或其他特殊情况）
		///</summary>
		public static void InterruptCurrentSkill()
		{
			currentSkill = 0;//当前技能
			skillDuration = 0;//持续时间
			skillCooldown = 60;//短暂冷却
		}
		///<summary>
		///检查是否有技能正在执行
		///</summary>
		public static bool IsSkillActive()
		{
			return currentSkill != (int)Skills.None && skillDuration > 0;
		}
		///<summary>
		///获取当前技能的剩余时间（0-1的比例）
		///</summary>
		public static float GetSkillProgress()
		{
			if (currentSkill == (int)Skills.None) return 0f; 
			//根据技能类型返回不同的最大持续时间
			float maxDuration = GetMaxSkillDuration((Skills)currentSkill);
			return skillDuration / maxDuration;
		}
		private static float GetMaxSkillDuration(Skills skill)
		{
			return skill switch
			{
				Skills.ToxicSplash => 60f,
				Skills.ChargeAttack => 90f,
				Skills.WromLaser => 90f,
				_ => 1f
			};
		}
	}
}