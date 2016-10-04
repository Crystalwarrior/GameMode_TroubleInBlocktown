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
	%this.initDayCycle(1);
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
	%this.disableDayCycle();
	%this.reset(0);
}

function MiniGameSO::initDayCycle(%this, %day)
{
	if(%day $= "")
		%day = 1;

	setEnvironment("DayCycleEnabled", 1);
	$EnvGuiServer::SunFlareTopTexture = "base/lighting/corona2.png";
	$EnvGuiServer::SunFlareBottomTexture = "base/lighting/lightFalloffMono.png";
	$EnvGuiServer::SunFlareSize = 1;
	%this.DoDayCycle(%day);
}

function MiniGameSO::DoDayCycle(%this, %day)
{
	cancel(%this.daySchedule);
	if(%day)
	{
		SunLight.setFlareBitmaps($EnvGuiServer::SunFlareTopTexture, $EnvGuiServer::SunFlareBottomTexture);
		SunLight.flareSize = $EnvGuiServer::SunFlareSize;
		SunLight.sendUpdate();
		$EnvGuiServer::DayCycleFile = "Add-Ons/DayCycle_BlocktownTrials/day.daycycle";
		loadDayCycle($EnvGuiServer::DayCycleFile);
		$EnvGuiServer::DayLength = 300;
		DayCycle.setDayLength($EnvGuiServer::DayLength);
		setDayCycleTime(0.95);
	}
	else
	{
		%top = "Add-Ons/SunFlare_MoonPhases/moon_glow.png";
		%bottom = "Add-Ons/SunFlare_MoonPhases/moon_full.png";
		SunLight.setFlareBitmaps(%top, %bottom);
		SunLight.flareSize = $EnvGuiServer::SunFlareSize * 0.5;
		SunLight.sendUpdate();
		$EnvGuiServer::DayCycleFile = "Add-Ons/DayCycle_BlocktownTrials/night.daycycle";
		loadDayCycle($EnvGuiServer::DayCycleFile);
		$EnvGuiServer::DayLength = 240;
		DayCycle.setDayLength($EnvGuiServer::DayLength);
		setDayCycleTime(0.95);		
	}
	%this.daySchedule = %this.schedule((DayCycle.dayLength * 1000) * 0.7, DoDayCycle, !%day);
}

function MiniGameSO::disableDayCycle(%this)
{
	cancel(%this.daySchedule);
	setEnvironment("DayCycleEnabled", 0);
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
		cancel(%this.resetSchedule);
		parent::reset(%this, %client);
	}
};
activatePackage(BT_Main);