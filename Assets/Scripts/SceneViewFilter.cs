using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneViewFilter : MonoBehaviour
{
#if UNITY_EDITOR
    private bool _hasChanged;

    public void OnValidate()
    {
        _hasChanged = true;
    }

    static SceneViewFilter()
    {
        SceneView.duringSceneGui += Check;
    }

    private static void Check(SceneView sceneView)
    {
        if (Event.current.type != EventType.Layout) return;
        if (!Camera.main) return;

        SceneViewFilter[] cameraFilters = Camera.main.GetComponents<SceneViewFilter>();
        SceneViewFilter[] sceneFilters = sceneView.camera.GetComponents<SceneViewFilter>();

        if (cameraFilters.Length != sceneFilters.Length)
        {
            Recreate(sceneView);
            return;
        }

        for (int i = 0; i < cameraFilters.Length; i++)
        {
            if (cameraFilters[i].GetType() != sceneFilters[i].GetType())
            {
                Recreate(sceneView);
                return;
            }
        }

        for (int i = 0; i < cameraFilters.Length; i++)
        {
            if (cameraFilters[i]._hasChanged || sceneFilters[i].enabled != cameraFilters[i].enabled)
            {
                EditorUtility.CopySerialized(cameraFilters[i], sceneFilters[i]);
                cameraFilters[i]._hasChanged = false;
            }
        }
    }

    private static void Recreate(SceneView sceneView)
    {
        SceneViewFilter filter;
        while (filter = sceneView.camera.GetComponent<SceneViewFilter>())
        {
            DestroyImmediate(filter);
        }

        foreach (SceneViewFilter f in Camera.main.GetComponents<SceneViewFilter>())
        {
            SceneViewFilter newFilter = sceneView.camera.gameObject.AddComponent(f.GetType()) as SceneViewFilter;
            EditorUtility.CopySerialized(f, newFilter);
        }
    }
#endif
}