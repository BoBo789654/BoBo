using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Accessories.FightAcc
{
	public class WoJianJiWoDe : ModItem
	{
		public override string Texture => Pictures.FightAcc + Name;
		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 32;
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(3, 0, 0, 0);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<WoJianPlayer>().HasWoJian = true;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.BlackLens, 10);//黑晶状体	
			recipe.AddIngredient(ItemID.EyeOfCthulhuPetItem, 1);//可疑咧嘴眼球
			recipe.AddIngredient(ItemID.DeerclopsPetItem, 1);//独眼巨鹿眼球
			recipe.AddIngredient(ItemID.Eyebrella, 1);//眼球伞
			recipe.AddTile(TileID.TinkerersWorkbench);//工匠作坊
			recipe.Register();
		}
	}
	public class WoJianPlayer : ModPlayer
	{
		public bool HasWoJian = false;
		public float CurrentDefense = 0f;
		public float CurrentEndurance = 0f;
		private int Timer = 0;
		public override void ResetEffects()
		{
			HasWoJian = false;
			CurrentDefense = 0f;
			CurrentEndurance = 0f;
		}
		public override void PostUpdate()
		{
			if (!HasWoJian) return;
			Timer++;
			float TotalDefenseReduction = 0f;
			float TotalAttackReduction = 0f;
			int AffectedEnemies = 0;
			for (int i = 0; i < Main.maxNPCs; i++)//遍历所有NPC
			{
				NPC npc = Main.npc[i];//检查NPC是否有效
				if (npc.active && !npc.friendly && npc.life > 0 && npc.chaseable)
				{
					float distance = Vector2.Distance(Player.Center, npc.Center);
					if (distance <= 800f)//800范围内生效
					{
						AffectedEnemies++;
						float OriginalDefense = npc.defense;//计算防御降低
						float ReducedDefense = OriginalDefense * 0.8f; //降低20%
						float DefenseReduction = OriginalDefense * 0.2f;
						if (DefenseReduction < 0) DefenseReduction = 0;//确保不会出现负数
						TotalDefenseReduction += DefenseReduction;
						float OriginalDamage = npc.damage;//计算攻击力降低
						float Endurance = 25f; //基础降低25点
						if (distance <= 160f)//160范围内额外降低25点
							Endurance += 25f;
						if (Endurance > OriginalDamage)//确保不会降低到负数
							Endurance = OriginalDamage;
						TotalAttackReduction += Endurance;
						if (Timer % 20 == 0)//添加敌人身上的视觉效果
						{
							Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height,
								DustID.BlueCrystalShard, 0f, 0f, 100, default, 0.8f);
							dust.noGravity = true;
							dust.velocity *= 0.5f;
						}
					}
				}
			}
			//计算玩家增益
			CurrentDefense = Math.Min(TotalDefenseReduction, 150f);
			CurrentEndurance = Math.Min(TotalAttackReduction * 0.003f / 25, 0.15f);
			//应用增益
			Player.statDefense += (int)CurrentDefense;
			Player.endurance += CurrentEndurance;
			//视觉反馈系统
			HandleVisualFeedback(AffectedEnemies);
		}
		private void HandleVisualFeedback(int AffectedEnemies)
		{
			if (AffectedEnemies > 0)//根据影响敌人的数量改变玩家周围的粒子效果
			{
				Lighting.AddLight(Player.Center, 0.1f, 0.2f, 0.4f);//基础发光效果
				float intensity = Math.Max(CurrentDefense / 150f, CurrentEndurance / 0.15f);//粒子效果强度基于增益大小
				if (Timer % 5 == 0)//产生环绕粒子
				{
					for (int i = 0; i < 2; i++)
					{
						Vector2 offset = new Vector2(0, 40).RotatedBy(MathHelper.TwoPi * Main.rand.NextFloat());
						Dust dust = Dust.NewDustDirect(Player.Center + offset, 10, 10,
							DustID.BlueFairy, 0f, 0f, 100, default, 1.2f * intensity);
						dust.noGravity = true;
						dust.velocity = Player.velocity * 0.5f;
					}
				}
				if (CurrentDefense > 100f || CurrentEndurance > 0.1f)//当增益接近上限时显示更强烈的效果
				{
					Lighting.AddLight(Player.Center, 0.3f, 0.5f, 0.8f);
					if (Timer % 3 == 0)
					{
						Vector2 offset = new Vector2(0, 30).RotatedBy(MathHelper.TwoPi * Main.rand.NextFloat());
						Dust dust = Dust.NewDustDirect(Player.Center + offset, 10, 10,
							DustID.GoldFlame, 0f, 0f, 100, default, 1.5f);
						dust.noGravity = true;
					}
				}
			}
			if (Timer > 1000) Timer = 0;//重置计时器
		}
		public override void PostHurt(Player.HurtInfo info)//在玩家受到伤害时显示特殊效果
		{
			if (HasWoJian && (CurrentDefense > 0 || CurrentEndurance > 0))
			{
				for (int i = 0; i < 10; i++)//显示减伤效果
				{
					Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height,
						DustID.BlueCrystalShard, 0f, 0f, 100, default, 1.5f);
					dust.noGravity = true;
					dust.velocity *= 2f;
				}
			}
		}
	}
	public class WoJianTooltip : GlobalItem
	{
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (item.type == ModContent.ItemType<WoJianJiWoDe>())
			{
				Player player = Main.LocalPlayer;
				var modPlayer = player.GetModPlayer<WoJianPlayer>();
				if (modPlayer.HasWoJian)//添加当前增益信息到工具提示
				{
					tooltips.Add(new TooltipLine(Mod, "WoJianDefense",
						$"当前防御加成(CurrentDefense): +{modPlayer.CurrentDefense:F0}/150"));//只在鼠标悬停时显示基本信息
					tooltips.Add(new TooltipLine(Mod, "WoJianReduction",
						$"当前减伤加成(CurrentEndurance): +{modPlayer.CurrentEndurance:P1}/15%"));//只在鼠标悬停时显示基本信息
					tooltips[tooltips.Count - 2].OverrideColor = Color.LightBlue;//颜色
					tooltips[tooltips.Count - 1].OverrideColor = Color.LightGreen;//颜色
				}
			}
		}
	}
}