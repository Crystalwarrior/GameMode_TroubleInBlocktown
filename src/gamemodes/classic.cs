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
	%this.testLoop();
}

function BTGameMode_Classic::onEnd(%this,%msg)
{
	if(isEventPending($defaultMiniGame.resetSchedule))
		return;
	talk(%msg);
	$defaultMiniGame.resetSchedule = $defaultMiniGame.schedule(5000, stopGame);
}

function BTGameMode_Classic::testLoop(%this)
{
	cancel(%this.testLoop);
	bottomPrintAll("\c6DAYCYCLE: " @ getDayCycleTime());
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
			TAB "Random Town"
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

function BTGameMode_Classic::assignRoles(%this)
{
	%miniGame = $defaultMiniGame;

	//Shuffle list for randomness
	%count = %miniGame.numMembers;
	for (%i = 0; %i < %count; %i++)
		%member[%i] = %miniGame.member[%i];
	for (%i = %miniGame.numMembers - 1; %i >= 0; %i--)
	{
		%j = getRandom(%i);
		%temp = %member[%i];
		%member[%i] = %member[%j];
		%member[%j] = %temp;
	}

	//Parse role list
	%a = getFieldCount(%this.roleList);
	for (%i = 0; %i < %a; %i++)
	{
		%role[%i] = getField(%this.roleList, %i);
	}
	//Assign roles
	for (%i = 0; %i < %count; %i++)
	{
		%member = %member[%i];
		if (isObject(%member.player))
		{
			if (BTRoleGroup.getRole(%role[%i]) !$= "")
				%member.assignRole(BTRoleGroup.getRole(%role[%i]));
			else if (%this.roleGroup[%role[%i]] !$= "")
			{
				%field = %this.roleGroup[%role[%i]];
				%a = getFieldCount(%field);
				%b = getField(%field, getRandom(0, %a-1));
				%member.assignRole(BTRoleGroup.getRole(%b));
			}
			else
				talk("error, failed to assign role");
		}
	}
}

function BTGameMode_Classic::cleanUp(%this)
{
	
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