if (!isObject(BTGameMode_Classic))
{
	new ScriptObject(BTGameMode_Classic)
	{
		class = BTGameMode;
	};
}

function BTGameMode_Classic::onStart(%this)
{
	talk("Woo hoo GAMEMODE CLASSIC is started nerds");
}

function BTGameMode_Classic::initRoles(%this)
{
	talk("Initialising possible roles yo");
}

function BTGameMode_Classic::cleanUp(%this)
{
	talk("gamemode is rip in kill");
}

function BTGameMode_Classic::checkLastManStanding(%this)
{
	
}