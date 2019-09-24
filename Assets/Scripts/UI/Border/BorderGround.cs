using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BorderGround : MonoBehaviour
{
    public Color borderColor = Color.white;
    public float borderWidth = 2;
    private RectTransform[] borderTranss;
    private Image[] borderImages;

    // Start is called before the first frame update
    void Start()
    {
        borderImages = transform.GetComponentsInChildren<Image>();
        borderTranss = new RectTransform[borderImages.Length];
        for(int i = 0;i<borderImages.Length;i++)
        {
            borderTranss[i] = borderImages[i].rectTransform;
        }

        SetBorderColor();
        SetBorderWidth();
    }

    private void SetBorderColor()
    {
        foreach(Image tempImage in borderImages)
        {
            tempImage.color = borderColor;
        }
    }

    private void SetBorderWidth()
    {
        foreach(RectTransform tempTrans in borderTranss)
        {
            BorderMask tempMask = tempTrans.GetComponent<BorderMask>();
            Vector2 sizeDelta = Vector2.zero;
            switch (tempMask.borderSite)
            {
                case BorderSite.Left:
                case BorderSite.Right:
                    sizeDelta = Vector2.right * borderWidth + Vector2.up * borderWidth * 2;
                    break;
                case BorderSite.Top:
                case BorderSite.Bottom:
                    sizeDelta = Vector2.right * borderWidth * 2 + Vector2.up * borderWidth;
                    break;
            }
            tempTrans.sizeDelta = sizeDelta;
        }
    }
}
