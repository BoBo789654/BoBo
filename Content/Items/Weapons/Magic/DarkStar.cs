using BoBo.Content.Projectiles.Weapons.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Weapons.Magic
{
	public class DarkStar : ModItem//暗星：手持弹幕（拖尾研究结束了，原来是三层的顶点绘制么）
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
			Item.shoot = ModContent.ProjectileType<DarkStarProj>();
			Item.shootSpeed = 16f;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(0, 5, 0, 0);
			Item.rare = ItemRarityID.Purple;
			Item.channel = true;
			Item.UseSound = SoundID.Item8;
			Item.staff[Type] = true;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			int projectile = Projectile.NewProjectile(
				source,
				position,
				velocity,
				ModContent.ProjectileType<DarkStarProj>(),
				damage,
				knockback,
				player.whoAmI
			);
			//可以设置初始状态
			Main.projectile[projectile].ai[0] = 0f;//0表示手持状态
			return false;
		}
		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-20f, 0f);//调整手持位置
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.RainbowRod, 1);//彩虹魔杖
			recipe.AddIngredient(ItemID.BlackFairyDust, 1);//黑色仙尘
			recipe.AddIngredient(ItemID.SoulofLight, 10);//光明之魂
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
		
}

