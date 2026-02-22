using BoBo.Content.NPCs.EvilWorm;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;

namespace BoBo.Asset.BossBars
{
	///<summary>
	///邪恶蠕虫自定义Boss血条类
	///继承自ModBossBar，实现Boss血条的图标显示、生命值更新等核心功能
	///使用原版默认血条纹理，专注于逻辑处理
	///</summary>
	public class EvilWormBossBar : ModBossBar
	{
		//Boss头像纹理索引，初始为-1表示未设置
		//这个值会在ModifyInfo方法中根据NPC动态更新
		private int bossHeadIndex = -1;
		///<summary>
		///获取血条上显示的图标纹理
		///这个方法会被血条系统单独调用，用于显示Boss头像
		///</summary>
		///<param name="iconFrame">图标帧矩形（可空），用于处理动画帧</param>
		///<returns>返回Boss头像的纹理资源</returns>
		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
		{
			if (bossHeadIndex != -1)//如果已经设置了有效的头像索引，返回对应的Boss头像纹理
				return TextureAssets.NpcHeadBoss[bossHeadIndex];
			return null;//未设置头像索引时返回null，血条将不显示图标
		}
		///<summary>
		///修改血条信息的核心方法（每帧调用）
		///负责更新生命值、护盾值等数据，并决定是否显示血条
		///</summary>
		///<param name="info">血条信息引用，包含目标NPC索引等</param>
		///<param name="life">当前生命值引用</param>
		///<param name="lifeMax">最大生命值引用</param>
		///<param name="shield">当前护盾值引用</param>
		///<param name="shieldMax">最大护盾值引用</param>
		///<returns>
		///true: 显示血条
		///false: 隐藏血条
		///null: 使用默认逻辑
		///</returns>
		public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)
		{
			NPC npc = Main.npc[info.npcIndexToAimAt];//通过索引获取目标NPC实例
			if (!npc.active || npc.type != ModContent.NPCType<EvilWormHead>())//防御性检查：如果NPC不存在或未激活，不显示血条
				return false;
			//在GetIconTexture被调用前更新头像索引
			//这是必要的，因为GetIconTexture可能在其他时间被单独调用
			bossHeadIndex = npc.GetBossHeadTextureIndex();
			//设置生命值数据
			life = npc.life;
			lifeMax = npc.lifeMax;
			//如果需要护盾值显示，可以在这里设置护盾数据
			//例如：shield = npc.GetGlobalNPC<EvilWormShieldNPC>().shieldValue;
			//shieldMax = npc.GetGlobalNPC<EvilWormShieldNPC>().shieldMaxValue;
			//所有条件满足，显示血条
			return true;
		}
		///<summary>
		///自定义血条绘制逻辑（可选重写）
		///可以在这里添加震动效果、颜色变化等视觉特效
		///</summary>
		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
		{
			float lifePercent = drawParams.Life / drawParams.LifeMax;//计算生命值百分比
			if (lifePercent < 0.1f)//生命值低于10%时添加震动效果
			{
				float shakeIntensity = 1f - lifePercent;//震动强度随生命值减少而增加
				drawParams.BarCenter += Main.rand.NextVector2Circular(0.5f, 0.5f) * shakeIntensity * 10f;
			}
			return true;//返回true使用自定义绘制，false使用默认绘制
		}
	}
}