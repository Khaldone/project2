using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

public class UI_DailyRewardsGenerator : MonoBehaviour
{
    [Header("Script Mapping")]
    public string mainViewScript = "DailyRewardsView";
    public string nodeWidgetScript = "DailyRewardNodeWidget";
    public string safeAreaScript = "SafeAreaFitter";

    private void Start()
    {
        GenerateDailyRewards();
    }

    public void GenerateDailyRewards()
    {
        // 1. UI_DailyRewards (Prefab Root)
        GameObject root = new GameObject("UI_DailyRewards");

        // 2. DailyRewardsCanvas
        GameObject canvasGo = CreateUIObject("DailyRewardsCanvas", root.transform);
        SetupCanvas(canvasGo);
        TryAddGenericComponent(canvasGo, mainViewScript);

        // 3. Background_Dimmer
        GameObject dimmer = CreateUIObject("Background_Dimmer", canvasGo.transform);
        var dimmerImg = dimmer.AddComponent<Image>();
        dimmerImg.color = new Color(0, 0, 0, 0.75f);
        dimmerImg.raycastTarget = true;
        Stretch(dimmer.GetComponent<RectTransform>());

        // 4. SafeArea_Panel
        GameObject safeArea = CreateUIObject("SafeArea_Panel", canvasGo.transform);
        TryAddGenericComponent(safeArea, safeAreaScript);
        Stretch(safeArea.GetComponent<RectTransform>());

        // --- HEADER ---
        GameObject header = CreateUIObject("Header_Container", safeArea.transform);
        SetRect(header.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -150), new Vector2(800, 200));

        var title = CreateUIObject("Title_Text", header.transform).AddComponent<TextMeshProUGUI>();
        title.text = "Daily Bonus";
        title.fontSize = 54;
        title.alignment = TextAlignmentOptions.Center;

        var subtitle = CreateUIObject("Subtitle_Text", header.transform).AddComponent<TextMeshProUGUI>();
        subtitle.text = "Come back every day for better loot!";
        subtitle.fontSize = 28;
        subtitle.alignment = TextAlignmentOptions.Center;
        subtitle.rectTransform.anchoredPosition = new Vector2(0, -50);

        // --- REWARDS GRID ---
        GameObject gridContainer = CreateUIObject("Rewards_Grid_Container", safeArea.transform);
        var gridLayout = gridContainer.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(180, 250);
        gridLayout.spacing = new Vector2(20, 20);
        gridLayout.childAlignment = TextAnchor.MiddleCenter;

        SetRect(gridContainer.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 600));

        // Generate Nodes Day 1-6
        for (int i = 1; i <= 6; i++)
        {
            CreateRewardNode($"Node_Day_{i}", gridContainer.transform, false);
        }

        // Generate Day 7 Grand Prize
        CreateRewardNode("Node_Day_7_GrandPrize", gridContainer.transform, true);

        // --- FOOTER ---
        GameObject footer = CreateUIObject("Footer_Container", safeArea.transform);
        SetRect(footer.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 100), new Vector2(400, 150));

        GameObject btnClose = CreateUIObject("Btn_Close", footer.transform);
        btnClose.AddComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);
        btnClose.AddComponent<Button>();
        btnClose.SetActive(false); // Hidden initially per your requirements

        Debug.Log("<color=green>Daily Rewards Hierarchy Generated Successfully!</color>");
    }

    private void CreateRewardNode(string nodeName, Transform parent, bool isGrandPrize)
    {
        GameObject node = CreateUIObject(nodeName, parent);
        node.AddComponent<Image>().color = isGrandPrize ? new Color(1, 0.85f, 0) : new Color(1, 1, 1, 0.1f);

        // Attachment of the widget logic
        TryAddGenericComponent(node, nodeWidgetScript);

        if (isGrandPrize)
        {
            // Grand Prize nodes usually need a LayoutElement to span multiple columns 
            // or just a custom scale if not using auto-layout.
            var le = node.AddComponent<LayoutElement>();
            le.minWidth = 220;
            le.minHeight = 300;
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
        else
            Debug.LogWarning($"[Generator] Script <b>{className}</b> not found in project.");
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