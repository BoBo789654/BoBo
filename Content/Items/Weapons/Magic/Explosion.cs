using BoBo.Content.Projectiles.Weapons.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Weapons.Magic
{
	public class Explosion : ModItem//Explosion：试了一下爆炸特效
	{
		public override string Texture => Pictures.Magic + Name;
		public override void SetDefaults()
		{
			Item.damage = 135;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 3;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 35;
			Item.useAnimation = 35;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<ExplosionProj>();
			Item.shootSpeed = 16f;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(0, 5, 0, 0);
			Item.rare = ItemRarityID.Blue;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item8;
		}
		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-2f, 0f);//调整手持位置
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.SpellTome, 1);//魔法书
			recipe.AddIngredient(ItemID.ExplosivePowder, 15);//爆炸粉
			recipe.AddIngredient(ItemID.SoulofLight, 10);//光明之魂
			recipe.AddTile(TileID.Bookcases);//书架
			recipe.Register();
		}
	}
		
}

