/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class DeviceChange : MonoBehaviour
{

    static DeviceChange instance = null;
    public static DeviceChange Instance { get { return instance; } }
    public static ScreenOrientation deviceOrientation;

    public delegate void OnOrientationChange();
    public static OnOrientationChange OrientationChange;

    public static void init()
    {
        if (instance != null) return;

        deviceOrientation = Screen.orientation;

        // create the canvas object to detect orientation change
        GameObject canvas = new GameObject("DeviceChange");
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        instance = canvas.AddComponent<DeviceChange>();
    }

    void Start()
    {
        if (instance != this)
        {
            Destroy(this);
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        ScreenOrientation orientation = Screen.orientation;
        if (deviceOrientation != orientation)
        {
            deviceOrientation = orientation;
            if (OrientationChange != null)
            {
                OrientationChange();
            }
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
