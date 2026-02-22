using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace BoBo.Content.Items.Fishes
{
	public class BoBoQuestFish1 : ModItem //幻影鱼
	{
		public override string Texture => Pictures.Fish + Name;
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;//研究解锁需要1个样本
			ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true;//允许放在武器架上
		}
		public override void SetDefaults()
		{
			Item.DefaultToQuestFish();//属性就是默认为任务鱼
		}
		public override bool IsQuestFish() => true;//声明为任务鱼
		public override bool IsAnglerQuestAvailable() => Main.hardMode;//仅在困难模式后出现
		//本地化支持
		public override void AnglerQuestChat(ref string description, ref string catchLocation)
		{
			description = "听说有种倒着游的鱼，你得把自己倒过来才能钓到！";
			catchLocation = "在倒立状态下捕获";
		}
		public override void PostUpdate()//倒立时鱼体发光
		{
			if (Main.LocalPlayer.gravDir == -1)
				Lighting.AddLight(Item.Center, Color.Blue.ToVector3() * 0.8f);
		}
		public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "Hint", "提示：使用重力药水翻转世界才能捕获！"));
		}
	}
}