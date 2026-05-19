using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class TournamentBracketGenerator : MonoBehaviour
{
    private void Start()
    {
        GenerateTournamentBracket();
    }

    private void GenerateTournamentBracket()
    {
        // 1. Root Prefab Root
        GameObject root = new GameObject("UI_TournamentBracket");

        // 2. Canvas Layer
        GameObject canvasGo = CreateUIObject("TournamentCanvas", root.transform);
        SetupCanvas(canvasGo);
        TryAddComponent(canvasGo, "TournamentBracketView");

        // 3. Background Tint
        GameObject tint = CreateUIObject("Background_Tint", canvasGo.transform);
        var tintImg = tint.AddComponent<Image>();
        tintImg.color = new Color(0, 0, 0, 0.85f);
        Stretch(tint.GetComponent<RectTransform>());

        // 4. Safe Area
        GameObject safeArea = CreateUIObject("SafeArea_Panel", canvasGo.transform);
        TryAddComponent(safeArea, "SafeAreaFitter");
        Stretch(safeArea.GetComponent<RectTransform>());

        // --- HEADER ---
        GameObject header = CreateUIObject("Header_Container", safeArea.transform);
        SetRect(header.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -100), new Vector2(0, 150));

        var nameTxt = CreateUIObject("Text_TournamentName", header.transform).AddComponent<TextMeshProUGUI>();
        nameTxt.text = "Sydney Silver Cup";
        nameTxt.fontSize = 48;

        var prizeTxt = CreateUIObject("Text_PrizePool", header.transform).AddComponent<TextMeshProUGUI>();
        prizeTxt.text = "Prize: 10,000 Coins";
        prizeTxt.fontSize = 32;

        // --- BRACKET VISUAL TREE ---
        GameObject visualTree = CreateUIObject("Bracket_Visual_Tree", safeArea.transform);
        var hlg = visualTree.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 100;
        Stretch(visualTree.GetComponent<RectTransform>(), new Vector2(100, 300));

        // Round 1: Quarter Finals (4 Matches)
        CreateRound(visualTree.transform, "Round_1_QuarterFinals", 4);

        // Round 2: Semi Finals (2 Matches)
        CreateRound(visualTree.transform, "Round_2_SemiFinals", 2);

        // Round 3: Finals (1 Match)
        CreateRound(visualTree.transform, "Round_3_Finals", 1);

        // Champion Slot
        GameObject crownSlot = CreateUIObject("Champion_Crown_Slot", visualTree.transform);
        TryAddComponent(crownSlot, "BracketNodeWidget");

        // --- CONNECTION LINES ---
        GameObject connections = CreateUIObject("Connection_Lines", visualTree.transform);
        connections.transform.SetAsFirstSibling(); // Put lines behind nodes

        GameObject line1 = CreateUIObject("Line_Q1_to_S1", connections.transform);
        TryAddComponent(line1, "BracketLineAnimator");

        // --- BOTTOM CONTROLS ---
        GameObject bottom = CreateUIObject("Bottom_Controls", safeArea.transform);
        SetRect(bottom.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(600, 250));

        GameObject btnPlay = CreateUIObject("Btn_PlayNextMatch", bottom.transform);
        btnPlay.AddComponent<Image>().color = Color.green;
        btnPlay.AddComponent<Button>();

        GameObject btnForfeit = CreateUIObject("Btn_ForfeitTournament", bottom.transform);
        btnForfeit.AddComponent<Image>().color = Color.red;
        btnForfeit.AddComponent<Button>();

        Debug.Log("Tournament Bracket generated successfully.");
    }

    private void CreateRound(Transform parent, string roundName, int matchCount)
    {
        GameObject round = CreateUIObject(roundName, parent);
        var vlg = round.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 50;

        for (int i = 1; i <= matchCount; i++)
        {
            GameObject match = CreateUIObject($"Match_{i}_Container", round.transform);
            match.AddComponent<VerticalLayoutGroup>().spacing = 10;

            // Create two nodes per match
            CreateNode("Node_Player_1", match.transform);
            CreateNode("Node_Player_2", match.transform);
        }
    }

    private void CreateNode(string nodeName, Transform parent)
    {
        GameObject node = CreateUIObject(nodeName, parent);
        node.AddComponent<Image>(); // Visual background for node
        TryAddComponent(node, "BracketNodeWidget");
    }

    // --- UTILS ---
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