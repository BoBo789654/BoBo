using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Accessories.FightAcc
{
	/// <summary>
	/// [c/E34234:首次生命值降低至30%以下时，直接回满血，冷却为250秒]
	/// [c/FFD700:造成的近战伤害受到（+1%/10最大魔力值与回魔）]
	/// [c/E34234:每使用近战武器时，消耗魔力，即使魔力耗尽也不影响武器使用]
	/// [c/FFD700:消耗魔力为玩家充能，每100魔力：+10%减伤、-10%防御]
	/// [c/E34234:魔法伤害加成同时给近战伤害加成]
	/// </summary>
	public class LoongAxe : ModItem
	{
		public override string Texture => Pictures.FightAcc + Name;
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(1, 0, 0, 0);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<LoongAxePlayer>().HasLoongAxe = true;
			player.GetDamage<MeleeDamageClass>() = player.GetDamage<MagicDamageClass>();
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.LunarHamaxeSolar, 1);//耀斑锤斧
			recipe.AddIngredient(ItemID.LunarHamaxeVortex, 1);//星旋锤斧
			recipe.AddIngredient(ItemID.LunarHamaxeNebula, 1);//星云锤斧
			recipe.AddIngredient(ItemID.LunarHamaxeStardust, 1);//星尘锤斧
			recipe.AddTile(TileID.TinkerersWorkbench);//工匠作坊
			recipe.Register();
		}
	}
	public class LoongAxePlayer : ModPlayer
	{
		public bool HasLoongAxe;
		public int ManaConsumed;
		public bool LifeSaveTriggered;
		public int CooldownTimer;//保命冷却
		private const int MaxCooldown = 250 * 60;//保命冷却
		public int DecrementTimer;//递减计时器
		private const int DecrementInterval = 15 * 60;//递减间隔
		public float TotalBonus;
		//上限为80%的减伤属性
		private float DamageReduction
		{
			get
			{
				float raw = (ManaConsumed / 100f) * 0.10f;
				return MathHelper.Clamp(raw, 0f, 0.80f);
			}
		}
		//上限为80%的防御惩罚属性
		private float DefensePenalty
		{
			get
			{
				float raw = (ManaConsumed / 100f) * 0.10f;
				return MathHelper.Clamp(raw, 0f, 0.80f);
			}
		}
		public override void ResetEffects()
		{
			HasLoongAxe = false;
		}
		public override void PostUpdate()
		{
			if (!HasLoongAxe)
			{
				ManaConsumed = 0;
				DecrementTimer = 0;
				return;
			}
			//冷却计时
			if (CooldownTimer > 0)
			{
				CooldownTimer--;
				if (CooldownTimer == 0 && LifeSaveTriggered)
				{
					LifeSaveTriggered = false;
					Main.NewText("戕钺之力已恢复！", Color.Gold);
				}
			}
			//递减计时器更新
			DecrementTimer++;
			if (DecrementTimer >= DecrementInterval)
			{
				DecrementTimer = 0;
				DecrementManaConsumed();//执行递减
			}
		}
		// 每15秒递减10%充能（减少100点ManaConsumed）
		private void DecrementManaConsumed()
		{
			if (ManaConsumed > 0)
			{
				int decrementAmount = Math.Min(100, ManaConsumed);//每次减少100点
				ManaConsumed -= decrementAmount;
				//显示递减提示
				int newReduction = (int)(DamageReduction * 100);
				if (newReduction > 0)
				{
					Main.NewText($"戕钺之力衰减: 充能-10% (当前减伤{newReduction}%, 防御-{newReduction}%)", Color.LightGray);
				}
				else
				{
					Main.NewText("戕钺之力已完全消散", Color.LightGray);
				}
			}
		}
		public override void OnHurt(Player.HurtInfo info)
		{
			if (!HasLoongAxe || LifeSaveTriggered || CooldownTimer > 0) return;
			//计算实际生命值
			int actualDamage = (int)(info.Damage * (1f - DamageReduction));
			float lifeAfterDamage = Player.statLife - actualDamage;
			float threshold = Player.statLifeMax2 * 0.3f;
			//检测生命值是否降至30%以下
			if (lifeAfterDamage <= threshold)
			{
				//触发保命效果
				Player.statLife = Player.statLifeMax2;
				Player.HealEffect(Player.statLifeMax2 - Player.statLife);
				for (int i = 0; i < 50; i++)
				{
					Dust.NewDust(Player.position, Player.width, Player.height,
						DustID.GoldFlame, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-7, 1),
						150, default, 2.5f);
				}
				LifeSaveTriggered = true;
				CooldownTimer = MaxCooldown;
				Main.NewText("戕钺守护！", Color.Gold);
				//完全吸收伤害
				Player.immune = true;
				Player.immuneTime = Math.Max(Player.immuneTime, 180);
			}
		}
		public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
		{
			if (HasLoongAxe && item.DamageType.CountsAsClass(DamageClass.Melee))
			{
				// 魔力加成计算
				float ManaBonus = Player.statManaMax2 / 10f * 0.01f;
				float RegenBonus = Player.manaRegen / 10f * 0.01f;
				float TotalBonus = ManaBonus + RegenBonus;
				damage += TotalBonus;
			}
		}
		public override void PostUpdateEquips()
		{
			if (!HasLoongAxe) return;
			//应用减伤和防御惩罚
			Player.endurance += DamageReduction;
			Player.statDefense *= (1 - DefensePenalty);
			//添加充能提示（每10秒一次）
			if (ManaConsumed > 0 && Main.rand.NextBool(600))
			{
				int chargeLevel = (int)(DamageReduction * 100);
				int defensePenalty = (int)(DefensePenalty * 100);
				Main.NewText($"戕钺充能: 减伤{chargeLevel}%，防御-{defensePenalty}%", Color.Goldenrod);
			}
		}
	}

	public class LoongAxeGlobalItem : GlobalItem
	{
		public override bool AppliesToEntity(Item entity, bool lateInstantiation)
		{
			//同时应用于近战和魔法武器
			return (entity.DamageType.CountsAsClass(DamageClass.Melee) ||
				   entity.DamageType.CountsAsClass(DamageClass.Magic)) &&
				   entity.damage > 0;
		}
		public override bool? UseItem(Item item, Player player)
		{
			var modPlayer = player.GetModPlayer<LoongAxePlayer>();
			if (!modPlayer.HasLoongAxe) return base.UseItem(item, player);
			if (player.statMana >= 20)
			{
				player.statMana -= 5;
				modPlayer.ManaConsumed += 1;
				modPlayer.DecrementTimer = 0;
				if (modPlayer.ManaConsumed > 800)//确保800为上限
					modPlayer.ManaConsumed = 800;
			}
			return base.UseItem(item, player);
		}
	}
}