using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace BoBo.Content.UIs.SkillTree
{
	public class SkillTreeUISystem : ModSystem
	{
		internal SkillTreeUI SkillTreeUI;
		private UserInterface UserInterface;

		public override void Load()
		{
			SkillTreeUI = new SkillTreeUI();
			UserInterface = new UserInterface();
			UserInterface.SetState(SkillTreeUI);
		}

		public override void UpdateUI(GameTime gameTime)
		{
			if (SkillTreeUI.Visible)
				UserInterface.Update(gameTime);
		}

		public void ToggleUI() => SkillTreeUI.Visible = !SkillTreeUI.Visible;

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int index = layers.FindIndex(layer => layer.Name == "Vanilla: Cursor");
			if (index == -1) return;

			layers.Insert(index, new LegacyGameInterfaceLayer(
				"BoBo: SkillTree",
				() => {
					if (SkillTreeUI.Visible)
						UserInterface.Draw(Main.spriteBatch, new GameTime());
					return true;
				},
				InterfaceScaleType.UI)
			);
		}
	}

	public class KeybindSystem : ModSystem
	{
		public static ModKeybind SkillTreeKey { get; private set; }

		public override void Load()
		{
			SkillTreeKey = KeybindLoader.RegisterKeybind(Mod, "技能树面板", "J");
		}
	}
}