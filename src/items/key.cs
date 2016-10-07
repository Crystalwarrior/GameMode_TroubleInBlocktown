datablock AudioProfile(DoorJiggleSound) {
	fileName = $BT::Path @ "res/sounds/door_jiggle.wav";
	description = audioClose3D;
	preload = true;
};

datablock AudioProfile(DoorKnockSound) {
	fileName = $BT::Path @ "res/sounds/knock.wav";
	description = audioClose3D;
	preload = true;
};

datablock AudioProfile(DoorLockSound) {
	fileName = $BT::Path @ "res/sounds/Lock.wav";
	description = audioClose3D;
	preload = true;
};

datablock AudioProfile(DoorUnlockSound) {
	fileName = $BT::Path @ "res/sounds/Unlock.wav";
	description = audioClose3D;
	preload = true;
};

datablock AudioProfile(WoodHitSound)
{
	fileName = $BT::Path @ "res/sounds/woodhit.wav";
	description = audioClose3D;
	preload = true;
};

datablock ItemData(KeyItem)
{
	shapeFile = "Add-Ons/Item_Key/keya.dts";

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;

	uiName = "Key";
	iconName = "Add-Ons/Item_Key/Icon_KeyA";
	doColorShift = true;
	colorShiftColor = "1 1 0 1";

	itemPropsClass = "KeyProps";
	itemPropsAlways = true;

	customPickupAlways = true;
	customPickupMultiple = true;

	image = KeyImage;

	canDrop = true;

	w_class = 2; //Weight class: 1 is tiny, 2 is small, 3 is normal-sized, 4 is bulky
	// collisionSFX = KeyImpactSFX;
	// collisionThreshold = 3;
};

function KeyProps::onAdd(%this)
{
	%this.name = "Key";
	%this.id = ""; // Can't open anything
}

datablock ShapeBaseImageData(KeyImage)
{
	className = "WeaponImage";
	shapeFile = "Add-Ons/Item_Key/keya.dts";

	isKey = true;
	item = KeyItem;
	armReady = true;

	doColorShift = KeyItem.doColorShift;
	colorShiftColor = KeyItem.colorShiftColor;
};

function KeyImage::onMount(%this, %obj, %slot)
{
	Parent::onMount(%this, %obj, %slot);
	%props = %obj.getItemProps();

	if (isObject(%obj.client))
		%obj.client.centerPrint("\c3" @ %props.name @ "\n", 2.5);
}

function fxDtsBrick::doorDamage(%brick, %damageChance)
{
	if (!%brick.getDatablock().isDoor || %brick.impervious)
		return;
	%words = getWordCount(%damageChance);
	for (%i = 0; %i < %words; %i++)
		%sum += getWord(%damageChance, %i);
	%select = getRandom(0, %sum - 1);
	for (%i = 0; %i < %words; %i++)
	{
		%select -= getWord(%damageChance, %i);
		if (%select < 0)
			break;
	}
	%brick.doorHits += %i;
	if (%brick.doorHits >= %brick.doorMaxHits)
	{
		%brick.doorOpen(%brick.isCCW, %obj.client);
		%brick.lockState = false;
		%brick.broken = true;
	}
}

function fxDtsBrick::checkKeyID(%brick, %id)
{
	if (%brick.lockId $= %id) return true; //Check if the key fits a numbered housedoor
	if ((%a = strpos(%id, "_")) != -1)
		%id = getSubStr(%id, 0, %a); //Strip out the numbered part
	if (%brick.lockId $= %id) return true; //Check if the key fits an unnumbered housedoor (entrance door, etc.)
	return false; //aww we failed :(
}

package KeyPackage
{
	function FxDTSBrick::door(%this, %state, %client)
	{
		if (%this.broken)
			%client.centerPrint("\c2The door lock is broken...", 2);
		else if (%this.lockId !$= "" && %this.lockState)
		{
			%client.centerPrint("\c2The door is locked.", 2);
			serverPlay3d(DoorJiggleSound, %this.getWorldBoxCenter(), 1);
		}
		else
			Parent::door(%this, %state, %client);
	}

	function Armor::onTrigger(%this, %obj, %slot, %state)
	{
		Parent::onTrigger(%this, %obj, %slot, %state);

		if (!%state || !%obj.getMountedImage(0).isKey || (%slot != 0 && %slot != 4))
			return;

		%a = %obj.getEyePoint();
		%b = vectorAdd(%a, vectorScale(%obj.getEyeVector(), 4));

		%mask =
			$TypeMasks::FxBrickObjectType |
			$TypeMasks::PlayerObjectType;

		%ray = containerRayCast(%a, %b, %mask, %obj);

		if (!%ray || %ray.lockId $= "")
			return;

		%props = %obj.getItemProps();

		%data = %ray.getDataBlock();

		if (%data.isDoor && %data.isOpen)
			return;

		if (%slot == 0) // Unlock
		{
			if (%ray.lockState && %ray.checkKeyID(%props.id))
			{
				%ray.lockState = false;
				serverPlay3d(DoorUnlockSound, %ray.getWorldBoxCenter(), 1);
				%obj.playThread(2, "rotCW");
				if (isObject(%obj.client))
					%obj.client.centerPrint("\c6You unlock the door.", 2);
			}
			else
			{
				serverPlay3d(DoorJiggleSound, %ray.getWorldBoxCenter(), 1);
				%obj.playThread(2, "shiftRight");
				if (isObject(%obj.client))
					%obj.client.centerPrint(%ray.checkKeyID(%props.id) ? "\c6The door is already unlocked." : "\c6The key doesn't fit.", 2);
			}
		}
		else if (%slot == 4) // Lock
		{
			if (!%ray.lockState && %ray.checkKeyID(%props.id))
			{
				%ray.lockState = true;
				serverPlay3d(DoorLockSound, %ray.getWorldBoxCenter(), 1);
				%obj.playThread(2, "rotCCW");
				if (isObject(%obj.client))
					%obj.client.centerPrint("\c6You lock the door.", 2);
			}
			else
			{
				serverPlay3d(DoorJiggleSound, %ray.getWorldBoxCenter(), 1);
				%obj.playThread(2, "shiftLeft");
				if (isObject(%obj.client))
					%obj.client.centerPrint(%ray.checkKeyID(%props.id) ? "\c6The door is already locked." : "\c6The key doesn't fit.", 2);
			}
		}
	}
};

activatePackage("KeyPackage");
