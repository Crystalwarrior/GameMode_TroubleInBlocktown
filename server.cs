$BT::Path = filePath(expandFileName("./description.txt")) @ "/";
exec("./lib/daycycles.cs");
exec("./lib/environment.cs");

exec("./src/gamemodes.cs");
exec("./src/gamemodes/classic.cs");

exec("./src/roles.cs");
exec("./src/roles/godfather.cs");
exec("./src/roles/innocent.cs");
exec("./src/roles/jailor.cs");
exec("./src/roles/mafia.cs");

exec("./src/commands.cs");
exec("./src/corpses.cs");
exec("./src/game.cs");