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
		private bool Visible;					//是否显示对话界面
		private float Alpha;					//透明度
		private int Click;						//当前点击次数
		private bool Closing;					//是否正在关闭对话
		private bool CanMove;					//玩家是否可以移动
		private Rectangle DialogArea;			//对话框点击区域
		private DialogueList CurrentDialogue;	//当前显示的对话数据

		private class DialogueList(string[] texts, string picture, string dialogbox, float dw, float dh, float pw, float ph, string name)
		{
			public string[] Texts = texts;				//对话文本
			public string PictureTex = picture;		    //人物图片纹理路径
			public string DialogBox = dialogbox;	    //对话框纹理路径
			public float DialogWidth = dw;				//对话框宽
			public float DialogHeight = dh;				//对话框高
			public float PictureWidth = pw;				//人物图片宽
			public float PictureHeight = ph;			//人物图片高
			public string Name = name;					//人物名字

			public Texture2D GetPictureTexture()         
			{
				return ModContent.Request<Texture2D>(PictureTex).Value;//获取人物的图片纹理
			}

			public Texture2D GetDialogTexture()         
			{
				return ModContent.Request<Texture2D>(DialogBox).Value;//获取对话框纹理
			}
		}

		private static readonly Dictionary<int, DialogueList> DialogueCatalog = new() //对话目录哈希表
        {
			{
				1, new DialogueList(
					texts: new[] { "测试对话1", "测试对话2" },
					picture: "BoBo/Asset/UIs/Dialogue/PictureTex/PictureTex",
					dialogbox: "BoBo/Asset/UIs/Dialogue/DialogueBox/DialogueBox",
					440, 190, 482, 690, "测试角色A")
			},
			{
				2, new DialogueList(
					texts: new[] { "测试对话1", "测试对话2", "测试对话3" },
					picture: "BoBo/Asset/UIs/Dialogue/PictureTex/PictureTex",
					dialogbox: "BoBo/Asset/UIs/Dialogue/DialogueBox/DialogueBox",
					440, 190, 181, 220, "测试角色B")
			},
			{
				3, new DialogueList(
					texts: new[] { "测试对话1", "测试对话2", "测试对话3", "测试对话4" },
					picture: "BoBo/Asset/UIs/Dialogue/PictureTex/PictureTex",
					dialogbox: "BoBo/Asset/UIs/Dialogue/DialogueBox/DialogueBox",
					440, 190, 482, 690, "测试角色C")
			}
		};
		public static DialogueUI instance;
		public DialogueUI()
		{
			instance = this;//设置静态实例引用
		}
		public override void Update(GameTime gameTime)
		{
			Player player = Main.LocalPlayer;
			if (Visible)
			{
				Alpha = Math.Min(Alpha + 0.05f, 1f);//淡入
				if (!CanMove)//对话期间禁止移动
				{
					//保存对话前的控制状态（只在对话开始时保存一次）
					if (Alpha <= 0.1f)
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
					//禁止所有移动控制
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
			}
			else
			{
				Alpha = Math.Max(Alpha - 0.05f, 0f);//淡出
				if (Alpha <= 0.1f && !CanMove)//对话结束，恢复移动控制
				{
					CanMove = true;//恢复对话前的控制状态
					player.controlLeft = false;
					player.controlRight = false;
					player.controlUp = false;
					player.controlDown = false;
					player.controlJump = false;
					player.controlUseItem = false;
					player.controlMount = false;
					player.controlHook = false;
				}
			}
			base.Update(gameTime);
		}
		public override void Draw(SpriteBatch spriteBatch)
		{
			if (CurrentDialogue == null || Alpha <= 0f)//无当前对话或完全透明时不绘制
				return;
			
			float DialogWidth = CurrentDialogue.DialogWidth;//对话框尺寸宽
			float DialogHeight = CurrentDialogue.DialogHeight;//对话框尺寸高

			#region 图片位置
			Texture2D PictureTex = CurrentDialogue.GetPictureTexture();//使用哈希缓存获取纹理
			Rectangle Picture = PictureTex.Frame();//纹理区域
			Vector2 PictureOrigin = Picture.Size() / 2f;//原点为中心点
			float PictureScale = 0.8f / Main.UIScale;//UI缩放调整比例
			Vector2 PicturePos = new Vector2(Main.screenWidth / 2 - 280 / Main.UIScale, Main.screenHeight / 2 + 200 / Main.UIScale);//图片位置
			spriteBatch.Draw(PictureTex, PicturePos, Picture, Color.White * Alpha, 0f, PictureOrigin, PictureScale, SpriteEffects.None, 1f);//人物图片
			#endregion

			#region 对话框位置
			Texture2D DialogTex = CurrentDialogue.GetDialogTexture();//使用哈希缓存获取纹理
			Rectangle Dialogue = DialogTex.Frame();//同上，这是对话框的
			Vector2 DialogOrigin = Dialogue.Size() / 2f;//同上，这是对话框的
			float DialogScale = 0.8f / Main.UIScale;//同上，这是对话框的
			Vector2 DialogPos = PicturePos + new Vector2(430 / Main.UIScale, 30 / Main.UIScale);//同上，这是对话框的
			spriteBatch.Draw(DialogTex, DialogPos, Dialogue, Color.White * Alpha, 0f, DialogOrigin, DialogScale, SpriteEffects.None, 0f);//同上，这是对话框的
			#endregion

			#region 名字位置
			string Name = CurrentDialogue.Name;
			Vector2 NamePos = DialogPos + new Vector2(-220 / Main.UIScale, -95 / Main.UIScale);//名字位置
			Utils.DrawBorderString(spriteBatch, Name, NamePos, Color.Red * Alpha, 1.0f / Main.UIScale, anchorx: 0.5f, anchory: 0.5f);//绘制名字
			#endregion

			#region 文字位置
			if (Click < CurrentDialogue.Texts.Length)//检查当前点击是否在文本数组范围内
			{
				string currentText = CurrentDialogue.Texts[Click];//获取文本
				Vector2 TextPos = DialogPos;//文本位置对齐对话框中心
				Utils.DrawBorderString(spriteBatch, currentText, TextPos, Color.Orange * Alpha, 1f / Main.UIScale, anchorx: 0.5f, anchory: 0.5f);//绘制对话文本
			}
			#endregion

			#region 点击区域
			DialogArea = new Rectangle((int)DialogPos.X - (int)DialogWidth / 2, (int)DialogPos.Y - (int)DialogHeight / 2, (int)DialogWidth, (int)DialogHeight);

			if (DialogArea.Contains(Main.MouseScreen.ToPoint()) && Main.mouseRight && Main.mouseRightRelease)//检测鼠标是否在对话框区域内且右键点击
			{
				Main.mouseRightRelease = false;//右键点击
				Main.LocalPlayer.mouseInterface = true;//标记鼠标交互

				if (Click < CurrentDialogue.Texts.Length - 1)//还有更多，则切换到下一句
				{
					Click++;
				}
				else//直到最后一句，关闭
				{
					Closing = true;
					if (Closing)
					{
						Visible = false;//隐藏界面
					}
					if (Alpha <= 0)//当完全透明时重置对话状态
					{
						ResetDialogue();
						CanMove = true;//恢复玩家移动
					}
				}
			}
			#endregion
		}

		private void ResetDialogue()
		{
			CurrentDialogue = null;					//清除当前对话
			CanMove = true;							//允许玩家移动
			Click = 0;								//重置对话进度
			Closing = false;                        //重置关闭状态
			Player player = Main.LocalPlayer;
			//恢复玩家控制状态
			player.controlLeft = true;
			player.controlRight = true;
			player.controlUp = true;
			player.controlDown = true;
			player.controlJump = true;
			player.controlUseItem = true;
			player.controlMount = true;
			player.controlHook = true;
		}

		public void StartDialogue(int dialogueID)//开始显示指定ID的对话
		{
			if (DialogueCatalog.TryGetValue(dialogueID, out var dialogue))
			{
				CurrentDialogue = dialogue;			//设置当前对话
				Click = 0;							//重置为第一句
				Visible = true;						//显示界面（触发淡入效果）
				Closing = false;					//重置关闭状态
				CanMove = false;					//禁止玩家移动
				Alpha = 0f;							//初始透明度为0（开始淡入）
			}
		}
		public bool GetCanMoveState()
		{
			return CanMove;
		}
		public  void EndDialogue()//强制结束对话的方法
		{
			Visible = false;
			ResetDialogue();
			Alpha = 0f;
		}
		public void Unload()//在Mod卸载时调用
		{
			DialogueCatalog.Clear();
			CurrentDialogue = null;
		}
	}
	public class UISystem : ModSystem//对话UI的加载、更新和绘制
	{
		public static UserInterface DialogueInterface;//用户界面实例
		public static DialogueUI DialogueInstance;//对话UI实例
		public override void Load()//Mod加载时初始化UI系统
		{
			DialogueInstance = new DialogueUI();			//创建对话UI实例
			DialogueInterface = new UserInterface();		//创建用户界面
			DialogueInterface.SetState(DialogueInstance);	//设置当前UI状态
		}
		public override void Unload()//Mod卸载时清理资源
		{
			DialogueUI.instance?.Unload();							//调用对话UI的清理方法
			DialogueInterface?.SetState(null);				//清除UI状态
			DialogueInstance = null;						//释放实例引用
			DialogueInterface = null;						//释放接口引用
		}
		public override void UpdateUI(GameTime gameTime)//每帧更新UI逻辑
		{
			DialogueInterface?.Update(gameTime);//更新对话界面状态     
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)//将对话UI插入到原版界面层中
		{
			int mouseTextIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");//查找原版的鼠标文本层（想要在其之前插入对话层）
			if (mouseTextIndex != -1)
			{
				layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("BoBo: Dialogue",//创建新的对话层
					delegate {
						if (DialogueInterface?.CurrentState != null)//确保有UI状态
							DialogueInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)//使用UI缩放模式
				);
			}
		}
	}
	public class DialoguePlayer : ModPlayer
	{
		public override void PreUpdate()
		{
			if (!Main.gameMenu && UISystem.DialogueInstance != null && !UISystem.DialogueInstance.GetCanMoveState())//只有在游戏内且对话中禁止移动时，才禁用控制
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