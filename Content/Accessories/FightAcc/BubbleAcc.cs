using BoBo.Content.Buffs.Good;
using BoBo.Content.Projectiles.Accessories.FightAcc;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic; //添加此命名空间以使用List
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Accessories.FightAcc
{
	public class BubbleAcc : ModItem
	{
		public override string Texture => Pictures.FightAcc + Name;
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPlayer = player.GetModPlayer<BubbleAccPlayer>();
			modPlayer.HasBubble = true;
			modPlayer.ShowBubble = !hideVisual;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Bubble, 150);//泡泡
			recipe.AddIngredient(ItemID.LifeCrystal, 5);//生命水晶
			recipe.AddIngredient(ItemID.LifeFruit, 5);//生命果
			recipe.AddTile(TileID.TinkerersWorkbench);//工匠作坊
			recipe.Register();
		}
		public class BubbleAccPlayer : ModPlayer
		{
			public bool HasBubble;
			public bool ShowBubble;
			public int BubbleTimer = 0;
			public const int BubbleCooldown = 300;
			public bool BubbleActive = false;
			private int BubbleShieldProjIndex = -1;
			private List<int> BubbleBurstIndices = new List<int>();//改为列表存储多个索引

			public override void ResetEffects()
			{
				if (!HasBubble)
				{
					//脱下饰品时清除所有相关弹幕
					if (BubbleShieldProjIndex != -1 && Main.projectile.IndexInRange(BubbleShieldProjIndex))
					{
						Projectile shieldProj = Main.projectile[BubbleShieldProjIndex];
						if (shieldProj.active && shieldProj.owner == Player.whoAmI &&
							shieldProj.type == ModContent.ProjectileType<BubbleShield>())
						{
							shieldProj.Kill();
						}
						BubbleShieldProjIndex = -1;
					}
					//清除所有BubbleBurst
					foreach (int index in BubbleBurstIndices)
					{
						if (index != -1 && Main.projectile.IndexInRange(index))
						{
							Projectile burstProj = Main.projectile[index];
							if (burstProj.active && burstProj.owner == Player.whoAmI &&
								burstProj.type == ModContent.ProjectileType<BubbleBurst>())
							{
								burstProj.Kill();
							}
						}
					}
					BubbleBurstIndices.Clear();
					ShowBubble = false;
					BubbleTimer = 0;
					BubbleActive = false;
				}
				HasBubble = false;
				if (BubbleActive)
				{
					BubbleTimer = 0;
				}
			}
			public override void PostUpdate()
			{
				if (ShowBubble && !BubbleActive && BubbleTimer >= BubbleCooldown)
				{
					if (Main.myPlayer == Player.whoAmI &&
						Player.ownedProjectileCounts[ModContent.ProjectileType<BubbleShield>()] == 0)
					{
						//生成泡泡护盾
						BubbleShieldProjIndex = Projectile.NewProjectile(
							Player.GetSource_FromThis(),
							Player.Center,
							Vector2.Zero,
							ModContent.ProjectileType<BubbleShield>(),
							0, 0, Player.whoAmI);
						BubbleBurstIndices.Clear();//清除之前的BubbleBurst索引
						for (int i = 0; i < 4; i++)//生成n组BubbleBurst，每个都有独立的索引
						{
							Vector2 velocity = Main.rand.NextVector2Circular(-3f, 3f);
							int newBurstIndex = Projectile.NewProjectile(
								Player.GetSource_FromThis(),
								Player.Center,
								velocity * 1.5f,
								ModContent.ProjectileType<BubbleBurst>(),
								0, 0, Player.whoAmI);
							BubbleBurstIndices.Add(newBurstIndex);
						}
						BubbleActive = true;
					}
					BubbleTimer = 0;
				}
				else if (ShowBubble && !BubbleActive)
				{
					BubbleTimer++;
					if (!BubbleActive)//只有在没有激活泡泡时才重置索引
					{
						BubbleShieldProjIndex = -1;
						BubbleBurstIndices.Clear();
					}
				}
			}
			//受击检测
			public override void OnHurt(Player.HurtInfo info)
			{
				if (ShowBubble && BubbleActive)
				{
					TriggerBubbleEffect();//泡泡效果
					BubbleActive = false;
					BubbleTimer = 0;
				}
			}
			private void TriggerBubbleEffect()
			{
				int heal = (int)(Player.statLifeMax2 * 0.2f);//玩家回血
				Player.HealEffect(heal);
				Vector2 playerCenter = Player.Center;
				for (int i = 0; i < Main.maxNPCs; i++)//击退周围敌人
				{
					NPC npc = Main.npc[i];
					if (npc.active && !npc.friendly && npc.Distance(playerCenter) < 300f)
					{
						Vector2 knockbackDir = npc.DirectionFrom(playerCenter);
						npc.velocity = knockbackDir * 15f;//强力击退
					}
				}
				//生成BubbleShieldCrack
				Projectile.NewProjectile(
					Player.GetSource_FromThis(),
					Player.Center,
					Vector2.Zero,
					ModContent.ProjectileType<BubbleShieldCrack>(),
					0, 0, Player.whoAmI);
				//清除BubbleShield
				if (BubbleShieldProjIndex != -1 && Main.projectile.IndexInRange(BubbleShieldProjIndex))
				{
					Projectile shieldProj = Main.projectile[BubbleShieldProjIndex];
					if (shieldProj.active && shieldProj.owner == Player.whoAmI &&
						shieldProj.type == ModContent.ProjectileType<BubbleShield>())
					{
						shieldProj.Kill();
					}
				}
				//清除所有BubbleBurst
				foreach (int index in BubbleBurstIndices)
				{
					if (index != -1 && Main.projectile.IndexInRange(index))
					{
						Projectile burstProj = Main.projectile[index];
						if (burstProj.active && burstProj.owner == Player.whoAmI &&
							burstProj.type == ModContent.ProjectileType<BubbleBurst>())
						{
							burstProj.Kill();
						}
					}
				}
				BubbleBurstIndices.Clear();
				Player.AddBuff(ModContent.BuffType<BubbleBuff>(), 600);//给个10秒
			}
			//当玩家死亡时也清除弹幕
			public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
			{
				//清除BubbleShield
				if (BubbleShieldProjIndex != -1 && Main.projectile.IndexInRange(BubbleShieldProjIndex))
				{
					Projectile shieldProj = Main.projectile[BubbleShieldProjIndex];
					if (shieldProj.active && shieldProj.owner == Player.whoAmI &&
						shieldProj.type == ModContent.ProjectileType<BubbleShield>())
					{
						shieldProj.Kill();
					}
				}
				//清除所有BubbleBurst
				foreach (int index in BubbleBurstIndices)
				{
					if (index != -1 && Main.projectile.IndexInRange(index))
					{
						Projectile burstProj = Main.projectile[index];
						if (burstProj.active && burstProj.owner == Player.whoAmI &&
							burstProj.type == ModContent.ProjectileType<BubbleBurst>())
						{
							burstProj.Kill();
						}
					}
				}
				BubbleBurstIndices.Clear();
			}
		}
	}
}