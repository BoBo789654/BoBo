using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BoBo.Content.Accessories.OtherAcc
{
	/// <summary>
	/// 与 <seealso cref="SummonInformation"/> 和 <seealso cref="SummonInfoDisplayPlayer"/> 配合使用的 InfoDisplay，
	/// 用于展示如何添加一个新的信息类饰品（例如雷达、生命体分析仪等）。
	/// </summary>
	public class SummonInfoDisplay : InfoDisplay
	{
		public static LocalizedText CurrentMinionsText { get; private set; }
		public static LocalizedText NoMinionsText { get; private set; }

		public override void SetStaticDefaults()
		{
			CurrentMinionsText = this.GetLocalization("CurrentMinions");
			NoMinionsText = this.GetLocalization("NoMinions");
		}

		public static Color RedInfoTextColor => new(17, 17, 255, Main.mouseTextColor);

		// 默认情况下，会使用原版圆形轮廓纹理。
		// 此信息显示的图标是方形的而非圆形，因此我们需要使用自定义的悬停轮廓纹理，而不是原版轮廓。
		// 仅当你的信息显示图标形状与原版不一致时，才需要使用自定义悬停纹理。
		public override string Texture => Pictures.OtherAcc + Name;

		// 决定该信息显示是否应处于激活状态
		public override bool Active()
		{
			return Main.LocalPlayer.GetModPlayer<SummonInfoDisplayPlayer>().showMinionCount;
		}

		// 在此修改游戏中显示的实际文本
		public override string DisplayValue(ref Color displayColor, ref Color displayShadowColor)
		{
			// 统计当前仆从数量
			// 这是在正常游戏中查看该显示时，图标旁会显示的值
			int minionCount = 0;
			foreach (var proj in Main.ActiveProjectiles)
			{
				if (proj.minion && proj.owner == Main.myPlayer)
				{
					minionCount++;
				}
			}

			bool noInfo = minionCount == 0;
			if (noInfo)
			{
				// 若显示“无仆从”，则将文字颜色设为灰色，类似 DPS 计量器或雷达
				displayColor = InactiveInfoTextColor;
			}
			else if (minionCount < Main.LocalPlayer.maxMinions)
			{
				// 红色用于提醒玩家尚未召唤全部仆从
				displayColor = RedInfoTextColor;
			}
			/* 
			else if (minionCount == Main.LocalPlayer.maxMinions) {
				// 生命体分析仪用于金色生物的金色文字颜色同样可直接使用
				displayColor = GoldInfoTextColor;
				displayShadowColor = GoldInfoTextShadowColor;
			}
			*/

			return !noInfo ? CurrentMinionsText.Format(minionCount) : NoMinionsText.Value;
		}
	}
	/// <summary>
	/// 与 <seealso cref="SummonInfoDisplay"/> 和 <seealso cref="SummonInformation"/> 配合使用的 ModPlayer 类，
	/// 用于展示如何正确地添加一个新的信息类饰品（例如雷达、生命体分析仪等）。
	/// </summary>
	public class SummonInfoDisplayPlayer : ModPlayer
	{
		// 用于标记信息是否应被激活的标志
		public bool showMinionCount;

		// 请确保使用正确的 Reset 钩子。这个钩子很特别，即使游戏处于暂停状态也会被调用；
		// 这使得信息饰品可以持续正常更新。
		public override void ResetInfoAccessories()
		{
			showMinionCount = false;
		}

		// 如果我们附近有同队玩家，我们希望他们的信息饰品也能对我们生效，就像原版一样。
		// 这个钩子就是为此而设计的。
		public override void RefreshInfoAccessoriesFromTeamPlayers(Player otherPlayer)
		{
			if (otherPlayer.GetModPlayer<SummonInfoDisplayPlayer>().showMinionCount)
			{
				showMinionCount = true;
			}
		}
	}
	/// <summary>
	/// 与 <seealso cref="SummonInfoDisplay"/> 和 <seealso cref="SummonInfoDisplayPlayer"/> 配合使用的 ModItem，
	/// 用于展示如何添加一个新的信息类饰品（例如雷达、生命体分析仪等）。
	/// </summary>
	public class SummonInformation : ModItem
	{
		public override string Texture => Pictures.OtherAcc + Name;
		public override void SetStaticDefaults()
		{
			// 我们希望该饰品的信息效果在虚空袋中也能生效，以保持与原版饰品一致；这是默认行为。
			// 如果你不希望你的信息饰品在虚空袋中生效，请添加：ItemID.Sets.WorksInVoidBag[Type] = false;
		}

		public override void SetDefaults()
		{
			// 我们不需要为该物品添加特别独特的属性；因此直接复制雷达的属性即可。
			Item.CloneDefaults(ItemID.Radar);
		}

		// 这是主要钩子，允许我们的信息显示与该饰品配合使用。
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<SummonInfoDisplayPlayer>().showMinionCount = true;
		}
	}
}