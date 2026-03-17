using BoBo.Content.Projectiles.Accessories;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Accessories
{
	public class HolyGuardian : ModItem//圣灵之护：杀死敌怪后，生成一个上抛的血瓶，砸到地面，可使范围内友方回25血
	{
		public override string Texture => Pictures.OtherAcc + Name;
		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 30;
			Item.value = Item.sellPrice(0, 8);
			Item.rare = ItemRarityID.Lime;
			Item.accessory = true;
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<HolyGuardianPlayer>().hasHolyGuardianAccessory = true;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.SuperHealingPotion, 1);//超级治疗药水
			recipe.AddIngredient(ItemID.SoulofLight, 8);//光明之魂
			recipe.AddIngredient(ItemID.HolyWater, 5);//圣水
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	public class HolyGuardianPlayer : ModPlayer
	{
		public bool hasHolyGuardianAccessory = false;
		public override void ResetEffects()
		{
			hasHolyGuardianAccessory = false;
		}
	}
	public class HolyGuardianGlobalNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
		{
			if (npc.ModNPC != null) return;

			bool anyPlayerHasAccessory = false;
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];
				if (player.active && !player.dead && player.GetModPlayer<HolyGuardianPlayer>().hasHolyGuardianAccessory)
				{
					anyPlayerHasAccessory = true;
					break;
				}
			}

			if (anyPlayerHasAccessory)
			{
				CreateHolyGuardianProjectile(npc);
			}
		}
		private void CreateHolyGuardianProjectile(NPC npc)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			Vector2 position = npc.Center;
			Vector2 velocity = new Vector2(0, -10f);
			int proj = Projectile.NewProjectile(
				npc.GetSource_Death(),
				position,
				velocity,
				ModContent.ProjectileType<HolyGuardianProj>(),
				0, 0, Main.myPlayer);
			if (Main.netMode == NetmodeID.Server)//多人
				NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
		}
	}
}