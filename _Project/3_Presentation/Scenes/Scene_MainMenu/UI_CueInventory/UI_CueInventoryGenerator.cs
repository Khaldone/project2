using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

public class UI_CueInventoryGenerator : MonoBehaviour
{
    [Header("Script Mapping")]
    public string mainViewScript = "CueInventoryView";
    public string viewerWidgetScript = "Cue3DViewerWidget";
    public string slotWidgetScript = "CueSlotWidget";
    public string statBarScript = "StatBarWidget";

    private void Start()
    {
        GenerateInventory();
    }

    public void GenerateInventory()
    {
        // 1. UI_CueInventory (Prefab Root)
        GameObject root = new GameObject("UI_CueInventory");

        // 2. CueInventoryCanvas
        GameObject canvasGo = CreateUIObject("CueInventoryCanvas", root.transform);
        SetupCanvas(canvasGo);
        TryAddGenericComponent(canvasGo, mainViewScript);

        // 3. Background_Blur
        GameObject blur = CreateUIObject("Background_Blur", canvasGo.transform);
        blur.AddComponent<Image>().color = new Color(0, 0, 0, 0.85f);
        blur.GetComponent<Image>().raycastTarget = true;
        Stretch(blur.GetComponent<RectTransform>());

        // 4. 3D_Inspection_Stage (The 3D Scene)
        GameObject stage = new GameObject("3D_Inspection_Stage");
        stage.transform.SetParent(canvasGo.transform, false);
        TryAddGenericComponent(stage, viewerWidgetScript);

        CreateUIObject("Cue_Spawn_Anchor", stage.transform);
        GameObject lightGo = new GameObject("Studio_Lighting");
        lightGo.transform.SetParent(stage.transform, false);
        lightGo.AddComponent<Light>().type = LightType.Directional;

        // 5. SafeArea_Panel
        GameObject safeArea = CreateUIObject("SafeArea_Panel", canvasGo.transform);
        TryAddGenericComponent(safeArea, "SafeAreaFitter");
        Stretch(safeArea.GetComponent<RectTransform>());

        // --- HEADER ---
        GameObject header = CreateUIObject("Header_Container", safeArea.transform);
        SetRect(header.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -60), new Vector2(0, 120));
        CreateUIObject("Text_Title", header.transform).AddComponent<TextMeshProUGUI>().text = "Cue Collection";
        CreateUIObject("Btn_Close", header.transform).AddComponent<Button>().gameObject.AddComponent<Image>();

        // --- SPLIT SCREEN LAYOUT ---
        GameObject splitScreen = CreateUIObject("Split_Screen_Layout", safeArea.transform);
        var hlg = splitScreen.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 40;
        hlg.childControlWidth = true;
        Stretch(splitScreen.GetComponent<RectTransform>(), new Vector2(40, 200));

        // LEFT SIDE: Inventory Scroll Area
        GameObject scrollArea = CreateUIObject("Inventory_Scroll_Area", splitScreen.transform);
        var sr = scrollArea.AddComponent<ScrollRect>();
        sr.horizontal = false;

        GameObject viewport = CreateUIObject("Viewport", scrollArea.transform);
        viewport.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        Stretch(viewport.GetComponent<RectTransform>());
        sr.viewport = viewport.GetComponent<RectTransform>();

        GameObject grid = CreateUIObject("Content_Grid", viewport.transform);
        var glg = grid.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(150, 150);
        glg.spacing = new Vector2(15, 15);
        sr.content = grid.GetComponent<RectTransform>();
        SetRect(sr.content, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, 800));

        // Spawn a placeholder slot
        GameObject slot = CreateUIObject("Cue_Slot_Prefab", grid.transform);
        TryAddGenericComponent(slot, slotWidgetScript);
        slot.AddComponent<Image>().color = Color.gray;

        // RIGHT SIDE: Inspection Panel Container
        GameObject inspection = CreateUIObject("Inspection_Panel_Container", splitScreen.transform);
        inspection.AddComponent<VerticalLayoutGroup>().spacing = 20;

        // Render Image (For the 3D Stage)
        GameObject renderImg = CreateUIObject("Render_Image", inspection.transform);
        renderImg.AddComponent<RawImage>(); // This will hold the RenderTexture
        renderImg.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 600);

        // Details Header
        GameObject details = CreateUIObject("Details_Header", inspection.transform);
        CreateUIObject("Text_CueName", details.transform).AddComponent<TextMeshProUGUI>().text = "Dragon Strike";
        CreateUIObject("Text_Rarity", details.transform).AddComponent<TextMeshProUGUI>().text = "Epic";

        // Stats Block
        GameObject stats = CreateUIObject("Stats_Block", inspection.transform);
        stats.AddComponent<VerticalLayoutGroup>().spacing = 10;
        CreateStatBar("Stat_Power", stats.transform);
        CreateStatBar("Stat_Spin", stats.transform);
        CreateStatBar("Stat_AimLine", stats.transform);

        // Footer Controls
        GameObject footer = CreateUIObject("Footer_Controls", inspection.transform);
        GameObject btnEquip = CreateUIObject("Btn_Equip", footer.transform);
        btnEquip.AddComponent<Button>().gameObject.AddComponent<Image>().color = Color.green;
        CreateUIObject("Text_EquipStatus", footer.transform).AddComponent<TextMeshProUGUI>().text = "Currently Equipped";

        Debug.Log("<color=cyan>Cue Inventory Hierarchy Generated.</color>");
    }

    private void CreateStatBar(string name, Transform parent)
    {
        GameObject bar = CreateUIObject(name, parent);
        TryAddGenericComponent(bar, statBarScript);
        bar.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
        // Visual indicator child
        GameObject fill = CreateUIObject("Fill", bar.transform);
        fill.AddComponent<Image>().color = Color.blue;
        SetRect(fill.GetComponent<RectTransform>(), Vector2.zero, new Vector2(0.5f, 1), Vector2.zero, Vector2.zero);
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

    private void Stretch(RectTransform rt, Vector2 margin = default)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.sizeDelta = -margin; rt.anchoredPosition = Vector2.zero;
    }

    private void SetRect(RectTransform rt, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = min; rt.anchorMax = max;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
    }
}