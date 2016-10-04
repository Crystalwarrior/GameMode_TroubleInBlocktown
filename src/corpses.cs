datablock PlayerData(PlayerCorpseArmor : PlayerStandardArmor) //NB: change PlayerStandardArmor to whatever will be our default playertype in the gamemode
{
	uiName = "Corpse Player";
	canJet = 0;
	boundingBox = "5 5 4";
	crouchBoundingBox = "5 5 4";
	firstPersonOnly = 1;
};

function PlayerCorpseArmor::onUnMount(%this, %obj, %mount, %slot)
{
	Parent::onUnMount(%this, %obj, %mount, %slot);
	if (!isObject(%mount.heldCorpse) || %mount.heldCorpse != %obj)
		return;
	%mount.heldCorpse = "";
	%mount.playThread(2, "root");
}

function Player::grabCorpse(%obj, %corpse)
{
	%obj.unMountImage(0);
	fixArmReady(%obj);
	%obj.mountObject(%corpse, 0);
	%obj.playThread(2, "ArmReadyBoth");
	%obj.heldCorpse = %corpse;
	%corpse.holder = %obj;
	%corpse.setTransform("0 0 -100 0 0 -1 -1.5709");
}

function Player::throwCorpse(%obj)
{
	if (!isObject(%corpse = %obj.heldCorpse))
		return 0;

	%obj.playThread(2, "shiftUp");
	%a = %obj.getEyePoint();
	%b = vectorAdd(vectorScale(%obj.getEyeVector(), 5), %a);
	%ray = containerRayCast(%a, %b, $TypeMasks::All ^ $TypeMasks::fxAlwaysBrickObjectType, %obj);
	if(%ray)
		%b = getWords(%ray, 1, 3);
	%velocity = vectorScale(%obj.getEyeVector(), vectorDist(%a, %b));
	%velocity = vectorAdd(%velocity, %obj.getVelocity()); //velocity inheritance
	%pos = vectorScale(vectorAdd(%a, %b), 0.5); //Get middle of raycast

	%obj.mountObject(%corpse, 8);
	%corpse.dismount();
	%corpse.setTransform(%pos);
	%corpse.setVelocity(%velocity);
	return 1;
}

function Player::dropCorpse(%obj)
{
	if (!isObject(%corpse = %obj.heldCorpse))
		return 0;

	%obj.playThread(2, "shiftDown");
	%a = %obj.getPosition();
	%b = vectorAdd(vectorScale(%obj.getForwardVector(), 3), %a);
	%ray = containerRayCast(%a, %b, $TypeMasks::All ^ $TypeMasks::fxAlwaysBrickObjectType, %obj);
	if(%ray)
		%b = getWords(%ray, 1, 3);
	%pos = vectorScale(vectorAdd(%a, %b), 0.5); //Get middle of raycast

	%obj.mountObject(%corpse, 8);
	%corpse.dismount();
	%corpse.setTransform(%pos);
	%corpse.setVelocity(%obj.getVelocity());
	return 1;
}

function Player::findCorpseRayCast(%obj)
{
	%a = %obj.getEyePoint();
	%b = vectorAdd(vectorScale(%obj.getEyeVector(), 5), %a);
	%ray = containerRayCast(%a, %b, $TypeMasks::All ^ $TypeMasks::fxAlwaysBrickObjectType, %obj);
	if(%ray)
		%b = getWords(%ray, 1, 3);
	%center = vectorScale(vectorAdd(%a, %b), 0.5); //Get middle of raycast
	%length = vectorDist(%a, %b) / 2;

	%maxdist = 1; //how fatass our fat raycast is
	initContainerRadiusSearch(%center, %length + %maxdist, $TypeMasks::CorpseObjectType); //Scale radius search so it searches the entirety of raycast
	while (isObject(%col = containerSearchNext()))
	{
		if (!%col.isBody)
			continue;
		%p = %col.getHackPosition();
		%ab = vectorSub(%b, %a);
		%ap = vectorSub(%p, %a);

		%project = vectorDot(%ap, %ab) / vectorDot(%ab, %ab); //Projection, aka "check against closest point on raycast" or something.

		if (%project < 0 || %project > 1)
			continue;

		%j = vectorAdd(%a, vectorScale(%ab, %project));
		%distance = vectorDist(%p, %j);
		if (%distance <= %maxdist) //Give 'em the corpse!
		{
			return %col;
		}
	}
	return 0;
}


if (!isObject(CorpseGroup))
	new SimSet(CorpseGroup);

package BT_Corpses
{
	function Player::mountImage(%this, %image, %slot, %loaded, %skintag)
	{
		parent::mountImage(%this, %image, %slot, %loaded, %skintag);
		if (%slot == 0 && isObject(%this.heldCorpse))
			%this.throwCorpse();
	}

	function Player::removeBody(%this)
	{
		if (!%this.isBody)
			return Parent::removeBody(%this);
		return;
	}

	function Armor::onTrigger(%this, %obj, %slot, %state)
	{
		Parent::onTrigger(%this, %obj, %slot, %state);

		if(%slot == 0 && %state)
		{
			if (!%obj.dropCorpse())
			{
				if (isObject(%corpse = %obj.findCorpseRayCast()))
				{
					//examine the %corpse or something
				}
			}
		}

		if(%slot == 4 && %state)
		{
			if (!%obj.throwCorpse()) //No corpse to throw, try grabbing one instead
			{
				if (isObject(%corpse = %obj.findCorpseRayCast()))
				{
					%obj.grabCorpse(%corpse);
				}
			}
		}
	}

	function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
	{
		//if (%client.miniGame != $DefaultMiniGame)
		//	return Parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);

		if (%sourceObject.sourceObject.isBot)
		{
			%sourceClientIsBot = 1;
			%sourceClient = %sourceObject.sourceObject;
		}

		%player = %client.player;

		if (isObject(%player))
		{
			%player.changeDatablock(PlayerCorpseArmor); //Doing this before client is nullified is important for appearance stuff
			%player.playDeathAnimation(); //...still call this because datablock switch fucks with anims
			%player.playThread(0, "death1"); //Trying to fix "die standing" issue (spoiler alert: breaks when corpse is picked up)
			%player.setShapeName("", 8564862);
			if (isObject(%player.tempBrick))
			{
				%player.tempBrick.delete();
				%player.tempBrick = 0;
			}
			%player.isBody = true;
			%player.client = 0;
			%player.origClient = %client;
			CorpseGroup.add(%player);
		}
		else
			warn("WARNING: No player object in GameConnection::onDeath() for client '" @ %client @ "'");

		if (isObject(%client.camera) || isObject(%player))
		{ // this part of the code isn't accurate
			if (%client.getControlObject() != %client.camera)
			{
				%client.camera.setMode("Corpse", %player);
				%client.camera.setControlObject(0);
				%client.setControlObject(%client.camera);
			}
		}

		%client.player = 0;
		%client.corpse = %player;

		if ($Damage::Direct[$damageType] && getSimTime() - %player.lastDirectDamageTime < 100 && %player.lastDirectDamageType !$= "")
			%damageType = %player.lastDirectDamageType;

		if (%damageType == $DamageType::Impact && isObject(%player.lastPusher) && getSimTime() - %player.lastPushTime <= 1000)
			%sourceClient = %player.lastPusher;

		%message = '%2 killed %1';

		if (%sourceClient == %client || %sourceClient == 0)
		{
			%message = $DeathMessage_Suicide[%damageType];
			%client.corpse.suicide = true;
		}
		else
			%message = $DeathMessage_Murder[%damageType];

		// removed mid-air kills code here
		// removed mini-game kill points here

		%clientName = %client.getPlayerName();

		if (isObject(%sourceClient))
			%sourceClientName = %sourceClient.getPlayerName();
		else if (isObject(%sourceObject.sourceObject) && %sourceObject.sourceObject.getClassName() $= "AIPlayer")
			%sourceClientName = %sourceObject.sourceObject.name;
		else
			%sourceClientName = "";

		echo("\c4" SPC %sourceClientName SPC "killed" SPC (%clientName $= %sourceClientName ? "himself" : %clientName));
		%client.miniGame.checkLastManStanding();
		// removed death message print here
		// removed %message and %sourceClientName arguments
		messageClient(%client, 'MsgYourDeath', '', %clientName, '', %client.miniGame.respawnTime);
		//commandToClient(%client, 'CenterPrint', "", 1);
	}
};
activatePackage(BT_Corpses);