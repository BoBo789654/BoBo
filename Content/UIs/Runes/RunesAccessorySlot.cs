using BoBo.Content.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace BoBo.Content.UIs.Runes
{
	public class RuneAccessorySlot : ModAccessorySlot
	{
		private const float UiPositionX = 400f; //水平偏移量 (距屏幕右侧)
		private const float UiPositionY = 300f;  //垂直偏移量 (距顶部)
		private const float SlotWidth = 300f;    //槽位宽度
		private const float SlotHeight = 300f;   //槽位高度
		private const float RuneWidth = 34f;     //符文宽度
		private const float RuneHeight = 34f;    //符文高度
		private Vector2 FramePosition => new Vector2(Main.screenWidth / 2 + UiPositionX / Main.UIScale , UiPositionY / Main.UIScale);//框的位置
		public Vector2 SlotCenter => FramePosition + new Vector2(SlotWidth / 2, SlotHeight / 2) * 0.01f / Main.UIScale ;//槽位位置
		public override Vector2? CustomLocation => FramePosition;
		private Rectangle ClickableRectangle => new Rectangle(
			(int)(FramePosition.X - 0),
			(int)(FramePosition.Y - 0),
			(int)((SlotWidth - 0) / Main.UIScale ),
			(int)((SlotHeight - 0) / Main.UIScale )
		);//可点击范围
		public string RuneTexture => $"BoBo/Asset/UIs/Runes/RunesFrame{RuneItem.CurrentFrameType.Substring(5)}";//图片文件如果是类似Runes1、Runes2的就这样写
		public override string FunctionalBackgroundTexture => "Terraria/Images/Item_0";
		public override string FunctionalTexture => base.FunctionalTexture;
		public override string VanityBackgroundTexture => "Terraria/Images/Item_0";
		public override string VanityTexture => "Terraria/Images/Item_0";
		public override string DyeBackgroundTexture => "Terraria/Images/Item_0";
		public override string DyeTexture => "Terraria/Images/Item_0";
		//可点击范围
		public bool IsMouseInSlot()
		{
			Point MousePos = Main.MouseScreen.ToPoint();
			return ClickableRectangle.Contains(MousePos);
		}
		//只允许符文类物品放入
		public override bool CanAcceptItem(Item item, AccessorySlotType context)
		{
			if (context == AccessorySlotType.FunctionalSlot && !IsMouseInSlot())
				return false;
			return item.ModItem is RuneItem;
		}
		//描上去显示
		public override void OnMouseHover(AccessorySlotType context)
		{
			Main.hoverItemName = context switch
			{
				AccessorySlotType.FunctionalSlot => "符文核心槽",
				_ => " "
			};
		}
		//绘制
		public override bool PreDraw(AccessorySlotType context, Item item, Vector2 position, bool isHovered)
		{
			if (context == AccessorySlotType.FunctionalSlot)
			{
				//框绘制
				Texture2D FrameTexture = ModContent.Request<Texture2D>(RuneTexture).Value;
				Vector2 FrameOrigin = FrameTexture.Size() / 2f + new Vector2(1f, -2f);
				Main.spriteBatch.Draw(
					FrameTexture,
					FramePosition + new Vector2(SlotWidth / 2, SlotHeight / 2) * 0.01f / Main.UIScale * 2,
					null,
					Color.White * (item.IsAir ? 0.5f : 1f),
					0f,
					FrameOrigin,
					1 / Main.UIScale * 2,
					SpriteEffects.None,
					0f
				);
				if (!item.IsAir && item.ModItem is RuneItem rune)
				{
					//符文位置标识符与坐标的映射
					Dictionary<string, Vector2> runePositions = new Dictionary<string, Vector2>
					{
						{ "core", SlotCenter},
						{ "left", SlotCenter + new Vector2(29, 0) / Main.UIScale * 2 },
						{ "right", SlotCenter + new Vector2(-29, 0) / Main.UIScale * 2 },
						{ "down", SlotCenter + new Vector2(0, 43) / Main.UIScale * 2 }
					};
					//绘制三个技能的位置
					foreach (var pos in runePositions)
					{
						Texture2D specificTexture = rune.GetRuneTexture(pos.Key);
						Vector2 textureOrigin = specificTexture.Size() / 2f;
						float rotation = (pos.Key == "core") ? 0f : MathHelper.Pi;
						Main.spriteBatch.Draw(
							specificTexture,
							pos.Value,
							null,
							Color.White,
							rotation,
							textureOrigin,
							1.1f / Main.UIScale * 2,
							SpriteEffects.None,
							0f
						);
					}
					//鼠标悬停检测
					bool isHovering = false;
					foreach (var pos in runePositions)
					{
						if (CheckMouseHover(pos.Value, rune.GetRuneTexture(pos.Key)))
						{
							DrawRuneTooltip(item, pos.Value, rune.GetRuneDescription(pos.Key));
							isHovering = true;
							break;
						}
					}
				}
				return false;
			}
			return true;
		}
		//检测鼠标是否在符文上
		private bool CheckMouseHover(Vector2 position, Texture2D texture)
		{
			Vector2 size = texture.Size() * 1.1f / Main.UIScale ;
			Rectangle hitbox = new Rectangle(
				(int)(position.X - size.X / 2),
				(int)(position.Y - size.Y / 2),
				(int)size.X,
				(int)size.Y
			);
			return hitbox.Contains(Main.MouseScreen.ToPoint());
		}
		//绘制符文提示
		private void DrawRuneTooltip(Item item, Vector2 position, string description)
		{
			string tooltip = BuildTooltip(item, description);
			Vector2 textSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(tooltip);
			Vector2 tooltipPos = new Vector2(position.X / Main.UIScale , (position.Y - 50) / Main.UIScale );
			if (tooltipPos.Y < 0) tooltipPos.Y = 0;
			if (tooltipPos.X + textSize.X > Main.screenWidth)
				tooltipPos.X = Main.screenWidth - textSize.X;
			Utils.DrawInvBG(
				sb: Main.spriteBatch,
				R: new Rectangle((int)tooltipPos.X, (int)tooltipPos.Y, (int)textSize.X, (int)textSize.Y),
				c: new Color(155, 215, 0, 200)
			);
			Utils.DrawBorderStringFourWay(
				Main.spriteBatch,
				font: Terraria.GameContent.FontAssets.MouseText.Value,
				text: tooltip,
				x: tooltipPos.X + 16,
				y: tooltipPos.Y,
				textColor: Color.Cyan,
				borderColor: Color.Black,
				origin: Vector2.Zero,
				scale: 0.8f
			);
		}
		private string BuildTooltip(Item item, string description)
		{
			if (item.ModItem is RuneItem rune)
			{
				//添加符文名称和稀有度
				string rarityColor = GetRarityColor(item.rare);
				return $"                       " +
					   $"\n效果: {description}";
			}
			return item.Name;
		}
		private string GetRarityColor(int rarity)
		{
			return rarity switch
			{
				ItemRarityID.Blue => "00FFFF",
				_ => "FFFFFF"
			};
		}
		public override void PostDraw(AccessorySlotType context, Item item, Vector2 position, bool isHovered) { }
	}
}