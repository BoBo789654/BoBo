using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace BoBo.Content.UIs.Dialogue
{
	// 对话列表类，定义对话数据的结构
	public class DialogueList(string[] texts, string picture, string dialogbox, float dw, float dh, float pw, float ph, string name)
	{
		public string[] Texts = texts;          //对话文本
		public string PictureTex = picture;     //人物图片纹理路径
		public string DialogBox = dialogbox;    //对话框纹理路径
		public float DialogWidth = dw;          //对话框宽
		public float DialogHeight = dh;         //对话框高
		public float PictureWidth = pw;         //人物图片宽
		public float PictureHeight = ph;        //人物图片高
		public string Name = name;              //人物名字

		public Texture2D GetPictureTexture()
		{
			return ModContent.Request<Texture2D>(PictureTex).Value; //获取人物的图片纹理
		}

		public Texture2D GetDialogTexture()
		{
			return ModContent.Request<Texture2D>(DialogBox).Value; //获取对话框纹理
		}
	}

	//对话数据静态类，存放所有对话的目录
	public static class DialogueData
	{
		//对话目录哈希表
		public static readonly Dictionary<int, DialogueList> Catalog = new()
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
	}
}