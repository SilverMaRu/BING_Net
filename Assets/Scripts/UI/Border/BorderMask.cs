using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BorderSite
{
    Left,
    Top,
    Right,
    Bottom
}
public class BorderMask : MonoBehaviour
{
    public BorderSite borderSite = BorderSite.Left;
}
