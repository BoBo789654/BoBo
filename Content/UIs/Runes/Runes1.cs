using BoBo.Content.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.UIs.Runes
{
	public class Runes1 : RuneItem
	{
		public override Dictionary<string, Dictionary<string, (string TexturePath, string Description)>> RuneEffectsByFrame => new()//这里存两个之间有联系的
		{
			{
				"Frame1",
				new Dictionary<string, (string, string)>
				{
					{ "core", ("BoBo/Asset/UIs/Runes/Runes1", "中间") },
					{ "left", ("BoBo/Asset/UIs/Runes/Runes1", "叽里咕噜") },
					{ "right", ("BoBo/Asset/UIs/Runes/Runes1", "呜啊呜啊") },
					{ "down", ("BoBo/Asset/UIs/Runes/Runes1", "玛卡巴卡") }
				}
			},
			{
				"Frame2",
				new Dictionary<string, (string, string)>
				{
					{ "core", ("BoBo/Asset/UIs/Runes/Runes2", "中间") },
					{ "left", ("BoBo/Asset/UIs/Runes/Runes2", "叽里咕噜") },
					{ "right", ("BoBo/Asset/UIs/Runes/Runes2", "呜啊呜啊") },
					{ "down", ("BoBo/Asset/UIs/Runes/Runes2", "玛卡巴卡") }
				}
			}
		};
		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(999, 0, 0, 0);
			Item.maxStack = 1;
		}
	}
}