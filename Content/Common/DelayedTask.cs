using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoBo.Content.Common
{
	public abstract class DelayedTask//延迟触发的基类，看看什么时候用得上
	{
		public int DelayFrames { get; protected set; }    //延迟帧数
		public int CurrentFrame { get; private set; }     //当前计数
		public bool IsActive { get; private set; }        //激活状态
		public bool IsRepeating { get; protected set; }   //是否循环执行
		public bool IsCompleted { get; private set; }     //任务完成状态

		protected DelayedTask(int delayFrames, bool repeat = false)
		{
			DelayFrames = delayFrames;
			IsRepeating = repeat;
			Reset();
		}
		//状态控制
		public void Start() => IsActive = true;
		public void Stop() => IsActive = false;
		public void Reset()
		{
			CurrentFrame = 0;
			IsCompleted = false; // 重置完成状态
		}
		//每帧
		public void Update()
		{
			if (!IsActive || IsCompleted) return;

			if (CurrentFrame++ >= DelayFrames)
			{
				IsCompleted = true; // 标记任务完成
				if (IsRepeating) Reset();
				else Stop();
			}
		}
		//获取剩余帧数
		public int GetRemainingFrames() => Math.Max(0, DelayFrames - CurrentFrame);
	}
	public class HealAfterHit : DelayedTask
	{
		private Player Player;
		private int HealAmount;
		public HealAfterHit(Player player, int healAmount, int delayFrames)
			: base(delayFrames: 30)
		{
			Player = player;
			HealAmount = healAmount;
		}
		//在外部调用此方法触发行为（与时间逻辑解耦）
		public void ApplyEffect()
		{
			Player.Heal(HealAmount);
			for (int i = 0; i < 10; i++)
			{
				Dust.NewDustPerfect(
					Player.Center,
					DustID.GoldFlame,
					new Vector2(Main.rand.NextFloat(-3f, 3f), -Main.rand.NextFloat(2f, 5f)),
					Scale: Main.rand.NextFloat(1f, 2f)
				).noGravity = true;
			}
			SoundEngine.PlaySound(SoundID.Item4, Player.position);
		}
	}
	public class DelayedTaskExample : ModPlayer
	{
		private List<DelayedTask> tasks = new List<DelayedTask>();
		public void AddHealTask(Player player, int healAmount, int delayFrames)
		{
			var task = new HealAfterHit(player, healAmount, delayFrames);
			task.Start();//启动计时
			tasks.Add(task);
		}
		public override void PostUpdate()
		{
			for (int i = tasks.Count - 1; i >= 0; i--)
			{
				var task = tasks[i];
				task.Update();//更新所有任务时间
				if (task.IsCompleted)
				{
					//安全类型转换
					if (task is HealAfterHit healTask)
					{
						healTask.ApplyEffect();//时间到后触发行为
					}
					tasks.RemoveAt(i);//安全移除
				}
			}
		}
	}
}