using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ActivityNameInfo", menuName ="ScriptableObject/ActivityMoveInfo")]
public class ActivityMoveInfo : ActivityBaseInfo
{
    public ReviseInfo reviseInfo;
    public float enterPunishValue;
    [Range(0,1)]
    public float enterPunishPercent;
    public float exitPunishValue;
    [Range(0,1)]
    public float exitPunishPercent;
}
