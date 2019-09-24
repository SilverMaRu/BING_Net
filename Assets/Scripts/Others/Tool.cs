using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool
{
    public static Vector3 flatXZ = Vector3.right + Vector3.forward;
    public static Vector3 flatXY = Vector3.right + Vector3.up;
    public static Vector3 flatYZ = Vector3.up + Vector3.forward;

    private static GameObject camera;
    private static Vector3 cameraScale = Vector3.zero;

    public static bool IsOutOfCameraX(float positionX, float offset = 0)
    {
        bool ret = true;
        float deltaX = positionX - Camera.main.transform.position.x;
        float limitDistanceX = GetCameraScale().x * .5f + offset;
        //Debug.DrawLine(Vector3.Scale(GetCamera().transform.position, flatXY) + limitDistanceX * Vector3.right, Vector3.Scale(GetCamera().transform.position, flatXY) + limitDistanceX * Vector3.right + GetCameraScale().y * Vector3.up, Color.blue);
        //Debug.DrawLine(Vector3.Scale(GetCamera().transform.position, flatXY) + limitDistanceX * Vector3.right, Vector3.Scale(GetCamera().transform.position, flatXY) + limitDistanceX * Vector3.right - GetCameraScale().y * Vector3.up, Color.blue);
        ret = deltaX * deltaX > limitDistanceX * limitDistanceX;
        return ret;
    }

    public static bool IsOutOfCameraY(float positionY, float offset = 0)
    {
        bool ret = true;
        float deltaY = positionY - Camera.main.transform.position.y;
        float limitDistanceY = GetCameraScale().y * .5f + offset;
        ret = deltaY * deltaY > limitDistanceY * limitDistanceY;
        return ret;
    }

    public static Vector3 GetCameraScale()
    {
        if (cameraScale != Vector3.zero)
        {
            return cameraScale;
        }
        float width = 0;
        float height = 0;
        height = Camera.main.orthographicSize * 2;
        // 摄像机纵横比例
        float aspectRatio = Camera.main.aspect;
        width = height * aspectRatio;
        Vector3 ret = Vector3.right * width + Vector3.up * height;
        return ret;
    }

    public static float GetCameraOrthographicSizeByWidth(float width)
    {
        float aspectRatio = Camera.main.aspect;
        float height = width / aspectRatio;
        return GetCameraOrthographicSizeByHeight(height);
    }

    public static float GetCameraOrthographicSizeByHeight(float height)
    {
        return height * .5f;
    }

    public static GameObject GetCamera()
    {
        if (camera == null)
        {
            camera = Camera.main.gameObject;
        }
        return camera;
    }

    public static Vector3 GetSpriteSizeInScene(SpriteRenderer sr)
    {
        return GetSpriteSizeInScene(sr.sprite);
    }

    public static Vector2 GetSpriteSizeInScene(Sprite sprite)
    {
        float spriteHeight = sprite.rect.height;
        float spriteWidth = sprite.rect.width;
        float pixelsPerUnit = sprite.pixelsPerUnit;

        Vector2 ret = Vector2.up * spriteHeight / pixelsPerUnit + Vector2.right * spriteWidth / pixelsPerUnit;
        return ret;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="sourceArray"></param>
    /// <param name="increaseLength">是否增加数组长度</param>
    /// <returns></returns>
    public static T[] Prepend<T>(T item, T[] sourceArray, bool increaseLength)
    {
        T[] retArray = new T[1] { item };

        if (sourceArray != null && sourceArray.Length > 0)
        {
            int retArrayLength = increaseLength ? sourceArray.Length + 1 : sourceArray.Length;
            retArray = new T[retArrayLength];
            retArray[0] = item;
            for (int i = 1; i < retArray.Length; i++)
            {
                retArray[i] = sourceArray[i - 1];
            }
        }
        return retArray;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="sourceArray"></param>
    /// <param name="increaseLength">是否增加数组长度</param>
    /// <returns></returns>
    public static T[] Append<T>(T item, T[] sourceArray, bool increaseLength)
    {
        T[] retArray = new T[1] { item };

        if (sourceArray != null && sourceArray.Length > 0)
        {
            int retArrayLength = increaseLength ? sourceArray.Length + 1 : sourceArray.Length;
            if (increaseLength)
            {
                retArray = new T[sourceArray.Length + 1];
                for (int i = 0; i < sourceArray.Length; i++)
                {
                    retArray[i] = sourceArray[i];
                }
            }
            else
            {
                retArray = new T[sourceArray.Length];
                for (int i = 1; i < sourceArray.Length; i++)
                {
                    retArray[i - 1] = sourceArray[i];
                }
            }
            retArray[retArray.Length - 1] = item;
        }

        return retArray;
    }

    public static T[] Append<T>(T[] newArray, T[] sourceArray)
    {
        T[] retArray = new T[newArray.Length + sourceArray.Length];
        for (int i = 0; i < sourceArray.Length; i++)
        {
            retArray[i] = sourceArray[i];
        }

        for (int i = 0; i < newArray.Length; i++)
        {
            retArray[i + sourceArray.Length] = newArray[i];
        }

        return retArray;
    }

    public static T[] Sort<T>(T[] sourceArray)
    {
        List<T> resultList = new List<T>(sourceArray);
        resultList.Sort();
        return resultList.ToArray();
    }

    public static T[] Sort<T>(T[] sourceArray, System.Comparison<T> comparison)
    {
        List<T> resultList = new List<T>(sourceArray);
        resultList.Sort(comparison);
        return resultList.ToArray();
    }

    public static T[] Insert<T>(T[] array, int index, T item)
    {
        List<T> tempList = new List<T>(array);
        tempList.Insert(index, item);
        return tempList.ToArray();
    }

    public static GameObject FindGameObject(Transform rootTrans, string gameObjectName, bool includeInactive = false)
    {
        if (rootTrans == null) return null;
        GameObject resultGO = null;

        for (int i = 0; i < rootTrans.childCount; i++)
        {
            Transform childTrans = rootTrans.GetChild(i);
            resultGO = FindGameObject(childTrans, gameObjectName, includeInactive);
            if (resultGO != null) break;
            if ((includeInactive || !includeInactive && childTrans.gameObject.activeSelf) && childTrans.name.Equals(gameObjectName))
            {
                resultGO = childTrans.gameObject;
                break;
            }
        }
        return resultGO;
    }

    public static GameObject[] FindGameObjects(Transform rootTrans, string gameObjectName, bool includeInactive = false)
    {
        if (rootTrans == null) return null;
        List<GameObject> resultGOList = new List<GameObject>();

        for (int i = 0; i < rootTrans.childCount; i++)
        {
            Transform childTrans = rootTrans.GetChild(i);
            resultGOList.AddRange(FindGameObjects(childTrans, gameObjectName, includeInactive));
            if ((includeInactive || !includeInactive && childTrans.gameObject.activeSelf) && childTrans.name.Equals(gameObjectName))
            {
                resultGOList.Add(childTrans.gameObject);
            }
        }
        return resultGOList.ToArray();
    }

    public static GameObject FindParentGameObjectByName(Transform startTrans, string gameObjectName)
    {
        if (startTrans == null) return null;
        GameObject resultGO = null;
        Transform parentTrans = startTrans.parent;
        if (parentTrans != null)
        {
            if (parentTrans.name.Equals(gameObjectName))
            {
                resultGO = parentTrans.gameObject;
            }
            else
            {
                resultGO = FindParentGameObjectByName(parentTrans, gameObjectName);
            }
        }
        return resultGO;
    }

    public static GameObject FindParentGameObjectByTag(Transform startTrans, string tag)
    {
        if (startTrans == null) return null;
        GameObject resultGO = null;
        Transform parentTrans = startTrans.parent;
        if (parentTrans != null)
        {
            if (parentTrans.name.Equals(tag))
            {
                resultGO = parentTrans.gameObject;
            }
            else
            {
                resultGO = FindParentGameObjectByTag(parentTrans, tag);
            }
        }
        return resultGO;
    }

    public static RaycastHit2D[] RaycastAll2D(Vector2 origin, Vector2 direction, float distance, float halfWidthOrHeight, int oneBorderRayNum, int layerMask)
    {
        List<RaycastHit2D> resultHit2DList = new List<RaycastHit2D>();
        if (oneBorderRayNum < 2) oneBorderRayNum = 2;

        int maxIdx = oneBorderRayNum - 1;
        for (int i = 0; i < oneBorderRayNum; i++)
        {
            float t = i / (float)maxIdx;
            float offset = Mathf.Lerp(0, halfWidthOrHeight, t);
            Vector2 l = Quaternion.AngleAxis(90f, Vector3.forward) * direction;
            Vector2 tempOrigin = origin + l * offset;
            resultHit2DList.AddRange(Physics2D.RaycastAll(tempOrigin, direction, distance, layerMask));
            Debug.DrawLine(tempOrigin, tempOrigin + (direction * distance), Color.yellow, 2);
            tempOrigin = origin - l * offset;
            resultHit2DList.AddRange(Physics2D.RaycastAll(tempOrigin, direction, distance, layerMask));
            Debug.DrawLine(tempOrigin, tempOrigin + (direction * distance), Color.yellow, 2);
        }

        return resultHit2DList.ToArray();
    }

    public static RaycastHit[] PieRaycastAll(Vector3 origin, float radius, Vector3 direction, float distance, Vector3 forward,Vector3 up, int crossNum, int loopNum, int layerMask)
    {
        crossNum = Mathf.Max(1, crossNum);
        int equantNum = crossNum * 4;
        loopNum = Mathf.Max(1, loopNum);

        List<RaycastHit> hitList = new List<RaycastHit>();
        // 正中心
        hitList.AddRange(Physics.RaycastAll(origin, direction, distance, layerMask));
        // 
        for (int i = 0; i < loopNum; i++)
        {

            float thisRadius = (i + 1) / (float)loopNum * radius;
            for (int j = 0; j < equantNum; j++)
            {
                float thisAngle = j / (float)equantNum * 360;
                Vector3 thisForward = Quaternion.AngleAxis(thisAngle, up) * forward;
                //Debug.Log("thisForward = " + thisForward);
                hitList.AddRange(Physics.RaycastAll(origin + thisForward * thisRadius, direction, distance, layerMask));
                Debug.DrawLine(origin + thisForward * thisRadius, origin + thisForward * thisRadius + (direction * distance), Color.yellow, 2);
            }
        }
        //Debug.Log("hitList.Count = " + hitList.Count);
        return hitList.ToArray();
    }

    public static bool IsCast(RaycastHit2D[] hit2Ds)
    {
        bool result = false;
        foreach (RaycastHit2D tempHit2D in hit2Ds)
        {
            result = result || (tempHit2D.transform != null);
            if (result)
            {
                break;
            }
        }
        return result;
    }

    public static bool IsCast(RaycastHit[] hits)
    {
        bool isCast = false;
        foreach (RaycastHit tempHit in hits)
        {
            isCast = isCast || (tempHit.transform != null);
            if (isCast)
            {
                break;
            }
        }
        return isCast;
    }
}
