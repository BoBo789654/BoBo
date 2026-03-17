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
	/// [c/FF4500:获得3次复活机会，每次复活会+25%近战伤害、+25%近战暴击、-10%减伤、-10%防御]
	/// [c/FF4500:每次复活恢复的血量随当前复活阶段造成的伤害增加]
	/// </summary>
	public class HoohMirrorAcc : ModItem//复活镜子
	{
		public override string Texture => Pictures.FightAcc + Name;

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.accessory = true;
			Item.rare =	Main.rand.NextBool() ? ItemRarityID.Cyan : ItemRarityID.LightRed;
			Item.value = Item.sellPrice(1, 0, 0, 0);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<HoohMirrorPlayer>().HasHoohMirror = true;
			var modPlayer = player.GetModPlayer<BronzeBellPlayer>();
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.IceMirror, 2);//冰雪镜
			recipe.AddIngredient(ItemID.DaoofPow);//太极连枷
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.Register();
		}
	}
	public class HoohMirrorPlayer : ModPlayer
	{
		public bool HasHoohMirror;
		public int HoohCount = 3;//剩余复活次数
		public int HoohUsed;//已使用复活次数
		public float DamageDealtThisLife;//当前这条命造成的总伤害
		//复活惩罚/增益
		public float HoohEndurancePenalty;
		public int HoohDefensePenalty;
		public float HoohMeleeBonus;
		public override void ResetEffects()
		{
			HasHoohMirror = false;
			Player.GetDamage(DamageClass.Melee) += HoohMeleeBonus;
			Player.GetCritChance(DamageClass.Melee) += HoohMeleeBonus * 100;
			Player.endurance -= HoohEndurancePenalty;
			Player.statDefense *= (1 - HoohEndurancePenalty);
		}
		public override void PostUpdate()
		{
			if (Player.dead)
			{
				//玩家死亡时重置所有复活状态
				HoohCount = 3;
				HoohUsed = 0;
				HoohEndurancePenalty = 0f;
				HoohDefensePenalty = 0;
				HoohMeleeBonus = 0f;
				DamageDealtThisLife = 0;
			}
			if (!HasHoohMirror) return;
		}
		//记录玩家造成的伤害
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (HasHoohMirror)
			{
				DamageDealtThisLife += damageDone;
			}
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (HasHoohMirror)
			{
				DamageDealtThisLife += damageDone;
			}
		}
		public override void OnHurt(Player.HurtInfo info)
		{
			if (!HasHoohMirror) return;
			bool isLethal = Player.statLife - info.Damage <= 0;
			//触发复活
			if (isLethal && HoohCount > 0)
			{
				info.Damage = 0;
				//计算回复血量
				int healAmount = CalculateHoohHeal();
				Player.Heal(healAmount + 1);
				//应用复活效果
				Player.AddBuff(BuffID.ShadowDodge, 180);//3秒无敌
				//更新复活计数
				HoohCount--;
				HoohUsed++;
				//应用复活惩罚/增益
				ApplyHoohEffects();
				//复活特效
				for (int i = 0; i < 30; i++)
				{
					Dust.NewDust(Player.position, Player.width, Player.height,
						DustID.GreenTorch, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-6, 1),
						150, Color.BlueViolet, 1.8f);
				}
				if (HoohUsed < 3)
					Main.NewText($"涅槃！玩家进入第{HoohUsed}次复活阶段", Color.LimeGreen);
				else if (HoohUsed == 3)
					Main.NewText($"涅槃！玩家进入第最后1次复活阶段", Color.LimeGreen);
				//重置伤害计数
				DamageDealtThisLife = 0;
			}
		}
		//复活回复血量
		private int CalculateHoohHeal()
		{
			//根据游戏进度确定除数
			int divisor = 50;
			if (NPC.downedMechBossAny) divisor = 200;//肉山后
			if (NPC.downedPlantBoss) divisor = 600; //世纪之花后
			if (NPC.downedMoonlord) divisor = 1000;  //月亮领主后
			//计算回复量并限制在合理范围内
			int heal = (int)(DamageDealtThisLife / divisor);
			return Math.Clamp(heal, 0, 1000000);
		}
		//复活效果
		private void ApplyHoohEffects()
		{
			//每次复活增加惩罚/增益
			HoohEndurancePenalty += 0.1f;//减伤-10%
			//HoohDefensePenalty += (int)(Player.statDefense * 0.10f);//防御-10%
			HoohMeleeBonus += 0.25f;//近战伤害+25%
		}
	}
}