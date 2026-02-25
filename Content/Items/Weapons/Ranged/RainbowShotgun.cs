using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Weapons.Ranged
{
	public class RainbowShotgun : ModItem
	{
		public override string Texture => Pictures.Ranged + Name;
		private int AttackCounter = 0; //记录连续攻击次数
		private int LastAttackTime = 0; //记录最后攻击时间
		public override void SetDefaults()
		{
			Item.damage = 18;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 62;
			Item.height = 24;
			Item.useTime = 60;
			Item.useAnimation = 60;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 6;
			Item.value = Item.buyPrice(0, 20, 20, 20);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item36;
			Item.autoReuse = true;
			Item.shootSpeed = 15f;
			Item.useAmmo = AmmoID.Bullet;
			Item.shoot = ProjectileID.Bullet;
		}
		public override void HoldItem(Player player)
		{
			if (Main.GameUpdateCount - LastAttackTime > 60)//如果超过1s没有攻击，重置攻速
			{
				AttackCounter = 0;
				Item.useTime = 30;
				Item.useAnimation = 30;
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			int Count = 10;//计数
			float spreadAngle = Main.rand.NextFloat(7.5f, 9f);//初始随机
			for (int i = 0; i < Count; i++)
			{
				Vector2 RotatedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(spreadAngle));//随机偏转
				RotatedSpeed *= 1f - Main.rand.NextFloat() * 0.1f;//速度变
				Projectile.NewProjectile(source, position, RotatedSpeed, type, damage, knockback, player.whoAmI);
			}
			//增加攻击计数并更新最后攻击时间
			AttackCounter++;
			LastAttackTime = (int)Main.GameUpdateCount;
			//长按增加攻速，但有上限
			if (AttackCounter > 0 && AttackCounter % 3 == 0) //每攻击3次增加一次攻速
			{
				Item.useTime = Math.Max(10, Item.useTime - 2); //每次减少2帧，最低10帧
				Item.useAnimation = Item.useTime; //保持同步
			}
			return false;
		}
		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-10, 4);
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			float hue = (float)(Main.timeForVisualEffects * 0.01f % 1.0);//颜色循环
			Color rainbowColor = Main.hslToRgb(hue, 1f, 0.5f);
			string tex = Pictures.Ranged + Name;
			spriteBatch.Draw(ModContent.Request<Texture2D>(tex).Value, position, frame, rainbowColor, 0f, origin, scale * 1.2f, SpriteEffects.None, 0f);
			return false;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.TacticalShotgun, 1);//战术霰弹枪
			recipe.AddIngredient(ItemID.RainbowBrick, 15);//彩虹砖
			recipe.AddIngredient(ItemID.FragmentStardust, 10);//星辰碎片
			recipe.AddTile(TileID.LunarCraftingStation);//远古操纵机
			recipe.Register();
		}
	}
}