#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public static class CreateSnowAndColdFX_Builtin
{
    [MenuItem("Tools/Snow Kit (Built-in)/Create Snow + Cold FX Prefabs")]
    public static void CreateAll()
    {
        try
        {
            // 1) Thư mục
            CreateDir("Assets/Prefabs/FX");
            CreateDir("Assets/Prefabs/Lighting");
            CreateDir("Assets/Materials");
            CreateDir("Assets/Textures");

            // 2) Materials an toàn (có fallback shader)
            var snowShader = SafeFindShader(
                "Particles/Standard Unlit",
                "Unlit/Transparent",
                "Sprites/Default"
            );
            var snowMat = new Material(snowShader);
            AssetDatabase.CreateAsset(snowMat, "Assets/Materials/SnowParticles_Unlit.mat");

            var glowSprite     = CreateRadialSprite("Assets/Textures/GlowRadial.png",     256, invert:false, Color.white);
            var vignetteSprite = CreateRadialSprite("Assets/Textures/VignetteRadial.png", 512, invert:true,  Color.black);

            var glowShader = SafeFindShader(
                "Particles/Additive",
                "Legacy Shaders/Particles/Additive",
                "Unlit/Transparent",
                "Sprites/Default"
            );
            var glowMat = new Material(glowShader);
            if (glowSprite && glowMat.HasProperty("_MainTex"))
                glowMat.mainTexture = glowSprite.texture;
            AssetDatabase.CreateAsset(glowMat, "Assets/Materials/Glow_Additive.mat");

            // 3) Snow prefabs
            var far   = CreateSnowPrefab("Snow_Far",   snowMat, 30, 0.03f,0.06f, 0.2f,0.6f, 10,14, 0.2f);
            var mid   = CreateSnowPrefab("Snow_Mid",   snowMat, 60, 0.05f,0.10f, 0.4f,1.0f,  8,12, 0.35f);
            var front = CreateSnowPrefab("Snow_Front", snowMat, 90, 0.08f,0.14f, 0.8f,1.4f,  6,10, 0.5f);

            SetupSnow(far,   EnsureSortingLayer("BackgroundNear"), 0, 0.80f);
            SetupSnow(mid,   EnsureSortingLayer("Props"),          5, 0.95f);
            SetupSnow(front, EnsureSortingLayer("Foreground"),    10, 1.00f);

            // 4) FakePointLight (sprite additive)
            var fakeLight = new GameObject("FakePointLight");
            var sr = fakeLight.AddComponent<SpriteRenderer>();
            sr.sprite  = glowSprite;
            sr.material = glowMat;
            sr.color   = new Color(0.65f, 0.90f, 1f, 1f);
            sr.sortingLayerName = EnsureSortingLayer("VFX");
            fakeLight.transform.localScale = new Vector3(3f,3f,1f);
            SavePrefab(fakeLight, "Assets/Prefabs/Lighting/FakePointLight.prefab");

            // 5) ColdOverlay (UI tint + vignette)
            var overlay = new GameObject("ColdOverlay");
            var canvas = overlay.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = overlay.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920,1080);

            var tintGO = new GameObject("Tint");
            tintGO.transform.SetParent(overlay.transform, false);
            var tintImg = tintGO.AddComponent<Image>();
            tintImg.color = new Color(0.55f, 0.85f, 1f, 0.10f);
            StretchRect(tintImg.rectTransform);

            var vigGO = new GameObject("Vignette");
            vigGO.transform.SetParent(overlay.transform, false);
            var vigImg = vigGO.AddComponent<Image>();
            vigImg.sprite = vignetteSprite;
            vigImg.color  = new Color(1f,1f,1f,0.35f);
            StretchRect(vigImg.rectTransform);

            SavePrefab(overlay, "Assets/Prefabs/Lighting/ColdOverlay.prefab");

            EditorUtility.DisplayDialog("Snow Kit (Built-in)",
                "Đã tạo:\n- FX: Snow_Far/Mid/Front\n- Lighting: ColdOverlay (UI tint + vignette), FakePointLight (additive)",
                "OK");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Snow Kit Built-in] Lỗi khi tạo prefab: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            throw; // để Unity highlight lỗi
        }
    }

    // ---------- helpers ----------
    static Shader SafeFindShader(params string[] names)
    {
        foreach (var n in names)
        {
            var s = Shader.Find(n);
            if (s) return s;
        }
        // cực chẳng đã: lấy shader mặc định
        return Shader.Find("Sprites/Default");
    }

    static string EnsureSortingLayer(string name)
    {
        foreach (var l in SortingLayer.layers)
            if (l.name == name) return name;
        return "Default";
    }

    static void CreateDir(string path)
    {
        path = path.Replace("\\","/");
        if (AssetDatabase.IsValidFolder(path)) return;

        var parent = Path.GetDirectoryName(path).Replace("\\","/");
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            CreateDir(parent); // đệ quy tạo cha trước

        var name = Path.GetFileName(path);
        if (string.IsNullOrEmpty(name)) return;
        AssetDatabase.CreateFolder(string.IsNullOrEmpty(parent) ? "Assets" : parent, name);
    }

    static GameObject CreateSnowPrefab(string name, Material mat, int rate,
        float sizeMin,float sizeMax, float speedMin,float speedMax,
        float lifeMin,float lifeMax, float noise)
    {
        var go = new GameObject(name);
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true; main.duration = 10f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
        main.startSize     = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
        main.gravityModifier = 0.12f;

        var emission = ps.emission; emission.rateOverTime = rate;

        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(20f, 0.1f, 1f); // sẽ auto theo camera bởi SnowFollowCam

        var noiseM = ps.noise; noiseM.enabled = true;
        noiseM.strength = noise; noiseM.frequency = 0.3f; noiseM.scrollSpeed = 0.15f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = mat;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        go.AddComponent<SnowFollowCam>(); // bám camera

        return SavePrefab(go, $"Assets/Prefabs/FX/{name}.prefab");
    }

    static void SetupSnow(GameObject prefab, string sortingLayer, int order, float parallax){
        var psr = prefab.GetComponent<ParticleSystemRenderer>();
        if (psr){ psr.sortingLayerName = sortingLayer; psr.sortingOrder = order; }
        var sfc = prefab.GetComponent<SnowFollowCam>();
        if (sfc) sfc.parallax = parallax;
    }

    static Sprite CreateRadialSprite(string path, int size, bool invert, Color col)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y=0; y<size; y++){
            for (int x=0; x<size; x++){
                float nx = (x + 0.5f) / size * 2f - 1f;
                float ny = (y + 0.5f) / size * 2f - 1f;
                float d = Mathf.Sqrt(nx*nx + ny*ny);
                float a = Mathf.Clamp01(invert ? d : 1f - d);
                var c = col; c.a = a;
                tex.SetPixel(x,y,c);
            }
        }
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path);
        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spritePixelsPerUnit = 100;
        ti.filterMode = FilterMode.Bilinear;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void StretchRect(RectTransform rt){
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static GameObject SavePrefab(GameObject go, string path){
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        GameObject.DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
}
#endif

