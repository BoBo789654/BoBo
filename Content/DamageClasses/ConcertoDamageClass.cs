using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;

namespace BoBo.Content.DamageClasses
{
	//协奏伤害
	//Item.DamageType = ModContent.GetInstance<ConcertoDamageClass>();使用
	//玩家有0-9的计数A，每次击中+1；受击敌人有1-10的计数B，每次受击+1
	//若A=B-1时，恢复玩家4%的血量，以及使敌人受到前X次的伤害，X为头上计数
	public class ConcertoConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("显示玩家计数器(A)")]
		[Tooltip("是否在玩家头顶显示协奏曲计数器A")]
		[DefaultValue(true)]
		public bool ShowPlayerCounter;
		[Label("显示敌人计数器(B)")]
		[Tooltip("是否在敌人头顶显示协奏曲计数器B")]
		[DefaultValue(true)]
		public bool ShowEnemyCounter;
	}
	public class ConcertoDamageClass : DamageClass
	{
		public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
		{
			if (damageClass == Generic)
				return StatInheritanceData.Full;
			return new StatInheritanceData(
				damageInheritance: 0f,
				critChanceInheritance: 0f,
				attackSpeedInheritance: 0f,
				armorPenInheritance: 0f,
				knockbackInheritance: 0f
			);
		}
		public override bool GetEffectInheritance(DamageClass damageClass)
		{
			if (damageClass == Ranged) return true;
			if (damageClass == Magic) return true;
			return false;
		}
		public override void SetDefaultStats(Player player)
		{
			player.GetCritChance<ConcertoDamageClass>() += 4;
			player.GetArmorPenetration<ConcertoDamageClass>() += 10;
		}
		public override bool UseStandardCritCalcs => true;
	}
	public class ConcertoPlayer : ModPlayer//A计数
	{
		public int CounterA { get; set; } = 0;
		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (damageDone > 0 && item.DamageType == ModContent.GetInstance<ConcertoDamageClass>())
			{
				CounterA = (CounterA + 1) % 10;
			}
		}
		public override void OnHitNPCWithProj(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (damageDone > 0 && projectile.owner >= 0 && projectile.owner < Main.maxPlayers &&
				Main.player[projectile.owner].active && projectile.DamageType == ModContent.GetInstance<ConcertoDamageClass>())
			{
				CounterA = (CounterA + 1) % 10;
			}
		}
	}
	public class ConcertoNPC : GlobalNPC
	{
		public int CounterB { get; set; } = 1;
		public Queue<int> LastHits { get; } = new Queue<int>(5);
		public override bool InstancePerEntity => true;
		public override void OnKill(NPC npc)
		{
			CounterB = 1;
			LastHits.Clear();
		}
		public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			if (damageDone > 0 && item.DamageType == ModContent.GetInstance<ConcertoDamageClass>()) 
				ProcessPlayerHit(npc, player, hit.Damage);
		}
		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (damageDone > 0 && projectile.owner >= 0 && projectile.owner < Main.maxPlayers && 
				Main.player[projectile.owner].active && projectile.DamageType == ModContent.GetInstance<ConcertoDamageClass>())
			{
				Player player = Main.player[projectile.owner];
				ProcessPlayerHit(npc, player, hit.Damage);
			}
		}
		private void ProcessPlayerHit(NPC npc, Player player, int damage)//B计数
		{
			if (!player.TryGetModPlayer(out ConcertoPlayer modPlayer)) return;
			if (!npc.TryGetGlobalNPC(out ConcertoNPC globalNPC)) return;
			globalNPC.UpdateHitHistory(damage);
			globalNPC.CounterB = globalNPC.CounterB % 10 + 1;
			if (modPlayer.CounterA == globalNPC.CounterB - 1)
			{
				TriggerConcertoEffect(player, npc);
			}
			globalNPC.SyncCounters(npc);
		}
		public void UpdateHitHistory(int damage)
		{
			LastHits.Enqueue(damage);
			if (LastHits.Count > CounterB) LastHits.Dequeue();
		}
		public void TriggerConcertoEffect(Player player, NPC target)
		{
			int HealAmount = (int)(player.statLifeMax2 * 0.04);
			player.statLife = Math.Min(player.statLife + HealAmount, player.statLifeMax2);
			player.HealEffect(HealAmount, true);//玩家回血
			int TotalDamage = LastHits.Sum();
			CombatText.NewText(
		target.getRect(),
		new Color(80, 120, 255), // 深蓝色
		TotalDamage,
		dramatic: true,
		dot: false); //额外伤害
			for (int i = 0; i < 8; i++)
			{
				Dust dust = Dust.NewDustDirect(
					target.Top,
					target.width,
					10, // 只在顶部区域生成
					DustID.BlueFairy, // 蓝色仙尘
					Main.rand.NextFloat(-2f, 2f),
					Main.rand.NextFloat(-4f, -1f),
					100, new Color(100, 150, 255),
					Main.rand.NextFloat(1.2f, 1.8f)
				);
				dust.noGravity = true;
			}
			Lighting.AddLight(target.Center, 0.1f, 0.3f, 1f);
			SpawnEffects(target.Center);
			SoundEngine.PlaySound(SoundID.Item4, target.Center);//特效：粒子
			SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Item_43"), target.Center);//播放音效:叮
			target.SimpleStrikeNPC(TotalDamage, 0);
			SpawnEffects(target.Center);
			SoundEngine.PlaySound(SoundID.Item4, target.Center);
		}
		private static void SpawnEffects(Vector2 position)
		{
			for (int i = 0; i < 15; i++)
			{
				Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.BlueCrystalShard);
				dust.color = new Color(
					Main.rand.Next(80, 120),
					Main.rand.Next(120, 180),
					Main.rand.Next(200, 255)
				);
				dust.velocity = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-6f, -1f));
				dust.noGravity = true;
				dust.scale = Main.rand.NextFloat(1.0f, 1.6f);
			}
			Lighting.AddLight(position, 0.2f, 0.3f, 0.7f);
		}
		public void SyncCounters(NPC npc)
		{
			if (Main.netMode == NetmodeID.Server)
			{
				npc.netUpdate = true;
			}
		}
		public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter writer)
		{
			writer.Write(CounterB);
			writer.Write(LastHits.Count);
			foreach (int hit in LastHits) writer.Write(hit);
		}
		public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader reader)
		{
			CounterB = reader.ReadInt32();
			int count = reader.ReadInt32();
			LastHits.Clear();
			for (int i = 0; i < count; i++) LastHits.Enqueue(reader.ReadInt32());
		}
	}
	public class ConcertoPlayerDraw : PlayerDrawLayer//给玩家加计数显示
	{
		public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Head);
		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			var config = ModContent.GetInstance<ConcertoConfig>();
			if (!config.ShowPlayerCounter) return;
			Player drawPlayer = drawInfo.drawPlayer;
			if (drawPlayer.dead || !drawPlayer.active) return;
			var modPlayer = drawPlayer.GetModPlayer<ConcertoPlayer>();
			if (modPlayer == null) return;
			Vector2 position = drawPlayer.Top - new Vector2(10, 20) - Main.screenPosition;
			Vector2 shadowOffset = new Vector2(1, 1);
			Utils.DrawBorderString(Main.spriteBatch, $"A={modPlayer.CounterA}", position + shadowOffset, Color.Black * 0.7f, 0.9f);//阴影效果
			Utils.DrawBorderString(Main.spriteBatch, $"A={modPlayer.CounterA}", position, Color.Cyan, 0.9f);//主文本
		}
	}
	public class ConcertoCounterDraw : GlobalNPC//给NPC加计数显示
	{
		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var config = ModContent.GetInstance<ConcertoConfig>();
			if (!config.ShowEnemyCounter) return;
			if (!npc.active || npc.friendly || npc.townNPC) return;
			if (!npc.TryGetGlobalNPC(out ConcertoNPC globalNPC)) return;
			Vector2 position = npc.Top - new Vector2(16, 20) - Main.screenPosition;
			//检查是否在屏幕内
			if (position.X < -50 || position.X > Main.screenWidth + 50 ||
				position.Y < -50 || position.Y > Main.screenHeight + 50)
			{
				return;
			}
			Vector2 shadowOffset = new Vector2(1, 1);
			Utils.DrawBorderString(spriteBatch, $"B={globalNPC.CounterB}", position + shadowOffset, Color.Black * 0.7f, 0.9f);//阴影效果
			Utils.DrawBorderString(spriteBatch, $"B={globalNPC.CounterB}", position, Color.Orange, 0.9f);//主文本
		}
	}
	public class ConcertoUI : ModSystem//上方靠左有计数
	{
		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			var config = ModContent.GetInstance<ConcertoConfig>();
			if (!config.ShowPlayerCounter) return;
			Player player = Main.LocalPlayer;
			if (player.dead) return;
			var modPlayer = player.GetModPlayer<ConcertoPlayer>();
			Utils.DrawBorderString(spriteBatch, $"A={modPlayer.CounterA}", new Vector2(640, 20), Color.Cyan);//左上角显示玩家计数器
		}
	}
}