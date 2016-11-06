if (!isObject(BTGameMode_Classic))
{
	new ScriptObject(BTGameMode_Classic)
	{
		class = BTGameMode;
	};
}

function BTGameMode_Classic::onStart(%this)
{
	%this.initRoles();
	%this.assignRoles();
	%this.assignHouses();
	%this.testLoop();
}

function BTGameMode_Classic::onEnd(%this,%msg)
{
	if(isEventPending($defaultMiniGame.resetSchedule))
		return;
	talk(%msg);
	$defaultMiniGame.resetSchedule = $defaultMiniGame.schedule(5000, stopGame);
}

function BTGameMode_Classic::onDay(%this)
{
	$defaultMiniGame.setWeapons(0);
	%this.trialSchedule = %this.schedule((DayCycle.dayLength * 1000) * 0.25, doTrial);
}

function BTGameMode_Classic::doTrial(%this)
{
	talk("OMG ITS TRIAL TIME OK");
}

function BTGameMode_Classic::onNight(%this)
{
	$defaultMiniGame.setWeapons(1);
	talk("go to bed or in 30 seconds you'll be super drowsy!!!");
}

function BTGameMode_Classic::testLoop(%this)
{
	cancel(%this.testLoop);
	bottomPrintAll("\c6ass");
	%this.testLoop = %this.schedule(100, testLoop);
}

function BTGameMode_Classic::initRoles(%this)
{
	%this.roleGroup["Random Town"] = "Innocent" TAB "Innocent";
	%this.roleGroup["Random Mafia"] = "Mafioso" TAB "Mafioso";
	%this.roleGroup["Any"] = "Mafioso" TAB "Innocent";
	//Put a role name or rolegroup in this
	%this.roleList = "Godfather"
			TAB "Random Town"
			TAB "Jailor"
			TAB "Mafioso"
			TAB "Random Town"
			TAB "Random Town"
			TAB "Random Mafia"
			TAB "Random Town"
			TAB "Random Town"
			TAB "Random Town"
			TAB "Random Mafia"
			TAB "Random Town"
			TAB "Random Town"
			TAB "Random Town"
			TAB "Random Town"
			TAB "Random Town";
}

function BTGameMode_Classic::cleanUp(%this)
{
	cancel(%this.trialSchedule);
}

function BTGameMode_Classic::checkLastManStanding(%this)
{
	%miniGame = $defaultMiniGame;
	%alive["Mafia"] = 0;
	%alive["Town"] = 0;
	for (%i = 0; %i < %miniGame.numMembers; %i++)
	{
		%member = %miniGame.member[%i];

		if (isObject(%member.role) && isObject(%member.player))
		{
			%alive[%member.role.alignment]++;
			
		}
	}
	
	if (%alive["Mafia"] > 0 && %alive["Town"] <= 0)
		%this.onEnd("Mafia wins");
	else if (%alive["Mafia"] <= 0 && %alive["Town"] > 0)
		%this.onEnd("Town wins");
	else if (%alive["Mafia"] <= 0 && %alive["Town"] <= 0)
		%this.onEnd("Nobody wins");
}