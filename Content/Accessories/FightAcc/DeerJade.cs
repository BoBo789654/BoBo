using BoBo.Content.Buffs.Good;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Accessories.FightAcc
{       
	/// <summary>		
	/// [c/FFD700:★★★][c/DAA520:☆☆]
	/// [c/FF6347:血最大生命值减半，自然生命恢复为0，掉落的红心回复量为0，药水疾病debuff将一直存在]
	/// [c/00BFFF:防御技：受伤1次获得1层标记，标记层数无上限]
	/// [c/32CD32:获得20%的减伤基础，每层标记可以额外增加5%的减伤，上限50%]
	/// [c/FF8C00:复活技：首次受到致命伤害后复活并无敌3秒，复活冷却180秒]
	/// [c/FFD700:标记不消失，不再增加伤害减免，每层标记增加5%远程伤害，增加5%远程暴击]
	/// [c/FF69B4:治愈技：每45秒治愈当前最大生命值的50%血量]
	/// [c/FF69B4:治愈后，若此时生命值大于75%，则增加25%伤害，若小于75%，额外回复20生命值]
	/// </summary>
	public class DeerJade : ModItem
	{
		public override string Texture => Pictures.FightAcc + Name;
		public override void SetDefaults()
		{
			Item.width = 54;
			Item.height = 56;
			Item.accessory = true;
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(1, 0, 0, 0);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<DeerJadePlayer>().HasDeerJade = true;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.CharmofMyths);//神话护身符
			recipe.AddIngredient(ItemID.PanicNecklace);//恐慌项链
			recipe.AddIngredient(ItemID.LifeCrystal, 5);//生命水晶
			recipe.AddTile(TileID.TinkerersWorkbench);//工匠作坊
			recipe.AddTile(TileID.LunarCraftingStation);//远古操纵机
			recipe.Register();
		}
	}
	public class DeerJadePlayer : ModPlayer
	{
		public bool HasDeerJade;//判断是否穿戴该饰品
		public int MarkCount;//标记层数
		public int ReviveCooldown;//复活冷却
		public bool IsRevived;//判断是否用过复活
		public int HealTimer;//治疗时间
		public int MarkHealTimer;//标记恢复的时间
		private const int ReviveMaxCooldown = 180 * 60;//复活冷却时间
		private const int HealInterval = 45 * 60;//补充回血的时间间隔
		private const int MarkHealDelay = 30;//半秒后回血（用来表示标记层数的）
		public float TempDamageBonus;//存储临时伤害加成
		public float DamageReduction;//存储临时伤害减免
		private int PreviousLife;//先前生命值
		private int PreviousMaxLife;//先前最大生命值
		public override void ResetEffects()
		{
			if (HasDeerJade && !Player.dead)//这里要加一个记录血量的功能，免得拿这个饰品锁血
			{
				//计算装备状态下生命值比例
				float HealthRatio = (float)Player.statLife / Player.statLifeMax2;
				//当卸下饰品时恢复生命值比例
				if (!HasDeerJade)
				{
					int NewMaxLife = PreviousMaxLife;
					int NewLife = (int)(NewMaxLife * HealthRatio);
					Player.statLife = NewLife;
					Player.statLifeMax = NewMaxLife;
					Player.statLifeMax2 = NewMaxLife;
				}
			}
			HasDeerJade = false;
			TempDamageBonus = 0f;
			DamageReduction = 0f;
		}
		public override void UpdateDead()
		{
			if (IsRevived)
			{
				Player.immune = true;
				Player.immuneTime = Math.Max(Player.immuneTime, 180);
			}
		}
		public override void UpdateLifeRegen()
		{
			if (!HasDeerJade) return;
			Player.lifeRegen = 0;//生命恢复设为0
			Player.lifeRegenTime = 0;//这样没有了自然恢复时间
			Player.AddBuff(BuffID.PotionSickness, 2, true);//持续药水疾病debuff
		}
		public override void PostUpdate()
		{
			if (!HasDeerJade)
			{
				PreviousLife = Player.statLife;
				PreviousMaxLife = Player.statLifeMax2;
			}
			if (Player.dead)//玩家死亡，重置一些数值
			{
				MarkCount = 0;
				ReviveCooldown = 0;
				IsRevived = false;
			}
			if (!HasDeerJade) return;
			if (!IsRevived)//计算减伤效果（基础20% + 标记层数*5%，上限50%）
			{
				float BaseReduction = 0.20f;
				float MarkReduction = Math.Min(MarkCount * 0.05f, 0.50f - BaseReduction);
				DamageReduction = BaseReduction + MarkReduction;
			}
			else//复活状态下没有减伤
				DamageReduction = 0f;
			if (IsRevived)//计算伤害加成（复活状态下每层标记增加5%伤害）
				TempDamageBonus = MarkCount * 0.05f;
			else//没复活时没增伤
				TempDamageBonus = 0f;
			//伤害加成与伤害减免
			Player.GetDamage(DamageClass.Ranged) += TempDamageBonus;
			Player.GetCritChance(DamageClass.Ranged) += TempDamageBonus * 100;
			Player.endurance = DamageReduction;
			//复活冷却计时
			if (ReviveCooldown > 0 && !IsRevived)
			{
				ReviveCooldown--;
				if (ReviveCooldown == 0)
					Main.NewText("下次复活已就绪！", Color.Green);
			}
			//治愈计时器
			if (HealTimer < HealInterval)
				HealTimer++;
			else
			{
				PerformHealEffect();
				HealTimer = 0;
			}
			//标记层数回血计时器
			if (MarkHealTimer > 0)
			{
				MarkHealTimer--;
				//半秒后触发回血
				if (MarkHealTimer == 0)
				{
					PerformMarkHeal();
				}
			}
		}
		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
		{
			health = StatModifier.Default;
			mana = StatModifier.Default;
			if (HasDeerJade)
			{
				health = new StatModifier(0.5f, 0.5f);//最大生命值减半
			}
		}
		public override void OnHurt(Player.HurtInfo info)
		{
			if (!HasDeerJade) return;
			//增加标记层数
			MarkCount++;
			//启动回血计时器（半秒后回血）
			MarkHealTimer = MarkHealDelay;
			//检测致命伤害（考虑减伤后的实际伤害）
			int ActualDamage = (int)(info.Damage * (1f - DamageReduction));
			bool isLethal = (Player.statLife - ActualDamage) <= 0;
			if (isLethal && !IsRevived && ReviveCooldown <= 0)
			{
				//触发复活：完全吸收此次伤害
				info.Damage = 0;
				//设置复活状态
				Player.Heal((int)(Player.statLifeMax2 * 0.75f));//恢复75%生命值
				Player.AddBuff(BuffID.ShadowDodge, 180);//3秒无敌
				IsRevived = true;
				ReviveCooldown = ReviveMaxCooldown;
				//复活特效
				for (int i = 0; i < 30; i++)
				{
					Dust.NewDust(Player.position, Player.width, Player.height,
						DustID.GreenTorch, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-6, 1),
						150, Color.LimeGreen, 1.8f);
				}
				Main.NewText("通灵！玩家进入二阶段~~~", Color.LimeGreen);
			}
		}
		//标记层数回血方法
		private void PerformMarkHeal()
		{
			//每层标记回血1点
			int healAmount = MarkCount;
			Player.statLife = Math.Min(Player.statLife + healAmount, Player.statLifeMax2);
			//创建绿色治疗粒子效果
			Player.HealEffect(healAmount);
			//创建自定义粒子效果（绿色十字）
			for (int i = 0; i < Math.Min(healAmount, 10); i++)
			{
				Dust.NewDust(Player.position, Player.width, Player.height,
					DustID.GreenTorch, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-4, 0),
					150, Color.LimeGreen, 0.8f);
			}
		}
		private void PerformHealEffect()
		{
			//恢复50%最大生命值
			int HealAmount = (int)(Player.statLifeMax2 * 0.5f);
			Player.statLife = Math.Min(Player.statLife + HealAmount, Player.statLifeMax2);
			Player.HealEffect(HealAmount);
			//检查生命值比例
			float HealthRatio = (float)Player.statLife / Player.statLifeMax2;
			bool ShouldAddDamage = false;//标记是否应该添加伤害加成
			if (HealthRatio > 0.75f)//若血量大于75%
			{
				ShouldAddDamage = true;
			}
			else
			{
				int AdditionalHeal = 20;//额外回复20生命值
				Player.statLife = Math.Min(Player.statLife + AdditionalHeal, Player.statLifeMax2);
				Player.HealEffect(AdditionalHeal);
				//再次检查比例
				HealthRatio = (float)Player.statLife / Player.statLifeMax2;
				if (HealthRatio > 0.75f)//若血量大于75%，这样的就不算
				{
					ShouldAddDamage = false;
				}
			}
			if (ShouldAddDamage)
			{
				Player.AddBuff(ModContent.BuffType<DeerJadeDamageBuff>(), 300);
			}
		}
	}
	public class DeerJadeGlobalItem : GlobalItem//针对饰品的红心消耗设置
	{
		public override bool AppliesToEntity(Item entity, bool lateInstantiation)
		{
			return entity.type == ItemID.Heart;
		}
		public override bool OnPickup(Item item, Player player)
		{
			if (player.GetModPlayer<DeerJadePlayer>().HasDeerJade)
			{
				return false;//不执行默认的拾取行为（即不加血、不播放声音等），消耗物品（使其消失）
			}
			return base.OnPickup(item, player);
		}
	}
}