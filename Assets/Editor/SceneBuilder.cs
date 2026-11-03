using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Tools > Ember Moth > Build All: рисует спрайты кодом и собирает сцены Menu и Level.
// Всё воспроизводимо: сцены можно пересобрать одной командой.
public static class SceneBuilder
{
    const string Art = "Assets/Art/";
    static readonly Color Night = Hex("0B1030"), Amber = Hex("FFB547"), Cream = Hex("FFF4DC"), Navy = Hex("3A3F8F");

    static Sprite px, disc, glow, rrect, heartOn, heartOff, moth, ember, gloom;
    static Font font;

    public static Color Hex(string h) { ColorUtility.TryParseHtmlString("#" + h, out var c); return c; }

    [MenuItem("Tools/Ember Moth/Build All")]
    public static void BuildAll()
    {
        Directory.CreateDirectory(Art);
        Directory.CreateDirectory("Assets/Prefabs");
        MakeSprites();
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildMenu();
        BuildLevel();
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Menu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Level.unity", true),
        };
        PlayerSettings.productName = "Ember Moth";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        AssetDatabase.SaveAssets();
        Debug.Log("Ember Moth: build done");
    }

    // ---------- спрайты, нарисованные кодом ----------

    static Color Over(Color top, Color bottom)
    {
        float a = top.a + bottom.a * (1 - top.a);
        if (a <= 0) return Color.clear;
        var c = (top * top.a + bottom * bottom.a * (1 - top.a)) / a;
        c.a = a;
        return c;
    }

    static Sprite Save(string name, int w, int h, Func<float, float, Color> f, float ppu, int border = 0)
    {
        const int ss = 3;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color sum = Color.clear; float asum = 0;
                for (int sy = 0; sy < ss; sy++)
                    for (int sx = 0; sx < ss; sx++)
                    {
                        float u = ((x + (sx + .5f) / ss) / w) * 2 - 1, v = ((y + (sy + .5f) / ss) / h) * 2 - 1;
                        var c = f(u, v);
                        sum += c * c.a; asum += c.a;
                    }
                var o = asum > 0 ? sum / asum : Color.clear;
                o.a = asum / (ss * ss);
                tex.SetPixel(x, y, o);
            }
        string path = Art + name + ".png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.spritePixelsPerUnit = ppu;
        imp.spriteBorder = new Vector4(border, border, border, border);
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static bool InEllipse(float x, float y, float cx, float cy, float rx, float ry, float deg, out float edge)
    {
        float a = deg * Mathf.Deg2Rad, dx = x - cx, dy = y - cy;
        float px_ = dx * Mathf.Cos(a) + dy * Mathf.Sin(a), py_ = -dx * Mathf.Sin(a) + dy * Mathf.Cos(a);
        edge = Mathf.Sqrt(px_ * px_ / (rx * rx) + py_ * py_ / (ry * ry));
        return edge <= 1;
    }

    static Color Wing(float u, float v, float cx, float cy, float rx, float ry, float deg, Color fill, Color rim, Color under)
    {
        if (!InEllipse(u, v, cx, cy, rx, ry, deg, out var e)) return under;
        return Over(e > 0.86f ? rim : fill, under);
    }

    static void MakeSprites()
    {
        px = Save("px", 4, 4, (u, v) => Color.white, 4);
        disc = Save("disc", 128, 128, (u, v) => u * u + v * v <= 1 ? Color.white : Color.clear, 100);
        glow = Save("glow", 128, 128, (u, v) =>
        {
            float r = Mathf.Sqrt(u * u + v * v);
            return new Color(1, 1, 1, r >= 1 ? 0 : Mathf.Pow(1 - r, 2.2f));
        }, 100);

        // скруглённый прямоугольник под 9-slice: кнопки и карточки
        rrect = Save("rrect", 128, 128, (u, v) =>
        {
            float r = 0.4f, qx = Mathf.Max(Mathf.Abs(u) - (1 - r), 0), qy = Mathf.Max(Mathf.Abs(v) - (1 - r), 0);
            return qx * qx + qy * qy <= r * r ? Color.white : Color.clear;
        }, 100, 48);

        Func<Color, Func<float, float, Color>> heart = col => (u, v) =>
        {
            float x = u * 1.25f, y = v * 1.25f + 0.15f, a = x * x + y * y - 1;
            return a * a * a - x * x * y * y * y <= 0 ? col : Color.clear;
        };
        heartOn = Save("heart_on", 96, 96, heart(Hex("FF5A6E")), 100);
        heartOff = Save("heart_off", 96, 96, heart(new Color(1, 1, 1, 0.22f)), 100);

        moth = Save("moth", 192, 192, (u, v) =>
        {
            Color c = Color.clear;
            c = Wing(u, v, -0.34f, -0.18f, 0.32f, 0.2f, 30, Hex("E9C98B"), Hex("B58A4A"), c);
            c = Wing(u, v, 0.34f, -0.18f, 0.32f, 0.2f, -30, Hex("E9C98B"), Hex("B58A4A"), c);
            c = Wing(u, v, -0.4f, 0.2f, 0.48f, 0.3f, -25, Hex("F6E7C1"), Hex("C9A66B"), c);
            c = Wing(u, v, 0.4f, 0.2f, 0.48f, 0.3f, 25, Hex("F6E7C1"), Hex("C9A66B"), c);
            if (InEllipse(u, v, 0, 0, 0.11f, 0.45f, 0, out _)) c = Over(Hex("3B2A4F"), c);
            if (InEllipse(u, v, 0, 0.5f, 0.1f, 0.1f, 0, out _)) c = Over(Hex("3B2A4F"), c);
            if (InEllipse(u, v, 0, -0.6f, 0.13f, 0.13f, 0, out _)) c = Over(Hex("FFC24D"), c); // фонарик на хвосте
            return c;
        }, 140);

        ember = Save("ember", 96, 96, (u, v) =>
        {
            float r = Mathf.Sqrt(u * u + v * v);
            if (r < 0.36f) return Hex("FFE9A8");
            if (r < 0.5f) return Hex("FFB547");
            return new Color(1, 0.62f, 0.2f, r >= 1 ? 0 : Mathf.Pow(1 - r, 1.8f) * 0.8f);
        }, 100);

        gloom = Save("gloom", 128, 128, (u, v) =>
        {
            // тень: капля с мерцающим краем и двумя глазами
            Color c = Color.clear;
            float edge;
            if (InEllipse(u, v, 0, -0.1f, 0.9f, 0.75f, 0, out edge)) c = edge > 0.88f ? Hex("7B5FD0") : Hex("2A1B4D");
            if (InEllipse(u, v, -0.3f, 0, 0.17f, 0.2f, 0, out _)) c = Hex("FFF4DC");
            if (InEllipse(u, v, 0.3f, 0, 0.17f, 0.2f, 0, out _)) c = Hex("FFF4DC");
            if (InEllipse(u, v, -0.34f, -0.02f, 0.07f, 0.1f, 0, out _)) c = Hex("0B1030");
            if (InEllipse(u, v, 0.26f, -0.02f, 0.07f, 0.1f, 0, out _)) c = Hex("0B1030");
            return c;
        }, 100);
    }

    // ---------- хелперы для UI ----------

    static RectTransform Rt(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static RectTransform Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    // Элемент, привязанный к якорю (угол/край/центр); pos — отступ от якоря.
    static RectTransform Anchored(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var rt = Rt(parent, name);
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static Image Img(RectTransform rt, Sprite s, Color c, bool sliced = false, bool raycast = false)
    {
        var i = rt.gameObject.AddComponent<Image>();
        i.sprite = s; i.color = c; i.raycastTarget = raycast;
        if (sliced) i.type = Image.Type.Sliced;
        return i;
    }

    static Text Label(Transform parent, string name, string text, int size, Vector2 anchor, Vector2 pos, Vector2 box, Color col)
    {
        var rt = Anchored(parent, name, anchor, pos, box);
        var t = rt.gameObject.AddComponent<Text>();
        t.text = text; t.font = font; t.fontSize = size; t.fontStyle = FontStyle.Bold;
        t.alignment = TextAnchor.MiddleCenter; t.color = col; t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        var o = rt.gameObject.AddComponent<Outline>();
        o.effectColor = new Color(0.03f, 0.04f, 0.14f, 0.85f); o.effectDistance = new Vector2(3, -3);
        return t;
    }

    static Button MakeButton(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, Vector2 size, Color bg, Color fg, int fs = 60)
    {
        var rt = Anchored(parent, name, anchor, pos, size);
        Img(rt, rrect, bg, true, true);
        var b = rt.gameObject.AddComponent<Button>();
        var t = Label(rt, "Label", label, fs, new Vector2(.5f, .5f), Vector2.zero, size, fg);
        Stretch(t.rectTransform);
        Destroy(t.GetComponent<Outline>());
        return b;
    }

    static void Destroy(UnityEngine.Object o) { UnityEngine.Object.DestroyImmediate(o); }

    static RectTransform MakeCanvas(out Canvas canvas)
    {
        var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = go.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.matchWidthOrHeight = 1f;
        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        return (RectTransform)go.transform;
    }

    static RectTransform MakeSafeArea(Transform parent)
    {
        var rt = Stretch(Rt(parent, "SafeArea"));
        rt.gameObject.AddComponent<SafeArea>();
        return rt;
    }

    static Camera MakeCamera(bool fit)
    {
        var go = new GameObject("Main Camera", typeof(Camera)) { tag = "MainCamera" };
        var cam = go.GetComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 6;
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Night;
        go.transform.position = new Vector3(0, 0, -10);
        if (fit) go.AddComponent<CameraFit>();
        return cam;
    }

    static SpriteRenderer Spr(Transform parent, string name, Sprite s, Color c, Vector2 pos, Vector2 scale, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos; go.transform.localScale = new Vector3(scale.x, scale.y, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s; sr.color = c; sr.sortingOrder = order;
        sr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        return sr;
    }

    // ---------- сцена Menu ----------

    static void BuildMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MakeCamera(false);
        var root = MakeCanvas(out _);

        // фон на весь экран (за пределами SafeArea): ночное небо, луна, холмы
        var bg = Stretch(Rt(root, "Background"));
        Img(Stretch(Rt(bg, "Sky")), px, Hex("1B1A4E"));
        Img(Anchored(bg, "Horizon", new Vector2(.5f, 0), new Vector2(0, 300), new Vector2(3000, 700)), glow, new Color(1f, .45f, .35f, .55f));
        Img(Anchored(bg, "Moon", new Vector2(.82f, .8f), Vector2.zero, new Vector2(300, 300)), glow, new Color(1f, .93f, .75f, .9f));
        var rng = new System.Random(7);
        for (int i = 0; i < 40; i++)
        {
            float s = 6 + (float)rng.NextDouble() * 10;
            Img(Anchored(bg, "Star", new Vector2((float)rng.NextDouble(), .35f + (float)rng.NextDouble() * .65f), Vector2.zero, new Vector2(s, s)), disc, new Color(1, .96f, .85f, .7f));
        }
        Img(Anchored(bg, "Hill", new Vector2(.25f, 0), new Vector2(0, -60), new Vector2(1700, 520)), disc, Hex("141238"));
        Img(Anchored(bg, "Hill2", new Vector2(.85f, 0), new Vector2(0, -90), new Vector2(1600, 480)), disc, Hex("0D0C2B"));
        Img(Anchored(bg, "MothArt", new Vector2(.5f, .5f), new Vector2(-470, -20), new Vector2(380, 380)), glow, new Color(1f, .75f, .3f, .55f));
        Img(Anchored(bg, "MothArtSprite", new Vector2(.5f, .5f), new Vector2(-470, -20), new Vector2(300, 300)), moth, Color.white);

        var safe = MakeSafeArea(root);
        Label(safe, "Title", "EMBER MOTH", 120, new Vector2(.5f, 1), new Vector2(100, -170), new Vector2(1100, 170), Amber);
        Label(safe, "Tagline", "light the night, dodge the gloom", 52, new Vector2(.5f, 1), new Vector2(100, -290), new Vector2(1100, 80), Cream);

        var menu = new GameObject("MainMenu").AddComponent<MainMenu>();
        var play = MakeButton(safe, "PlayButton", "PLAY", new Vector2(.5f, .5f), new Vector2(240, -80), new Vector2(560, 160), Amber, Night, 76);
        var quit = MakeButton(safe, "QuitButton", "QUIT", new Vector2(.5f, .5f), new Vector2(240, -280), new Vector2(560, 140), Navy, Cream, 60);
        UnityEventTools.AddPersistentListener(play.onClick, menu.Play);
        UnityEventTools.AddPersistentListener(quit.onClick, menu.Quit);

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Menu.unity");
    }

    // ---------- сцена Level ----------

    static GameObject MakePrefab(string name, Sprite s, bool harmful, float radius)
    {
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s; sr.sortingOrder = 3;
        sr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        var col = go.AddComponent<CircleCollider2D>();
        col.radius = radius; col.isTrigger = true;
        go.AddComponent<Drifter>().harmful = harmful;
        go.transform.localScale = Vector3.one * (harmful ? 1.3f : 0.9f);
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/" + name + ".prefab");
        UnityEngine.Object.DestroyImmediate(go);
        return prefab;
    }

    static void BuildLevel()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MakeCamera(true);

        var world = new GameObject("World").transform;
        // небо: градиент «ночь сверху → тёплый закат у земли»
        var grad = Save("sky", 4, 256, (u, v) =>
        {
            float t = (v + 1) / 2;
            var low = Hex("7A3F73"); var mid = Hex("2B2A63"); var top = Hex("0B1030");
            return t < .38f ? Color.Lerp(Hex("2A1B52"), low, t / .38f * 0.9f + 0.1f) : t < .62f ? Color.Lerp(low, mid, (t - .38f) / .24f) : Color.Lerp(mid, top, (t - .62f) / .38f);
        }, 100);
        Spr(world, "Sky", grad, Color.white, new Vector2(0, 0), new Vector2(2000, 22f / 2.56f), -20);
        Spr(world, "Moon", glow, new Color(1f, .93f, .75f, .9f), new Vector2(7, 2.5f), new Vector2(3.4f, 3.4f), -18);
        var rng = new System.Random(3);
        for (int i = 0; i < 70; i++)
        {
            float s = 0.05f + (float)rng.NextDouble() * 0.1f;
            Spr(world, "Star", disc, new Color(1, .96f, .85f, .75f), new Vector2((float)rng.NextDouble() * 40 - 20, (float)rng.NextDouble() * 9 - 1f), new Vector2(s, s), -19);
        }
        Spr(world, "HillFar", disc, Hex("1B1A4E"), new Vector2(-7, -5.2f), new Vector2(18, 4.6f), -15);
        Spr(world, "HillFar2", disc, Hex("1B1A4E"), new Vector2(9, -5.4f), new Vector2(16, 4f), -15);
        Spr(world, "HillNear", disc, Hex("11103A"), new Vector2(1, -5.6f), new Vector2(22, 3.4f), -12);

        var ground = Spr(world, "Ground", px, Hex("0D0C2B"), new Vector2(0, -9), new Vector2(200, 11), -5);
        var gcol = ground.gameObject.AddComponent<BoxCollider2D>();
        gcol.size = Vector2.one;
        Spr(world, "GrassLine", px, Hex("5B4AA8"), new Vector2(0, -3.5f), new Vector2(200, 0.12f), -4);

        // герой
        var mothGo = new GameObject("Moth");
        mothGo.transform.position = new Vector3(-4, -3, 0);
        var msr = mothGo.AddComponent<SpriteRenderer>();
        msr.sprite = moth; msr.sortingOrder = 5;
        msr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        var rb = mothGo.AddComponent<Rigidbody2D>();
        rb.gravityScale = 4; rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        mothGo.AddComponent<CircleCollider2D>().radius = 0.5f;
        var mothComp = mothGo.AddComponent<Moth>();
        Spr(mothGo.transform, "Aura", glow, new Color(1, .75f, .3f, .35f), new Vector2(-4, -3), new Vector2(4, 4), 4);

        var emberPrefab = MakePrefab("Ember", ember, false, 0.5f);
        var gloomPrefab = MakePrefab("Gloom", gloom, true, 0.4f);

        // ---------- HUD ----------
        var root = MakeCanvas(out _);
        var safe = MakeSafeArea(root);

        var hearts = new Image[3];
        var heartRow = Anchored(safe, "Hearts", new Vector2(0, 1), new Vector2(40, -40), new Vector2(320, 100));
        heartRow.pivot = new Vector2(0, 1);
        for (int i = 0; i < 3; i++)
        {
            var h = Anchored(heartRow, "Heart" + i, new Vector2(0, .5f), new Vector2(i * 106, 0), new Vector2(96, 96));
            h.pivot = new Vector2(0, .5f);
            hearts[i] = Img(h, heartOn, Color.white);
        }

        var scoreBox = Anchored(safe, "Score", new Vector2(.5f, 1), new Vector2(0, -40), new Vector2(460, 110));
        scoreBox.pivot = new Vector2(.5f, 1);
        Img(scoreBox, rrect, new Color(0.04f, 0.05f, 0.2f, 0.6f), true);
        var scoreText = Label(scoreBox, "Text", "SCORE 0", 64, new Vector2(.5f, .5f), Vector2.zero, new Vector2(460, 110), Cream);
        Stretch(scoreText.rectTransform);

        var pauseBtn = MakeButton(safe, "PauseButton", "II", new Vector2(1, 1), new Vector2(-40, -40), new Vector2(130, 130), new Color(0.04f, 0.05f, 0.2f, 0.7f), Cream, 70);
        pauseBtn.GetComponent<RectTransform>().pivot = new Vector2(1, 1);

        // стик внизу слева
        var stickRt = Anchored(safe, "Stick", new Vector2(0, 0), new Vector2(60, 60), new Vector2(320, 320));
        stickRt.pivot = new Vector2(0, 0);
        Img(stickRt, disc, new Color(1, 1, 1, 0.16f), false, true);
        var knobRt = Anchored(stickRt, "Knob", new Vector2(.5f, .5f), Vector2.zero, new Vector2(150, 150));
        Img(knobRt, disc, new Color(1f, .71f, .28f, .9f));
        var stick = stickRt.gameObject.AddComponent<VirtualStick>();
        stick.knob = knobRt;

        // прыжок внизу справа
        var jumpRt = Anchored(safe, "Jump", new Vector2(1, 0), new Vector2(-60, 70), new Vector2(250, 250));
        jumpRt.pivot = new Vector2(1, 0);
        Img(jumpRt, disc, new Color(1f, .71f, .28f, .85f), false, true);
        jumpRt.gameObject.AddComponent<JumpButton>();
        Label(jumpRt, "Label", "JUMP", 56, new Vector2(.5f, .5f), Vector2.zero, new Vector2(250, 100), Night).GetComponent<Outline>().enabled = false;

        // ---------- пауза ----------
        var pause = Stretch(Rt(root, "PausePanel"));
        Img(pause, px, new Color(0.02f, 0.03f, 0.1f, 0.72f), false, true); // затемнение на весь экран, ловит клики
        var psafe = MakeSafeArea(pause);
        var card = Anchored(psafe, "Card", new Vector2(.5f, .5f), Vector2.zero, new Vector2(820, 820));
        Img(card, rrect, new Color(0.09f, 0.1f, 0.27f, 0.97f), true);
        var title = Label(card, "Title", "PAUSED", 100, new Vector2(.5f, 1), new Vector2(0, -110), new Vector2(760, 140), Amber);
        var resume = MakeButton(card, "ResumeButton", "RESUME", new Vector2(.5f, 1), new Vector2(0, -290), new Vector2(560, 150), Amber, Night, 64);
        var restart = MakeButton(card, "RestartButton", "RESTART", new Vector2(.5f, 1), new Vector2(0, -480), new Vector2(560, 140), Navy, Cream, 56);
        var toMenu = MakeButton(card, "MenuButton", "MENU", new Vector2(.5f, 1), new Vector2(0, -660), new Vector2(560, 140), Navy, Cream, 56);
        foreach (var b in new[] { resume, restart, toMenu }) b.GetComponent<RectTransform>().pivot = new Vector2(.5f, 1);
        // кнопки привязаны по верхнему краю карточки; сдвиг на половину высоты даёт «центр кнопки» в указанной позиции
        resume.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -250);
        restart.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -440);
        toMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -620);

        // ---------- логика ----------
        var gm = new GameObject("GameManager").AddComponent<GameManager>();
        gm.emberPrefab = emberPrefab; gm.gloomPrefab = gloomPrefab;
        gm.hearts = hearts; gm.scoreText = scoreText; gm.pauseTitle = title;
        gm.pausePanel = pause.gameObject; gm.resumeButton = resume.gameObject;
        gm.heartFull = heartOn; gm.heartEmpty = heartOff; gm.moth = mothComp;
        UnityEventTools.AddPersistentListener(pauseBtn.onClick, gm.Pause);
        UnityEventTools.AddPersistentListener(resume.onClick, gm.Resume);
        UnityEventTools.AddPersistentListener(restart.onClick, gm.Restart);
        UnityEventTools.AddPersistentListener(toMenu.onClick, gm.ToMenu);

        pause.gameObject.SetActive(false);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Level.unity");
    }
}
