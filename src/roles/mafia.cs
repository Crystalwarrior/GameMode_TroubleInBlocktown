if (!isObject(BTRole_Mafia))
{
	new ScriptObject(BTRole_Mafia)
	{
		class = BTRole;
		color = "FF0000";
		name = "Mafioso";
		alignment = "Mafia";
		showTeammates = true;

		tool[0] = GunItem;
	};
}
