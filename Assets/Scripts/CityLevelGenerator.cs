using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲーム10の LevelGenerator を直接移植（直線専用に簡略化）。
/// ゲーム10と同様に prefab を Instantiate してタイルを生成する。
/// </summary>
public class CityLevelGenerator : MonoBehaviour
{
    public static CityLevelGenerator Instance { get; private set; }

    [Header("設定（SceneSetupが自動入力）")]
    public GameObject tilePrefab;       // SceneSetupが作成するCityTileプレハブ
    public Transform  playerTransform;
    public float      tileLength    = 30f;
    public int        numberOfTiles = 7;

    /// <summary>ShopManager から書き込む街並みテーマ（0〜4）</summary>
    [HideInInspector] public int cityTheme = 0;

    private List<GameObject> activeTiles = new List<GameObject>();
    private float nextTileZ = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // ゲーム10の LevelGenerator.Start() と同様: z=0から開始
        nextTileZ = 0f;
        for (int i = 0; i < numberOfTiles; i++)
            SpawnTile();
    }

    /// <summary>StartGame()から呼ぶ: 既存タイルを破棄してプレイヤー位置から再生成</summary>
    public void StartGeneration()
    {
        foreach (var t in activeTiles)
            if (t != null) Destroy(t);
        activeTiles.Clear();

        float startZ = playerTransform != null ? playerTransform.position.z : 0f;
        nextTileZ = startZ;

        for (int i = 0; i < numberOfTiles; i++)
            SpawnTile();
    }

    void Update()
    {
        if (playerTransform == null || activeTiles.Count == 0) return;

        // ゲーム10と同じ判定:
        // playerがactiveTiles[0]から tileLength+35m 以上進んだら補充
        float passed = Vector3.Dot(
            playerTransform.position - activeTiles[0].transform.position,
            Vector3.forward);
        if (passed > tileLength + 35f)
        {
            SpawnTile();
            DeleteOldestTile();
        }
    }

    // ─── ゲーム10の SpawnTile と同一（ターンなし版）─────────────
    void SpawnTile()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("CityLevelGenerator: tilePrefab が未設定です。SceneSetupを再実行してください。");
            return;
        }
        // ゲーム10: Instantiate(prefab, nextTilePos, rot)
        var go = Object.Instantiate(tilePrefab,
                                    new Vector3(0f, 0f, nextTileZ),
                                    Quaternion.identity);
        go.name = "CityTile_" + activeTiles.Count;
        var ct = go.GetComponent<CityTile>();
        if (ct != null) ct.themeIndex = cityTheme;
        activeTiles.Add(go);
        nextTileZ += tileLength;
    }

    void DeleteOldestTile()
    {
        if (activeTiles.Count == 0) return;
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
    }
}
