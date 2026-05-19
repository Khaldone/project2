using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class TrophyRoadGenerator : MonoBehaviour
{
    private void Start()
    {
        GenerateTrophyRoad();
    }

    private void GenerateTrophyRoad()
    {
        // 1. Root
        GameObject root = new GameObject("UI_TrophyRoad");

        // 2. Canvas & View Script
        GameObject canvasGo = CreateUIObject("TrophyRoadCanvas", root.transform);
        SetupCanvas(canvasGo);
        TryAddComponent(canvasGo, "TrophyRoadView");

        // 3. Background Blur
        GameObject blur = CreateUIObject("Background_Blur", canvasGo.transform);
        var blurImg = blur.AddComponent<Image>();
        blurImg.color = new Color(0, 0, 0, 0.75f);
        blurImg.raycastTarget = true;
        Stretch(blur.GetComponent<RectTransform>());

        // 4. Safe Area
        GameObject safeArea = CreateUIObject("SafeArea_Panel", canvasGo.transform);
        TryAddComponent(safeArea, "SafeAreaFitter");
        Stretch(safeArea.GetComponent<RectTransform>());

        // --- TOP HEADER BAR ---
        GameObject header = CreateUIObject("Top_Header_Bar", safeArea.transform);
        SetRect(header.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -75), new Vector2(0, 150));

        CreateUIObject("Btn_Close", header.transform).AddComponent<Button>().gameObject.AddComponent<Image>();

        GameObject title = CreateUIObject("Title_Text", header.transform);
        title.AddComponent<TextMeshProUGUI>().text = "Trophy Road";

        GameObject trophyWidget = CreateUIObject("Current_Trophies_Widget", header.transform);
        CreateUIObject("Icon_Trophy", trophyWidget.transform).AddComponent<Image>();
        CreateUIObject("Text_TrophyTotal", trophyWidget.transform).AddComponent<TextMeshProUGUI>().text = "1,450";

        // --- PROGRESSION SCROLL AREA ---
        GameObject scrollArea = CreateUIObject("Progression_Scroll_Area", safeArea.transform);
        var sr = scrollArea.AddComponent<ScrollRect>();
        Stretch(scrollArea.GetComponent<RectTransform>(), new Vector2(0, 300));

        // Viewport
        GameObject viewport = CreateUIObject("Viewport", scrollArea.transform);
        viewport.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f); // Needs slight alpha for interaction
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        Stretch(viewport.GetComponent<RectTransform>());
        sr.viewport = viewport.GetComponent<RectTransform>();

        // Content Track
        GameObject content = CreateUIObject("Content_Track", viewport.transform);
        var hlg = content.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.spacing = 200; // Space between nodes

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        sr.content = content.GetComponent<RectTransform>();
        SetRect(sr.content, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(2000, 400));

        // Track Lines (Behind Nodes)
        GameObject bgLine = CreateUIObject("Background_Track_Line", content.transform);
        bgLine.AddComponent<Image>().color = Color.grey;
        bgLine.transform.SetAsFirstSibling(); // Ensure it's behind nodes

        GameObject activeLine = CreateUIObject("Active_Track_Line", content.transform);
        activeLine.AddComponent<Image>().color = Color.yellow;

        // Node Pool & Prefab Placeholder
        CreateUIObject("[Node_Pool_Container]", content.transform).SetActive(false);
        GameObject nodePlaceholder = CreateUIObject("Node_Prefab", content.transform);
        TryAddComponent(nodePlaceholder, "TrophyNodeWidget");
        nodePlaceholder.SetActive(false); // To be spawned dynamically

        // Scrollbar
        GameObject sbar = CreateUIObject("Scrollbar_Horizontal", scrollArea.transform);
        sbar.AddComponent<Scrollbar>();
        sr.horizontalScrollbar = sbar.GetComponent<Scrollbar>();

        // --- SCREEN BLOCKER (LOADING) ---
        GameObject blocker = CreateUIObject("Screen_Blocker", safeArea.transform);
        blocker.AddComponent<CanvasGroup>().alpha = 0; // Hidden by default
        Stretch(blocker.GetComponent<RectTransform>());

        GameObject spinner = CreateUIObject("Spinner_Graphic", blocker.transform);
        spinner.AddComponent<Image>(); // Add your loading sprite here

        Debug.Log("Trophy Road Hierarchy Generated.");
    }

    // --- UTILS (Same as MatchResults) ---
    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private void TryAddComponent(GameObject target, string className)
    {
        Type t = Type.GetType(className);
        if (t != null) target.AddComponent(t);
        else Debug.LogWarning($"Script <b>{className}</b> missing. Skipping.");
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
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = -margin;
        rt.anchoredPosition = Vector2.zero;
    }

    private void SetRect(RectTransform rt, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}