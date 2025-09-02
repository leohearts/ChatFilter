using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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
			if (Main.gameMenu) return;

			string name = GetCallingName(true);
			if (name == string.Empty)
				return;

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

			var snippet = parsedText[0];

			//OriginalText because vanilla recalculates parsedText on window resize based on OriginalText
			var textOriginal = lastMessage.OriginalText;

			if (textOriginal.StartsWith(name))
				return;

			if (snippet[0].Text.StartsWith(name))
				return;

			var newSnippet = new TextSnippet(name);
			//newSnippet.Color = snippet[0].Color; //Keep it white
			var snippetList = new List<TextSnippet>(snippet);
			snippetList.Insert(0, newSnippet);
			parsedText[0] = snippetList.ToArray();

			lastMessage.OriginalText = name + textOriginal;
		}

		private static string GetCallingName(bool whitespace = false)
		{
			string name = string.Empty;
			if (!Config.Instance.ChatFilterEnabled)
				return string.Empty;

			StackFrame[] frames/* = new StackFrame[1]*/;

			//Suppress OutOfBoundsRead from appearing entirely, may invalidate activelyModdingField shenanigans
			/*
            [19:03:00] [1/WARN] [tML]: Silently Caught Exception: 
            System.BadImageFormatException: OutOfBoundsRead
               at System.Reflection.Throw.OutOfBounds()
               at System.Reflection.Metadata.Ecma335.MethodDebugInformationTableReader.GetSequencePoints(MethodDebugInformationHandle handle)
               at System.Diagnostics.StackTraceSymbols.GetSourceLineInfoWithoutCasAssert(String assemblyPath, IntPtr loadedPeAddress, Int32 loadedPeSize, IntPtr inMemoryPdbAddress, Int32 inMemoryPdbSize, Int32 methodToken, Int32 ilOffset, String& sourceFile, Int32& sourceLine, Int32& sourceColumn)
               at System.Diagnostics.StackFrameHelper.InitializeSourceInfo(Int32 iSkip, Boolean fNeedFileInfo, Exception exception)
               at System.Diagnostics.StackTrace.CaptureStackTrace(Int32 iSkip, Boolean fNeedFileInfo, Thread targetThread, Exception e)
               at ChatFilter.ChatFilter.GetCallingName(Boolean whitespace) in ChatFilter.cs:line 74
               at ChatFilter.ChatFilter.ModifyLastChatMessage() in ChatFilter.cs:line 51
               at ChatFilter.ChatFilter.Main_NewText_string_byte_byte_byte_bool(orig_NewText_string_byte_byte_byte_bool orig, String newText, Byte R, Byte G, Byte B, Boolean force) in ChatFilter.cs:line 47
               at DMD<DMD<Hook<Terraria.Main::NewText>?34669516>?51491948::Hook<Terraria.Main::NewText>?34669516>(String , Byte , Byte , Byte , Boolean )
               at DMD<Terraria.Main::NewText>(String newText, Byte R, Byte G, Byte B, Boolean force)
               at DMD<DMD<Trampoline<Terraria.Main::NewText>?39771549>?45271378::Trampoline<Terraria.Main::NewText>?39771549>(String , Byte , Byte , Byte , Boolean )
               at ChatFilter.ChatFilter.Main_NewText_string_byte_byte_byte_bool(orig_NewText_string_byte_byte_byte_bool orig, String newText, Byte R, Byte G, Byte B, Boolean force) in ChatFilter.cs:line 46
               at DMD<DMD<Hook<Terraria.Main::NewText>?34669516>?51491948::Hook<Terraria.Main::NewText>?34669516>(String , Byte , Byte , Byte , Boolean )
               at AlchemistNPC.AlchemistNPCPlayer.OnEnterWorld(Player player) in AlchemistNPCPlayer.cs:line 499
            */
			using var _ = new Logging.QuietExceptionHandle();
			try
			{
				frames = new StackTrace(true).GetFrames();
				Logging.PrettifyStackTraceSources(frames);
				//We want to find the call after the first found, last NewText or AddNewMessage
				int index;
				bool correctSequenceFound = false;
				for (index = 0; index < frames.Length; index++)
				{
					var method = frames[index].GetMethod();
					var methodName = method.Name;
					if (methodName.Contains("NewText") || methodName.Contains("AddNewMessage"))
					{
						correctSequenceFound = true;
					}
					else if (correctSequenceFound)
					{
						break; //Done
					}
				}

				if (index == frames.Length)
				{
					name = string.Empty;
				}
				else
				{
					var frame = frames[index];
					var method = frame.GetMethod();

					Type declaringType = method.DeclaringType;
					if (declaringType != null && declaringType.Namespace != null)
					{
						name = declaringType.Namespace.Split('.')[0];
					}
					else
					{
						//Autogenerated methods (i.e. MonoMod detours) do not have a declaring type
						//Dynamically compiled invocations (i.e. Modders Toolkit REPL) do not have namespaces
						name = "Terraria";
					}

					if (name != "Terraria")
					{
						if (Config.Instance.ShowDisplayName)
						{
							if (ModLoader.TryGetMod(name, out Mod mod))
							{
								name = mod.DisplayNameClean;
							}
							else
							{
								if (ModLoader.TryGetMod(name + "Mod", out Mod mod2))
								{
									name = mod2.DisplayNameClean;
								}
							}
						}
					}
					else if (!Config.Instance.DisplayTerrariaSource)
					{
						name = string.Empty;
					}
				}
			}
			catch
			{
				//var logger = ModContent.GetInstance<ChatFilter>().Logger;
				//logger.Info("#####");
				//foreach (var frame in frames)
				//{
				//    logger.Info(frame?.ToString() ?? "frame null");
				//}
				//logger.Info("#####");
			}
			if (string.IsNullOrEmpty(name))
				return string.Empty;

			return $"[{name}]" + (whitespace ? " " : "");
		}
	}
}
