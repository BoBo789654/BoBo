using BoBo.Content.DamageClasses;
using BoBo.Content.Projectiles.Weapons.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items
{
	public class ReapingScythe : ModItem//收割镰刀：仿照ExampleCustomSwingSword
	{
		public override string Texture => Pictures.Meiee + Name;
		private int AttackCombo = 0;//连击计数 (0-3循环)
		private int ComboTimer = 0;//连击超时计时器
		public override void SetDefaults()
		{
			Item.width = 70;
			Item.height = 64;
			Item.scale = 2.5f;
			Item.damage = 233;
			Item.DamageType = DamageClass.Melee;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shoot = ModContent.ProjectileType<ReapingScytheProjectile>();
			Item.knockBack = 6;
			Item.value = Item.buyPrice(0, 20, 30, 40);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, AttackCombo);//传递当前连击阶段
			AttackCombo = (AttackCombo + 1) % 4;//更新连击计数 (0-3循环)
			ComboTimer = 0;//重置超时计时器
			return false;//阻止原始弹幕创建
		}
		public override void UpdateInventory(Player player)
		{
			if (ComboTimer++ > 120) AttackCombo = 0;//2秒不使用武器则重置连击
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DeathSickle, 1);//死神镰刀
			recipe.AddIngredient(ItemID.LunarBar, 3);//夜明锭
			recipe.AddTile(TileID.LunarCraftingStation);//制作站：远古操纵机
			recipe.Register();
		}
	}
}
