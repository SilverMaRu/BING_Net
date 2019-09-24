using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void TouchDelegate(Part sender, TouchEventData eventDate);
public class Part : MonoBehaviour
{
    public event TouchDelegate TouchEvent;

    public PartType partType = PartType.Body;
    public PartManager partManager { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("PlayerPart"))
        {
            Part otherPart = other.GetComponent<Part>();
            if (otherPart != null && !otherPart.partManager.Equals(partManager) && TouchEvent != null)
            {
                Debug.Log(partManager.gameObject.name + " & " + other.name + " Touch");
                TouchEvent(this, new TouchEventData(otherPart));
            }
        }
    }
}

public enum PartType
{
    Head,
    Body,
    LeftUpperArm,
    LeftLowerArm,
    LeftHand,
    RightUpperArm,
    RightLowerArm,
    RightHand,
    Hip,
    LeftUpperLeg,
    LeftLowerLeg,
    LeftFoot,
    RightUpperLeg,
    RightLowerLeg,
    RightFoot
}

public class TouchEventData
{
    public Part otherPart;

    public TouchEventData(Part otherPart)
    {
        this.otherPart = otherPart;
    }
}