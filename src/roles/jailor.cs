if (!isObject(BTRole_Jailor))
{
	new ScriptObject(BTRole_Jailor)
	{
		class = BTRole;
		color = "00FFFF";
		name = "Jailor";
		alignment = "Town";

		max = 1; //Only one jailor per game

		tool[0] = GunItem;
	};
}
