using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class MatchResultsSceneGenerator : MonoBehaviour
{
    private void Start()
    {
        GenerateMatchResults();
    }

    private void GenerateMatchResults()
    {
        // 1. UI_MatchResults (Prefab Root)
        GameObject root = new GameObject("UI_MatchResults");

        // 2. MatchResultsCanvas
        GameObject canvasGo = CreateUIObject("MatchResultsCanvas", root.transform);
        SetupCanvas(canvasGo);
        TryAddComponent(canvasGo, "MatchResultsView");

        // 3. Background_Blur
        GameObject blur = CreateUIObject("Background_Blur", canvasGo.transform);
        Image blurImg = blur.AddComponent<Image>();
        blurImg.color = new Color(0, 0, 0, 0.6f); // Placeholder for shader
        blurImg.raycastTarget = true;
        Stretch(blur.GetComponent<RectTransform>());

        // 4. SafeArea_Panel
        GameObject safeArea = CreateUIObject("SafeArea_Panel", canvasGo.transform);
        TryAddComponent(safeArea, "SafeAreaFitter");
        Stretch(safeArea.GetComponent<RectTransform>());

        // --- TOP SECTION ---
        // 5. Header_Banner
        GameObject header = CreateUIObject("Header_Banner", safeArea.transform);
        header.AddComponent<Animator>();
        SetRect(header.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(800, 200));

        GameObject bgGlow = CreateUIObject("BG_Glow", header.transform);
        bgGlow.AddComponent<Image>().color = Color.yellow; // Gold default

        GameObject titleText = CreateUIObject("Title_Text", header.transform);
        var titleTxt = titleText.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "YOU WIN!";
        titleTxt.alignment = TextAlignmentOptions.Center;

        // --- CENTER SECTION ---
        // 6. Center_Content_Group
        GameObject centerGroup = CreateUIObject("Center_Content_Group", safeArea.transform);
        var vlg = centerGroup.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlHeight = false;
        vlg.spacing = 40;
        Stretch(centerGroup.GetComponent<RectTransform>(), new Vector2(200, 400));

        // 7. Player_Comparison_Widget
        GameObject comparison = CreateUIObject("Player_Comparison_Widget", centerGroup.transform);
        comparison.AddComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;

        CreatePlayerCard("Winner_Card", comparison.transform);
        GameObject vsText = CreateUIObject("VS_Text", comparison.transform);
        vsText.AddComponent<TextMeshProUGUI>().text = "VS";
        CreatePlayerCard("Loser_Card", comparison.transform);

        // 8. Economy_Tally_Widget
        GameObject economy = CreateUIObject("Economy_Tally_Widget", centerGroup.transform);
        TryAddComponent(economy, "RewardTallyWidget");
        CreateUIObject("Coin_Icon", economy.transform).AddComponent<Image>();
        CreateUIObject("Payout_Text", economy.transform).AddComponent<TextMeshProUGUI>().text = "0";
        CreateUIObject("ParticleSystem_CoinBurst", economy.transform).AddComponent<ParticleSystem>();

        // 9. Trophy_Change_Widget
        GameObject trophy = CreateUIObject("Trophy_Change_Widget", centerGroup.transform);
        TryAddComponent(trophy, "TrophyUpdateWidget");
        CreateUIObject("Trophy_Icon", trophy.transform).AddComponent<Image>();
        CreateUIObject("Progress_Slider", trophy.transform).AddComponent<Slider>();
        CreateUIObject("Delta_Text", trophy.transform).AddComponent<TextMeshProUGUI>().text = "+30";

        // --- BOTTOM SECTION ---
        // 10. Lootbox_Drop_Anchor
        GameObject lootbox = CreateUIObject("Lootbox_Drop_Anchor", safeArea.transform);
        TryAddComponent(lootbox, "LootboxDropWidget");
        SetRect(lootbox.GetComponent<RectTransform>(), new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero, new Vector2(200, 200));

        CreateUIObject("3D_Box_Spawn_Point", lootbox.transform); // Empty Transform
        GameObject tapText = CreateUIObject("Text_TapToContinue", lootbox.transform);
        tapText.AddComponent<TextMeshProUGUI>().text = "TAP TO CONTINUE";
        tapText.SetActive(false);

        // 11. Bottom_Navigation
        GameObject nav = CreateUIObject("Bottom_Navigation", safeArea.transform);
        SetRect(nav.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 80), new Vector2(300, 100));

        GameObject btn = CreateUIObject("Btn_Return_To_Hub", nav.transform);
        btn.AddComponent<Image>();
        btn.AddComponent<Button>();
        btn.SetActive(false);

        Debug.Log("Hierarchy generated successfully.");
    }

    // --- UTILS ---

    private void CreatePlayerCard(string name, Transform parent)
    {
        GameObject card = CreateUIObject(name, parent);
        TryAddComponent(card, "PlayerResultWidget");
        CreateUIObject("Avatar_Image", card.transform).AddComponent<Image>();
        CreateUIObject("Name_Text", card.transform).AddComponent<TextMeshProUGUI>().text = "PlayerName";
    }

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
        else Debug.LogWarning($"Script <b>{className}</b> missing from project. Skipping.");
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