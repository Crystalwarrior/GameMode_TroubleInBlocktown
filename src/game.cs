//Main game logic and stuff
$BT::GameMode = BTGameMode_Classic;

function MiniGameSO::initGameMode(%this)
{
	%this.gameModeObj = new ScriptObject()
	{
		class = BTGameModeInstance;

		game = %this;
		type = $BT::GameMode;
	};
}

function MiniGameSO::startGame(%this)
{
	// Play nice with the default rate limiting.
	if (getSimTime() - %this.lastResetTime < 5000)
		return;
	talk("Starting game...");
	%this.reset(0);
	%this.initGameMode();
	if (!isObject(%this.gameModeObj))
		talk("Gamemode missing! Failed to start!");
	else
		%this.gameModeObj.call("onStart");
}

function MiniGameSO::stopGame(%this)
{
	// Play nice with the default rate limiting.
	if (getSimTime() - %this.lastResetTime < 5000)
		return;
	talk("Stopping game...");
	%this.gameModeObj.call("cleanUp"); //Incase your gamemode has any special stuff to clean up
	%this.gameModeObj.delete();
	%this.reset(0);
}

package BT_Main
{
	function MiniGameSO::checkLastManStanding(%this)
	{
		if (!isObject(%this.gameModeObj))
			return;
		%this.gameModeObj.call("checkLastManStanding");
	}

	function MiniGameSO::Reset(%this, %client)
	{
		if (%this.owner != 0)
			return Parent::reset(%this, %client);

		// Play nice with the default rate limiting.
		if (getSimTime() - %this.lastResetTime < 5000)
			return;

		if (isObject(CorpseGroup))
			CorpseGroup.deleteAll();

		parent::reset(%this, %client);
	}
};
activatePackage(BT_Main);