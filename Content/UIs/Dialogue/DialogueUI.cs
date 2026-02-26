using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace BoBo.Content.UIs.Dialogue
{
	public class DialogueUI : UIState
	{
		private enum UIState
		{
			Hidden,                                 //初始隐藏
			Entering,                               //正在进入（从下方移入）
			Showing,                                //正在显示（对话进行中）
			Exiting                                 //正在退出（从当前位置移出）
		}

		private UIState CurrentState = UIState.Hidden;
		private float Alpha;                        //透明度
		private int Click;                          //当前点击次数
		private Rectangle DialogArea;               //对话框点击区域
		private DialogueList CurrentDialogue;       //当前显示的对话数据
		private float PictureOffsetY;               //人物框的Y轴偏移（移动较慢）
		private float DialogOffsetY;                //对话框的Y轴偏移（移动较快）
		public static DialogueUI instance;

		public DialogueUI()
		{
			instance = this; //设置静态实例引用
		}

		public override void Update(GameTime gameTime)
		{
			Player player = Main.LocalPlayer;
			float PictureMoveSpeed = 0.10f;  //人物框移动速度（较慢）
			float DialogMoveSpeed = 0.18f;   //对话框移动速度（较快）

			switch (CurrentState)
			{
				case UIState.Entering:
					//淡入并上移
					Alpha = Math.Min(Alpha + 0.05f, 1f);

					//人物框以较慢速度移动到目标位置
					PictureOffsetY = MathHelper.Lerp(PictureOffsetY, 0f, PictureMoveSpeed);

					//对话框以较快速度移动到目标位置
					DialogOffsetY = MathHelper.Lerp(DialogOffsetY, 0f, DialogMoveSpeed);

					//当到达目标位置且完全透明时，切换到显示状态
					if (Alpha >= 0.95f && Math.Abs(PictureOffsetY) < 5f && Math.Abs(DialogOffsetY) < 5f)
					{
						CurrentState = UIState.Showing;
						//确保位置准确归零
						PictureOffsetY = 0f;
						DialogOffsetY = 0f;
					}
					DisablePlayerControls(player);
					break;

				case UIState.Showing:
					//保持完全可见状态
					Alpha = 1f;
					PictureOffsetY = 0f;
					DialogOffsetY = 0f;
					DisablePlayerControls(player);
					break;

				case UIState.Exiting:
					//淡出并下移
					Alpha = Math.Max(Alpha - 0.05f, 0f);

					//人物框以较慢速度移动到屏幕外
					PictureOffsetY = MathHelper.Lerp(PictureOffsetY, 300f, PictureMoveSpeed);

					//对话框以较快速度移动到屏幕外
					DialogOffsetY = MathHelper.Lerp(DialogOffsetY, 300f, DialogMoveSpeed);

					//当完全透明且都移到屏幕外时，切换到隐藏状态
					if (Alpha <= 0.05f && PictureOffsetY >= 290f && DialogOffsetY >= 290f)
					{
						CurrentState = UIState.Hidden;
						ResetDialogue();
					}
					DisablePlayerControls(player);
					break;

				case UIState.Hidden:
					Alpha = 0f;
					EnablePlayerControls(player);
					break;
			}
			base.Update(gameTime);
		}

		private void DisablePlayerControls(Player player)
		{
			player.controlLeft = false;
			player.controlRight = false;
			player.controlUp = false;
			player.controlDown = false;
			player.controlJump = false;
			player.controlUseItem = false;
			player.controlMount = false;
			player.controlHook = false;
			player.controlTorch = false;
			player.controlSmart = false;
		}

		private void EnablePlayerControls(Player player)
		{
			player.controlLeft = true;
			player.controlRight = true;
			player.controlUp = true;
			player.controlDown = true;
			player.controlJump = true;
			player.controlUseItem = true;
			player.controlMount = true;
			player.controlHook = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (CurrentDialogue == null || Alpha <= 0f) //无当前对话或完全透明时不绘制
				return;
			float DialogWidth = CurrentDialogue.DialogWidth; //对话框尺寸宽
			float DialogHeight = CurrentDialogue.DialogHeight; //对话框尺寸高

			#region 图片位置
			Texture2D PictureTex = CurrentDialogue.GetPictureTexture(); //使用哈希缓存获取纹理
			Rectangle Picture = PictureTex.Frame(); //纹理区域
			Vector2 PictureOrigin = Picture.Size() / 2f; //原点为中心点
			float PictureScale = 0.8f / Main.UIScale; //UI缩放调整比例

			//图片位置（使用PictureOffsetY，移动较慢）
			Vector2 PicturePos = new Vector2(Main.screenWidth / 2 - 280 / Main.UIScale,
											Main.screenHeight / 2 + 200 / Main.UIScale + PictureOffsetY);
			spriteBatch.Draw(PictureTex, PicturePos, Picture, Color.White * Alpha, 0f, PictureOrigin, PictureScale, SpriteEffects.None, 1f);
			#endregion

			#region 对话框位置
			Texture2D DialogTex = CurrentDialogue.GetDialogTexture(); //使用哈希缓存获取纹理
			Rectangle Dialogue = DialogTex.Frame(); //同上，这是对话框的
			Vector2 DialogOrigin = Dialogue.Size() / 2f; //同上，这是对话框的
			float DialogScale = 0.8f / Main.UIScale; //同上，这是对话框的

			//对话框位置（使用DialogOffsetY，移动较快，相对于图片位置的偏移保持不变）
			Vector2 DialogPos = new Vector2(PicturePos.X + 430 / Main.UIScale,
											PicturePos.Y - PictureOffsetY + DialogOffsetY + 30 / Main.UIScale);
			spriteBatch.Draw(DialogTex, DialogPos, Dialogue, Color.White * Alpha, 0f, DialogOrigin, DialogScale, SpriteEffects.None, 0f);
			#endregion

			#region 名字位置
			string Name = CurrentDialogue.Name;
			Vector2 NamePos = DialogPos + new Vector2(-220 / Main.UIScale, -95 / Main.UIScale); //名字位置
			Utils.DrawBorderString(spriteBatch, Name, NamePos, Color.Red * Alpha, 1.0f / Main.UIScale, anchorx: 0.5f, anchory: 0.5f); //绘制名字
			#endregion

			#region 文字位置
			if (Click < CurrentDialogue.Texts.Length && CurrentState == UIState.Showing) //只在显示状态下绘制文本
			{
				string currentText = CurrentDialogue.Texts[Click]; //获取文本
				Vector2 TextPos = DialogPos; //文本位置对齐对话框中心
				Utils.DrawBorderString(spriteBatch, currentText, TextPos, Color.Orange * Alpha, 1f / Main.UIScale, anchorx: 0.5f, anchory: 0.5f); //绘制对话文本
			}
			#endregion

			#region 点击区域
			if (CurrentState == UIState.Showing) //只在显示状态下处理点击
			{
				DialogArea = new Rectangle((int)DialogPos.X - (int)DialogWidth / 2, (int)DialogPos.Y - (int)DialogHeight / 2, (int)DialogWidth, (int)DialogHeight);

				if (DialogArea.Contains(Main.MouseScreen.ToPoint()) && Main.mouseRight && Main.mouseRightRelease) //检测鼠标是否在对话框区域内且右键点击
				{
					Main.mouseRightRelease = false; //右键点击
					Main.LocalPlayer.mouseInterface = true; //标记鼠标交互

					if (Click < CurrentDialogue.Texts.Length - 1) //还有更多，则切换到下一句
					{
						Click++;
					}
					else //直到最后一句，开始退出动画
					{
						CurrentState = UIState.Exiting; //立即切换到退出状态
					}
				}
			}
			#endregion
		}

		private void ResetDialogue()
		{
			CurrentDialogue = null; //清除当前对话
			Click = 0; //重置对话进度
			PictureOffsetY = 300f; //重置人物框偏移到屏幕外
			DialogOffsetY = 300f; //重置对话框偏移到屏幕外

			Player player = Main.LocalPlayer;
			EnablePlayerControls(player);
		}

		public void StartDialogue(int DialogueID) //开始显示指定ID的对话
		{
			if (DialogueData.Catalog.TryGetValue(DialogueID, out var Dialogue))//从 DialogueData.Catalog 中获取数据
			{
				CurrentDialogue = Dialogue; //设置当前对话
				Click = 0; //重置为第一句
				CurrentState = UIState.Entering; //设置为进入状态
				Alpha = 0f; //初始透明度为0（开始淡入）
				PictureOffsetY = 300f; //初始人物框偏移为屏幕外
				DialogOffsetY = 300f; //初始对话框偏移为屏幕外
			}
		}

		public bool GetCanMoveState()
		{
			return CurrentState == UIState.Hidden;
		}

		public void EndDialogue() //强制结束对话的方法
		{
			CurrentState = UIState.Exiting; //直接切换到退出状态
		}

		public void Unload() //在Mod卸载时调用
		{
			CurrentDialogue = null;
		}
	}

	public class UISystem : ModSystem //对话UI的加载、更新和绘制
	{
		public static UserInterface DialogueInterface; //用户界面实例
		public static DialogueUI DialogueInstance; //对话UI实例

		public override void Load() //Mod加载时初始化UI系统
		{
			DialogueInstance = new DialogueUI(); //创建对话UI实例
			DialogueInterface = new UserInterface(); //创建用户界面
			DialogueInterface.SetState(DialogueInstance); //设置当前UI状态
		}

		public override void Unload() //Mod卸载时清理资源
		{
			DialogueUI.instance?.Unload(); //调用对话UI的清理方法
			DialogueInterface?.SetState(null); //清除UI状态
			DialogueInstance = null; //释放实例引用
			DialogueInterface = null; //释放接口引用
		}

		public override void UpdateUI(GameTime gameTime) //每帧更新UI逻辑
		{
			DialogueInterface?.Update(gameTime); //更新对话界面状态
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) //将对话UI插入到原版界面层中
		{
			int mouseTextIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text"); //查找原版的鼠标文本层（想要在其之前插入对话层）
			if (mouseTextIndex != -1)
			{
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("BoBo: Dialogue", //创建新的对话层
					delegate
					{
						if (DialogueInterface?.CurrentState != null) //确保有UI状态
							DialogueInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI) //使用UI缩放模式
				);
			}
		}
	}

	public class DialoguePlayer : ModPlayer
	{
		public override void PreUpdate()
		{
			if (!Main.gameMenu && UISystem.DialogueInstance != null && !UISystem.DialogueInstance.GetCanMoveState()) //只有在游戏内且对话中禁止移动时，才禁用控制
			{
				Player.controlLeft = false;
				Player.controlRight = false;
				Player.controlUp = false;
				Player.controlDown = false;
				Player.controlJump = false;
				Player.controlUseItem = false;
				Player.controlMount = false;
				Player.controlHook = false;
			}
		}
	}
}