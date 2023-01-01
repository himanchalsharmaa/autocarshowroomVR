using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[UnityEditor.CustomEditor(typeof(RenderToTexture))]
public class RenderToTextureEditor : UnityEditor.Editor
{
#if UNITY_EDITOR

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        RenderToTexture ren2Tex = (RenderToTexture)target;

        if (Application.isPlaying && ren2Tex.colorTex != null)
        {
            GUILayout.Label($"{ren2Tex.colorTex.name}:");
            GUILayout.Label(ren2Tex.colorTex, GUILayout.Height(200));
        }

        if (Application.isPlaying && ren2Tex.depthTex != null)
        {
            GUILayout.Label($"{ren2Tex.depthTex.name}:");
            GUILayout.Label(ren2Tex.depthTex, GUILayout.Height(200));
        }
    }

    public override bool RequiresConstantRepaint()
    {
        RenderToTexture ren2Tex = (RenderToTexture)target;
        if (Application.isPlaying && ren2Tex.colorTex != null)
        {
            return true;
        }
        return base.RequiresConstantRepaint();
    }

#endif
}
