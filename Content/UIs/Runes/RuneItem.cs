using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.UIs.Runes
{
	public abstract class RuneItem : ModItem
	{
		public virtual Dictionary<string, Dictionary<string, (string TexturePath, string Description)>> RuneEffectsByFrame { get; } = new()//这里存全部的
		{
			{
				"Frame1",
				new Dictionary<string, (string, string)>
				{
					{ "left", ("BoBo/Asset/UIs/Runes/Runes1", "左：基础效果") },
					{ "right", ("BoBo/Asset/UIs/Runes/Runes1", "右：基础效果") },
					{ "down", ("BoBo/Asset/UIs/Runes/Runes1", "下：基础效果") }
				}
			},
			{
				"Frame2",
				new Dictionary<string, (string, string)>
				{
					{ "left", ("BoBo/Asset/UIs/Runes/Runes2", "左：高级效果") },
					{ "right", ("BoBo/Asset/UIs/Runes/Runes2", "右：高级效果") },
					{ "down", ("BoBo/Asset/UIs/Runes/Runes2", "s下：高级效果") }
				}
			}
		};
		public static string CurrentFrameType = "Frame1";
		//符文材质
		public Texture2D GetRuneTexture(string position)
		{
			if (RuneEffectsByFrame.TryGetValue(CurrentFrameType, out var frameEffects) &&
				frameEffects.TryGetValue(position, out var effect))
			{
				return ModContent.Request<Texture2D>(effect.TexturePath).Value;
			}
			return Terraria.GameContent.TextureAssets.Item[Item.type].Value;
		}
		//符文描述
		public string GetRuneDescription(string position)
		{
			if (RuneEffectsByFrame.TryGetValue(CurrentFrameType, out var frameEffects) &&
				frameEffects.TryGetValue(position, out var effect))
			{
				return effect.Description;
			}
			return "哈哈哈哈哈哈哈哈哈哈哈哈";
		}
		public override string Texture => "BoBo/Asset/UIs/Runes/" + Name;
		public virtual int GetRunePowerLevel()
		{
			return Item.rare switch
			{
				ItemRarityID.Blue => 1,
				_ => 0
			};
		}
		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 34;
			Item.accessory = true;
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 1;
		}
		public virtual void ApplyRuneEffect(Player player) { }
		public virtual void RemoveRuneEffect(Player player) { }

		public override bool CanEquipAccessory(Player player, int slot, bool modded)
		{
			return slot == ModContent.GetInstance<RuneAccessorySlot>().Type;
		}
	}
}