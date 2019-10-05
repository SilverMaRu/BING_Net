using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public Transform followTrans;
    public float sensitivityH = 5;
    public float sensitivityV = 5;
    // 垂直方向最大偏移角度
    public float limitAngleV = 75;
    public static Transform rigTrans { get; private set; }
    public static Transform pivotTrans { get; private set; }
    public static Transform cameraTrans { get; private set; }

    private void Awake()
    {
        rigTrans = transform;
        pivotTrans = transform.Find("Pivot");
        cameraTrans = Camera.main.transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        Client.ins.SpawnedLocalPlayerEvent += OnSpawnedLocalPlayer;
    }

    private void OnNetCodeChanged()
    {
        followTrans = Client.ins.GetLocalPlayerGO().transform;
    }

    private void OnSpawnedLocalPlayer()
    {
        followTrans = Client.ins.GetLocalPlayerGO().transform;
    }

    // Update is called once per frame
    void Update()
    {
        rigTrans.position = followTrans.position;
        rigTrans.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * sensitivityH, 0);

        float mouseY = -Input.GetAxis("Mouse Y");
        float angleOfV = (pivotTrans.localRotation * Quaternion.Euler(mouseY * sensitivityV, 0, 0)).eulerAngles.x;
        //Debug.Log("angleOfV = " + angleOfV);
        if (angleOfV > limitAngleV && angleOfV < 360 - limitAngleV)
        {
            if (mouseY > 0)
            {
                angleOfV = limitAngleV;
            }
            else if (mouseY < 0)
            {
                angleOfV = -limitAngleV;
            }
        }
        pivotTrans.localRotation = Quaternion.Euler(angleOfV, 0, 0);
    }
}
