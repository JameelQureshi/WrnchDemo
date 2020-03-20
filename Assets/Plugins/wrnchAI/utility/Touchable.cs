/*
 Copyright (c) 2019 Wrnch Inc.
 All rights reserved
*/

// Touchable component
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;

public class Touchable : Text
{
    protected override void Awake()
    {
        base.Awake();
    }
}

#if UNITY_EDITOR

// Touchable_Editor component, to prevent treating the component as a Text object.
[CustomEditor(typeof(Touchable))]
public class Touchable_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        // Do nothing
    }
}
#endif
