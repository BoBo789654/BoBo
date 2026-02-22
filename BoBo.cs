//using BoBo.SubWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo
{
	//Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents
	//for more information about the various files in a mod.
	public class BoBo : Mod
	{
		public override void PostSetupContent()
		{
			if (!Main.dedServ)//确保Boss血条系统知道血条
			{
				//在这里添加血条注册逻辑
			}
			MusicOne = MusicID.MenuMusic;
				//MusicLoader.GetMusicSlot("BoBo/Asset/Music/");//1.展示BOSS音乐冲突后的处理
		}
		public static int MusicOne = -1;//1.展示BOSS音乐冲突后的处理
		public override void Load()//1.展示BOSS音乐冲突后的处理
		{
			On_LegacyAudioSystem.Update += On_LegacyAudioSystem_Update;
			On_DisabledAudioSystem.Update += On_DisabledAudioSystem_Update;
		}
		private void On_DisabledAudioSystem_Update(On_DisabledAudioSystem.orig_Update orig, DisabledAudioSystem self)//1.展示BOSS音乐冲突后的处理
		{
			orig(self);
			bool boss1 = false;
			bool boss2 = false;
			foreach (NPC npc in Main.npc)
			{
				if (npc.Distance(Main.LocalPlayer.Center) < 1500)
				{
					if (npc.boss)
					{
						if (boss2)
							Main.newMusic = MusicOne;
						else boss1 = true;
					}
					if (npc.type == NPCID.RainbowSlime)
					{
						if (boss1)
							Main.newMusic = MusicOne;
						else boss2 = true;
					}
				}
			}
		}
		private void On_LegacyAudioSystem_Update(On_LegacyAudioSystem.orig_Update orig, LegacyAudioSystem self)//1.展示BOSS音乐冲突后的处理
		{
			orig(self);
			bool boss1 = false;
			bool boss2 = false;
			foreach (NPC npc in Main.npc)
			{
				if (npc.active && npc.Distance(Main.LocalPlayer.Center) < 1500)
				{
					if (npc.boss)
					{
						if (boss2)
							Main.newMusic = MusicOne;
						else boss1 = true;
					}
					if (npc.type == NPCID.RainbowSlime)
					{
						if (boss1)
							Main.newMusic = MusicOne;
						else boss2 = true;
					}
				}
			}
		}
	}
}
