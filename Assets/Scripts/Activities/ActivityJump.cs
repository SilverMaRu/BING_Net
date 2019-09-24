using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityJump : ActivityBase
{
    private Rigidbody rigidbody;
    private Transform transform;

    public ActivityJump(GameObject ownerGO) : base(ownerGO)
    {
        transform = ownerGO.transform;
        rigidbody = ownerGO.GetComponent<Rigidbody>();
    }

    public ActivityJump(GameObject ownerGO, ActivityBaseInfo activityInfo) : base(ownerGO, activityInfo)
    {
        transform = ownerGO.transform;
        rigidbody = ownerGO.GetComponent<Rigidbody>();
    }

    public override bool MeetEnterCondition()
    {
        //return Input.GetKeyDown(KeyCode.Space)
        return networkInput.GetKeyDown(KeyCode.Space)
            && animator.GetBool("OnGround")
            ;
    }

    public override void EnterActivity()
    {
        base.EnterActivity();
        rigidbody.AddForce(transform.up * 5, ForceMode.Impulse);
    }
}
