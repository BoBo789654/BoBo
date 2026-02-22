using BoBo.Content.Buffs.MinionBuff;
using BoBo.Content.Projectiles.Weapons.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Weapons.Summon
{
	public class BatSummoningStaffC : ModItem//待机排队，很好玩
	{
		public override string Texture => Pictures.Summon + Name;
		public override void SetStaticDefaults()
		{
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<BatSummoningStaffA>();
			ItemID.Sets.GamepadWholeScreenUseRange[Type] = true;
			ItemID.Sets.LockOnIgnoresCollision[Type] = true;
			ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
		}
		public override void SetDefaults()
		{
			Item.damage = 100;
			Item.knockBack = 3f;
			Item.mana = 10;
			Item.width = 56;
			Item.height = 52;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.sellPrice(0, 30);
			Item.rare = ItemRarityID.Cyan;
			Item.UseSound = SoundID.Item44;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<BatSummoningBuffC>();
			Item.shoot = ModContent.ProjectileType<BatC>();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = Main.MouseWorld;
			player.LimitPointToPlayerReachableArea(ref position);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 2);
			return true;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();//创建一个配方
			recipe.AddIngredient(ItemID.BatWings, 1);//蝙蝠之翼
			recipe.AddIngredient(ItemID.SanguineStaff, 1);//血红法杖
			recipe.AddIngredient(ItemID.SoulBottleFright, 10);//恐惧之魂
			recipe.AddTile(TileID.LunarCraftingStation);//制作站：远古操纵机
			recipe.Register();
		}
	}
}