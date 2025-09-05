make:
	cd `cat ../tModLoader.targets | grep -E -o '".+tModLoader.+"' | xargs echo | sed 's#tMLMod.targets##'`;pwd;dotnet tModLoader.dll -build $(CURDIR);
