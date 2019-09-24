using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ActivityBaseDelegate(ActivityBase sender, ActivityEventData eventData);

public class ActivityBase
{
    public static event ActivityBaseDelegate EnterActivityEvent;
    public static event ActivityBaseDelegate ExitActivityEvent;
    protected GameObject ownerGO;
    protected ActivityBaseInfo activityInfo;
    //protected ActivityManager activityManager { get { return ownerGO.GetComponent<ActivityManager>(); } }
    protected ActivityManager activityManager { get; private set; }
    //protected Animator animator { get { return ownerGO.GetComponent<Animator>(); } }
    protected Animator animator { get; private set; }
    protected NetworkInput networkInput { get; private set; }

    public ActivityBase(GameObject ownerGO)
    {
        this.ownerGO = ownerGO;
        activityManager = ownerGO.GetComponent<ActivityManager>();
        animator = ownerGO.GetComponent<Animator>();
        networkInput = ownerGO.GetComponent<NetworkInput>();
        activityInfo = FindActivityInfo();
    }
    public ActivityBase(GameObject ownerGO, ActivityBaseInfo activityInfo)
    {
        this.ownerGO = ownerGO;
        activityManager = ownerGO.GetComponent<ActivityManager>();
        animator = ownerGO.GetComponent<Animator>();
        networkInput = ownerGO.GetComponent<NetworkInput>();
        this.activityInfo = activityInfo;
    }

    public virtual bool MeetEnterCondition()
    {
        return false;
    }

    public virtual void EnterActivity()
    {
        Debug.Log("Enter " + GetType().Name);
        #region 如果动画机参数类型为Trigger/Bool, 则触发/设为true
        switch (activityInfo.animatorParamType)
        {
            case ParamType.Bool:
            case ParamType.Trigger:
                SetAnimatorParam(1);
                break;
        }
        #endregion 如果动画机参数类型为Trigger/Bool, 则触发/设为true
        #region 把该行为添加到为当前行为列表
        activityManager.EnterActivity(this);
        #endregion 把该行为添加到为当前行为列表
        #region 发送事件
        if (EnterActivityEvent != null)
        {
            ActivityEventData eventDate = new ActivityEventData(ownerGO);
            EnterActivityEvent(this, eventDate);
        }
        #endregion 发送事件
    }

    public virtual void Update()
    {
        // 如果达到了离开条件, 执行离开
        if (MeetExitCondition()) ExitActivity();
    }

    public virtual bool MeetExitCondition()
    {
        return true;
    }

    public virtual void ExitActivity()
    {
        Debug.Log("Exit " + GetType().Name);
        #region 如果动画机参数类型为Bool, 则设置为false
        if (activityInfo.animatorParamType == ParamType.Bool) SetAnimatorParam(0);
        #endregion 如果动画机参数类型为Bool, 则设置为false
        #region 把该行为从当前行为列表中移除
        activityManager.ExitActivity(this);
        #endregion 把该行为从当前行为列表中移除
        #region 发送事件
        if (EnterActivityEvent != null)
        {
            ActivityEventData eventDate = new ActivityEventData(ownerGO);
            ExitActivityEvent(this, eventDate);
        }
        #endregion 发送事件
    }

    /// <summary>
    /// 从项目中查找并返回行为信息
    /// </summary>
    /// <returns></returns>
    protected virtual ActivityBaseInfo FindActivityInfo()
    {
        return null;
    }

    /// <summary>
    /// 设置动画机参数
    /// 参数类型为Bool时, value 大于 0 为true; value 小于等于 0 为false
    /// 参数类型为Int时, 把value强转为Int类型
    /// </summary>
    /// <param name="paramType">动画机参数类型</param>
    /// <param name="value">动画机参数值</param>
    /// <param name="dampTime"></param>
    /// <param name="deltaTime"></param>
    protected virtual void SetAnimatorParam(float value = 0, float dampTime = 0, float deltaTime = 0)
    {
        string paramName = activityInfo.animatorParamName;
        switch (activityInfo.animatorParamType)
        {
            case ParamType.Int:
                animator.SetInteger(paramName, (int)value);
                break;
            case ParamType.Float:
                animator.SetFloat(paramName, value, dampTime, deltaTime);
                break;
            case ParamType.Bool:
                animator.SetBool(paramName, value > 0);
                break;
            case ParamType.Trigger:
                animator.SetTrigger(paramName);
                break;
        }
    }
}

public enum ParamType { Int, Float, Bool, Trigger }

public class ActivityEventData
{
    public GameObject ownerGO;

    public ActivityEventData(GameObject ownerGO)
    {
        this.ownerGO = ownerGO;
    }
}
