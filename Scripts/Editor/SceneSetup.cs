using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;

public class SceneSetup : EditorWindow
{
    [MenuItem("Tower Defense/Montar Cena")]
    static void Setup()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        AddTag("PlacementArea");

        SetupLight();
        SetupCamera();
        CreateGround();

        Transform[] waypoints = CreatePath();

        GameObject bulletPrefab = CreateBulletPrefab();
        GameObject enemyPrefab  = CreateEnemyPrefab();
        GameObject towerPrefab  = CreateTowerPrefab(bulletPrefab);

        CreateWaveSpawner(enemyPrefab, waypoints);
        GameManager gm = CreateGameManager();
        CreateUI(gm);
        CreateTowerPlacer(towerPrefab);

        Debug.Log("Cena montada! Pressione Play para testar.");
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    static Material MakeMaterial(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;

        mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    // ── scene objects ─────────────────────────────────────────────────────────

    static void SetupLight()
    {
        if (GameObject.Find("Directional Light") != null) return;
        GameObject obj = new GameObject("Directional Light");
        Light l = obj.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 1f;
        obj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    static void SetupCamera()
    {
        Camera.main.transform.position = new Vector3(0f, 18f, -8f);
        Camera.main.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }

    static void CreateGround()
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Plane);
        g.name = "Ground";
        g.transform.localScale = new Vector3(3f, 1f, 3f);
        g.tag = "PlacementArea";
        g.GetComponent<Renderer>().material = MakeMaterial("Ground", new Color(0.2f, 0.55f, 0.2f));
    }

    static Transform[] CreatePath()
    {
        GameObject parent = new GameObject("Path");

        Vector3[] positions =
        {
            new Vector3(-13f, 0.1f,  0f),
            new Vector3( -6f, 0.1f,  0f),
            new Vector3( -6f, 0.1f,  7f),
            new Vector3(  6f, 0.1f,  7f),
            new Vector3(  6f, 0.1f, -7f),
            new Vector3( 13f, 0.1f, -7f),
        };

        Transform[] waypoints = new Transform[positions.Length];
        Material pathMat = MakeMaterial("Path", new Color(0.7f, 0.6f, 0.3f));

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject wp = new GameObject("Waypoint " + i);
            wp.transform.position   = positions[i];
            wp.transform.SetParent(parent.transform);
            wp.AddComponent<Waypoint>();
            waypoints[i] = wp.transform;
        }

        // draw path segments as flat cubes so the player can see the road
        for (int i = 0; i < positions.Length - 1; i++)
        {
            Vector3 a = positions[i];
            Vector3 b = positions[i + 1];
            Vector3 mid = (a + b) * 0.5f;
            float len = Vector3.Distance(a, b);
            bool horizontal = Mathf.Abs(b.z - a.z) < 0.01f;

            GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.name = "PathSegment " + i;
            seg.transform.position   = mid;
            seg.transform.localScale = horizontal
                ? new Vector3(len, 0.05f, 2.5f)
                : new Vector3(2.5f, 0.05f, len);
            seg.transform.SetParent(parent.transform);
            seg.GetComponent<Renderer>().material = pathMat;
            DestroyImmediate(seg.GetComponent<BoxCollider>());
        }

        return waypoints;
    }

    // ── prefabs ───────────────────────────────────────────────────────────────

    static GameObject CreateBulletPrefab()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = "Bullet";
        obj.transform.localScale = Vector3.one * 0.25f;
        obj.GetComponent<Renderer>().material = MakeMaterial("Bullet", Color.yellow);
        DestroyImmediate(obj.GetComponent<SphereCollider>());
        obj.AddComponent<Bullet>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, "Assets/Prefabs/Bullet.prefab");
        DestroyImmediate(obj);
        return prefab;
    }

    static GameObject CreateEnemyPrefab()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        obj.name = "Enemy";
        obj.GetComponent<Renderer>().material = MakeMaterial("Enemy", new Color(0.9f, 0.1f, 0.1f));
        obj.AddComponent<Enemy>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, "Assets/Prefabs/Enemy.prefab");
        DestroyImmediate(obj);
        return prefab;
    }

    static GameObject CreateTowerPrefab(GameObject bulletPrefab)
    {
        Material towerMat = MakeMaterial("Tower", new Color(0.2f, 0.2f, 0.8f));

        GameObject root = new GameObject("Tower");

        // base cylinder
        GameObject baseObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseObj.name = "Base";
        baseObj.transform.SetParent(root.transform);
        baseObj.transform.localPosition = Vector3.zero;
        baseObj.transform.localScale    = new Vector3(1f, 0.3f, 1f);
        baseObj.GetComponent<Renderer>().material = towerMat;
        DestroyImmediate(baseObj.GetComponent<CapsuleCollider>());

        // barrel
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.name = "Barrel";
        barrel.transform.SetParent(root.transform);
        barrel.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        barrel.transform.localScale    = new Vector3(0.2f, 0.5f, 0.2f);
        barrel.GetComponent<Renderer>().material = towerMat;
        DestroyImmediate(barrel.GetComponent<CapsuleCollider>());

        // fire point
        GameObject fp = new GameObject("FirePoint");
        fp.transform.SetParent(root.transform);
        fp.transform.localPosition = new Vector3(0f, 1.4f, 0f);

        Tower t = root.AddComponent<Tower>();
        t.bulletPrefab = bulletPrefab;
        t.firePoint    = fp.transform;
        t.range        = 6f;
        t.fireRate     = 1f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Tower.prefab");
        DestroyImmediate(root);
        return prefab;
    }

    // ── scene managers ────────────────────────────────────────────────────────

    static void CreateWaveSpawner(GameObject enemyPrefab, Transform[] waypoints)
    {
        GameObject obj = new GameObject("WaveSpawner");
        WaveSpawner ws = obj.AddComponent<WaveSpawner>();
        ws.enemyPrefab       = enemyPrefab;
        ws.waypoints         = waypoints;
        ws.enemiesPerWave    = 5;
        ws.timeBetweenEnemies = 0.8f;
        ws.timeBetweenWaves  = 5f;
    }

    static GameManager CreateGameManager()
    {
        GameObject obj = new GameObject("GameManager");
        return obj.AddComponent<GameManager>();
    }

    static void CreateUI(GameManager gm)
    {
        // Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                 ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        gm.livesText  = MakeLabel(canvasObj, "LivesText",  "Vidas: 20",    font, Color.white,  new Vector2(10, -10));
        gm.moneyText  = MakeLabel(canvasObj, "MoneyText",  "Dinheiro: $150", font, Color.yellow, new Vector2(10, -50));
    }

    static Text MakeLabel(GameObject parent, string name, string text, Font font, Color color, Vector2 pos)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);

        Text t = obj.AddComponent<Text>();
        t.text     = text;
        t.font     = font;
        t.fontSize = 24;
        t.color    = color;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0, 1);
        rt.anchorMax        = new Vector2(0, 1);
        rt.pivot            = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta        = new Vector2(250, 40);

        return t;
    }

    static void CreateTowerPlacer(GameObject towerPrefab)
    {
        GameObject obj = new GameObject("TowerPlacer");
        TowerPlacer tp = obj.AddComponent<TowerPlacer>();
        tp.towerPrefab = towerPrefab;
        tp.towerCost   = 50;
    }
}
