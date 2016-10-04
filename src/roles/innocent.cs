if (!isObject(BTRole_Innocent))
{
	new ScriptObject(BTRole_Innocent)
	{
		class = BTRole;
		color = "00FF00";
		name = "Innocent";
		alignment = "Town";

		tool[0] = nameToID(GunItem);
	};
}
