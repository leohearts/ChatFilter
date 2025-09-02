using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ChatFilter
{
	public class ChatFilter : Mod
	{
		public override void Load()
		{
			On_RemadeChatMonitor.AddNewMessage += RemadeChatMonitor_AddNewMessage;
		}

		private static void RemadeChatMonitor_AddNewMessage(On_RemadeChatMonitor.orig_AddNewMessage orig, RemadeChatMonitor self, string text, Color color, int widthLimitInPixels)
		{
			orig(self, text, color, widthLimitInPixels);

			ModifyLastChatMessage();
		}

		private static void ModifyLastChatMessage()
		{
			if (Main.gameMenu || !Config.Instance.ChatFilterEnabled) return;

			if (Main.chatMonitor is not RemadeChatMonitor chat)
				return;

			//All classes public
			//RemadeChatMonitor has private List<ChatMessageContainer> _messages;
			//ChatMessageContainer has private List<TextSnippet[]> _parsedText;
			//TextSnippet has public string Text;
			FieldInfo messagesField = typeof(RemadeChatMonitor).GetField("_messages", BindingFlags.Instance | BindingFlags.NonPublic);
			List<ChatMessageContainer> messages = messagesField.GetValue(chat) as List<ChatMessageContainer>;

			FieldInfo parsedTextField = typeof(ChatMessageContainer).GetField("_parsedText", BindingFlags.Instance | BindingFlags.NonPublic);

			var lastMessage = messages[0];

			List<TextSnippet[]> parsedText = parsedTextField.GetValue(lastMessage) as List<TextSnippet[]>;

			if (parsedText.Count <= 0)
				return;


			foreach (string word in Config.Instance.BlockedWords)
			{
				// Use Regex.Replace to replace all occurrences of the word
				// Regex.Escape is used to treat the word as a literal string, not a regex pattern
				if (Config.Instance.BlockEntireMessage && Regex.IsMatch(lastMessage.OriginalText, word))
				{
					lastMessage.OriginalText = "";
					foreach (var snippets in parsedText)
					{
						foreach (var snippet in snippets)
						{
							snippet.Text = "";
						}
					}
				}
				else
				{
					lastMessage.OriginalText = Regex.Replace(lastMessage.OriginalText, word, new string('*', word.Length));
					foreach (var snippets in parsedText)
					{
						foreach (var snippet in snippets)
						{
							snippet.Text = Regex.Replace(snippet.Text, word, new string('*', word.Length));
						}
					}
				}

			}
			

			// var snippet = parsedText[0];

			// //OriginalText because vanilla recalculates parsedText on window resize based on OriginalText
			// var textOriginal = lastMessage.OriginalText;

			// if (textOriginal.StartsWith(name))
			// 	return;

			// if (snippet[0].Text.StartsWith(name))
			// 	return;

			// var newSnippet = new TextSnippet(name);
			// //newSnippet.Color = snippet[0].Color; //Keep it white
			// var snippetList = new List<TextSnippet>(snippet);
			// snippetList.Insert(0, newSnippet);
			// parsedText[0] = snippetList.ToArray();

			// lastMessage.OriginalText = name + textOriginal;
		}

	}
}
