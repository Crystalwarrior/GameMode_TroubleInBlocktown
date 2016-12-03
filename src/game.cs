//Main game logic and stuff
$BT::GameMode = BTGameMode_Classic;
$BT::DayLength = 300 * 0.5;
$BT::NightLength = 240 * 0.5;

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

gunImage.isWeapon = 1;
function MiniGameSO::SetWeapons(%this, %tog)
{
	%this.noWeapons = !%tog;
	for (%i = 0; %i < %this.numMembers; %i++)
	{
		%member = %this.member[%i];
		%player = %member.player;
		if (!isObject(%player))
			continue;
		if (%tog)
		{
			if(isObject(%player.getMountedImage(0)) || !isObject(%item = %player.tool[%player.currTool]))
				continue;
			%player.mountImage(%item.image);
		}
		else
		{
			if (%player.getMountedImage(0) && %player.getMountedImage(0).isWeapon)
				%player.unMountImage(0);
		}
		fixArmReady(%player);
	}
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
		$EnvGuiServer::DayLength = $BT::DayLength / 0.7; //So during schedule multiplication its an accurate amount of time
		DayCycle.setDayLength($EnvGuiServer::DayLength);
		setDayCycleTime(0.95);
		%this.gameModeObj.call("onDay");
		%this.lastDayCycle = getSimTime();
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
		$EnvGuiServer::DayLength = $BT::NightLength / 0.7; //So during schedule multiplication its an accurate amount of time
		DayCycle.setDayLength($EnvGuiServer::DayLength);
		setDayCycleTime(0.95);	
		%this.gameModeObj.call("onNight");
		%this.lastNightCycle = getSimTime();
	}
	%this.dayPhase++;
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
	%this.lastDayCycle = "";
	%this.lastNightCycle = "";
	setEnvironment("DayCycleEnabled", 0);
}

package BT_Main
{
	function Player::mountImage(%this, %image, %slot, %loaded, %skinTag)
	{
		if (isObject(%this.client) && %this.client.inDefaultGame() && (%image.isWeapon && $DefaultMiniGame.noWeapons))
		{
			fixArmReady(%this);
			return;
		}
		parent::mountImage(%this, %image, %slot, %loaded, %skinTag);
	}

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

		// Close *all* doors
		%count = BrickGroup_888888.getCount();

		for (%i = 0; %i < %count; %i++)
		{
			%brick = BrickGroup_888888.getObject(%i);

			%data = %brick.getDataBlock();
			%name = %brick.getName();
			if (%data.isDoor)
			{
				%brick.lockState = false;
				%brick.doorHits = 0;
				%brick.broken = false;
				%brick.setDataBlock(%brick.isCCW ? %data.closedCCW : %data.closedCW);
				%brick.doorMaxHits = 4;
			}
		}

		if (isObject(CorpseGroup))
			CorpseGroup.deleteAll();
		cancel(%this.resetSchedule);
		parent::reset(%this, %client);
	}
};
activatePackage(BT_Main);