using BoBo.Content.Buffs.Good;
using BoBo.Content.Projectiles.Accessories.FightAcc;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace BoBo.Content.Accessories.FightAcc
{
    public class MagicalCircle : ModItem//法阵饰品
	{
		public override string Texture => Pictures.FightAcc + Name;
		public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.buffType = ModContent.BuffType<MagicalCircleBuff>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddBuff(Item.buffType, 3);
            if (player.ownedProjectileCounts[ModContent.ProjectileType<MagicalCircleProj>()] == 0)//召唤物数量检测
			{
				Projectile.NewProjectile(player.GetSource_FromAI(), player.position, Vector2.Zero,
							ModContent.ProjectileType<MagicalCircleProj>(), 0, 0);//召唤一个法阵
			}
			if(hideVisual)
			{ 

			}
		}
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();//创建配方
            recipe.AddIngredient(ItemID.Ectoplasm, 100);//灵气
            recipe.AddIngredient(ItemID.StaffoftheFrostHydra, 1);//寒霜九头蛇法杖
            recipe.AddTile(TileID.LunarCraftingStation);//合成站：远古操纵机
            recipe.Register(); 
        }
    }
}
