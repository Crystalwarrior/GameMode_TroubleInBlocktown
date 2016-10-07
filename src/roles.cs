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