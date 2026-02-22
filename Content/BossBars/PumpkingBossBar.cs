/*using BoBo.Content.NPCs.EvilPumpking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;

namespace BoBo.Asset.BossBars
{
	//自定义Boss血条类：实现图标显示、生命值与护盾值的核心逻辑
	//未指定自定义纹理，因此将使用原版默认血条贴图
	public class PumpkingBossBar : ModBossBar
	{
		private int bossHeadIndex = -1;//初始化为 -1（表示未设置）
		//获取血条图标纹理
		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
		{
			//如果已分配头像索引，返回对应纹理
			if (bossHeadIndex != -1)
			{
				return TextureAssets.NpcHeadBoss[bossHeadIndex];
			}
			return null;//未设置时返回空
		}
		//动态更新血条信息（生命周期中每帧调用）
		public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
		{
			//游戏通过此方法决定是否绘制血条。若条件不满足应返回 false
			//注意：未正确处理可能导致血条在不应显示时出现，需编写防御性代码！
			NPC npc = Main.npc[info.npcIndexToAimAt];//获取目标 NPC
			if (!npc.active)
			{
				return false;//NPC未存活时不显示血条
			}
			//在GetIconTexture前更新头像索引（因该方法会被单独调用）
			bossHeadIndex = npc.GetBossHeadTextureIndex();
			//设置本体生命值数据
			life = npc.life;
			lifeMax = npc.lifeMax;
			//如果NPC是MinionBossBody类型，提取召唤物护盾数据
			if (npc.ModNPC is EvilPumpking body)
			{
				//护盾值已通过 MinionHealthTotal 预先计算完成，直接获取
				//shield = 10000;//当前护盾值
				shieldMax = 2000;//最大护盾值
			}
			return true;//满足条件，绘制血条
		}
	}
}*/