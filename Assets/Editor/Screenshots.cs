using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Tools > Ember Moth > Capture Screenshots: снимает Menu, HUD и Pause для трёх устройств.
// Это эмуляция Device Simulator: размер экрана и Safe Area подставляются вручную.
public static class Screenshots
{
    struct Device { public string name; public int w, h; public RectInt safe; public bool notch; }

    // safe = left, bottom, width, height (в пикселях экрана); значения iPhone в альбомной ориентации
    static readonly Device[] Devices =
    {
        new Device { name = "16x9_1920x1080", w = 1920, h = 1080, safe = new RectInt(0, 0, 1920, 1080) },
        new Device { name = "notch_2532x1170", w = 2532, h = 1170, safe = new RectInt(141, 63, 2250, 1107), notch = true },
        new Device { name = "tablet_2048x1536", w = 2048, h = 1536, safe = new RectInt(0, 0, 2048, 1536) },
    };

    [MenuItem("Tools/Ember Moth/Capture Screenshots")]
    public static void CaptureAll()
    {
        Directory.CreateDirectory("Screenshots");
        foreach (var d in Devices)
        {
            Shot("Assets/Scenes/Menu.unity", d, "menu", null);
            Shot("Assets/Scenes/Level.unity", d, "hud", false);
            Shot("Assets/Scenes/Level.unity", d, "pause", true);
        }
        EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");
        SafeArea.Override = null;
        Debug.Log("Ember Moth: screenshots done");
    }

    static void Shot(string scenePath, Device d, string kind, bool? pause)
    {
        EditorSceneManager.OpenScene(scenePath);
        var cam = Object.FindFirstObjectByType<Camera>();
        var rt = new RenderTexture(d.w, d.h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        cam.targetTexture = rt;
        cam.aspect = d.w / (float)d.h;
        var fit = cam.GetComponent<CameraFit>();
        if (fit) fit.Fit();

        var canvas = Object.FindFirstObjectByType<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 5; canvas.sortingOrder = 100;

        SafeArea.Override = new Rect(d.safe.x, d.safe.y, d.safe.width, d.safe.height);
        SafeArea.OverrideScreen = new Vector2Int(d.w, d.h);

        if (pause.HasValue)
        {
            var gm = Object.FindFirstObjectByType<GameManager>();
            gm.scoreText.text = "SCORE 1250";
            gm.hearts[2].sprite = gm.heartEmpty;
            gm.moth.transform.position = new Vector3(-4, -2.2f, 0);
            Spawn(gm.emberPrefab, 1, -0.5f); Spawn(gm.emberPrefab, 5, 0.8f); Spawn(gm.emberPrefab, 8.5f, -1.2f);
            Spawn(gm.gloomPrefab, 3, -3f); Spawn(gm.gloomPrefab, 9.5f, -3f);
            gm.pausePanel.SetActive(pause.Value);
        }

        // в режиме редактора Canvas не пересчитывается сам
        foreach (var sa in Object.FindObjectsByType<SafeArea>(FindObjectsInactive.Include, FindObjectsSortMode.None)) sa.Apply();
        var scaler = canvas.GetComponent<CanvasScaler>();
        typeof(CanvasScaler).GetMethod("Handle", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(scaler, null);
        Canvas.ForceUpdateCanvases();
        cam.Render();

        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(d.w, d.h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, d.w, d.h), 0, 0);
        RenderTexture.active = prev;

        if (d.notch) DrawNotch(tex, d);
        File.WriteAllBytes($"Screenshots/{kind}_{d.name}.png", tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        rt.Release();
    }

    static void Spawn(GameObject prefab, float x, float y)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.position = new Vector3(x, y, 0);
    }

    // вырез слева (как в брифе) + жёлтая рамка Safe Area
    static void DrawNotch(Texture2D t, Device d)
    {
        int nh = (int)(d.h * 0.36f), y0 = (d.h - nh) / 2;
        for (int y = y0; y < y0 + nh; y++)
            for (int x = 0; x < 64; x++) t.SetPixel(x, y, Color.black);
        var yellow = new Color(0.96f, 0.78f, 0.26f);
        for (int x = d.safe.x; x < d.safe.xMax; x++) { t.SetPixel(x, d.safe.yMax - 1, yellow); t.SetPixel(x, d.safe.y, yellow); }
        for (int y = d.safe.y; y < d.safe.yMax; y++) { t.SetPixel(d.safe.x, y, yellow); t.SetPixel(d.safe.xMax - 1, y, yellow); }
        t.Apply();
    }
}
