using BoBo.Content.Common;
using BoBo.Content.Projectiles.Accessories.FightAcc;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Accessories.FightAcc
{
	/// <summary>
	/// [c/FFD700:★★★][c/DAA520:☆☆]
	/// [c/BA55D3:⚡「雷电的力量」⚡][c/CCCCFF:-------------------------------------][c/5D3FD3:⚡雷电之尾⚡]
	/// [c/A03CFF:■ 雷电之尾] [c/A03CFF:受击时获得1条][c/5D3FD3:⚡雷电之尾⚡][c/808080:上限为9层][c/5D3FD3:⚡层数⚡]
	/// [c/9370DB:■ 雷电净化] [c/9370DB:免疫所有原版异常状态与减益，受伤时恢复5倍][c/5D3FD3:⚡层数⚡][c/9370DB:血量]
	/// [c/CCCCFF:每层][c/5D3FD3:⚡层数⚡][c/CCCCFF:效果：][c/CCCCFF:+3 防御；+5% 魔法伤害；+5% 魔法暴击；+5% 魔法武器攻速]
	/// [c/DA70D6:■ 雷电之盾：][c/DA70D6:大于150的单次伤害，会将过量的减伤数倍，并会随][c/5D3FD3:⚡层数⚡][c/DA70D6:增加效果增加]-------这一行暂时做不到
	/// [c/FF69B4:■ 雷电治愈：][c/FF69B4:魔法伤害5%用于恢复魔力（0.5秒冷却）]
	/// [c/C3B1E1:■ 雷电锁链：][c/C3B1E1:每层][c/5D3FD3:⚡层数⚡][c/C3B1E1:增加1.5%概率触发额外闪电攻击]
	/// [c/DDA0DD:■ 雷电套装：][c/DDA0DD:若穿着星云套装，雷电之尾的][c/5D3FD3:⚡层数⚡][c/808080:上限增加到11层]
	/// </summary>
	public class BronzeBell : ModItem//已知公式：（传入伤害+ Base）* Additive * Multipcative + Flat = 修正伤害
	{
		public override string Texture => Pictures.FightAcc + Name;
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.accessory = true;
			Item.rare = ItemRarityID.Purple;
			Item.value = Item.sellPrice(1, 0, 0, 0);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPlayer = player.GetModPlayer<BronzeBellPlayer>();//获取玩家自定义状态
			modPlayer.HasThunderTail = true;
			foreach (int buffID in BaseDeBuff.DeBuffIDs)//免疫所有负面状态
			{
				int index = player.FindBuffIndex(buffID);
				if (index == -1)
				{
					player.buffImmune[buffID] = true;
				}
			}
			if (player.armor[0].type == ItemID.NebulaHelmet &&
				player.armor[1].type == ItemID.NebulaBreastplate &&
				player.armor[2].type == ItemID.NebulaLeggings)//套装效果提升层数上限
				modPlayer.MaxMarkCount = 11;//星云套装多2层
			else if (player.armor[0].type != ItemID.NebulaHelmet ||
				player.armor[1].type != ItemID.NebulaBreastplate ||
				player.armor[2].type != ItemID.NebulaLeggings)//套装效果提升层数上限
				modPlayer.MaxMarkCount = 9;//回到9层
			else modPlayer.MaxMarkCount = 9;//默认上限
			player.statDefense += modPlayer.MarkCount * 3;
			player.GetDamage<MagicDamageClass>() += modPlayer.MarkCount * 0.05f;
			player.GetCritChance<MagicDamageClass>() += modPlayer.MarkCount * 5f;
			player.GetAttackSpeed<MagicDamageClass>() += modPlayer.MarkCount * 0.05f;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.FragmentNebula, 15);//星云碎片
			recipe.AddIngredient(ItemID.LunarBar, 10);//夜明锭
			recipe.AddIngredient(ItemID.NimbusRod, 1);//雨云法杖
			recipe.AddTile(TileID.TinkerersWorkbench);//工匠作坊
			recipe.AddTile(TileID.LunarCraftingStation);//远古操纵机
			recipe.Register();
		}
	}
	public class BronzeBellPlayer : ModPlayer
	{
		public bool HasThunderTail;
		public int MarkCount = 0;		//当前层数
		public int MaxMarkCount;		//默认层数上限
		public int ShieldCooldown;		//雷之盾冷却计时器
		public const int ShieldCD = 30;	//限伤盾的冷却
		public int ManaTime = 30;		//魔力回复时间
		public int HealTime = 30;		//血量回复时间
		public int Time;				//时间计时
		public int Time2;				//时间计时
		public override void ResetEffects() => HasThunderTail = false;
		public override void PostUpdate()
		{
			Time++;
			if (!HasThunderTail) return;
			if (ShieldCooldown > 0) ShieldCooldown--;//雷之盾冷却更新
			if (Time2 > 0)
			{
				Time2--;
				if (Time2 == 0)//受伤半秒后触发回血，每层标记回血5点
				{
					int healAmount = MarkCount;
					Player.Heal(MarkCount * 5);
					for (int i = 0; i < Math.Min(MarkCount * 5, 100); i++)
					{
						Dust.NewDust(Player.position, Player.width, Player.height, DustID.GreenTorch,
							Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-4, 0), 150, Color.LimeGreen, 0.8f);
					}
				}
			}
		}
		public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)//NPC攻击提示
		{
			if (!HasThunderTail || ShieldCooldown > 0) return;
			Main.NewText($"[c/DDA0DD:来源: NPC {npc.FullName} ]" +
				$"[c/DDA0DD:伤害：{npc.damage} ]" +
				$"[c/DDA0DD:生命：{npc.lifeMax} ]" +
				$"[c/DDA0DD:速度：{npc.velocity:F2} ]" +
				$"[c/DDA0DD:防御：{npc.defense} ]" +
				$"[c/DDA0DD:击退抗性：{npc.knockBackResist} ]" +
				$"[c/DDA0DD:重力影响：{npc.noGravity} ]", Color.White);
		}
		public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)//弹幕攻击提示
		{
			if (!HasThunderTail || ShieldCooldown > 0) return;
			Main.NewText($"[c/DDA0DD:来源: 弹幕 {proj.Name} ]" +
				$"[c/DDA0DD:伤害：{proj.damage} ]" +
				$"[c/DDA0DD:伤害类型：{proj.DamageType} ]" +
				$"[c/DDA0DD:速度：{proj.velocity:F2} ]" +
				$"[c/DDA0DD:剩余时间：{proj.timeLeft} ]" +
				$"[c/DDA0DD:是否墙体碰撞：{proj.tileCollide} ]" +
				$"[c/DDA0DD:是否忽略液体：{proj.ignoreWater} ]" +
				$"[c/DDA0DD:穿透数：{proj.penetrate} ]" +
				$"[c/DDA0DD:大小：{proj.scale} ]" , Color.White);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)//雷之触手
		{
			if (!HasThunderTail) return;
			int ManaGain = (int)(hit.Damage * 0.05f);//攻击回魔：伤害的5%转化为魔力
			if (Time > ManaTime) 
			{
				Player.ManaEffect(ManaGain);
				Player.statMana += ManaGain;
				Time = 0;
			}
			if (Main.rand.NextFloat() < MarkCount * 0.015f)//雷电弹幕触发（每层1.5%概率）
			{
				for (int i = 0; i < 5; i++)
				{
					float length = Main.rand.NextFloat(210f, 340f);
					Projectile.NewProjectile(Player.GetSource_FromAI(), Player.Center, Vector2.Zero, ModContent.ProjectileType<ThunderAccProj>(), 
						MarkCount * 500, 0, Player.whoAmI, Player.velocity.ToRotation() +  Main.rand.NextFloat(-3.14f, 3.14f), length);//伤害500倍层数
				}
				for (int i = 0; i < 4; i++)
				{
					Dust.NewDustPerfect(Player.Center, DustID.Electric, Main.rand.NextFloat(0f, MathHelper.TwoPi).ToRotationVector2(), 
						Scale: Main.rand.NextFloat(1f, 3f)).noGravity = true;
				}
			}
		}
		public override void OnHurt(Player.HurtInfo hurtInfo)//雷之盾：抵挡单次>150的伤害
		{
			if (!HasThunderTail) return;
			float damage = hurtInfo.Damage;
			Main.NewText($"[c/5D3FD3:实际伤害: {damage}]", Color.White);
			if (MarkCount < MaxMarkCount) MarkCount++;//受伤叠加层数（不超过上限）
			Time2 = HealTime;
		}
	}
}