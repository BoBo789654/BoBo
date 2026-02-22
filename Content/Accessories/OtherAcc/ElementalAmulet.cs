using BoBo.Content.Accessories.FightAcc;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Accessories.OtherAcc
{
	/// <summary>
	/// [c/FFD700:★★★★][c/DAA520:☆]
	/// 根据环境提供不同增益：
	/// [c/8A2BE2:♈♉♊♋♌♍♎♏♐♑♒♓]
	/// [i:497] [c/00FFFF:海洋&水中&雨中→移速+10%｜加速度+8%｜飞行加速度+6%☔]
	/// [i:3794] [c/D2691E:沙漠→防御+10%｜沙暴中防御额外+10%⚑]
	/// [i:331] [c/32CD32:丛林→生命上限+20%⚜]
	/// [i:3109] [c/2E8B57:黑夜&地底→强照明范围(绿光)⚞⚟]
	/// [i:320] [c/A9A9A9:天空→伤害减免+25%✈]
	/// [i:154] [c/800080:地牢→暴击率+12%♕]
	/// [i:68][i:1330] [c/FF0000:邪恶之地→33%概率吸血5点☕]
	/// [i:183] [c/FF69B4:蘑菇地→攻速+70%✌]
	/// [i:3086][i:3081] [c/808080:花岗岩&大理石→生命上限+300☙]
	/// [i:117] [c/FF4500:陨石坑→伤害+100%✊]
	/// [i:593] [c/8B0000:雪原→攻击附加着火、狱炎]｜[c/ADD8E6:玩家免疫霜冻、冻伤❄]
	/// [i:173] [c/ADD8E6:地狱→攻击附加霜冻、冻伤]｜[c/8B0000:玩家免疫着火、狱炎☀]
	/// 根据职业提供不同增益：
	/// [i:3457] [c/DA70D6:法师→魔力回复+160｜魔力上限+160☘]
	/// [i:3458] [c/FFA500:战士→击退效果+80%⤴]
	/// [i:3456] [c/7FFFD4:射手→80%概率不消耗弹药｜无限飞行时间☉]
	/// [i:3459] [c/40E0D0:召唤师→生命恢复+50❤]
	/// </summary>
	public class ElementalAmulet : ModItem
	{
		public override string Texture => Pictures.OtherAcc + Name;
		public override void SetDefaults()
		{
			Item.width = 54;
			Item.height = 48;
			Item.accessory = true;
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(1, 0, 0, 0);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<ElementalAmuletPlayer>().HasElementalAmulet = true;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();//腐化
			recipe.AddIngredient(ItemID.NeptunesShell, 1);//​​海神贝壳
			recipe.AddIngredient(ItemID.AncientCloth, 5);//​​远古布匹​
			recipe.AddIngredient(ItemID.JungleSpores, 5);//丛林孢子​
			recipe.AddIngredient(ItemID.NightVisionHelmet, 1);//​​夜视头盔
			recipe.AddIngredient(ItemID.Feather, 5);//羽毛
			recipe.AddIngredient(ItemID.Bone, 50);//​​骨头​
			recipe.AddIngredient(ItemID.RottenChunk, 5);//腐肉​
			recipe.AddIngredient(ItemID.GlowingMushroom, 15);//发光蘑菇​
			recipe.AddIngredient(ItemID.Granite, 25);//​​花岗岩
			recipe.AddIngredient(ItemID.Marble, 25);//大理石
			recipe.AddIngredient(ItemID.MeteoriteBar, 10);//陨石锭​
			recipe.AddIngredient(ItemID.SnowBlock, 50);//雪块
			recipe.AddIngredient(ItemID.Obsidian, 15);//黑曜石
			recipe.AddIngredient(ItemID.FrostsparkBoots, 1);//霜花靴​
			recipe.AddTile(TileID.LunarCraftingStation);//制作站：远古操纵机
			recipe.Register();
			Recipe recipe2 = CreateRecipe();//猩红
			recipe2.AddIngredient(ItemID.NeptunesShell, 1);//​​海神贝壳
			recipe2.AddIngredient(ItemID.AncientCloth, 5);//​​远古布匹​
			recipe2.AddIngredient(ItemID.JungleSpores, 5);//丛林孢子​
			recipe2.AddIngredient(ItemID.NightVisionHelmet, 1);//​​夜视头盔
			recipe2.AddIngredient(ItemID.Feather, 5);//羽毛
			recipe2.AddIngredient(ItemID.Bone, 50);//​​骨头​
			recipe2.AddIngredient(ItemID.Vertebrae, 5);//椎骨​
			recipe2.AddIngredient(ItemID.GlowingMushroom, 15);//发光蘑菇​
			recipe2.AddIngredient(ItemID.Granite, 25);//​​花岗岩
			recipe2.AddIngredient(ItemID.Marble, 25);//大理石
			recipe2.AddIngredient(ItemID.MeteoriteBar, 10);//陨石锭​
			recipe2.AddIngredient(ItemID.SnowBlock, 50);//雪块
			recipe2.AddIngredient(ItemID.Obsidian, 15);//黑曜石
			recipe2.AddIngredient(ItemID.FrostsparkBoots, 1);//霜花靴​
			recipe2.AddTile(TileID.LunarCraftingStation);//制作站：远古操纵机
			recipe2.Register();
		}
	}
	public class ElementalAmuletPlayer : ModPlayer
	{
		public bool HasElementalAmulet;
		public int DamageTypeNum = 0;
		public override void ResetEffects()
		{
			HasElementalAmulet = false;
		}
		public override void PostUpdateEquips()
		{
			if (!HasElementalAmulet) return;
			if (Main.rand.NextBool(10))
			{
				Dust.NewDust(Player.position, Player.width, Player.height, DustID.MagicMirror, 
					Main.rand.NextFloat(-0.5f, 0.5f), -1f, Alpha: 100, new Color(180, 255, 255), Scale: 1.2f);
			}
			//职业增益
			//法师+160魔力回复且+160魔力上限
			//战士+80%击退
			//射手80%不消耗弹药且无限飞行时间
			//召唤师+50生命恢复
			if (Player.HeldItem.DamageType.Type == DamageClass.Magic.Type)
				DamageTypeNum = 1;
			else if (Player.HeldItem.DamageType.Type == DamageClass.Melee.Type)
				DamageTypeNum = 2;
			else if (Player.HeldItem.DamageType.Type == DamageClass.Ranged.Type)
				DamageTypeNum = 3;
			else if (Player.HeldItem.DamageType.Type == DamageClass.Summon.Type)
				DamageTypeNum = 4;
			else
				DamageTypeNum = 5;
			switch (DamageTypeNum)
			{
				case 1:
					Player.manaRegen += 160;
					Player.statManaMax2 += 160;
					break;
				case 2:
					Player.GetKnockback(DamageClass.Generic) *= 1.8f;
					break;
				case 3:
					Player.ammoCost80 = true;
					Player.wingTime = Player.wingTimeMax;
					break;
				case 4:
					Player.lifeRegen += 50;
					break;
				default: break;
			}
			//海洋增益：+10%移速，+8%加速度，+6%飞行加速度
			if (Player.ZoneBeach && Player.wet)
			{
				Player.moveSpeed += 0.1f;
				Player.accRunSpeed += 0.08f;
				Player.wingAccRunSpeed += 0.06f;
				Player.trapDebuffSource = false;
			}
			//沙漠增益：+10%防御，沙尘暴增益：再+10%防御
			if (Player.ZoneDesert)
			{
				Player.statDefense += (int)(Player.statDefense * 0.1f);
				if (Sandstorm.Happening)
				{
					Player.statDefense += (int)(Player.statDefense * 0.1f);
				}
			}
			//丛林增益：+20%生命上限
			if (Player.ZoneJungle)
			{
				Player.statLifeMax2 += (int)(Player.statLifeMax2 * 0.2f);//生命上限
			}
			//黑夜及地底增益：高强度照明
			if (!Main.dayTime || Player.ZoneRockLayerHeight || Player.ZoneDirtLayerHeight)
			{
				Lighting.AddLight(Player.Center, 1.5f, 4.5f, 2.5f);
			}
			//天空增益：+25%伤害减免
			if (Player.ZoneSkyHeight || Player.ZoneOverworldHeight)
			{
				Player.endurance = 0.25f;
			}
			//地牢增益：+12%暴击
			if (Player.ZoneDungeon)
			{
				Player.GetCritChance(DamageClass.Generic) += 12;
			}
			//邪恶之地增益：1/3概率吸血5
			if (Player.ZoneCorrupt || Player.ZoneCrimson)
			{
				if (Main.rand.NextBool(3)) 
				Player.HealEffect(5);
			}
			//蘑菇地增益：+70%攻速
			if (Player.ZoneGlowshroom)
			{
				Player.GetAttackSpeed(DamageClass.Generic) *= 1.7f;//沙暴攻速
			}
			//花岗岩及大理石增益：+ 300血量上限
			if (Player.ZoneGranite || Player.ZoneMarble)
			{
				Player.statLifeMax2 += 300;
			}
			//陨石增益：+100%伤害
			if (Player.ZoneMeteor)
			{
				Player.GetDamage(DamageClass.Generic) *= 2f;
			}
			//地表增益：+1幸运
			if (Player.ZoneOverworldHeight && !Player.ZoneSkyHeight && !Player.ZoneDirtLayerHeight && !Player.ZoneRockLayerHeight)
			{
				Player.luck += 1f;
			}
		}
		//攻击特效（雪原/地狱）
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!HasElementalAmulet) return;
			//雪原附加OnFire、OnFire3，玩家免疫Frostburn、Frostburn2
			if (Player.ZoneSnow)
			{
				target.AddBuff(BuffID.OnFire, 300);
				target.AddBuff(BuffID.OnFire3, 300);
				Player.buffImmune[BuffID.Frostburn] = true;
				Player.buffImmune[BuffID.Frostburn2] = true;
			}
			//地狱附加Frostburn、Frostburn2，玩家免疫OnFire+OnFire3
			else if (Player.ZoneUnderworldHeight)
			{
				target.AddBuff(BuffID.Frostburn, 300);
				target.AddBuff(BuffID.Frostburn2, 300);
				Player.buffImmune[BuffID.OnFire] = true;
				Player.buffImmune[BuffID.OnFire3] = true;
			}
		}
	}
}