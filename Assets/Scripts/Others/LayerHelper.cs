using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerHelper
{
    /// <summary>
    /// 关闭两个layer的碰撞检测
    /// </summary>
    /// <param name="layerName1"></param>
    /// <param name="layerName2"></param>
    public static void OffLayerCollisionMask(string layerName1, string layerName2)
    {
        OffLayerCollisionMask(LayerMask.NameToLayer(layerName1), LayerMask.NameToLayer(layerName2));
    }

    /// <summary>
    /// 关闭两个layer的碰撞检测
    /// </summary>
    /// <param name="layer1"></param>
    /// <param name="layer2"></param>
    public static void OffLayerCollisionMask(int layer1, int layer2)
    {
        Physics2D.SetLayerCollisionMask(layer1, GetLayerMask(layer2, false));
    }

    /// <summary>
    /// 打开两个layer的碰撞检测
    /// </summary>
    /// <param name="layerName1"></param>
    /// <param name="layerName2"></param>
    public static void OnLayerCollisionMask(string layerName1, string layerName2)
    {
        OnLayerCollisionMask(LayerMask.NameToLayer(layerName1), LayerMask.NameToLayer(layerName2));
    }

    /// <summary>
    /// 打开两个layer的碰撞检测
    /// </summary>
    /// <param name="layer1"></param>
    /// <param name="layer2"></param>
    public static void OnLayerCollisionMask(int layer1, int layer2)
    {
        Physics2D.SetLayerCollisionMask(layer1, GetLayerMask(layer2, true));
    }

    public static int GetLayerMask(string layerName, bool on)
    {
        return GetLayerMask(LayerMask.NameToLayer(layerName), on);
    }

    public static int GetLayerMask(int layer, bool on)
    {
        // 位移值
        int d = on ? 1 : 0;
        return layer >= 0 && layer < 32 ? d << layer : layer;
    }

    public static int GetLayerMask(string[] layerNames, bool on)
    {
        int[] layers = new int[layerNames.Length];
        for (int i = 0; i < layerNames.Length; i++)
        {
            layers[i] = LayerMask.NameToLayer(layerNames[i]);
        }
        return GetLayerMask(layers, on);
    }

    public static int GetLayerMask(int[] layers, bool on)
    {
        int layerMask = 0;
        // 位移值
        int d = on ? 1 : 0;
        foreach (int tempLayer in layers)
        {
            if(tempLayer >=0 && tempLayer < 32)
            {
                layerMask = layerMask | (d << tempLayer);
            }
        }
        return layerMask;
    }
}
