# ChatFilter

![Icon](https://raw.githubusercontent.com/direwolf420/ChatFilter/1.4/icon.png)

Terraria Forum link: https://forums.terraria.org/index.php?threads/chat-source-shows-the-source-of-messages-in-the-chat.86574/

For those who need to know which mod writes stuff to the chat!

#### Common situation:

![Situation](https://raw.githubusercontent.com/direwolf420/ChatFilter/1.4/situation.png)

Shows the source of messages in the chat. Clientside, toggleable via config.

#### Examples:

**Before:**

```
"Your world has been blessed with Cobalt!"
"Player got 10 tier 1 Reversivity coins and now has 70 in total."
```

**After:**

```
"Your world has been blessed with Cobalt!"
"[AlchemistNPC] Player got 10 tier 1 Reversivity coins and now has 70 in total."
```

![Fixed](https://raw.githubusercontent.com/direwolf420/ChatFilter/1.4/fixed.png)

**Notes:**
* Some stuff like ore spawn announcements in multiplayer will show no mod associated because those messages are serversided
* Death messages won't have an associated mod
* "[Terraria]" prefix for vanilla messages is by default disabled
* Toggle "Display Name" and "Internal Name" of a mod via config

## Localization
If you wish to contribute translations, visit the [tML wiki page](https://github.com/tModLoader/tModLoader/wiki/Contributing-Localization) on that.
This mod uses `.hjson` files in the `Localization` folder.
Translate things that are in english and commented out (either via `//` or `/* */`, remove the comment markers after translating)

List of localization contributors:
* Russian: **GodHybrid, Alino4kaHvoshch**
* Italian: **Sea Noodle**
* Simplified Chinese: **Cyrilly**
* French: **TheBrutalSkull**
* German: **Lighty**
