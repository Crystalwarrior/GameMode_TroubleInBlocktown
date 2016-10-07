$BT::Path = filePath(expandFileName("./description.txt")) @ "/";
exec("./lib/addExtraResource.cs");
exec("./lib/daycycles.cs");
exec("./lib/environment.cs");
exec("./lib/itemprops.cs");
exec("./lib/items.cs");

exec("./src/resources.cs");

exec("./src/items/key.cs");

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

function GameConnection::inDefaultGame(%this)
{
	return isObject($DefaultMiniGame) && %this.miniGame == $DefaultMiniGame;
}