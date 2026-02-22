using BoBo.Content.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.UIs.Runes
{
	public class RuneUpgrader : ModItem
	{
		public override string Texture => "BoBo/Asset/UIs/Runes/RuneUpgrader";

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.value = Item.sellPrice(0, 5);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item4;
			Item.consumable = true;
			Item.maxStack = 9999;
		}
		public override bool? UseItem(Player player)
		{
			//切换
			RuneItem.CurrentFrameType = RuneItem.CurrentFrameType == "Frame1"
				? "Frame2"
				: "Frame1";
			Main.NewText($"已切{RuneItem.CurrentFrameType}", Color.Blue);
			return true;
		}
	}
}