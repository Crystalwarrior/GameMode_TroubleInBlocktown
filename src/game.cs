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
	%this.respawnTime = -1000;
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
	%this.respawnTime = 3;
}

function MiniGameSO::initDayCycle(%this, %day)
{
	if(%day $= "")
		%day = 1;

	setEnvironment("DayCycleEnabled", 1);
	setEnvironment("sunFlareTopIdx", findSunFlareIndex("base/lighting/corona2.png"));
	setEnvironment("sunFlareBottomIdx", findSunFlareIndex("base/lighting/lightFalloffMono.png"));
	$EnvGuiServer::SunFlareSize = 1;
	%this.dayPhase = 0; //1 = day, 2 = night, 3 = day, etc.
	%this.moonPhase = -1;
	%this.DoDayCycle(%day);
}

function MiniGameSO::DoDayCycle(%this, %day)
{
	cancel(%this.daySchedule);
	if(%day)
	{
		setEnvironment("sunFlareTopIdx", findSunFlareIndex("base/lighting/corona2.png"));
		setEnvironment("sunFlareBottomIdx", findSunFlareIndex("base/lighting/lightFalloffMono.png"));
		SunLight.flareSize = $EnvGuiServer::SunFlareSize;
		SunLight.sendUpdate();
		$EnvGuiServer::DayCycleFile = "Add-Ons/DayCycle_BlocktownTrials/day.daycycle";
		loadDayCycle($EnvGuiServer::DayCycleFile);
		$EnvGuiServer::DayLength = 300 * 0.1;
		DayCycle.setDayLength($EnvGuiServer::DayLength);
		setDayCycleTime(0.95);
	}
	else
	{
		%this.moonPhase = (%this.moonPhase + 1) % 7;
		%bottom[0]	= "Add-Ons/SunFlare_MoonPhases/moon_crescent_wan.png";
		%top[0]		= "Add-Ons/SunFlare_Blank/blank.png";
		%bottom[1]	= "Add-Ons/SunFlare_MoonPhases/moon_quarter_wan.png";
		%top[1]		= "Add-Ons/SunFlare_Blank/blank.png";
		%bottom[2]	= "Add-Ons/SunFlare_MoonPhases/moon_gibbous_wan.png";
		%top[2]		= "Add-Ons/SunFlare_MoonPhases/moon_glow.png";
		%bottom[3]	= "Add-Ons/SunFlare_MoonPhases/moon_full.png";
		%top[3]		= "Add-Ons/SunFlare_MoonPhases/moon_glow.png";
		%bottom[4]	= "Add-Ons/SunFlare_MoonPhases/moon_gibbous_wax.png";
		%top[4]		= "Add-Ons/SunFlare_MoonPhases/moon_glow.png";
		%bottom[5]	= "Add-Ons/SunFlare_MoonPhases/moon_quarter_wax.png";
		%top[5]		= "Add-Ons/SunFlare_Blank/blank.png";
		%bottom[6]	= "Add-Ons/SunFlare_MoonPhases/moon_crescent_wax.png";
		%top[6]		= "Add-Ons/SunFlare_Blank/blank.png";
		%top = findSunFlareIndex(%top[%this.moonPhase]);
		%bottom = findSunFlareIndex(%bottom[%this.moonPhase]);
		setEnvironment("sunFlareTopIdx", %top);
		setEnvironment("sunFlareBottomIdx", %bottom);
		SunLight.flareSize = $EnvGuiServer::SunFlareSize * 0.5;
		SunLight.sendUpdate();
		$EnvGuiServer::DayCycleFile = "Add-Ons/DayCycle_BlocktownTrials/night.daycycle";
		loadDayCycle($EnvGuiServer::DayCycleFile);
		$EnvGuiServer::DayLength = 240 * 0.1;
		DayCycle.setDayLength($EnvGuiServer::DayLength);
		setDayCycleTime(0.95);		
	}
	%this.dayPhase++;
	talk("dayphase" SPC %this.dayPhase);
	%this.daySchedule = %this.schedule((DayCycle.dayLength * 1000) * 0.7, DoDayCycle, !%day);
}

//helper funcs
function MiniGameSO::isNight(%this)
{
	return !(%this.dayPhase % 2); //inverted so it's 0 for day and 1 for night
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