//Main game logic and stuff

function MiniGameSO::initGameMode(%this)
{
	
}

function MiniGameSO::startGame(%this)
{
	// Play nice with the default rate limiting.
	if (getSimTime() - %this.lastResetTime < 5000)
		return;

	%this.reset();
	%this.gameModeObj = %this.initGameMode();
}

function MiniGameSO::stopGame(%this)
{
	// Play nice with the default rate limiting.
	if (getSimTime() - %this.lastResetTime < 5000)
		return;

	%this.gameModeObj.delete();
	%this.reset();
}

package TIB_Main
{
	function MiniGameSO::Reset(%this, %client)
	{
		if (%this.owner != 0)
			return Parent::reset(%this, %client);

		// Play nice with the default rate limiting.
		if (getSimTime() - %this.lastResetTime < 5000)
			return;

		if (isObject(CorpseGroup))
			CorpseGroup.deleteAll();
	}
};
activatePackage(TIB_Main);