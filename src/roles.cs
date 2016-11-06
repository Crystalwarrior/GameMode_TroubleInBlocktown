if (!isObject(BTRoleGroup))
{
	new ScriptGroup(BTRoleGroup);
}

function BTRoleGroup::getRole(%this, %name)
{
	for (%i = 0; %i < BTRoleGroup.getCount(); %i++)
	{
		%role = BTRoleGroup.getObject(%i);
		if (%role.name $= %name)
			return %role;
	}
	return "";
}

function BTRole::onAdd(%this)
{
	BTRoleGroup.add(%this);
}

function BTRole::greetRole(%this, %client)
{
	if (!isObject(%client))
		return;

	messageClient(%client, '', "\c6You are <color:" @ %client.role.color @ ">" @ %client.role.name @ "\c6! You are aligned with \c3"@ %client.role.alignment);
	if (%client.role.showTeammates)
	{
		%miniGame = $defaultMiniGame;
		%count = %miniGame.numMembers;

		messageClient(%client, '', "\c4Your fellow teammates are:");
		%a = -1;
		for (%i = 0; %i < %count; %i++)
		{
			%member = %miniGame.member[%i];
			if (!isObject(%member.role))
				continue;

			if (%member.role.alignment $= %client.role.alignment)
			{
				messageClient(%client, '', "\c2" SPC %member.getPlayerName());
			}
		}
	}
}

function BTRole::onAssigned(%this, %client)
{
	//boop
}