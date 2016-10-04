if (!isObject(BTRole_Jailor))
{
	new ScriptObject(BTRole_Jailor)
	{
		class = BTRole;
		color = "00FFFF";
		name = "Jailor";
		alignment = "Town";

		tool[0] = nameToID(GunItem);
	};
}
