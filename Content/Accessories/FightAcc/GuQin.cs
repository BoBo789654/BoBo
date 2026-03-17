using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Accessories.FightAcc
{
	public class GuQin : ModItem
	{
		public override string Texture => Pictures.FightAcc + Name;
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 36;
			Item.accessory = true;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(2, 0, 0, 0);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<GuQinPlayer>().HasGuQin = true;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.SolarPiano, 1);//耀斑钢琴
			recipe.AddIngredient(ItemID.VortexPiano, 1);//星旋钢琴
			recipe.AddIngredient(ItemID.NebulaPiano, 1);//星云钢琴
			recipe.AddIngredient(ItemID.StardustPiano, 1);//星尘钢琴
			recipe.AddTile(TileID.TinkerersWorkbench);//工匠作坊
		}
	}
	public class GuQinPlayer : ModPlayer
	{
		public bool HasGuQin;
		public float RangedAttackSpeedBonus = 0f;//远程攻速加成
		public List<string> ActiveBonuses = new List<string>();//存储当前激活的加成信息，用于UI显示
		private int LifestealCooldown = 0;//生命偷取相关变量
		private const int LifestealChance = 33;//33%触发概率
		public override void ResetEffects()
		{
			HasGuQin = false;
			RangedAttackSpeedBonus = 0f;
			ActiveBonuses.Clear();
		}
		public override void PostUpdate()
		{
			if (!HasGuQin) return;//检查激活条件
			if (!CheckActivationConditions()) return;//应用武器组合加成
			ApplyWeaponBonuses();//应用攻速掠夺
			ApplyAttackSpeedSteal();
			if (LifestealCooldown > 0)//更新生命偷取冷却
				LifestealCooldown--;
		}
		/// <summary>
		/// 检查激活条件：主武器为远程，7、8、9、0位置为四种类型各一把
		/// </summary>
		/// <returns>是否满足激活条件</returns>
		private bool CheckActivationConditions()
		{
			//检查主武器栏（位置1）是否为远程武器
			if (!IsSlotRangedWeapon(0)) return false;
			//检查7-0位置是否有四种类型各一把
			int[] SlotIndices = { 6, 7, 8, 9 };//7,8,9,0位置
			bool HasMelee = false, HasRanged = false, HasMagic = false, HasSummon = false;
			foreach (int SlotIndex in SlotIndices)
			{
				int WeaponType = GetWeaponType(SlotIndex);
				switch (WeaponType)
				{
					case 1: HasMelee = true; break;
					case 2: HasRanged = true; break;
					case 3: HasMagic = true; break;
					case 4: HasSummon = true; break;
				}
			}
			return HasMelee && HasRanged && HasMagic && HasSummon;
		}
		/// <summary>
		/// 检查指定位置是否为远程武器
		/// </summary>
		/// <param name="SlotIndex">背包槽位索引</param>
		/// <returns>是否为远程武器</returns>
		private bool IsSlotRangedWeapon(int SlotIndex)
		{
			Item Item = Player.inventory[SlotIndex];
			return Item.damage > 0 && Item.DamageType.CountsAsClass(DamageClass.Ranged);
		}
		/// <summary>
		/// 获取武器类型
		/// </summary>
		/// <param name="SlotIndex">背包槽位索引</param>
		/// <returns>武器类型：0=非武器，1=近战，2=远程，3=魔法，4=召唤</returns>
		private int GetWeaponType(int SlotIndex)
		{
			Item Item = Player.inventory[SlotIndex];
			if (Item.damage <= 0) return 0;

			if (Item.DamageType.CountsAsClass(DamageClass.Melee)) return 1;
			if (Item.DamageType.CountsAsClass(DamageClass.Ranged)) return 2;
			if (Item.DamageType.CountsAsClass(DamageClass.Magic)) return 3;
			if (Item.DamageType.CountsAsClass(DamageClass.Summon)) return 4;

			return 0;
		}
		private void ApplyWeaponBonuses()//应用武器组合加成
		{
			ActiveBonuses.Clear();//清空之前的加成信息
			int[] SlotIndices = { 6, 7, 8, 9 };//7,8,9,0位置
			for (int i = 0; i < 4; i++)
			{
				Item Weapon = Player.inventory[SlotIndices[i]];
				int WeaponType = GetWeaponType(SlotIndices[i]);
				if (WeaponType == 0) continue;
				switch (i)
				{
					case 0://位置7
						ApplySlot7Bonus(Weapon, WeaponType);
						break;
					case 1://位置8
						ApplySlot8Bonus(Weapon, WeaponType);
						break;
					case 2://位置9
						ApplySlot9Bonus(Weapon, WeaponType);
						break;
					case 3://位置0
						ApplySlot0Bonus(Weapon, WeaponType);
						break;
				}
			}
		}
		private void ApplySlot7Bonus(Item Weapon, int WeaponType)//位置7加成
		{
			switch (WeaponType)
			{
				case 1: //近战→暴击伤害
					Player.GetModPlayer<CriticalDamagePlayer>().HasCriticalDamage = true;
					Player.GetModPlayer<CriticalDamagePlayer>().CriticalDamageMultiplier = 5.0f;
					ActiveBonuses.Add("位置7[近战]: +400%暴击伤害");
					break;
				case 2: //远程→子弹速度
					Weapon.shootSpeed += 0.30f;
					ActiveBonuses.Add("位置7[远程]: +30%子弹速度");
					break;
				case 3: //魔法→
					ActiveBonuses.Add("位置7[魔法]: 还没做");
					break;
				case 4: //召唤→移动速度
					Player.moveSpeed += 0.35f;
					ActiveBonuses.Add("位置7[召唤]: +35%移动速度");
					break;
			}
		}
		private void ApplySlot8Bonus(Item Weapon, int WeaponType)//位置8加成
		{
			switch (WeaponType)
			{
				case 1: //近战→

					ActiveBonuses.Add("位置8[近战]:还没做");
					break;
				case 2: //远程→远程伤害
					Player.GetDamage(DamageClass.Ranged) += 1f;
					ActiveBonuses.Add("位置8[远程]: +100%远程伤害");
					break;
				case 3: //魔法→弹药效果
					Player.GetModPlayer<AmmoEffectPlayer>().HasAmmoEffect = true;
					ActiveBonuses.Add("位置8[魔法]: 弹药附加debuff");
					break;
				case 4: //召唤→

					ActiveBonuses.Add("位置8[召唤]: 还没做");
					break;
			}
		}
		private void ApplySlot9Bonus(Item Weapon, int WeaponType)//位置9加成
		{
			switch (WeaponType)
			{
				case 1: //近战→击退增强
					Player.GetKnockback(DamageClass.Ranged) += 0.30f;
					ActiveBonuses.Add("位置9[近战]: +30%击退效果");
					break;
				case 2: //远程→暴击几率
					Player.GetCritChance(DamageClass.Ranged) += 28;
					ActiveBonuses.Add("位置9[远程]: +28%远程暴击率");
					break;
				case 3: //魔法→
					ActiveBonuses.Add("位置9[魔法]: 还没做");
					break;
				case 4: //召唤→召唤物持续时间
					Player.GetAttackSpeed(DamageClass.Ranged) += 0.3f;
					ActiveBonuses.Add("位置9[召唤]: +30%远程攻速");
					break;
			}
		}
		private void ApplySlot0Bonus(Item Weapon, int WeaponType)//位置0加成
		{
			switch (WeaponType)
			{
				case 1: //近战→吸血效果
					Player.GetModPlayer<LifestealPlayer>().HasLifesteal = true;
					Player.GetModPlayer<LifestealPlayer>().LifestealAmount = 0.08f;
					Player.GetModPlayer<LifestealPlayer>().LifestealChance = 0.33f;
				ActiveBonuses.Add("位置0[近战]: 8%生命偷取(33%/击)");
					break;
				case 2: //远程→

					ActiveBonuses.Add("位置0[远程]: 还没做");
					break;
				case 3: //魔法→

					ActiveBonuses.Add("位置0[魔法]: 还没做");
					break;
				case 4: //召唤→召唤栏位
					Player.maxMinions += 3;
					ActiveBonuses.Add("位置0[召唤]: +3召唤栏位");
					break;
			}
		}
		public class CriticalDamagePlayer : ModPlayer
		{
			public bool HasCriticalDamage = false;
			public float CriticalDamageMultiplier = 1.0f;
			private bool isCriticalHit = false;
			private float baseDamage = 0f;
			public override void ResetEffects()
			{
				HasCriticalDamage = false;
				CriticalDamageMultiplier = 1.0f;
			}
			/*
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
			{
				if (HasCriticalDamage && hit.Crit)
				{
					//暴击命中，额外伤害
					int extraDamage = (int)(damageDone * (CriticalDamageMultiplier - 2f) / 2f); //减去默认的2倍暴击
					if (extraDamage > 0)
					{
						target.SimpleStrikeNPC(extraDamage, 0);
						CombatText.NewText(target.getRect(), Color.Purple, extraDamage, true, true);
					}
				}
			}*/
			public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
			{
				if (HasCriticalDamage && hit.Crit && proj.DamageType.CountsAsClass(DamageClass.Ranged))
				{
					//暴击命中，额外伤害
					int extraDamage = (int)(damageDone * (CriticalDamageMultiplier - 2f) / 2f);//减去默认的2倍暴击
					if (extraDamage > 0)
					{
						target.SimpleStrikeNPC(extraDamage, 0);
						CombatText.NewText(target.getRect(), Color.Purple, extraDamage, true, true);
					}
				}
			}
		}
		public class LifestealPlayer : ModPlayer//生命窃取
		{
			public bool HasLifesteal = false;
			public float LifestealAmount = 0f;
			public float LifestealChance = 0f;
			public override void ResetEffects()
			{
				HasLifesteal = false;
				LifestealAmount = 0f;
			}
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)//近战攻击命中时触发生命偷取
			{
				
				if (HasLifesteal && hit.Damage > 0 && Main.rand.NextFloat() < LifestealChance)
				{
					int healAmount = (int)(hit.Damage * LifestealAmount);
					if (healAmount > 0)
					{
						Player.HealEffect(healAmount);
						Player.statLife += healAmount;
						if (Player.statLife > Player.statLifeMax2)
							Player.statLife = Player.statLifeMax2;
					}
				}
			}
			public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)//远程攻击命中时触发生命偷取
			{
				
				if (HasLifesteal && hit.Damage > 0 && Main.rand.NextFloat() < LifestealChance)
				{
					int healAmount = (int)(hit.Damage * LifestealAmount);
					if (healAmount > 0)
					{
						Player.HealEffect(healAmount);
						Player.statLife += healAmount;
						if (Player.statLife > Player.statLifeMax2)
						{
							Player.statLife = Player.statLifeMax2;
							for (int i = 0; i < 10; i++)
							{
								ParticleOrchestraSettings p = default;//声明一个高级粒子生成器
								p.MovementVector = Main.rand.NextVector2Circular(20, 20);//速度设为横轴与纵轴半径为x的圆内的随机向量
								p.PositionInWorld = target.Center;//初始位置
								ParticleOrchestrator.SpawnParticlesDirect(ParticleOrchestraType.RainbowRodHit, p);//生成一个彩虹法杖的粒子
							}
						}
					}
				}
			}
		}
		//弹药效果辅助类
		public class AmmoEffectPlayer : ModPlayer
		{
			public bool HasAmmoEffect = false;
			public override void ResetEffects()
			{
				HasAmmoEffect = false;
			}
			public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
			{
				if (HasAmmoEffect && proj.DamageType.CountsAsClass(DamageClass.Ranged))
				{
					//随机附加debuff效果
					if (Main.rand.NextBool(3))
					{
						int[] possibleDebuffs = { BuffID.OnFire3, BuffID.Frostburn2, BuffID.CursedInferno, BuffID.Ichor };
						int selectedDebuff = possibleDebuffs[Main.rand.Next(possibleDebuffs.Length)];
						target.AddBuff(selectedDebuff, 330);
					}
				}
			}
		}
		private void ApplyAttackSpeedSteal()//应用攻速掠夺机制
		{
			Item MainWeapon = Player.inventory[0];//位置1的主武器
			if (MainWeapon.damage <= 0 || !MainWeapon.DamageType.CountsAsClass(DamageClass.Ranged)) return;
			int StealingUseTime = MainWeapon.useTime;//主武器的UseTime
			//查找7-0位置的远程武器
			int[] SlotIndices = { 6, 7, 8, 9 };
			foreach (int SlotIndex in SlotIndices)
			{
				Item StolenWeapon = Player.inventory[SlotIndex];
				if (StolenWeapon.damage > 0 && StolenWeapon.DamageType.CountsAsClass(DamageClass.Ranged))
				{
					int StolenUseTime = StolenWeapon.useTime;
					float SpeedBonus = CalculateSpeedBonus(StealingUseTime, StolenUseTime);
					RangedAttackSpeedBonus = Math.Min(SpeedBonus, 0.7f);//上限70%
					//添加攻速掠夺信息到加成列表
					ActiveBonuses.Add($"攻速掠夺: +{RangedAttackSpeedBonus:P1}远程攻速");
					break;//只取第一个找到的远程武器
				}
			}
			//应用攻速加成
			Player.GetAttackSpeed(DamageClass.Ranged) += RangedAttackSpeedBonus;
		}
		/// <summary>
		/// 计算攻速加成
		/// </summary>
		/// <param name="StealingUseTime">主武器的UseTime</param>
		/// <param name="StolenUseTime">被掠夺武器的UseTime</param>
		/// <returns>攻速加成百分比</returns>
		private float CalculateSpeedBonus(int StealingUseTime, int StolenUseTime)
		{
			if (StolenUseTime > StealingUseTime)
				return (float)StolenUseTime / StealingUseTime - 1f;//被偷的攻速慢：+(Stolen/Stealing-1)
			else
				return (float)StealingUseTime / StolenUseTime - 1f;//被偷的攻速快：+(Stealing/Stolen-1)
		}
	}
	//UI显示类
	public class GuQinUI : ModSystem
	{
		private const int TextSpacing = 20;//文本行间距
		private const int PanelPadding = 10;//面板内边距
		private const int PanelWidth = 200;//面板宽度

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			Player player = Main.LocalPlayer;
			var modPlayer = player.GetModPlayer<GuQinPlayer>();
			if (!modPlayer.HasGuQin) return;
			//优先判断ESC关闭背包的状态，ESC隐藏背包或玩家主动隐藏UI，把UI加成属性和那个框全部隐藏
			if (!Main.playerInventory || Main.hideUI) return;
			//检查激活状态
			bool isActive = CheckActivationStatus(player);
			//在屏幕左下角显示详细信息
			DrawDetailedStatusDisplay(spriteBatch, isActive, modPlayer);
		}
		private bool CheckActivationStatus(Player player)// 检查激活状态
		{
			if (!IsSlotRangedWeapon(player, 0)) return false;//检查主武器是否为远程
			int[] SlotIndices = { 6, 7, 8, 9 };//检查7-0位置是否有四种类型
			bool[] HasType = new bool[4];//近战,远程,魔法,召唤
			foreach (int SlotIndex in SlotIndices)
			{
				int WeaponType = GetWeaponType(player, SlotIndex);
				if (WeaponType > 0 && WeaponType <= 4)
					HasType[WeaponType - 1] = true;
			}
			return HasType[0] && HasType[1] && HasType[2] && HasType[3];
		}
		private bool IsSlotRangedWeapon(Player player, int SlotIndex)//检查指定位置是否为远程武器
		{
			Item Item = player.inventory[SlotIndex];
			return Item.damage > 0 && Item.DamageType.CountsAsClass(DamageClass.Ranged);
		}
		private int GetWeaponType(Player player, int SlotIndex)//获取武器类型
		{
			Item Item = player.inventory[SlotIndex];
			if (Item.damage <= 0) return 0;
			if (Item.DamageType.CountsAsClass(DamageClass.Melee)) return 1;
			if (Item.DamageType.CountsAsClass(DamageClass.Ranged)) return 2;
			if (Item.DamageType.CountsAsClass(DamageClass.Magic)) return 3;
			if (Item.DamageType.CountsAsClass(DamageClass.Summon)) return 4;
			return 0;
		}
		private void DrawDetailedStatusDisplay(SpriteBatch spriteBatch, bool IsActive, GuQinPlayer ModPlayer)//在屏幕左下角绘制详细的属性加成信息
		{
			Vector2 BasePosition = new Vector2(100, Main.screenHeight - 200);//面板起始位置在左下角
			DrawPanelBackground(spriteBatch, BasePosition, ModPlayer.ActiveBonuses.Count, IsActive);//绘制面板背景
			string Title = IsActive ? "攻速掠夺 - 已激活" : "攻速掠夺 - 未激活";//绘制标题
			Color TitleColor = IsActive ? Color.LightGreen : Color.LightCoral;
			Vector2 TitlePosition = BasePosition + new Vector2(PanelPadding, PanelPadding);
			Utils.DrawBorderString(spriteBatch, Title, TitlePosition, TitleColor, 0.9f);
			if (!IsActive)//如果未激活，显示激活条件提示
			{
				string ConditionText = "需要:1位置远程&7-0位置\n四职业武器各一个";
				Vector2 ConditionPosition = TitlePosition + new Vector2(0, TextSpacing);
				Utils.DrawBorderString(spriteBatch, ConditionText, ConditionPosition, Color.LightGray, 0.7f);
				return;
			}
			Vector2 BonusPosition = TitlePosition + new Vector2(0, TextSpacing);//绘制激活的加成列表
			foreach (string Bonus in ModPlayer.ActiveBonuses)
			{
				Utils.DrawBorderString(spriteBatch, Bonus, BonusPosition, Color.LightYellow, 0.8f);
				BonusPosition.Y += TextSpacing;
			}
		}
		private void DrawPanelBackground(SpriteBatch spriteBatch, Vector2 Position, int BonusCount, bool IsActive)//绘制信息面板背景
		{
			Player player = Main.LocalPlayer;
			var modPlayer = player.GetModPlayer<GuQinPlayer>();
			if (!modPlayer.HasGuQin) return;
			int PanelHeight = PanelPadding * 2 + TextSpacing * (BonusCount + 1);//计算面板高度（根据加成数量动态调整）
			if (!IsActive)
				PanelHeight = PanelPadding * 2 + TextSpacing * 3;
			Rectangle BackgroundRect = new Rectangle((int)Position.X, (int)Position.Y, PanelWidth, PanelHeight);//创建背景矩形
			//绘制半透明黑色背景
			Texture2D Pixel = ModContent.Request<Texture2D>("Terraria/Images/UI/HotbarRadial_1").Value;
			spriteBatch.Draw(Pixel, BackgroundRect, Color.Black * 0.7f);
			//绘制边框
			Rectangle BorderRect = new Rectangle(BackgroundRect.X - 2, BackgroundRect.Y - 2, BackgroundRect.Width + 4, BackgroundRect.Height + 4);
			spriteBatch.Draw(Pixel, BorderRect, Color.Gray * 0.5f);
		}
	}
}
/*
一些背包位置的判断方法
方法1：遍历背包查找物品位置
//查找物品在背包中的位置
public static int FindItemSlot(Player player, Item targetItem)
{
    for (int i = 0; i < player.inventory.Length; i++)
    {
        if (player.inventory[i] == targetItem)
        {
            return i;
        }
    }
    return -1; //未找到
}
//在ModItem中使用
public override void UpdateAccessory(Player player, bool hideVisual)
{
    int slot = FindItemSlot(player, this.Item);
    if (slot != -1)
    {
        //根据格子位置应用不同效果
        if (slot < 10) //快捷栏
        {
            //快捷栏特效
        }
        else if (slot >= 10 && slot < 50) //主背包
        {
            //主背包特效
        }
    }
}
方法2：直接获取当前物品的背包索引
public override void UpdateAccessory(Player player, bool hideVisual)
{
    //直接比较引用找到当前物品位置
    for (int i = 0; i < player.inventory.Length; i++)
    {
        if (player.inventory[i] == this.Item)
        {
            //i 就是当前物品在背包中的格子索引
            int currentSlot = i;
            
            //根据位置应用效果
            switch (currentSlot)
            {
                case int n when (n >= 0 && n < 10): //快捷栏
                    //快捷栏加成
                    break;
                case int n when (n >= 10 && n < 50): //主背包
                    //主背包加成
                    break;
            }
            break;
        }
    }
}
方法3：在UI中显示物品位置信息
public class ***UI : ModSystem
{
    public override void PostDrawInterface(SpriteBatch spriteBatch)
    {
        Player player = Main.LocalPlayer;
        var modPlayer = player.GetModPlayer<***Player>();
        
        if (!modPlayer.***) return;
        
        //查找游魂杯在背包中的位置
        int cupSlot = -1;
        for (int i = 0; i < player.inventory.Length; i++)
        {
            if (player.inventory[i].type == ModContent.ItemType<***>())
            {
                cupSlot = i;
                break;
            }
        }
        //在UI中显示位置信息
        if (cupSlot != -1)
        {
            string positionText = $"位置: {GetSlotDescription(cupSlot)}";
            //绘制位置文本...
        }
    }
    private string GetSlotDescription(int slot)
    {
        if (slot < 10) return $"快捷栏{slot + 1}";
        if (slot < 50) return $"背包{slot - 9}";
        return "装备栏";
    }
}
背包索引说明：0-9: 快捷栏；10-49: 主背包（40个格子）；50-53: 钱币栏；54-57: 弹药栏；58-93: 其他特殊栏位
实用工具方法：
public static class InventoryHelper
{
    public static bool IsInHotbar(int slot) => slot >= 0 && slot < 10;
    public static bool IsInMainInventory(int slot) => slot >= 10 && slot < 50;
    public static bool IsEquipped(Player player, Item item) => player.armor.Any(a => a == item);
    public static int FindItemSlot(Player player, int itemType)
    {
        for (int i = 0; i < player.inventory.Length; i++)
        {
            if (player.inventory[i].type == itemType)
                return i;
        }
        return -1;
    }
}
*/