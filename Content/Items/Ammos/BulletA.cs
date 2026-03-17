using BoBo.Content.Projectiles.Ammos;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Ammos
{
	public class BulletA : ModItem
	{
		public override string Texture => Pictures.Ammos + Name;
		public override void SetDefaults()
		{
			Item.width = 14;
			Item.height = 14;
			Item.value = Item.sellPrice(copper: 1);
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 9999;
			Item.consumable = true;
			Item.ammo = AmmoID.Bullet;
			Item.shoot = ModContent.ProjectileType<BulletAProj>();
			Item.shootSpeed = 5f;
			Item.damage = 10;
			Item.knockBack = 2f;
			Item.DamageType = DamageClass.Ranged;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe(200);
			recipe.AddIngredient(ItemID.MusketBall, 200);//火枪子弹
			recipe.AddIngredient(ItemID.GrayBrick, 1);//狂星之怒
			recipe.AddTile(TileID.Anvils);//制作站：铁砧
			recipe.Register();
		}
	}
}