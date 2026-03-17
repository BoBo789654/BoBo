using BoBo.Content.Players;
using BoBo.Content.Projectiles.Weapons.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items
{ 
	public class JianXian : ModItem//剑仙：击中敌人会生成随机材质的悬浮剑
	{
        public override string Texture => Pictures.Meiee + Name;

        public override void SetDefaults()
		{
			Item.damage = 210;
			Item.DamageType = DamageClass.Melee;
			Item.width = 40;
			Item.height = 40;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 1;
			Item.value = Item.buyPrice(0, 10, 20, 30);
			Item.rare = ItemRarityID.Master;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.scale = 2.5f + 2.5f;
		}
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
		{
			//每次命中时生成4把悬浮剑
			for (int i = 0; i < 16; i++)
			{
				Projectile.NewProjectile(player.GetSource_ItemUse(Item), target.Center, Vector2.Zero,
					ModContent.ProjectileType<XianJian>(), Item.damage / 2, 0f, player.whoAmI, target.whoAmI, ai2: (MathHelper.Pi / 180 * (-35 + i * 22.5f)) + MathHelper.Pi / 2);
			}
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Meowmere, 1);//彩虹猫之刃
			recipe.AddIngredient(ItemID.StarWrath, 1);//狂星之怒
			recipe.AddIngredient(ItemID.LunarBar, 3);//夜明锭
			recipe.AddTile(TileID.LunarCraftingStation);//制作站：远古操纵机
			recipe.Register();
		}
	}
}
