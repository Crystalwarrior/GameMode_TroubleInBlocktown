serverCmdStartGame(%this)
{
	if(!%this.isAdmin)
		return;

	if(!isObject($defaultMiniGame) || isObject($defaultMiniGame.gameModeObj)) //Gamemode will only be present if the game has already started
		return;

	$defaultMiniGame.startGame();
}

serverCmdStopGame(%this)
{
	if(!%this.isAdmin)
		return;

	if(!isObject($defaultMiniGame) || !isObject($defaultMiniGame.gameModeObj))
		return;

	$defaultMiniGame.stopGame();
}