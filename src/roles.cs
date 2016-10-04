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

function GameConnection::assignRole(%this, %role)
{
	if(!isObject(%role))
		return;

	%this.role = %role;
	if(!isObject(%this.player))
		return;

	%maxtools = %this.player.getDatablock().maxTools;
	for(%i = 0; %i < %maxtools; %i++)
	{
		if(!isObject(%this.role.tool[%i]))
			continue;

		%this.player.tool[%i] = %this.role.tool[%i];
		messageClient(%this, 'MsgItemPickup', '', %i, %this.player.tool[%i]);
	}

	messageClient(%this, '', "\c6You are <color:" @ %this.role.color @ ">" @ %this.role.name @ "\c6! You are aligned with \c3"@ %this.role.alignment);
}