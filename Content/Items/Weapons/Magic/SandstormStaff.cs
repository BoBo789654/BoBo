using BoBo.Content.Projectiles.Weapons.Magic;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Weapons.Magic
{
	public class SandstormStaff : ModItem
	{
		public override string Texture => Pictures.Magic + Name;
		public override void SetStaticDefaults()
		{
			Item.staff[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.damage = 165;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 15;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 120;
			Item.useAnimation = 120;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0f;
			Item.value = Item.sellPrice(0, 15, 0, 0);
			Item.rare = ItemRarityID.Yellow;
			Item.UseSound = SoundID.Item66 with { Pitch = -0.2f };
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<LargeDesertTornado>();
			Item.shootSpeed = 10f; 
			Item.scale = 1f;
		}
		public override bool AltFunctionUse(Player player)//启用右键
		{
			return true;
		}
		public override bool CanUseItem(Player player)
		{
			List<Vector2> TornadoPositions = new List<Vector2>();//列表
			if (player.altFunctionUse == 2)//右键
			{
				//寻找并引爆已有的龙卷风
				bool hasTornado = false;
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile proj = Main.projectile[i];
					if (proj.active && proj.type == ModContent.ProjectileType<LargeDesertTornado>() &&
						proj.owner == player.whoAmI)
					{
						TornadoPositions.Add(proj.Center);//记位置
						proj.Kill();//引爆龙卷风
						hasTornado = true;
					}
				}
				/*if (hasTornado)
				{
					foreach (Vector2 TornadoCenter in TornadoPositions)
					{
						//右键引爆音效
						SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.8f }, player.Center);
						//创建爆炸特效
						for (int i = 0; i < 250; i++)
						{
							Dust dust = Dust.NewDustDirect(player.Center, 0, 0, DustID.Sandstorm,
								Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 2f);
							dust.noGravity = true;
						}
						//爆炸伤害
						foreach (NPC npc in Main.npc)
						{
							if (npc.active && !npc.friendly && npc.Distance(TornadoCenter) < 200f)
							{
								int damage = (int)(Item.damage * 1.5f);
								npc.SimpleStrikeNPC(damage, 0, false, 0f, null, false, 0, true);
							}
						}
					}
				}*/
				return false;//右键不发射新龙卷风
			}
			if (player.altFunctionUse != 1)//左键发射一个新的，一个玩家场上只能有一个LargeDesertTornado
			{
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile proj = Main.projectile[i];
					if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<LargeDesertTornado>())
					{
						proj.Kill();
					}
				}
			}
			return base.CanUseItem(player);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse == 2) return false;//右键不发射
			Vector2 target = Main.MouseWorld;//从鼠标位置发射
			Vector2 direction = target - player.Center;
			direction.Normalize();
			direction *= Item.shootSpeed;
			position = player.Center + new Vector2(0, -20); //从玩家上方发射
			Projectile.NewProjectile(source, position, direction, type, damage, knockback, player.whoAmI);//发射龙卷风弹幕
			for (int i = 0; i < 10; i++)//发射粒子效果
			{
				Vector2 dustVelocity = direction.RotatedByRandom(MathHelper.PiOver4 * 0.5f) * Main.rand.NextFloat(0.5f, 1f);
				Dust dust = Dust.NewDustPerfect(position, DustID.Sand, dustVelocity, 100, default, 1.5f);
				dust.noGravity = true;
			}
			return false;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.SandBlock, 150);//沙块
			recipe.AddIngredient(ItemID.SoulofFlight, 5);//飞翔之魂
			recipe.AddIngredient(ItemID.TitaniumBar, 12);//钛金锭
			recipe.AddTile(TileID.MythrilAnvil);//秘银砧
			recipe.Register();
			Recipe recipe2 = CreateRecipe();
			recipe2.AddIngredient(ItemID.SandBlock, 150);//沙块
			recipe2.AddIngredient(ItemID.SoulofFlight, 5);//飞翔之魂
			recipe2.AddIngredient(ItemID.AdamantiteBar, 12);//精金锭
			recipe2.AddTile(TileID.MythrilAnvil);//秘银砧
			recipe2.Register();
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips)//Tooltip可显示额外信息，顺便看看颜色是怎么添加的吧
		{
			//添加描述
			TooltipLine line = new TooltipLine(Mod, "Description",
				"左键发射龙卷风，只能存在一个\n" +
				"左键可重新发射龙卷风，造成范围伤害\n" +
				"右键可主动引爆龙卷风，造成范围伤害");
			line.OverrideColor = new Color(255, 106, 0);
			//添加状态效果
			TooltipLine line2 = new TooltipLine(Mod, "Effects",
				"[c/FFA500:龙卷风效果:]\n" +
				"[c/FFA500:持续吸引敌人]\n" +
				"[c/FFA500:对接触敌人造成持续伤害]");
			tooltips.Add(line);
			tooltips.Add(line2);
		}
	}
}