using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ActivityNameInfo",menuName ="ScriptableObject/ActivityReachOutInfo")]
public class ActivityReachOutInfo : ActivityBaseInfo
{
    public ReviseInfo reviseInfo;
    [Header("根游戏对象下的游戏对象名称,调用Transform.Find时的参数")]
    public string IKGameObjectName;
    public AvatarIKGoal effectGoal = AvatarIKGoal.RightHand;
    [Min(0)]
    public float reachOutUseTime = 1;
    [Min(0)]
    public float countermandUseTime = 0.5f;
}
