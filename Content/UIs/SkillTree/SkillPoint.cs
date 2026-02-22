using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.UIs.SkillTree
{
	public class SkillPoint : ModItem//技能点数
	{
		public override string Texture => Pictures.SkillTree + Name;
		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 30;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.UseSound = SoundID.Item3;
			Item.consumable = true;
			Item.maxStack = 9999;
		}

		public override bool? UseItem(Player player)
		{
			player.GetModPlayer<SkillTreePlayer>().SkillPoints++;
			return true;
		}

		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "Hint", "+1技能点数！"));
		}
	}
}