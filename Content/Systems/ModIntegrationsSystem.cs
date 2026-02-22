/*using BoBo.Content.Items.Consumables;
using BoBo.Content.NPCs.EvilPumpking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Systems
{
	//展示如何使用其他Mod的Mod.Call功能来实现Mod整合/兼容/支持。
	//Mod.Call机制详解见此: https://github.com/tModLoader/tModLoader/wiki/Expert-Cross-Mod-Content#call-aka-modcall-intermediate
	//这仅展示一种实现此类整合的方式，你可以自由探索其他选项或其他Mod的示例。
	//你需要查找目标Mod开发者提供的关于如何添加其Mod兼容性的资源。
	//这些资源可能位于他们的主页、创意工坊页面、Wiki、GitHub、Discord或其他联系方式中。
	//如果Mod是开源的，你可以访问其代码托管平台（通常是 GitHub）并在其Mod类中搜索"Call"。
	//除了此处展示的示例外，ExampleMod还与Census Mod (https://steamcommunity.com/sharedfiles/filedetails/?id=2687866031) 进行了整合。
	//该整合完全通过本地化文件实现，请查看.hjson文件中的"Census.SpawnCondition"。
	public class ModIntegrationsSystem : ModSystem
	{
		public override void PostSetupContent()
		{
			//大多数情况下，Mod要求你在PostSetupContent钩子中调用它们的方法。这确保了各种数据已正确初始化和设置。
			//Boss Checklist在其自身UI中展示Boss的全面信息。我们可以对其进行自定义:
			//https://forums.terraria.org/index.php?threads/.50668/
			DoBossChecklistIntegration();
			//我们可以在这里通过相同的模式与其他Mod进行整合。一些Mod开发者可能更喜欢为每个他们整合的Mod单独编写一个 ModSystem，或其他设计。
		}
		private void DoBossChecklistIntegration()
		{
			//该Mod的主页链接到其Wiki，其中解释了调用方式: https://github.com/JavidPack/BossChecklist/wiki/%5B1.4.4%5D-Boss-Log-Entry-Mod-Call
			//如果我们浏览Wiki，可以找到"LogBoss"方法，这正是我们本例中需要的。
			//该调用的一个特点是，它将为指定的NPC类型在其本地化文件中创建一个条目用于生成信息，因此请确保在你的Mod运行一次后访问该本地化文件进行编辑。
			if (!ModLoader.TryGetMod("BossChecklist", out Mod bossChecklistMod))
			{
				return;//未找到 BossChecklist Mod，跳过整合
			}
			//对于某些功能，Mod可能在其发布时尚未包含它们，因此我们需要确认该方法变体的最新版本是在何时首次添加到Mod中的（本例中是1.6 版）。
			//通常Mod会自行提供这些信息，或者在GitHub上通过提交历史/blame功能找到。
			if (bossChecklistMod.Version < new Version(1, 6))
			{
				return; //BossChecklist版本过低（低于1.6），不兼容所需功能，跳过整合
			}
			//"LogBoss"方法需要许多参数，在下面分别定义：
			//你的条目键值 (internalName) 可被其他开发者用于向你的条目提交跨Mod协作数据。一旦定义不应更改。
			string internalName = "EvilPumpking";
			//权重值根据Boss的进度推断而得，详见Wiki。
			float weight = 0.7f;
			//用于追踪Boss是否已被击败，关联检查清单进度。
			Func<bool> downed = () => DownedBossSystem.downedEvilPumpking;
			//Boss的NPC类型ID。
			int bossType = ModContent.NPCType<EvilPumpking>();
			//（如果有的话）用于召唤Boss的物品ID。
			int spawnItem = ModContent.ItemType<EvilPumpkin>();
			//掉落物列表，如遗物 (relic)、奖杯 (trophy)、面具 (mask)、宠物 (pet)。
			List<int> collectibles = new List<int>()
			{
				//ModContent.ItemType<Items.Placeable.Furniture.EvilPumpkingRelic>(),
				//ModContent.ItemType<Content.Pets.EvilPumpkingPet.EvilPumpkingPetItem>(),
				//ModContent.ItemType<Items.Placeable.Furniture.EvilPumpkingTrophy>(),
				//ModContent.ItemType<Items.Armor.Vanity.EvilPumpkingMask>()
				ItemID.Zenith,
			};
			//默认情况下，它绘制Boss的第一帧贴图。如果你不需要自定义绘制，可以省略此参数。
			//但我们希望绘制图鉴 (Bestiary) 中的贴图，因此创建绘制代码，使其在预定位置居中绘制。
			var customPortrait = (SpriteBatch sb, Rectangle rect, Color color) => {
				Texture2D texture = ModContent.Request<Texture2D>(Pictures.NPCs + "EvilPumpking/EvilPumpking1").Value;
				Vector2 centered = new Vector2(
					rect.X + rect.Width / 2 - texture.Width / 2,
					rect.Y + rect.Height / 2 - texture.Height / 2
				);
				sb.Draw(texture, centered, color);
			};
			//调用BossChecklist Mod的LogBoss方法注册Boss，这种方法可以让BOSS进入BOSS列表模组
			bossChecklistMod.Call(
				"LogBoss",   //方法名
				Mod,         //本 Mod 的实例
				internalName,//Boss 的内部标识符
				weight,      //进度权重
				downed,      //判断是否已击败的函数
				bossType,    //Boss 的 NPC 类型 ID
				new Dictionary<string, object>()//可选参数字典，根据需要添加Wiki中提到的其他可选参数
				{
					["spawnItems"] = spawnItem,//召唤物品
					["collectibles"] = collectibles,//收藏品列表
					["customPortrait"] = customPortrait//自定义肖像绘制委托
				}
			);
			//其他Boss的注册或其他Mod.Call可以在此处进行。
		}
	}
}*/