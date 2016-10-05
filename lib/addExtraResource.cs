function addExtraResource(%name)
{
	if (MissionGroup.addedResource[%name]) return;
	MissionGroup.addedResource[%name] = "1";
	$EnvGuiServer::Resource[$EnvGuiServer::ResourceCount] = %name;
	$EnvGuiServer::ResourceCount++;
	setManifestDirty();
}