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

function BTGameMode::assignHouses(%this)
{
	%miniGame = $defaultMiniGame;

	//maroon, orange, brown, gray, navy, white, apt, rosewood, green, shack, bad
	//init door stuff
	%count = BrickGroup_888888.getCount();

	for (%i = 0; %i < %count; %i++)
	{
		%brick = BrickGroup_888888.getObject(%i);

		%data = %brick.getDataBlock();
		%name = %brick.getName();
		if (%data.isDoor && getSubStr(%name, 0, 3) $= "_d_") //house door
		{
			%str = getSubStr(%name, 3, strlen(%name));
			%brick.lockState = true;
			%brick.lockID = %str;
			
			//if ((%a = strpos(%str, "_")) != -1)
			//	%str = getSubStr(%str, 0, %a);

			if (strpos(%this.houselist, %str) == -1)
			{
				%this.houselist = (getWordCount(%this.houselist) > 0 ? (%this.houselist SPC %str) : %str);
			}
		}
		else if (getSubStr(%name, 0, 5) $= "_bed_") //beds
		{
			if (strpos(%this.bedlist, %name) == -1)
			{
				%this.bedlist = (getWordCount(%this.bedlist) > 0 ? (%this.bedlist SPC %name) : %name);
			}
		}
	}
	//talk(%this.bedlist);

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
	//Assign housing
	for (%i = 0; %i < %count; %i++)
	{
		%member = %member[%i];
		if (isObject(%player = %member.player))
		{
			if (getWordCount(%this.bedlist) > 0)
			{
				%r = getRandom(0, getWordCount(%this.bedlist)-1);
				//talk(getWord(%this.bedlist, %r) SPC %R);
				%brick = BrickGroup_888888.NTObject[getWord(%this.bedlist, %r), 0];
				%this.bedlist = removeWord(%this.bedlist, %r);
				%z = 0.1 * %brick.getdatablock().bricksizez;
				%pos = vectoradd(getwords(%brick.gettransform(), 0, 2), "0 0" SPC %z);
				%player.settransform(%pos);

				// Give them a key to their room (and house)
				%str = getSubStr(%brick.getName(), 5, strlen(%brick.getName()));
				%props = KeyItem.newItemProps(%player, 4);
				%props.name = "Room #" @ %str @ " Key";
				%props.id = %str;

				%player.addTool(KeyItem, %props);
			}

		}
	}
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
				%this.assignRole(%member, BTRoleGroup.getRole(%role[%i]));
			else if (%this.roleGroup[%role[%i]] !$= "")
			{
				%field = %this.roleGroup[%role[%i]];
				%a = getFieldCount(%field);
				%b = getField(%field, getRandom(0, %a-1));
				%this.assignRole(%member, BTRoleGroup.getRole(%b));
			}
			else
				talk("error, failed to assign role");
		}
	}
}

function BTGameMode::assignRole(%this, %client, %role)
{
	if(!isObject(%role))
		return;

	%client.role = %role;
	if(!isObject(%client.player))
		return;

	%maxtools = %client.player.getDatablock().maxTools;
	for(%i = 0; %i < %maxtools; %i++)
	{
		if(!isObject(%client.role.tool[%i]))
			continue;
		%client.player.tool[%i] = %client.role.tool[%i].getID();
		messageClient(%client, 'MsgItemPickup', '', %i, %client.player.tool[%i]);
	}

	messageClient(%client, '', "\c6You are <color:" @ %client.role.color @ ">" @ %client.role.name @ "\c6! You are aligned with \c3"@ %client.role.alignment);
}