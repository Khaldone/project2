using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

public class UI_GlobalNotificationGenerator : MonoBehaviour
{
    [Header("Script Mapping")]
    public string mainViewScript = "GlobalNotificationView";
    public string bannerWidgetScript = "NotificationBannerWidget";
    public string modalWidgetScript = "SystemModalWidget";

    private void Start()
    {
        GenerateGlobalNotifications();
    }

    public void GenerateGlobalNotifications()
    {
        // 1. UI_GlobalNotifications (Prefab Root)
        GameObject root = new GameObject("UI_GlobalNotifications");

        // 2. GlobalNotificationCanvas
        GameObject canvasGo = CreateUIObject("GlobalNotificationCanvas", root.transform);
        SetupHighPriorityCanvas(canvasGo);
        TryAddGenericComponent(canvasGo, mainViewScript);

        // 3. SafeArea_Panel
        GameObject safeArea = CreateUIObject("SafeArea_Panel", canvasGo.transform);
        TryAddGenericComponent(safeArea, "SafeAreaFitter");
        Stretch(safeArea.GetComponent<RectTransform>());

        // --- BANNER ANCHOR TOP ---
        GameObject bannerAnchor = CreateUIObject("Banner_Anchor_Top", safeArea.transform);
        var vlg = bannerAnchor.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 10;
        vlg.childControlHeight = false;
        vlg.childForceExpandHeight = false;

        // Position at the top of the screen
        SetRect(bannerAnchor.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -150), new Vector2(-100, 400));

        // Create the Template Prefab (Hidden)
        GameObject bannerPrefab = CreateNotificationBanner(bannerAnchor.transform);
        bannerPrefab.SetActive(false);

        // --- CRITICAL MODAL ANCHOR ---
        GameObject modalAnchor = CreateUIObject("Critical_Modal_Anchor", safeArea.transform);
        Stretch(modalAnchor.GetComponent<RectTransform>()); // Center anchored by default stretch

        // System_Modal_Widget
        GameObject modalWidget = CreateUIObject("System_Modal_Widget", modalAnchor.transform);
        TryAddGenericComponent(modalWidget, modalWidgetScript);
        SetRect(modalWidget.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 400));

        // Modal Children
        GameObject blocker = CreateUIObject("Dark_Screen_Blocker", modalWidget.transform);
        var blockerImg = blocker.AddComponent<Image>();
        blockerImg.color = new Color(0, 0, 0, 0.8f);
        blockerImg.raycastTarget = true;
        Stretch(blocker.GetComponent<RectTransform>(), new Vector2(-2000, -2000)); // Ensure it covers whole screen

        GameObject panelBg = CreateUIObject("Panel_Background", modalWidget.transform);
        panelBg.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);
        Stretch(panelBg.GetComponent<RectTransform>());

        var header = CreateUIObject("Text_Header", modalWidget.transform).AddComponent<TextMeshProUGUI>();
        header.text = "Connection Lost";
        header.alignment = TextAlignmentOptions.Center;
        header.rectTransform.anchoredPosition = new Vector2(0, 100);

        var body = CreateUIObject("Text_Body", modalWidget.transform).AddComponent<TextMeshProUGUI>();
        body.text = "You have been disconnected from Photon.";
        body.fontSize = 24;
        body.alignment = TextAlignmentOptions.Center;

        GameObject btnAck = CreateUIObject("Btn_Acknowledge", modalWidget.transform);
        btnAck.AddComponent<Button>().gameObject.AddComponent<Image>().color = Color.grey;
        var btnText = CreateUIObject("Text", btnAck.transform).AddComponent<TextMeshProUGUI>();
        btnText.text = "Return to Menu";
        btnText.alignment = TextAlignmentOptions.Center;
        SetRect(btnAck.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(300, 60));

        modalWidget.SetActive(false); // Hidden initially

        Debug.Log("<color=yellow>Global Notification System Generated.</color>");
    }

    private GameObject CreateNotificationBanner(Transform parent)
    {
        GameObject banner = CreateUIObject("Notification_Banner_Prefab", parent);
        TryAddGenericComponent(banner, bannerWidgetScript);
        banner.AddComponent<LayoutElement>().minHeight = 100;

        GameObject bg = CreateUIObject("Background_Panel", banner.transform);
        bg.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        bg.GetComponent<Image>().raycastTarget = true;
        Stretch(bg.GetComponent<RectTransform>());

        GameObject icon = CreateUIObject("Icon_Type", banner.transform);
        icon.AddComponent<Image>();
        SetRect(icon.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(60, 0), new Vector2(60, 60));

        var title = CreateUIObject("Text_Title", banner.transform).AddComponent<TextMeshProUGUI>();
        title.text = "Notification Title";
        title.fontSize = 28;
        SetRect(title.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(120, 20), new Vector2(-150, 40));

        var msg = CreateUIObject("Text_Message", banner.transform).AddComponent<TextMeshProUGUI>();
        msg.text = "This is a detailed message description.";
        msg.fontSize = 20;
        SetRect(msg.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(120, -20), new Vector2(-150, 30));

        return banner;
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

    private void SetupHighPriorityCanvas(GameObject go)
    {
        Canvas c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100; // Ensures it is above all other game UI
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