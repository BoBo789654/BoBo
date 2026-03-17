using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Terraria;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace BoBo.Content.Players
{
	public class ShakingModConfig : ModConfig//屏幕震动配置
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		[Label("屏幕受击摇晃")]
		[Tooltip("启用后，玩家受到伤害时屏幕会震动")]
		[DefaultValue(true)]
		public bool EnableHurtScreenShake;
	}
	public class RestrictCameraModifier : ICameraModifier//实现ICameraModifier接口，用于创建平滑的镜头移动效果，可以将镜头从一个点平滑移动到另一个点
	{
		public string UniqueIdentity { get; private set; }//用于识别和避免重复的相机修改器
		public bool Finished { get; private set; }//当效果执行完毕后设置为true
		public RestrictCameraModifier(Vector2 fromCenter, Vector2 toCenter, float amount, Func<bool> finish, string uniqueIdentity = null, bool reset = true)
		{
			UniqueIdentity = uniqueIdentity;	//唯一标识符
			fromCenterPoint = fromCenter;		//起始中心点
			toCenterPoint = toCenter;			//目标中心点
			totalAmount = amount;				//移动总量（总帧数）
			this.reset = reset;					//是否在完成后重置
			canFinish = finish;					//完成条件的委托函数
		}
		public Vector2 Remap(Vector2 from, Vector2 to, float amount)//向量插值方法，线性插值
		{
			return new Vector2(MathHelper.Lerp(from.X, to.X, amount), MathHelper.Lerp(from.Y, to.Y, amount));
		}
		public Vector2 CenterToScreenPos(Vector2 center)//中心点转屏幕位置，将世界坐标中心点转换为屏幕左上角位置
		{
			return center - new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
		}
		public void Update(ref CameraInfo cameraInfo)//更新相机位置，每帧调用，平滑移动相机位置
		{
			
			cameraInfo.CameraPosition = CenterToScreenPos(fromCenterPoint) + (CenterToScreenPos(toCenterPoint) - CenterToScreenPos(fromCenterPoint)) * nowT;//计算当前帧的相机位置
			if (nowT < amount)//更新进度
				nowT += 1f / totalAmount;
			if ((amount - nowT <= 0.01f || amount <= nowT) && canFinish())//检查是否完成
				Finished = true;
		}
		public void Reset()//重置相机位置，当效果完成后重置相机到玩家中心
		{
			if (reset)
			{
				RestrictCameraModifier shaking = new RestrictCameraModifier
					(toCenterPoint, Main.LocalPlayer.Center, amount, () => true, "RockRestrictCameraModifierReset", false);
				CameraModifierPlayer.ShakeStack.Add(shaking);
			}
		}
		private float nowT = 0f;					//当前进度
		private readonly float amount = 1f;			//目标进度
		private readonly float totalAmount = 1f;	//总帧数
		private Vector2 fromCenterPoint;			//起始中心点
		private Vector2 toCenterPoint;				//目标中心点
		private readonly bool reset = true;			//是否重置
		private readonly Func<bool> canFinish;		//完成条件检查委托
	}
	public class CameraModifierPlayer : ModPlayer//相机修改器玩家类
	{
		private static ShakingModConfig Config => ModContent.GetInstance<ShakingModConfig>();//配置实例，获取屏幕震动的配置设置
		public int ScreenShake;//基础震动变量，控制简单随机震动的持续时间
		public static CameraModifierStack ShakeStack = new CameraModifierStack();//相机修改器栈，存储所有活动的相机修改器，静态，所有玩家实例共享
		public override void ModifyScreenPosition()//修改屏幕位置，每帧调用，应用所有屏幕震动效果
		{
			base.ModifyScreenPosition();
			if (ScreenShake > 0)//应用基础随机震动
			{
				Main.screenPosition += Main.rand.NextVector2Circular(7, 7);//在圆形区域内随机偏移屏幕位置
				ShakeStack.ApplyTo(ref Main.screenPosition);//应用相机修改器栈中的所有效果
			}
		}
		public override void ResetEffects()//更新震动计时器
		{
			if (ScreenShake > 0)
			{
				ScreenShake--;
			}
		}
		public override void OnHurt(Player.HurtInfo info)
		{
			//检查配置是否启用受击摇晃
			if (Config != null && Config.EnableHurtScreenShake)//计算摇晃强度：伤害值与200伤害的比例，乘以基准强度20f
			{
				float baseIntensity = 20f;
				float baseDamage = 200f; 
				float intensity = info.Damage / baseDamage * baseIntensity;//根据伤害值比例计算实际强度
				ScreenShake = 10;//触发简单震动
				TriggerPunchShake(Player.Center, intensity, 25);//触发定向冲击震动，使用计算出的强度
			}
		}
		public void TriggerSimpleShake(int duration = 30)//触发简单震动的方法，设置简单的随机震动持续时间
		{
			ScreenShake = duration;
		}
		public void TriggerPunchShake(Vector2 eventCenter, float intensity = 15f, int duration = 20)//触发定向冲击震动的方法，创建PunchCameraModifier实现有方向的冲击震动
		{
			PunchCameraModifier shaking = new PunchCameraModifier(				//创建相机修改
				eventCenter,													//震动中心点
				(Main.rand.NextFloat() * MathHelper.TwoPi).ToRotationVector2(), //随机方向
				intensity,														//震动强度
				6f,																//衰减
				duration,														//持续时间
				1000f,															//衰减距离
				"CustomPunch"													//唯一标识符
			);

			//添加到修改器栈
			ShakeStack.Add(shaking);
		}
	}

	///<summary>
	///相机修改器栈管理类
	///管理多个相机修改器的添加、更新和移除
	///确保多个震动效果可以叠加而不冲突
	///</summary>
	public class CameraModifierStack
	{
		///<summary>
		///添加相机修改器
		///在添加前移除具有相同标识符的旧修改器
		///</summary>
		///<param name="modifier">要添加的相机修改器</param>
		public void Add(ICameraModifier modifier)
		{
			RemoveIdenticalModifiers(modifier);
			modifiers.Add(modifier);
		}
		private void RemoveIdenticalModifiers(ICameraModifier modifier)//移除相同标识符的修改器，避免同一效果重复叠加
		{
			string uniqueIdentity = modifier.UniqueIdentity;
			if (uniqueIdentity == null) return;
			for (int i = modifiers.Count - 1; i >= 0; i--)
				if (modifiers[i].UniqueIdentity == uniqueIdentity)
					modifiers.RemoveAt(i);
		}
		public void ApplyTo(ref Vector2 cameraPosition)//应用所有修改器到相机位置，按顺序更新所有活动的相机修改器
		{
			CameraInfo cameraInfo = new CameraInfo(cameraPosition);
			ClearFinishedModifiers();
			for (int i = 0; i < modifiers.Count; i++)
				modifiers[i].Update(ref cameraInfo);
			cameraPosition = cameraInfo.CameraPosition;
		}
		private void ClearFinishedModifiers()//清除已完成的修改器，遍历所有修改器，移除已完成的效果，对于RestrictCameraModifier，重置相机位置
		{
			for (int i = modifiers.Count - 1; i >= 0; i--)
				if (modifiers[i].Finished)
				{
					if (modifiers[i] is RestrictCameraModifier)
						(modifiers[i] as RestrictCameraModifier).Reset();
					modifiers.RemoveAt(i);
				}
		}
		public void Clear()//清空所有修改器，移除所有活动的相机效果
		{
			modifiers.Clear();
		}
		private readonly List<ICameraModifier> modifiers = new List<ICameraModifier>();//存储所有相机修改器的列表
	}
}