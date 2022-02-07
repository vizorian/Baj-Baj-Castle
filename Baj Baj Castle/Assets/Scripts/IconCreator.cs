using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class IconCreator : MonoBehaviour
{
    public string Path;
    public string Name;
    private Camera _cam;

    [ContextMenu("Take screenshot")]
    void TakeScreenshot()
    {
        if(_cam == null)
        {
            _cam = GetComponent<Camera>();
        }

        RenderTexture rt = new RenderTexture(256, 256, 24);
        _cam.targetTexture = rt;

        Texture2D screenshot = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        _cam.Render();

        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        _cam.targetTexture = null;
        RenderTexture.active = null;

        if (Application.isEditor)
        {
            DestroyImmediate(rt);
        }
        else
        {
            Destroy(rt);
        }

        byte[] bytes = screenshot.EncodeToPNG();

        Debug.Log(bytes.Length);

        System.IO.File.WriteAllBytes(Path + Name + ".png", bytes);
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}
