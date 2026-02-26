using BoBo.Content.Projectiles.Weapons.Ranged;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Weapons.Ranged
{
    public class TerrorBow : ModItem//恐怖弓：背后射箭特效，自定义弹药类型
    {
        public override string Texture => Pictures.Ranged + Name;
        public override void SetDefaults() {
            Item.damage = 16;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 14;
            Item.height = 29;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3;
            Item.value = Item.buyPrice(0, 20, 20, 20);
			Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.crit = 4;
            Item.maxStack = 1;
            Item.noMelee = true;
            Item.shootSpeed = 15f;
            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
			Item.scale = 2;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox) {
            _ = Dust.NewDust(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.CrimsonPlants, 0, 0, 100, Color.White, 1.0f);

        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			const float SpawnDistance = 60f;
			const float RandomRadius = 50f;
			Vector2 mouseDirection = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitY);
			for (int i = 0; i < 1; i++)
			{
				Vector2 spawnPosition = player.Center - mouseDirection * SpawnDistance + Main.rand.NextVector2Circular(RandomRadius, RandomRadius);
				Vector2 shootVelocity = mouseDirection * velocity.Length();
				int projType = (type == ProjectileID.WoodenArrowFriendly) ? ModContent.ProjectileType<TerrorArrow>() : type;
				Projectile.NewProjectile(source, spawnPosition, shootVelocity, projType, damage, knockback, player.whoAmI);
				GenerateEllipseParticles(spawnPosition, shootVelocity);
			}
			SoundEngine.PlaySound(SoundID.Item5, position);
			return false;
		}
		private static void GenerateEllipseParticles(Vector2 center, Vector2 direction)
		{
			const float EllipseMajorAxis = 30f;
			const float EllipseMinorAxis = 15f;
			const int ParticleCount = 12;
			float rotation = direction.ToRotation();
			for (int i = 0; i < ParticleCount; i++)
			{
				float angle = MathHelper.TwoPi * i / ParticleCount;
				Vector2 ellipsePoint = new Vector2(MathF.Cos(angle) * EllipseMinorAxis, MathF.Sin(angle) * EllipseMajorAxis).RotatedBy(rotation);
				float scale = Main.rand.NextFloat(0.8f, 1.2f);
				float opacity = Main.rand.NextFloat(0.7f, 0.95f);
				Vector2 particlePos = center + ellipsePoint;
				Dust particle = Dust.NewDustPerfect(particlePos, DustID.CrimsonPlants, ellipsePoint * 0.05f, Scale: scale);
				particle.noGravity = true;
				particle.alpha = (int)(255 * (1 - opacity));
				particle.color = Color.Lerp(Color.DarkRed, Color.Red, Main.rand.NextFloat());
			}
		}
		public override Vector2? HoldoutOffset() => new Vector2(-0f, 0f);//持弓位置微调
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.BloodRainBow, 1);//血雨弓
			recipe.AddIngredient(ItemID.AdamantiteBar, 15);//精金锭
			recipe.AddIngredient(ItemID.SoulofFright, 10);//恐惧之魂
			recipe.AddTile(TileID.MythrilAnvil);//秘银砧
			recipe.Register();
		}
	}
}
