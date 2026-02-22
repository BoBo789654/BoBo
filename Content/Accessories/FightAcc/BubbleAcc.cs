using BoBo.Content.Buffs.Good;
using BoBo.Content.Projectiles.Accessories.FightAcc;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Accessories.FightAcc
{
	public class BubbleAcc : ModItem//出现问题：如何只有一个弹幕（已解决：可搜Index看如何使用Main.projectile[BubbleShieldProjIndex]，并且可以使得泡泡破碎后泡泡消失）
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
			public override void ResetEffects()
			{
				if (!HasBubble)
				{
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
						BubbleShieldProjIndex = Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
							ModContent.ProjectileType<BubbleShield>(), 0, 0, Player.whoAmI);
						BubbleActive = true;
					}
					BubbleTimer = 0;
				}
				else if (ShowBubble && !BubbleActive)
				{
					BubbleTimer++;
					BubbleShieldProjIndex = -1;
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
					Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero,
							ModContent.ProjectileType<BubbleShieldCrack>(), 0, 0, Player.whoAmI);
					Projectile proj = Main.projectile[BubbleShieldProjIndex];
					proj.Kill();
				}
			}
			private void TriggerBubbleEffect()
			{
				int heal = (int)(Player.statLifeMax2 * 0.2f);//玩家回血
				Player.HealEffect(heal);
				//击退周围敌人
				Vector2 playerCenter = Player.Center;
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					NPC npc = Main.npc[i];
					if (npc.active && !npc.friendly && npc.Distance(playerCenter) < 300f)
					{
						Vector2 knockbackDir = npc.DirectionFrom(playerCenter);
						npc.velocity = knockbackDir * 15f;//强力击退
					}
				}
				for (int i = 0; i < 16; i++)
				{
					Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
					Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, velocity * 1.5f,
						ModContent.ProjectileType<BubbleBurst>(), 0, 0, Player.whoAmI);
				}
				Player.AddBuff(ModContent.BuffType<BubbleBuff>(), 600);//给个10秒
			}
		}
	}
}