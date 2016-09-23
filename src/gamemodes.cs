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