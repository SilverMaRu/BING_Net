using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityOnSky : ActivityBase
{
    private const float RAYCAST_DISTENCE_OFFSET = 0.05f;
    private Transform transform;
    private Collider collider;
    private Rigidbody rigidbody;

    public ActivityOnSky(GameObject ownerGO) : base(ownerGO)
    {
        transform = ownerGO.transform;
        collider = ownerGO.GetComponent<Collider>();
        rigidbody = ownerGO.GetComponent<Rigidbody>();
    }

    public ActivityOnSky(GameObject ownerGO, ActivityBaseInfo activityInfo) : base(ownerGO, activityInfo)
    {
        transform = ownerGO.transform;
        collider = ownerGO.GetComponent<Collider>();
        rigidbody = ownerGO.GetComponent<Rigidbody>();
    }

    public override bool MeetEnterCondition()
    {
        Bounds bounds = collider.bounds;
        float extentX = bounds.extents.x;
        RaycastHit[] hits = Tool.PieRaycastAll(transform.position + extentX * Vector3.up
            , extentX
            , -transform.up
            , extentX + RAYCAST_DISTENCE_OFFSET
            , transform.forward
            , transform.up
            , 1
            , 2
            , 1 << 9);
        return !Tool.IsCast(hits);
    }

    public override void EnterActivity()
    {
        base.EnterActivity();
        animator.applyRootMotion = false;
        animator.SetBool("OnGround", false);
    }

    public override void Update()
    {
        SetAnimatorParam(rigidbody.velocity.y);
        base.Update();
    }

    public override bool MeetExitCondition()
    {
        Bounds bounds = collider.bounds;
        float extentX = bounds.extents.x;
        RaycastHit[] hits = Tool.PieRaycastAll(transform.position + extentX * Vector3.up
            , extentX
            , -transform.up
            , extentX + RAYCAST_DISTENCE_OFFSET
            , transform.forward
            , transform.up
            , 1
            , 2
            , 1 << 9);
        return Tool.IsCast(hits);
    }

    public override void ExitActivity()
    {
        base.ExitActivity();
        animator.applyRootMotion = true;
        animator.SetBool("OnGround", true);
    }
}
