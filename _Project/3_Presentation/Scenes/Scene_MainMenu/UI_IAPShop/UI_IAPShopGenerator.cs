using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

public class UI_IAPShopGenerator : MonoBehaviour
{
    [Header("Script Mapping")]
    public string mainViewScript = "IAPShopView";
    public string tabWidgetScript = "ShopTabWidget";
    public string currencyWidgetScript = "CurrencyWidgetView";
    public string itemWidgetScript = "StoreItemWidget";

    private void Start()
    {
        GenerateShop();
    }

    public void GenerateShop()
    {
        // 1. UI_IAPShop (Prefab Root)
        GameObject root = new GameObject("UI_IAPShop");

        // 2. IAPShopCanvas
        GameObject canvasGo = CreateUIObject("IAPShopCanvas", root.transform);
        SetupCanvas(canvasGo);
        TryAddGenericComponent(canvasGo, mainViewScript);

        // 3. Background_Blur
        GameObject blur = CreateUIObject("Background_Blur", canvasGo.transform);
        var blurImg = blur.AddComponent<Image>();
        blurImg.color = new Color(0, 0, 0, 0.8f);
        blurImg.raycastTarget = true;
        Stretch(blur.GetComponent<RectTransform>());

        // 4. SafeArea_Panel
        GameObject safeArea = CreateUIObject("SafeArea_Panel", canvasGo.transform);
        TryAddGenericComponent(safeArea, "SafeAreaFitter");
        Stretch(safeArea.GetComponent<RectTransform>());

        // --- HEADER BAR ---
        GameObject header = CreateUIObject("Header_Bar", safeArea.transform);
        SetRect(header.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -60), new Vector2(0, 120));

        CreateUIObject("Btn_Close", header.transform).AddComponent<Button>().gameObject.AddComponent<Image>();

        var title = CreateUIObject("Text_StoreTitle", header.transform).AddComponent<TextMeshProUGUI>();
        title.text = "Pro Shop";
        title.alignment = TextAlignmentOptions.Left;

        GameObject wallet = CreateUIObject("Player_Wallet_Widget", header.transform);
        CreateCurrencyWidget("Currency_Coins", wallet.transform);
        CreateCurrencyWidget("Currency_Gems", wallet.transform);

        // --- CATEGORY TABS ---
        GameObject tabs = CreateUIObject("Category_Tabs", safeArea.transform);
        var hlg = tabs.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childControlWidth = true;
        SetRect(tabs.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -150), new Vector2(-100, 80));

        string[] tabNames = { "Tab_Offers", "Tab_Gems", "Tab_Coins", "Tab_Cues" };
        foreach (string tName in tabNames)
        {
            GameObject tab = CreateUIObject(tName, tabs.transform);
            tab.AddComponent<Image>().color = Color.gray;
            TryAddGenericComponent(tab, tabWidgetScript);
        }

        // --- STORE SCROLL AREA ---
        GameObject scrollArea = CreateUIObject("Store_Scroll_Area", safeArea.transform);
        var sr = scrollArea.AddComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        SetRect(scrollArea.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, -250), new Vector2(-40, 500));

        // Viewport
        GameObject viewport = CreateUIObject("Viewport", scrollArea.transform);
        viewport.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        Stretch(viewport.GetComponent<RectTransform>());
        sr.viewport = viewport.GetComponent<RectTransform>();

        // Content Container
        GameObject content = CreateUIObject("Content_Container", viewport.transform);
        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 30;
        vlg.childControlHeight = false;
        vlg.childForceExpandHeight = false;

        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = content.GetComponent<RectTransform>();
        SetRect(sr.content, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, 1000));

        // --- STORE SECTIONS (Example: Offers) ---
        CreateStoreSection("Offers", "Limited Time Offers!", content.transform, 1);
        CreateStoreSection("Gems", "Gem Packs", content.transform, 3);

        // Scrollbar
        GameObject sbar = CreateUIObject("Scrollbar_Vertical", scrollArea.transform);
        sbar.AddComponent<Scrollbar>().direction = Scrollbar.Direction.BottomToTop;
        sr.verticalScrollbar = sbar.GetComponent<Scrollbar>();

        // --- TRANSACTION BLOCKER OVERLAY ---
        GameObject blocker = CreateUIObject("Transaction_Blocker_Overlay", safeArea.transform);
        var cg = blocker.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        cg.blocksRaycasts = false; // Toggle this in script during purchases
        Stretch(blocker.GetComponent<RectTransform>());

        GameObject tint = CreateUIObject("Dark_Tint", blocker.transform);
        tint.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        Stretch(tint.GetComponent<RectTransform>());

        CreateUIObject("Spinner_Graphic", blocker.transform).AddComponent<Image>();
        var status = CreateUIObject("Text_Status", blocker.transform).AddComponent<TextMeshProUGUI>();
        status.text = "Waiting for Apple...";
        status.alignment = TextAlignmentOptions.Center;

        Debug.Log("<color=magenta>IAP Shop Generated!</color>");
    }

    private void CreateCurrencyWidget(string name, Transform parent)
    {
        GameObject widget = CreateUIObject(name, parent);
        widget.AddComponent<Image>();
        TryAddGenericComponent(widget, currencyWidgetScript);
    }

    private void CreateStoreSection(string id, string headerText, Transform parent, int itemCount)
    {
        GameObject sectionHeader = CreateUIObject($"Section_Header_{id}", parent);
        sectionHeader.AddComponent<TextMeshProUGUI>().text = headerText;

        GameObject grid = CreateUIObject($"Grid_{id}", parent);
        var glg = grid.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(250, 350);
        glg.spacing = new Vector2(15, 15);
        glg.childAlignment = TextAnchor.UpperLeft;

        for (int i = 0; i < itemCount; i++)
        {
            GameObject item = CreateUIObject("Store_Item_Prefab", grid.transform);
            item.AddComponent<Image>().color = Color.gray;
            TryAddGenericComponent(item, itemWidgetScript);
        }
    }

    // --- GENERIC UTILS ---
    private void TryAddGenericComponent(GameObject target, string className)
    {
        if (string.IsNullOrEmpty(className)) return;
        Type type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == className);
        if (type != null && typeof(Component).IsAssignableFrom(type))
            target.AddComponent(type);
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private void SetupCanvas(GameObject go)
    {
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        go.AddComponent<GraphicRaycaster>();
    }

    private void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero; rt.anchoredPosition = Vector2.zero;
    }

    private void SetRect(RectTransform rt, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = min; rt.anchorMax = max;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
    }
}