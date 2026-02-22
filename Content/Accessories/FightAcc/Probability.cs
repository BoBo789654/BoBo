using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.Audio;
using Microsoft.Xna.Framework;

namespace BoBo.Content.Accessories.FightAcc
{
	public class Probability : ModItem
	{
		public override string Texture => Pictures.FightAcc + Name;
		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 34;
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(0, 9, 0, 0);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<ProbablityPlayer>().hasProbablity = true;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Amber, 25);//琥珀
			recipe.AddIngredient(ItemID.Ruby, 25);//红宝石
			recipe.AddIngredient(ItemID.Emerald, 25);//翡翠
			recipe.AddIngredient(ItemID.Sapphire, 25);//蓝宝石
			recipe.AddIngredient(ItemID.Diamond, 25);//钻石
			recipe.AddIngredient(ItemID.Topaz, 25);//黄玉
			recipe.AddIngredient(ItemID.Amethyst, 25);//紫晶
			recipe.AddTile(TileID.LunarCraftingStation);//制作站：远古操纵机
			recipe.Register();
		}
	}
	public class ProbablityPlayer : ModPlayer
	{
		public bool hasProbablity;
		private readonly List<int> boostedDamageTargets = new List<int>();
		private bool damageBoostedThisHit;
		public override void ResetEffects()
		{
			hasProbablity = false;
		}
		//伤害修改
		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (hasProbablity && Main.rand.NextFloat() < 0.5f)//50%概率
			{
				modifiers.SourceDamage *= 1.5f;//提升50%伤害
				damageBoostedThisHit = true;
			}
		}
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (hasProbablity && Main.rand.NextFloat() < 0.5f)
			{
				modifiers.SourceDamage *= 1.5f;//提升50%伤害
				damageBoostedThisHit = true;
			}
		}
		//玩家造成伤害
		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (damageBoostedThisHit)
			{
				SpawnDamageBoostEffect(target.Center);
				boostedDamageTargets.Add(target.whoAmI);
				damageBoostedThisHit = false;
			}
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (damageBoostedThisHit)
			{
				SpawnDamageBoostEffect(target.Center);
				boostedDamageTargets.Add(target.whoAmI);
				damageBoostedThisHit = false;
			}
		}
		//玩家受到伤害
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (hasProbablity && Main.rand.NextFloat() < 0.5f)
			{
				modifiers.FinalDamage *= 0.5f;//减免50%伤害
				SpawnDamageReductionEffect(Player.Center);
				SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.5f }, Player.Center);
			}
		}
		//粒子提示一下
		private void SpawnDamageBoostEffect(Vector2 position)
		{
			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustPerfect(
					position,
					DustID.GemRuby,
					new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-6f, -2f)),
					0, default, 1.8f
				);
				dust.noGravity = true;
				dust.fadeIn = 1.2f;
			}
			SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.8f }, position);
		}
		private void SpawnDamageReductionEffect(Vector2 position)
		{
			for (int i = 0; i < 15; i++)
			{
				Dust dust = Dust.NewDustPerfect(position, DustID.GemSapphire,
					new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-4f, 0f)),
					100, new Color(80, 150, 255), 1.5f
				);
				dust.noGravity = true;
				dust.fadeIn = 1.5f;
			}
		}
	}
}