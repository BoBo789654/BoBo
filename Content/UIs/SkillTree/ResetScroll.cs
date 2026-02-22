using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.UIs.SkillTree
{
	public class ResetScroll : ModItem
	{
		public override string Texture => Pictures.SkillTree + Name;

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = SoundID.Item8;
			Item.maxStack = 1;
		}

		public override bool? UseItem(Player player)
		{
			SkillTreePlayer modPlayer = player.GetModPlayer<SkillTreePlayer>();

			// 不返还点数直接清空（保留核心）
			modPlayer.ActiveSkills.RemoveWhere(skill => skill != "核心");
			modPlayer.SkillPoints = 0;
			modPlayer.RecalculateStats();

			// 重置UI
			SkillTreeUISystem uiSystem = ModContent.GetInstance<SkillTreeUISystem>();
			foreach (SkillNode node in uiSystem.SkillTreeUI.Nodes)
			{
				if (node.SkillName != "核心")
				{
					node.ResetNode();
				}
			}

			Main.NewText("所有技能点已被清除！", Color.Red);
			return true;
		}

		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "Hint", "清除所有已激活技能，不返还技能点"));
		}
	}
}
	