if (!isObject(BTGamemodeGroup))
{
	new ScriptGroup(BTGamemodeGroup);
}

function BTGamemodeInstance::exists(%this, %name)
{
	return isFunction(%this.type.getName(), %name);
}

function BTGamemodeInstance::call(%this, %name, %a, %b, %c, %d, %e)
{
	if (!%this.exists(%name))
	{
		return "";
	}

	return eval("return %this.type." @ %name @ "(%this,%this.game,%a,%b,%c,%d,%e);");
}

function BTGamemode::onAdd(%this)
{
	BTGamemodeGroup.add(%this);
}

function BTGameMode::assignRoles(%this)
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