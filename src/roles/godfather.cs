if (!isObject(BTRole_Godfather))
{
	new ScriptObject(BTRole_Godfather)
	{
		class = BTRole;
		color = "FFFFFF";
		name = "Godfather";
		alignment = "Mafia";
		fake_alignment = "Town"; //Investigation result

		max = 1; //Only one godfather per game

		tool[0] = GunItem;
	};
}
