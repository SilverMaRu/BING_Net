using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SomeoneAttributes", menuName = "ScriptableObject/Attributes")]
public class Attributes : ScriptableObject
{
    public const int FIELD_COUNT = 3;
    public float maxSP = 0;
    public float recoverSP = 5;
    public float timeScale = 1;

    public float this[int idx]{
        get
        {
            float resultValue = 0;
            switch (idx)
            {
                case 0:
                    resultValue = maxSP;
                    break;
                case 1:
                    resultValue = recoverSP;
                    break;
                case 2:
                    resultValue = timeScale;
                    break;
                default:
                    resultValue = maxSP;
                    break;
            }
            return resultValue;
        }

        set
        {
            switch (idx)
            {
                case 0:
                    maxSP = value;
                    break;
                case 1:
                    recoverSP = value;
                    break;
                case 2:
                    timeScale = value;
                    break;
                default:
                    maxSP = value;
                    break;
            }
        }
    }

    public Attributes Clone()
    {
        Attributes newAttr = new Attributes();
        for(int i = 0;i< FIELD_COUNT; i++)
        {
            newAttr[i] = this[i];
        }
        return newAttr;
    }
}
