using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityTurn : ActivityBase
{
    private Vector3 inputDirection
    {
        get
        {
            //Vector3 result = Input.GetAxis("V") * CameraCtrl.rigTrans.forward + Input.GetAxis("H") * CameraCtrl.cameraTrans.right;
            Vector3 result = networkInput.GetAxis("V") * CameraCtrl.rigTrans.forward + networkInput.GetAxis("H") * CameraCtrl.cameraTrans.right;
            result.Normalize();
            return result;
        }
    }
    private Coroutine smoothStop;
    public ActivityTurn(GameObject ownerGO) : base(ownerGO)
    {
    }

    public ActivityTurn(GameObject ownerGO, ActivityBaseInfo activityInfo) : base(ownerGO, activityInfo)
    {
    }

    public override bool MeetEnterCondition()
    {
        return Vector3.Dot(inputDirection, ownerGO.transform.forward) < 1
            && animator.GetBool("OnGround")
            ;
    }

    public override void EnterActivity()
    {
        base.EnterActivity();
        animator.applyRootMotion = true;
        if (smoothStop != null) activityManager.StopCoroutine(smoothStop);
    }

    public override void Update()
    {
        Vector3 inputDir = ownerGO.transform.InverseTransformDirection(inputDirection);
        float turnAmount = Mathf.Atan2(inputDir.x, inputDir.z);
        SetAnimatorParam(turnAmount, 0.1f, Time.deltaTime);
        base.Update();
    }

    public override bool MeetExitCondition()
    {
        return Vector3.Dot(inputDirection, ownerGO.transform.forward) >= 1
            || !animator.GetBool("OnGround")
            ;
    }

    public override void ExitActivity()
    {
        smoothStop = activityManager.StartCoroutine(SmoothStop());
        base.ExitActivity();
    }

    private IEnumerator SmoothStop()
    {
        while (animator.GetFloat(activityInfo.animatorParamName) != 0)
        {
            SetAnimatorParam(0, 0.2f, Time.deltaTime);
            yield return null;
        }
    }
}
