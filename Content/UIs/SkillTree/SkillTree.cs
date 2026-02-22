using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace BoBo.Content.UIs.SkillTree
{
	public class SkillTreePlayer : ModPlayer
	{
		public HashSet<string> ActiveSkills = [];
		#region 声明有哪些技能
		//目前布置16种属性 加算
		public float DamageBonus;//武器伤害加成（不想做职业区分：Generic）
		public float CritBonus;//武器暴击加成（不想做职业区分：Generic）
		public float KnockBackBonus;//武器击退加成（不想做职业区分：Generic）
		public int LifeBonus;//当前最大生命值
		public int ManaBonus;//当前最大魔力值
		public float LifeRegenBonus;//玩家在两秒内自然恢复的血量，+2就是多回复两点生命值
		public float ManaRegenBonus;//玩家在两秒内自然恢复的蓝量，+2就是多回复两点魔力值
		public int DefenseBonus;//玩家当前防御值
		public float MaxRunSpeedBonus;//玩家最快能跑多快
		public float RunAccelerationBonus;//玩家加速到最快速度的加速度值
		public float RunSlowdownBonus;//玩家停止移动时的减速度
		public float MoveSpeedBonus;//玩家的移动速度，最多能设为1.6
		public float MeleeSpeedBonus;//近战攻击速度，按照+0.05等于+5%攻击速度这这种方式计算的，而不是按照挥动一次的帧数
		public float LifeRegenTimeBonus;//增加生命恢复，但是不是数值上，而是开始生命回复的速率，加的越多就越快开始生命回复
		public float ManaCostBonus;//玩家魔法消耗的百分比，-0.05代表减少5%总体魔法消耗
		public int MaxMinionsBonus;//玩家的最大召唤栏
		public int SkillPoints;
		public struct SkillEffect()
		{
			public float Damage { get; set; } = 0;
			public float Crit { get; set; } = 0;
			public float Knockback { get; set; } = 0;
			public int Life { get; set; } = 0;
			public int Mana { get; set; } = 0;
			public float LifeRegen { get; set; } = 0;
			public float ManaRegen { get; set; } = 0;
			public int Defense { get; set; } = 0;
			public float MaxRunSpeed { get; set; } = 0;
			public float RunAcceleration { get; set; } = 0;
			public float RunSlowdown { get; set; } = 0;
			public float MoveSpeed { get; set; } = 0;
			public float MeleeSpeed { get; set; } = 0;
			public float LifeRegenTime { get; set; } = 0;
			public float ManaCost { get; set; } = 0;
			public int MaxMinions { get; set; } = 0;
		}
		#endregion
		#region 技能展示设置
		//技能效果
		public static readonly Dictionary<string, SkillEffect> SkillEffects = new()
		{
			//基础技能（1-3级）
			{"伤害1级", new SkillEffect { Damage = 4f }},
			{"伤害2级", new SkillEffect { Damage = 8f }},
			{"伤害3级", new SkillEffect { Damage = 12f }},
			{"暴击1级", new SkillEffect { Crit = 3f }},
			{"暴击2级", new SkillEffect { Crit = 6f }},
			{"暴击3级", new SkillEffect { Crit = 9f }},
			{"血量1级", new SkillEffect { Life = 20 }},
			{"血量2级", new SkillEffect { Life = 40 }},
			{"血量3级", new SkillEffect { Life = 60 }},
			{"魔力1级", new SkillEffect { Mana = 20 }},
			{"魔力2级", new SkillEffect { Mana = 40 }},
			{"魔力3级", new SkillEffect { Mana = 60 }},
			//血量分支技能（4-6级）
			{"防御1级", new SkillEffect { Defense = 5 }},
			{"防御2级", new SkillEffect { Defense = 10 }},
			{"防御3级", new SkillEffect { Defense = 15 }},
			{"生命恢复1级", new SkillEffect { LifeRegen = 2f }},
			{"生命恢复2级", new SkillEffect { LifeRegen = 4f }},
			{"生命恢复3级", new SkillEffect { LifeRegen = 6f }},
			//魔力分支技能（4-6级）
			{"减少消耗1级", new SkillEffect { ManaCost = -0.050f }},
			{"减少消耗2级", new SkillEffect { ManaCost = -0.100f }},
			{"减少消耗3级", new SkillEffect { ManaCost = -0.175f }},
			{"魔力恢复1级", new SkillEffect { ManaRegen = 15f }},
			{"魔力恢复2级", new SkillEffect { ManaRegen = 25f }},
			{"魔力恢复3级", new SkillEffect { ManaRegen = 35f }}
		};
		#endregion
		#region 技能点数设置
		//技能消耗点数
		public static readonly Dictionary<string, int> SkillCosts = new()
		{
			//基础技能 (1-3级)
			{"伤害1级", 1}, {"伤害2级", 2}, {"伤害3级", 3},
			{"暴击1级", 1}, {"暴击2级", 2}, {"暴击3级", 3},
			{"血量1级", 1}, {"血量2级", 2}, {"血量3级", 3},
			{"魔力1级", 1}, {"魔力2级", 2}, {"魔力3级", 3},
			//血量分支技能 (4-6级)
			{"防御1级", 4}, {"防御2级", 5}, {"防御3级", 6},
			{"生命恢复1级", 4}, {"生命恢复2级", 5}, {"生命恢复3级", 6},
			//魔力分支技能 (4-6级)
			{"魔力恢复1级", 4}, {"魔力恢复2级", 5}, {"魔力恢复3级", 6},
			{"减少消耗1级", 4}, {"减少消耗2级", 5}, {"减少消耗3级", 6},
		};
		#endregion
		#region 技能绑定设置
		//技能的下一级，需要先点亮上一级
		public static Dictionary<string, string[]> SkillDependencies = new()
		{
			{"伤害2级", new[]{"伤害1级"}}, {"伤害3级", new[]{"伤害2级"}},
			{"暴击2级", new[]{"暴击1级"}}, {"暴击3级", new[]{"暴击2级"}},
			{"血量2级", new[]{"血量1级"}}, {"血量3级", new[]{"血量2级"}},
			{"魔力2级", new[]{"魔力1级"}}, {"魔力3级", new[]{"魔力2级"}},
			//血量分支
			{"防御1级", new[]{"血量3级"}}, {"防御2级", new[]{"防御1级"}}, 
			{"防御3级", new[]{"防御2级"}}, {"生命恢复1级", new[]{"血量3级"}}, 
			{"生命恢复2级", new[]{"生命恢复1级"}}, {"生命恢复3级", new[]{"生命恢复2级"}},
			//魔力分支
			{"魔力恢复1级", new[]{"魔力3级"}}, {"魔力恢复2级", new[]{"魔力恢复1级"}},
			{"魔力恢复3级", new[]{"魔力恢复2级"}}, {"减少消耗1级", new[]{"魔力3级"}},
			{"减少消耗2级", new[]{"减少消耗1级"}}, {"减少消耗3级", new[]{"减少消耗2级"}},
		};
		#endregion
		#region 技能互斥设置
		//互斥的技能
		public static readonly Dictionary<string, List<string>> MutuallyExclusiveSkills = new()
		{
			{"防御1级", new List<string>{"生命恢复1级"}}, {"生命恢复1级", new List<string>{"防御1级"}},
			{"减少消耗1级", new List<string>{"魔力恢复1级"}}, {"魔力恢复1级", new List<string>{"减少消耗1级"}}
		};
		#endregion
		#region 点数设置
		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (KeybindSystem.SkillTreeKey.JustPressed)
			{
				var uiSystem = ModContent.GetInstance<SkillTreeUISystem>();
				uiSystem.ToggleUI();
				SoundEngine.PlaySound(SoundID.MenuOpen);
			}
		}
		public override void LoadData(TagCompound tag)
		{
			SkillPoints = tag.GetInt("SkillPoints");
			ActiveSkills = new HashSet<string>(tag.GetList<string>("ActiveSkills"));
			RecalculateStats();
		}
		public override void SaveData(TagCompound tag)
		{
			tag["SkillPoints"] = SkillPoints;
			tag["ActiveSkills"] = ActiveSkills.ToList();
		}
		#endregion
		#region 技能加成重置
		public void OnSkillChanged() => RecalculateStats();
		public void RecalculateStats()
		{
			//重置所有加成（16种）
			DamageBonus = 0;
			CritBonus = 0;
			KnockBackBonus = 0;
			LifeBonus = 0;
			ManaBonus = 0;
			LifeRegenBonus = 0;
			ManaRegenBonus = 0;
			DefenseBonus = 0;
			MaxRunSpeedBonus = 0;
			RunAccelerationBonus = 0;
			RunSlowdownBonus = 0;
			MoveSpeedBonus = 0;
			MeleeSpeedBonus = 0;
			LifeRegenTimeBonus = 0;
			ManaCostBonus = 0;
			MaxMinionsBonus = 0;
			//应用所有激活技能的加成
			foreach (var skill in ActiveSkills)
			{
				if (SkillEffects.TryGetValue(skill, out var effect))
				{
					DamageBonus += effect.Damage;
					CritBonus += effect.Crit;
					KnockBackBonus += effect.Knockback;
					LifeBonus += effect.Life;
					ManaBonus += effect.Mana;
					LifeRegenBonus += effect.LifeRegen;
					ManaRegenBonus += effect.ManaRegen;
					DefenseBonus += effect.Defense;
					MaxRunSpeedBonus += effect.MaxRunSpeed;
					RunAccelerationBonus += effect.RunAcceleration;
					RunSlowdownBonus += effect.RunSlowdown;
					MoveSpeedBonus += effect.MoveSpeed;
					MeleeSpeedBonus += effect.MeleeSpeed;
					LifeRegenTimeBonus += effect.LifeRegenTime;
					ManaCostBonus += effect.ManaCost;
					MaxMinionsBonus += effect.MaxMinions;
				}
			}
		}
		public static int GetSkillCost(string skillName)
		{
			if (SkillCosts.TryGetValue(skillName, out int cost))
				return cost;
			return 0;
		}
		#endregion
		#region 技能加成计算
		public override void PostUpdateMiscEffects()
		{
			Player.GetDamage(DamageClass.Generic) += DamageBonus / 100;
			Player.GetCritChance(DamageClass.Generic) += CritBonus;
			Player.GetKnockback(DamageClass.Generic) += KnockBackBonus;
			Player.statLifeMax2 += LifeBonus;
			Player.statManaMax2 += ManaBonus;
			Player.lifeRegen += (int)(LifeRegenBonus * 1);
			Player.manaRegen += (int)(ManaRegenBonus * 1);
			Player.statDefense += DefenseBonus;
			Player.maxRunSpeed += MaxRunSpeedBonus;
			Player.runAcceleration += RunAccelerationBonus;
			Player.runSlowdown += RunSlowdownBonus;
			Player.moveSpeed += MoveSpeedBonus;
			Player.GetAttackSpeed(DamageClass.Generic) += MeleeSpeedBonus;
			Player.lifeRegenTime += LifeRegenTimeBonus;
			Player.manaCost += ManaCostBonus;
			Player.maxMinions += MaxMinionsBonus;
		}
		#endregion
	}
	public class SkillTreeUI : UIState
	{
		public static bool Visible;
		private UIPanel MainPanel;
		private UIText PointsDisplay;
		public readonly List<SkillNode> Nodes = new();
		#region 技能位置
		//技能位置位置配置（相对于核心技能位置(0,0)）
		private readonly Dictionary<string, (float X, float Y)> NodePositions = new()
		{
			{"核心", (0, 0)},
			{"伤害1级", (-60, 0)}, {"伤害2级", (-120, 0)}, {"伤害3级", (-180, 0)},
			{"暴击1级", (60, 0)}, {"暴击2级", (120, 0)}, {"暴击3级", (180, 0)},
			{"血量1级", (0, -45)}, {"血量2级", (0, -90)}, {"血量3级", (0, -135)},
			{"魔力1级", (0, 45)}, {"魔力2级", (0, 90)}, {"魔力3级", (0, 135)},
			{"防御1级", (-50, -160)}, {"防御2级", (-70, -200)}, {"防御3级", (-90, -240)},
			{"生命恢复1级", (50, -160)}, {"生命恢复2级", (70, -200)}, {"生命恢复3级", (90, -240)},
			{"减少消耗1级", (-50, 160)}, {"减少消耗2级", (-70, 200)}, {"减少消耗3级", (-90, 240)},
			{"魔力恢复1级", (50, 160)}, {"魔力恢复2级", (70, 200)}, {"魔力恢复3级", (90, 240)}
		};
		#endregion
		#region 技能位置关系
		//技能位置依赖关系（用于UI连线）
		private readonly Dictionary<string, string> NodeParents = new()
		{
			//基础技能，前面要选
			{"伤害1级", "核心"}, {"伤害2级", "伤害1级"}, {"伤害3级", "伤害2级"},
			{"暴击1级", "核心"}, {"暴击2级", "暴击1级"}, {"暴击3级", "暴击2级"},
			{"血量1级", "核心"}, {"血量2级", "血量1级"}, {"血量3级", "血量2级"},
			{"魔力1级", "核心"}, {"魔力2级", "魔力1级"}, {"魔力3级", "魔力2级"},
			//分支技能，二选一
			{"防御1级", "血量3级"}, {"防御2级", "防御1级"}, {"防御3级", "防御2级"},
			{"生命恢复1级", "血量3级"}, {"生命恢复2级", "生命恢复1级"}, {"生命恢复3级", "生命恢复2级"},
			{"减少消耗1级", "魔力3级"}, {"减少消耗2级", "减少消耗1级"}, {"减少消耗3级", "减少消耗2级"},
			{"魔力恢复1级", "魔力3级"}, {"魔力恢复2级", "魔力恢复1级"}, {"魔力恢复3级", "魔力恢复2级"}
		};
		#endregion
		#region 点数位置
		public override void OnInitialize()
		{
			MainPanel = new UIPanel();
			MainPanel.Width.Set(900, 0);
			MainPanel.Height.Set(700, 0);
			MainPanel.HAlign = 0.5f;
			MainPanel.VAlign = 0.5f;
			Append(MainPanel);
			PointsDisplay = new UIText("点数：0");
			PointsDisplay.Top.Set(10, 0);
			PointsDisplay.Left.Set(10, 0);
			MainPanel.Append(PointsDisplay);
			//创建核心技能位置
			var coreNode = CreateNode("核心", 0, 0, 0);
			coreNode.IsActive = true;
			//创建所有其他技能位置
			foreach (var kvp in NodePositions)
			{
				if (kvp.Key == "核心") continue;//创建核心技能位置
				var (X, Y) = kvp.Value;
				CreateNode(kvp.Key, X, Y, SkillTreePlayer.SkillCosts[kvp.Key]);
			}
			//设置技能依赖关系
			foreach (var node in Nodes)
			{
				if (NodeParents.TryGetValue(node.SkillName, out string parentName))
				{
					node.ParentNode = Nodes.FirstOrDefault(n => n.SkillName == parentName);
				}
			}
			AddControlButtons();
		}
		//创建技能位置辅助函数
		private SkillNode CreateNode(string name, float x, float y, int baseCost)
		{
			var node = new SkillNode(name, baseCost);
			node.Left.Set(MainPanel.Width.Pixels / 2 + x, 0);
			node.Top.Set(MainPanel.Height.Pixels / 2 + y, 0);
			MainPanel.Append(node);
			Nodes.Add(node);
			return node;
		}
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			foreach (var node in Nodes)
			{
				if (node.ParentNode != null)
				{
					Vector2 start = node.GetDimensions().Center();
					Vector2 end = node.ParentNode.GetDimensions().Center();
					DrawThickLine(spriteBatch, start, end, node.IsActive ? Color.Red : Color.White, 20);
				}
			}
		}
		private static void DrawThickLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int width)
		{
			Vector2 direction = Vector2.Normalize(end - start);
			Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
			float step = width / 2f;
			for (float offset = -step; offset <= step; offset += 1f)
			{
				Vector2 offsetVec = perpendicular * offset;
				Utils.DrawLine(spriteBatch, start + offsetVec, end + offsetVec, color);
			}
		}
		#endregion
		#region 重置技能
		private void AddControlButtons()
		{
			var resetButton = new UIText("重置技能");
			resetButton.Width.Set(80, 0);
			resetButton.Height.Set(30, 0);
			resetButton.Top.Set(MainPanel.Height.Pixels - 40, 0);
			resetButton.OnLeftClick += (evt, _) =>
			{
				if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
					ResetAllSkills();
				else
					Main.NewText("按住Shift点击重置", Color.Yellow);
			};
			MainPanel.Append(resetButton);
		}
		#endregion
		#region 核心处
		public void ResetAllSkills()
		{
			var player = Main.LocalPlayer.GetModPlayer<SkillTreePlayer>();
			if (player == null) return;
			foreach (var skill in player.ActiveSkills.ToList())
			{
				if (skill == "核心") continue;
				player.SkillPoints += SkillTreePlayer.GetSkillCost(skill);
				player.ActiveSkills.Remove(skill);
			}
			player.RecalculateStats();
			foreach (var node in Nodes) node.ResetNode();
			Nodes.First(n => n.SkillName == "核心").IsActive = true;
			SoundEngine.PlaySound(SoundID.Research);
		}
		#endregion
		#region 技能点数
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			if (Main.LocalPlayer.TryGetModPlayer(out SkillTreePlayer player))
			{
				PointsDisplay.SetText($"点数：{player.SkillPoints}");
				foreach (var node in Nodes)
				{
					node.IsActive = player.ActiveSkills.Contains(node.SkillName);
				}
			}
		}
		#endregion
	}
	public class SkillNode : UIPanel
	{
		public SkillNode ParentNode;
		public bool IsActive { get; set; }
		public string SkillName { get; }
		public int BaseCost { get; }
		public SkillNode(string skillName, int baseCost)//技能框
		{
			SkillName = skillName;
			BaseCost = baseCost;
			Width.Set(60, 0);
			Height.Set(45, 0);
			SetPadding(0);
			BackgroundColor = Color.Gray;
			BorderColor = Color.Gold;
			tooltip = BuildTooltip(skillName);
		}
		public Vector2 GetPositionOffset()
		{
			return new Vector2(Left.Pixels, Top.Pixels);
		}
		#region 允许点击的检查
		public override void LeftMouseDown(UIMouseEvent evt)
		{
			var player = Main.LocalPlayer.GetModPlayer<SkillTreePlayer>();
			//尽可能的检查
			bool CanActivate =
				!IsActive &&
				player.SkillPoints >= BaseCost &&
				(ParentNode?.IsActive ?? true) &&
				CheckDependencies(player) &&
				!IsMutuallyExclusive(player, SkillName);//是否互斥
			if (CanActivate)
			{
				IsActive = true;
				player.SkillPoints -= BaseCost;
				player.ActiveSkills.Add(SkillName);
				player.RecalculateStats();
				SoundEngine.PlaySound(SoundID.Item4); 
			}
			if (IsMutuallyExclusive(player, SkillName))
			{
				Main.NewText("别贪心", Color.IndianRed);
			}
		}
		#endregion
		private static bool IsMutuallyExclusive(SkillTreePlayer player, string skillName)
		{
			if (!SkillTreePlayer.MutuallyExclusiveSkills.TryGetValue(skillName, out var exclusiveList))
				return false;
			foreach (var exclusiveSkill in exclusiveList)
			{
				if (player.ActiveSkills.Contains(exclusiveSkill))
					return true;
			}
			return false;
		}
		private bool CheckDependencies(SkillTreePlayer player)
		{
			if (SkillTreePlayer.SkillDependencies.TryGetValue(SkillName, out var dependencies))
			{
				foreach (var dep in dependencies)
				{
					if (!player.ActiveSkills.Contains(dep)) return false;
				}
			}
			return true;
		}
		private string tooltip;
		//技能提示文本
		private string BuildTooltip(string skillName)
		{
			if (SkillTreePlayer.SkillEffects.TryGetValue(skillName, out var effect))
			{
				List<string> parts = new List<string>();
				//检查每个属性并生成描述
				if (effect.Damage != 0) parts.Add($"+{effect.Damage}% 伤害");
				if (effect.Crit != 0) parts.Add($"+{effect.Crit}% 暴击率");
				if (effect.Knockback != 0) parts.Add($"+{effect.Knockback}% 暴击率");
				if (effect.Life != 0) parts.Add($"+{effect.Life} 生命");
				if (effect.Mana != 0) parts.Add($"+{effect.Mana} 魔力");
				if (effect.LifeRegen != 0) parts.Add($"+{effect.LifeRegen} 生命恢复");
				if (effect.ManaRegen != 0) parts.Add($"+{effect.ManaRegen} 魔力恢复");
				if (effect.Defense != 0) parts.Add($"+{effect.Defense} 防御");
				if (effect.MaxRunSpeed != 0) parts.Add($"+{effect.MaxRunSpeed} 最大移速");
				if (effect.RunAcceleration != 0) parts.Add($"+{effect.RunAcceleration} 加速度");
				if (effect.RunSlowdown != 0) parts.Add($"+{effect.RunSlowdown} 减速度");
				if (effect.MoveSpeed != 0) parts.Add($"+{effect.MoveSpeed} 移速");
				if (effect.MeleeSpeed != 0) parts.Add($"+{effect.MeleeSpeed} 近战攻速");
				if (effect.LifeRegenTime != 0) parts.Add($"+{effect.LifeRegenTime} 生命回复的速率");
				if (effect.ManaCost != 0) parts.Add($"{effect.ManaCost * 100}% 魔力消耗");
				if (effect.MaxMinions != 0) parts.Add($"+{effect.MaxMinions} 召唤栏");
				return string.Join("\n", parts);
			}
			return "无效果";
		}
		//绘制提示的具体属性
		private void DrawTooltip(SpriteBatch spriteBatch, string text, Vector2 pos)
		{
			DynamicSpriteFont font = Terraria.GameContent.FontAssets.DeathText.Value;//获取字体
			Vector2 TextSize = font.MeasureString(text);
			Vector2 CenterPos = new Vector2(pos.X - TextSize.X / 2, pos.Y);//横向居中
			Utils.DrawBorderStringFourWay(spriteBatch, font, text, CenterPos.X, CenterPos.Y, Color.Cyan, Color.Black, Vector2.Zero, 0.6f);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			//本来想在鼠标旁边的，但还是放右下角吧
			if (IsMouseHovering && !string.IsNullOrEmpty(tooltip))
			{
				Vector2 Pos = new Vector2(Main.screenWidth / 2 - 80 / Main.UIScale, Main.screenHeight / 2 + 400f / Main.UIScale);
				DrawTooltip(spriteBatch, tooltip, Pos);
			}
			//框内框
			Texture2D texture = ModContent.Request<Texture2D>(Pictures.SkillTree + "SkillTreeFrame").Value;
			Rectangle dimensions = GetDimensions().ToRectangle();
			Color DrawColor = IsActive ? Color.White : new Color(150, 150, 150, 255);//这里激活换颜色
			spriteBatch.Draw(texture, dimensions, DrawColor);
			//绘制文本
			Vector2 center = new Vector2(dimensions.X + dimensions.Width / 2, dimensions.Y + dimensions.Height / 2);
			Color textColor = IsActive ? Color.Lime : Color.White;
			Utils.DrawBorderString(spriteBatch, SkillName, center - new Vector2(0, 8) / Main.UIScale, textColor, 0.7f, 0.5f);
			if (!IsActive)
			{
				Utils.DrawBorderString(spriteBatch, $"{BaseCost}点", center + new Vector2(0, 8) / Main.UIScale, Color.Yellow, 0.6f, 0.5f);
			}
		}
		public void ResetNode()
		{
			IsActive = false;
			BorderColor = Color.Gold;
		}
	}
}