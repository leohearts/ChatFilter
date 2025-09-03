using Humanizer;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Timers;
using Terraria;
using Terraria.GameContent.UI.Chat;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.WorldBuilding;

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

			if (Main.chatMonitor is not RemadeChatMonitor chat) return;

			//All classes public
			//RemadeChatMonitor has private List<ChatMessageContainer> _messages;
			//ChatMessageContainer has private List<TextSnippet[]> _parsedText;
			//TextSnippet has public string Text;
			FieldInfo messagesField = typeof(RemadeChatMonitor).GetField("_messages", BindingFlags.Instance | BindingFlags.NonPublic);
			List<ChatMessageContainer> messages = messagesField.GetValue(chat) as List<ChatMessageContainer>;

			FieldInfo parsedTextField = typeof(ChatMessageContainer).GetField("_parsedText", BindingFlags.Instance | BindingFlags.NonPublic);

			ModifyMessage(messages[0], parsedTextField);

			// var lastMessage = messages[0];
		}

		public static void ModifyAllMessages()
		{
			Console.WriteLine("ChatFilter: triggered ModifyAllMessages, possibly on world enter");
			if (Main.gameMenu || !Config.Instance.ChatFilterEnabled) return;

			if (Main.chatMonitor is not RemadeChatMonitor chat) return;



			FieldInfo messagesField = typeof(RemadeChatMonitor).GetField("_messages", BindingFlags.Instance | BindingFlags.NonPublic);
			List<ChatMessageContainer> messages = messagesField.GetValue(chat) as List<ChatMessageContainer>;

			FieldInfo parsedTextField = typeof(ChatMessageContainer).GetField("_parsedText", BindingFlags.Instance | BindingFlags.NonPublic);

			foreach (ChatMessageContainer m in messages)
			{
				ModifyMessage(m, parsedTextField);
			}
		}

		private static void ModifyMessage(ChatMessageContainer lastMessage, FieldInfo parsedTextField)
		{
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
		}

	}

	public class PlayerHooks : ModPlayer
	{
		public override void OnEnterWorld()
		{
			// do it every 500ms for next 2s
			Timer t = new Timer(500);
			t.AutoReset = true;
			t.Elapsed += new ElapsedEventHandler((Object source, ElapsedEventArgs e) =>
			{
				ChatFilter.ModifyAllMessages();
			});
			t.Start();
			Timer stopper = new Timer(2000);
			stopper.AutoReset = false;
			stopper.Elapsed += new ElapsedEventHandler((Object source, ElapsedEventArgs e) =>
			{
				t.Stop();
			});
			stopper.Start();
		}

	}
}
