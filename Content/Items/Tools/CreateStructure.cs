using BoBo.Content.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Items.Tools
{
	public class CreateStructure : ModItem
	{
		public override string Texture => Pictures.Tool + Name;
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item1;
		}
		public override bool? UseItem(Player player)
		{
			//获取鼠标位置并转换为物块坐标
			Vector2 mousePos = Main.MouseWorld;
			Point origin = mousePos.ToTileCoordinates();
			//调用建筑生成方法
			ModContent.GetInstance<BoBoStructure>().GenerateSingleBuilding(origin.X, origin.Y);
			for (int i = 0; i < 10; i++)
			{
				Dust.NewDust(mousePos, 10, 10, DustID.Firefly);
			}
			return true;
		}
	}
}