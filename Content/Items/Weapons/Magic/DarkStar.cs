using BoBo.Content.Projectiles.Weapons.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Weapons.Magic
{
	public class DarkStar : ModItem//暗星：手持弹幕（拖尾研究失败了）
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
			Item.rare = ItemRarityID.Blue;
			Item.channel = true;
			Item.UseSound = SoundID.Item8;
		}
		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-2f, 0f);//调整手持位置
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

