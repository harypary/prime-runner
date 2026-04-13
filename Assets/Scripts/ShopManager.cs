using UnityEngine;

// ─── スキン定義 ───────────────────────────────────────────────────────────────
public static class SkinDef
{
    public struct Skin
    {
        public string name;
        public int    price;
        public Color  shirt, pants, skin, hair, shoe;
    }

    public static readonly Skin[] All = new Skin[]
    {
        // 0: デフォルト（無料）
        new Skin { name="ランナー",  price=0,
            shirt=new Color(0.90f,0.25f,0.15f), pants=new Color(0.18f,0.22f,0.55f),
            skin =new Color(1.00f,0.75f,0.56f), hair =new Color(0.22f,0.14f,0.06f),
            shoe =new Color(0.15f,0.08f,0.03f) },
        // 1: 勇者
        new Skin { name="勇者",      price=500,
            shirt=new Color(0.95f,0.78f,0.10f), pants=new Color(0.50f,0.28f,0.10f),
            skin =new Color(1.00f,0.75f,0.56f), hair =new Color(0.60f,0.40f,0.10f),
            shoe =new Color(0.35f,0.20f,0.05f) },
        // 2: 忍者
        new Skin { name="忍者",      price=1000,
            shirt=new Color(0.08f,0.08f,0.10f), pants=new Color(0.08f,0.08f,0.10f),
            skin =new Color(0.62f,0.44f,0.32f), hair =new Color(0.05f,0.05f,0.05f),
            shoe =new Color(0.05f,0.05f,0.05f) },
        // 3: ロボット
        new Skin { name="ロボット",  price=2000,
            shirt=new Color(0.55f,0.60f,0.65f), pants=new Color(0.40f,0.44f,0.50f),
            skin =new Color(0.70f,0.72f,0.75f), hair =new Color(0.50f,0.52f,0.55f),
            shoe =new Color(0.28f,0.30f,0.34f) },
        // 4: 王様
        new Skin { name="王様",      price=5000,
            shirt=new Color(0.52f,0.10f,0.72f), pants=new Color(0.75f,0.62f,0.08f),
            skin =new Color(1.00f,0.80f,0.60f), hair =new Color(0.70f,0.55f,0.10f),
            shoe =new Color(0.60f,0.48f,0.08f) },
    };
}

// ─── 街並み定義 ───────────────────────────────────────────────────────────────
public static class CityDef
{
    public struct City
    {
        public string name;
        public int    price;
        public int    themeIndex;
        public Color  previewColor;
    }

    public static readonly City[] All = new City[]
    {
        new City { name="普通の街",  price=0,    themeIndex=0, previewColor=new Color(0.72f,0.72f,0.74f) },
        new City { name="夜の街",    price=1000,  themeIndex=1, previewColor=new Color(0.08f,0.10f,0.22f) },
        new City { name="砂漠の町",  price=2000,  themeIndex=2, previewColor=new Color(0.85f,0.68f,0.40f) },
        new City { name="未来都市",  price=5000,  themeIndex=3, previewColor=new Color(0.22f,0.38f,0.58f) },
        new City { name="黄金都市",  price=10000, themeIndex=4, previewColor=new Color(0.85f,0.72f,0.20f) },
    };
}

// ─── ショップ管理 ─────────────────────────────────────────────────────────────
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public const bool DEV_UNLOCK_ALL = false;

    const string SKIN_OWN = "PR_SkinOwn_";
    const string CITY_OWN = "PR_CityOwn_";
    const string SKIN_SEL = "PR_SkinSel";
    const string CITY_SEL = "PR_CitySel";

    public int SelectedSkin { get; private set; }
    public int SelectedCity { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        SelectedSkin = PlayerPrefs.GetInt(SKIN_SEL, 0);
        SelectedCity = PlayerPrefs.GetInt(CITY_SEL, 0);
    }

    void Start()
    {
        ApplySkin();
        ApplyCity();
    }

    // ─── 所持確認 ───────────────────────────────────────────────────────────
    public bool IsSkinOwned(int i) => DEV_UNLOCK_ALL || i == 0 || PlayerPrefs.GetInt(SKIN_OWN + i, 0) == 1;
    public bool IsCityOwned(int i) => DEV_UNLOCK_ALL || i == 0 || PlayerPrefs.GetInt(CITY_OWN + i, 0) == 1;

    // ─── 購入 ───────────────────────────────────────────────────────────────
    public bool BuySkin(int i)
    {
        if (i < 0 || i >= SkinDef.All.Length || IsSkinOwned(i)) return false;
        if (!CoinManager.Instance.SpendCoins(SkinDef.All[i].price)) return false;
        PlayerPrefs.SetInt(SKIN_OWN + i, 1);
        return true;
    }

    public bool BuyCity(int i)
    {
        if (i < 0 || i >= CityDef.All.Length || IsCityOwned(i)) return false;
        if (!CoinManager.Instance.SpendCoins(CityDef.All[i].price)) return false;
        PlayerPrefs.SetInt(CITY_OWN + i, 1);
        return true;
    }

    // ─── 選択 ───────────────────────────────────────────────────────────────
    public void SelectSkin(int i)
    {
        if (!IsSkinOwned(i)) return;
        SelectedSkin = i;
        PlayerPrefs.SetInt(SKIN_SEL, i);
        ApplySkin();
    }

    public void SelectCity(int i)
    {
        if (!IsCityOwned(i)) return;
        SelectedCity = i;
        PlayerPrefs.SetInt(CITY_SEL, i);
        ApplyCity();
    }

    // ─── 適用 ───────────────────────────────────────────────────────────────
    public void ApplySkin()
    {
        var sc = GameManager.Instance?.player?.GetComponentInChildren<SkinController>();
        sc?.Apply(SelectedSkin);
    }

    void ApplyCity()
    {
        if (CityLevelGenerator.Instance != null)
            CityLevelGenerator.Instance.cityTheme = CityDef.All[SelectedCity].themeIndex;
    }
}
