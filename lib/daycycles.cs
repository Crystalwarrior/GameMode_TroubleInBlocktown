function getDayCycleTime()
{
    if (!$EnvGuiServer::DayCycleEnabled || !isObject(DayCycle))
        return -1;

    %time = $Sim::Time / DayCycle.dayLength + DayCycle.dayOffset;
    return %time - mFloor(%time);
}

function calculateDaycycleFraction() {
    %len = DayCycle.DayLength;
    return ($Sim::Time % %len) / %len;
}

function getDayCycleTimeString(%time, %mod12) {
    %time = getTimeString(mFloor((%time * 86400) / 60));

    if (!%mod12) {
        if (strLen(%time) == 3) {
            return 0 @ %time;
        }

        return %time;
    }

    %time = strReplace(%time, ":", " ");

    %hour = getWord(%time, 0);
    %mins = getWord(%time, 1);

    if (%hour >= 13) {
        return %hour - 12 @ ":" @ %mins SPC "PM";
    }

    return %hour @ ":" @ %mins SPC "AM";
}

function setDayCycleTime(%frac) {
    %time = calculateDayCycleFraction();
    %off = %frac - %time;
    if(%off < 0) {
        %off = 1 + %off;
    }
    $EnvGuiServer::DayOffset = %off;
    DayCycle.setDayOffset(%off);
}

function getSunVector()
{
    %azim = mDegToRad($EnvGuiServer::SunAzimuth);

    if ($EnvGuiServer::DayCycleEnabled && isObject(DayCycle))
    {
        %time = getDayCycleTime();
        %badspotIsWeird = 0.583;

        if (%time < %badspotIsWeird)
            %elev = mDegToRad((%time / %badspotIsWeird) * 180);
        else
            %elev = mDegToRad(180 + ((%time - %badspotIsWeird) / (1 - %badspotIsWeird)) * 180);
    }
    else
        %elev = mDegToRad($EnvGuiServer::SunElevation);

    %h = mCos(%elev);
    return %h * mSin(%azim) SPC %h * mCos(%azim) SPC mSin(%elev);
}

function syncDayCycle()
{
    if (!isObject(DayCycle))
    {
        error("ERROR: DayCycle does not exist.");
        return;
    }

    if (DayCycle.dayLength != 86400)
    {
        error("ERROR: DayCycle length is not 86400.");
        return;
    }

    %all = strReplace(getWord(getDateTime(), 1), ":", " ");

    %real = getWord(%all, 0) * 3600 + getWord(%all, 1) * 60 + getWord(%all, 2);
    %curr = $Sim::Time / 86400;

    DayCycle.setDayOffset(%real - (%curr - mFloor(%curr)));
}
