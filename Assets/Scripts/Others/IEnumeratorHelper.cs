using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IEnumeratorHelper
{
    public static IEnumerator After(System.Action action, float time)
    {
        yield return new WaitForSeconds(time);
        action();
    }

    public static IEnumerator After<T>(System.Action<T> action, T arg, float time)
    {
        yield return new WaitForSeconds(time);
        action(arg);
    }

    public static IEnumerator After<T1, T2>(System.Action<T1, T2> action, T1 arg1, T2 arg2, float time)
    {
        yield return new WaitForSeconds(time);
        action(arg1, arg2);
    }

    public static IEnumerator Continuous(System.Action<float> action, float duration)
    {
        float usedTime = 0;
        while (usedTime <= duration)
        {
            action(usedTime);
            usedTime += Time.deltaTime;
            yield return null;
        }
    }
}
