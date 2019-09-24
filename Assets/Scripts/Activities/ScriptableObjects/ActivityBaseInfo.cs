using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="ActivityNameInfo", menuName ="ScriptableObject/ActivitBaseInfo")]
public class ActivityBaseInfo : ScriptableObject
{
    public string activityTypeName;
    public string animatorParamName;
    public ParamType animatorParamType;

    public virtual ActivityBaseInfo Clone()
    {
        ActivityBaseInfo resultObject = new ActivityBaseInfo();
        resultObject.activityTypeName = activityTypeName;
        resultObject.animatorParamName = animatorParamName;
        resultObject.animatorParamType = animatorParamType;
        return resultObject;
    }
}
