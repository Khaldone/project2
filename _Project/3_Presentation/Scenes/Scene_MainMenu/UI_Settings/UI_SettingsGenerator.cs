using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using TMPro;

public class UI_SettingsGenerator : MonoBehaviour
{
    [Header("Script Mapping")]
    public string mainViewScript = "SettingsView";
    public string sliderWidgetScript = "SettingsSliderWidget";
    public string toggleWidgetScript = "SettingToggleWidget";
    public string accountLinkScript = "AccountLinkWidget";

    private void Start()
    {
        GenerateSettings();
    }

    public void GenerateSettings()
    {
        // 1. UI_Settings (Prefab Root)
        GameObject root = new GameObject("UI_Settings");

        // 2. SettingsCanvas
        GameObject canvasGo = CreateUIObject("SettingsCanvas", root.transform);
        SetupCanvas(canvasGo);
        TryAddGenericComponent(canvasGo, mainViewScript);

        // 3. Background_Blur_Overlay
        GameObject blur = CreateUIObject("Background_Blur_Overlay", canvasGo.transform);
        var blurImg = blur.AddComponent<Image>();
        blurImg.color = new Color(0, 0, 0, 0.8f);
        blurImg.raycastTarget = true;
        Stretch(blur.GetComponent<RectTransform>());

        // 4. SafeArea_Panel
        GameObject safeArea = CreateUIObject("SafeArea_Panel", canvasGo.transform);
        TryAddGenericComponent(safeArea, "SafeAreaFitter");
        Stretch(safeArea.GetComponent<RectTransform>());

        // --- HEADER ---
        GameObject header = CreateUIObject("Header_Container", safeArea.transform);
        SetRect(header.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -60), new Vector2(-100, 120));
        CreateUIObject("Text_Title", header.transform).AddComponent<TextMeshProUGUI>().text = "Settings";
        CreateUIObject("Btn_Close", header.transform).AddComponent<Button>().gameObject.AddComponent<Image>();

        // --- SETTINGS SCROLL AREA ---
        GameObject scrollArea = CreateUIObject("Settings_Scroll_Area", safeArea.transform);
        var sr = scrollArea.AddComponent<ScrollRect>();
        sr.horizontal = false;
        SetRect(scrollArea.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, -100), new Vector2(-100, 400));

        // Viewport
        GameObject viewport = CreateUIObject("Viewport", scrollArea.transform);
        viewport.AddComponent<Image>().color = new Color(1, 1, 1, 0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        Stretch(viewport.GetComponent<RectTransform>());
        sr.viewport = viewport.GetComponent<RectTransform>();

        // Content List
        GameObject content = CreateUIObject("Content_List", viewport.transform);
        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 25;
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.childControlHeight = false;
        vlg.childForceExpandHeight = false;

        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = content.GetComponent<RectTransform>();
        SetRect(sr.content, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, new Vector2(0, 1200));

        // --- CATEGORY: AUDIO ---
        CreateCategoryHeader("Category_Header_Audio", "Audio Settings", content.transform);
        CreateSettingRow("Setting_MasterVolume", "Master Volume", content.transform, sliderWidgetScript);
        CreateSettingRow("Setting_MusicVolume", "Music Volume", content.transform, sliderWidgetScript);
        CreateSettingRow("Setting_SFXVolume", "SFX Volume", content.transform, sliderWidgetScript);

        // --- CATEGORY: GAMEPLAY ---
        CreateCategoryHeader("Category_Header_Gameplay", "Gameplay", content.transform);
        CreateSettingRow("Setting_LeftHandedMode", "Left-Handed Mode", content.transform, toggleWidgetScript);
        CreateSettingRow("Setting_HapticFeedback", "Haptic Feedback", content.transform, toggleWidgetScript);

        // --- CATEGORY: ACCOUNT ---
        CreateCategoryHeader("Category_Header_Account", "Account Services", content.transform);
        CreateSettingRow("Account_Link_Google", "Google Play", content.transform, accountLinkScript);
        CreateSettingRow("Account_Link_Apple", "Apple ID", content.transform, accountLinkScript);

        // --- CATEGORY: DANGER ZONE ---
        GameObject dangerHeader = CreateCategoryHeader("Category_Header_DangerZone", "Danger Zone", content.transform);
        dangerHeader.GetComponent<TextMeshProUGUI>().color = Color.red;

        CreateUIObject("Btn_Logout", content.transform).AddComponent<Button>().gameObject.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
        CreateUIObject("Btn_DeleteAccount", content.transform).AddComponent<Button>().gameObject.AddComponent<Image>().color = new Color(0.5f, 0.1f, 0.1f);

        // --- FOOTER ---
        GameObject footer = CreateUIObject("Footer_Container", safeArea.transform);
        SetRect(footer.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 80), new Vector2(-100, 100));

        var versionText = CreateUIObject("Text_Version", footer.transform).AddComponent<TextMeshProUGUI>();
        versionText.text = "v1.2.4 (Build 8901)";
        versionText.fontSize = 20;

        CreateUIObject("Btn_SupportTicket", footer.transform).AddComponent<Button>().gameObject.AddComponent<Image>();

        Debug.Log("<color=green>Settings UI Generated Successfully!</color>");
    }

    private GameObject CreateCategoryHeader(string name, string label, Transform parent)
    {
        GameObject header = CreateUIObject(name, parent);
        var txt = header.AddComponent<TextMeshProUGUI>();
        txt.text = label.ToUpper();
        txt.fontSize = 24;
        txt.fontStyle = FontStyles.Bold;
        return header;
    }

    private void CreateSettingRow(string name, string label, Transform parent, string scriptName)
    {
        GameObject row = CreateUIObject(name, parent);
        row.AddComponent<LayoutElement>().minHeight = 80;

        // Add a visual background for the row
        row.AddComponent<Image>().color = new Color(1, 1, 1, 0.05f);

        GameObject labelGo = CreateUIObject("Label", row.transform);
        labelGo.AddComponent<TextMeshProUGUI>().text = label;
        SetRect(labelGo.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(200, 0), new Vector2(350, 50));

        TryAddGenericComponent(row, scriptName);
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