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
	/// [c/FFD700:★★★][c/DAA520:☆☆]
	///	[c/BA55D3:⚡「雷电的力量」⚡]
	///	[c/A03CFF:■ 雷之尾][c/A03CFF:受击时获得1条][c/5D3FD3:雷之尾⚡][c/808080:（上限9层）]
	///	[c/9370DB:■ 雷净化][c/9370DB:免疫所有原版异常状态与减益，受伤恢复5倍][c/5D3FD3:⚡层数][c/9370DB:血量]
	///	[c/CCCCFF:每层效果：][c/CCCCFF:+3 防御][c/CCCCFF:+5% 魔法伤害][c/CCCCFF:+5% 魔法暴击]
	///	[c/DA70D6:■ 雷之盾：][c/DA70D6:大于150的单次伤害，会将过量的减伤数倍，并会随][c/5D3FD3:⚡层数][c/DA70D6:增加效果增加]
	///	[c/FF69B4:■ 雷治愈：][c/FF69B4:魔法伤害5%用于恢复魔力]
	///	[c/C3B1E1:■ 雷锁链：][c/5D3FD3:每层⚡][c/C3B1E1:增加0.5%概率触发][c/C3B1E1:额外闪电攻击]
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
			//获取玩家自定义状态
			var modPlayer = player.GetModPlayer<BronzeBellPlayer>();
			modPlayer.HasThunderTail = true;
			//免疫所有负面状态
			foreach (int buffID in BaseDeBuff.DeBuffIDs)
			{
				int index = player.FindBuffIndex(buffID);
				if (index == -1)
				{
					player.buffImmune[buffID] = true;
				}
			}
			//套装效果提升层数上限
			if (player.head == 170 && player.body == 176 && player.body == 110) modPlayer.MaxMarkCount = 11;//星云套装多两层（实现不了）
			else modPlayer.MaxMarkCount = 9;//默认上限
			player.statDefense += modPlayer.MarkCount * 3;
			player.GetDamage<MagicDamageClass>() += modPlayer.MarkCount * 0.05f;
			player.GetCritChance<MagicDamageClass>() += modPlayer.MarkCount * 5f;
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
		public int MarkCount = 0;        //当前层数
		public int MaxMarkCount;		 //默认层数上限
		public int ShieldCooldown;       //雷之盾冷却计时器
		public const int ShieldCD = 30;  //限伤盾的冷却
		public int ManaTime = 30;        //魔力回复时间
		public int HealTime = 30;		 //血量回复时间
		public int Time;                 //时间计时
		public int Time2;                //时间计时
		private float damage;			 //当前伤害
		public override void ResetEffects() => HasThunderTail = false;
		public override void PostUpdate()
		{
			Time++;
			if (!HasThunderTail) return;
			if (ShieldCooldown > 0) ShieldCooldown--;//雷之盾冷却更新
			if (Time2 > 0)
			{
				Time2--;
				//半秒后触发回血
				if (Time2 == 0)
				{
					//每层标记回血1点
					int healAmount = MarkCount;
					Player.HealEffect(MarkCount * 5);//受伤回复生命
					for (int i = 0; i < Math.Min(MarkCount * 5, 100); i++)
					{
						Dust.NewDust(Player.position, Player.width, Player.height, DustID.GreenTorch,
							Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-4, 0), 150, Color.LimeGreen, 0.8f);
					}
				}
			}
		}
		public override void ModifyHurt(ref Player.HurtModifiers modifiers)
		{
			if (!HasThunderTail || ShieldCooldown > 0) return;
			float MaxDamage = Player.statLifeMax2 * (0.20f + MarkCount * 0.05f);
			//if (modifiers.FinalDamage.Flat > -150 && ShieldCooldown == 0)//过量伤害减免
			if (modifiers.FinalDamage.Flat > -150)//过量伤害减免
			{
				modifiers.FinalDamage /= 4.5f * (1 - MathF.Exp(-0.3f * MarkCount));//减免伤害
																   //modifiers.FinalDamage.Flat -= 10;//这是固定减伤
																   //modifiers.SourceDamage += 0.3f;//这是伤害加成
				if (modifiers.FinalDamage.Flat > 1000)
				{
					modifiers.FinalDamage /= Main.rand.NextFloat(60, 80);
				}
				//Player.Heal((int)(damage));//治疗补偿
				//ShieldCooldown = ShieldCD;//触发冷却
			}
			if (modifiers.FinalDamage.Base > 0 && MarkCount < MaxMarkCount)
				MarkCount++;
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
			if (Main.rand.NextFloat() < MarkCount * 0.005f)//雷电弹幕触发（每层0.5%概率）
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
			damage = hurtInfo.Damage;
			if (MarkCount < MaxMarkCount) MarkCount++;//受伤叠加层数（不超过上限）
			Time2 = HealTime;
		}
	}
}