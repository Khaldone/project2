using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

public class UI_LootboxRevealGenerator : MonoBehaviour
{
    [Header("Script Mapping")]
    public string mainViewScript = "LootboxRevealView";
    public string stageWidgetScript = "TheatricalStageWidget";
    public string tapWidgetScript = "FullScreenTapWidget";
    public string gridWidgetScript = "SummaryGridWidget";

    private void Start()
    {
        GenerateLootboxReveal();
    }

    public void GenerateLootboxReveal()
    {
        // 1. UI_LootboxReveal (Prefab Root)
        GameObject root = new GameObject("UI_LootboxReveal");

        // 2. LootboxRevealCanvas
        GameObject canvasGo = CreateUIObject("LootboxRevealCanvas", root.transform);
        SetupCameraCanvas(canvasGo);
        TryAddGenericComponent(canvasGo, mainViewScript);

        // 3. Background_Dimmer
        GameObject dimmer = CreateUIObject("Background_Dimmer", canvasGo.transform);
        var dimmerImg = dimmer.AddComponent<Image>();
        dimmerImg.color = new Color(0, 0, 0, 0); // Start transparent
        dimmerImg.raycastTarget = false;
        dimmer.AddComponent<Animator>();
        Stretch(dimmer.GetComponent<RectTransform>());

        // 4. 3D_Theatrical_Stage
        GameObject stage = CreateUIObject("3D_Theatrical_Stage", canvasGo.transform);
        TryAddGenericComponent(stage, stageWidgetScript);
        Stretch(stage.GetComponent<RectTransform>());

        // Stage Children
        CreateUIObject("Box_Spawn_Anchor", stage.transform);
        GameObject revealAnchor = CreateUIObject("Item_Reveal_Anchor", stage.transform);
        revealAnchor.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 200);

        GameObject vfxPool = CreateUIObject("Eruption_VFX_Pool", stage.transform);
        CreateUIObject("VFX_Burst_Common", vfxPool.transform);
        CreateUIObject("VFX_Burst_Rare", vfxPool.transform);
        CreateUIObject("VFX_Burst_Epic", vfxPool.transform);

        // 5. SafeArea_Panel
        GameObject safeArea = CreateUIObject("SafeArea_Panel", canvasGo.transform);
        TryAddGenericComponent(safeArea, "SafeAreaFitter");
        Stretch(safeArea.GetComponent<RectTransform>());

        // Screen_Tap_Zone
        GameObject tapZone = CreateUIObject("Screen_Tap_Zone", safeArea.transform);
        var tapImg = tapZone.AddComponent<Image>();
        tapImg.color = new Color(1, 1, 1, 0); // Invisible
        tapImg.raycastTarget = true;
        TryAddGenericComponent(tapZone, tapWidgetScript);
        Stretch(tapZone.GetComponent<RectTransform>());

        // Prompt_Text
        GameObject prompt = CreateUIObject("Prompt_Text", safeArea.transform);
        var promptTxt = prompt.AddComponent<TextMeshProUGUI>();
        promptTxt.text = "Tap to Open!";
        promptTxt.alignment = TextAlignmentOptions.Center;
        SetRect(prompt.GetComponent<RectTransform>(), new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.2f), Vector2.zero, new Vector2(500, 100));

        // Final_Summary_Grid
        GameObject grid = CreateUIObject("Final_Summary_Grid", safeArea.transform);
        var glg = grid.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(200, 300);
        glg.spacing = new Vector2(20, 0);
        glg.childAlignment = TextAnchor.MiddleCenter;

        TryAddGenericComponent(grid, gridWidgetScript);
        Stretch(grid.GetComponent<RectTransform>(), new Vector2(100, 200));
        grid.SetActive(false); // Hidden initially

        // Grid Children (Reward Cards)
        for (int i = 1; i <= 3; i++)
        {
            GameObject card = CreateUIObject($"Reward_Card_{i}", grid.transform);
            card.AddComponent<Image>().color = new Color(1, 1, 1, 0.2f);
        }

        Debug.Log("<color=orange>Lootbox Reveal Scene Generated!</color>");
    }

    // --- HELPER METHODS ---

    private void SetupCameraCanvas(GameObject go)
    {
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceCamera;
        c.worldCamera = Camera.main; // Usually needs a dedicated UI Camera for 3D overlay
        c.planeDistance = 5; // Positions 3D objects appropriately

        go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        go.AddComponent<GraphicRaycaster>();
    }

    private void TryAddGenericComponent(GameObject target, string className)
    {
        if (string.IsNullOrEmpty(className)) return;

        Type type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == className);

        if (type != null && typeof(Component).IsAssignableFrom(type))
            target.AddComponent(type);
        else
            Debug.LogWarning($"[Generator] Component <b>{className}</b> not found.");
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private void Stretch(RectTransform rt, Vector2 padding = default)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.sizeDelta = -padding; rt.anchoredPosition = Vector2.zero;
    }

    private void SetRect(RectTransform rt, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = min; rt.anchorMax = max;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
    }
}