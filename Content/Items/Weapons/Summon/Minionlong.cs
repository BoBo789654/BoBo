using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using BoBo.Content.Projectiles.Weapons.Summon;
using BoBo.Content.Buffs.MinionBuff;
using BoBo.Content.Projectiles;
namespace BoBo.Content.Items.Weapons.Summon
{
    public class Minionlong : ModItem//星尘之龙·改，追踪与原版略有不同
    //相关的内容在这写吧，总共有SimulateStarDustDragon基类、longMinion为弹幕
    {
        public override string Texture => Pictures.Summon + Name;
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Summon;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.width = 52;
            Item.height = 52;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.shoot = ModContent.ProjectileType<longMinion>();
            Item.shootSpeed = 1f;
            Item.damage = 111;
            Item.knockBack = 3;
            Item.value = Item.buyPrice(0, 11, 12, 0);
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = true;
            Item.noUseGraphic = false;
            Item.noMelee = true;
            Item.useAmmo = AmmoID.None;
            Item.buffType = ModContent.BuffType<longMinionBuff>();
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-20, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SimulateStarDustDragon.SummonSet<longMinion>(
                player, source, damage, knockback, 2,//体节数量
                ModContent.ProjectileType<longMinion>(),//逻辑弹幕类型
                ModContent.ProjectileType<LongMinion.Head>(),//头部弹幕类型
                ModContent.ProjectileType<LongMinion.Body>(),//身体弹幕类型
                ModContent.ProjectileType<LongMinion.Tail>(),//尾部弹幕类型
                ModContent.BuffType<longMinionBuff>()//buff类型
            );
            return false;//阻止原版发射
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();//创建一个配方
			recipe.AddIngredient(ItemID.LunarBar, 1);//夜明锭
			recipe.AddIngredient(ItemID.StardustDragonStaff, 1);//星尘之龙法杖
			recipe.AddTile(TileID.LunarCraftingStation);//制作站：远古操纵机
			recipe.Register();
        }
    }
}
