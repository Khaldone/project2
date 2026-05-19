```text
UI_Matchmaking/
├── MatchmakingLifetimeScope
│     ● MatchmakingLifetimeScope (C#)
└── MasterCanvas
      ● Canvas
      ● CanvasScaler
      ● GraphicRaycaster
      ● Matchmaking_NavHandler (C#)
    └── MasterPanel
          ● CanvasRenderer
          ● Image
          ● MatchmakingScreen (C#)
        ├── Top-Panel
        │     ● CanvasRenderer
        │     ● Image
        │   ├── Cancel_Btn
        │   │     ● CanvasRenderer
        │   │     ● Image
        │   │     ● Button
        │   │   └── Cancel_txt
        │   │         ● CanvasRenderer
        │   │         ● TextMeshProUGUI (C#)
        │   └── Coins+Cash_Container
        │         ● CanvasRenderer
        │         ● Image
        │       ├── Coins_Container
        │       │     ● CanvasRenderer
        │       │     ● Image
        │       │   ├── CoinsLabel_Txt
        │       │   │     ● CanvasRenderer
        │       │   │     ● TextMeshProUGUI (C#)
        │       │   └── CoinsAmount_Txt
        │       │         ● CanvasRenderer
        │       │         ● TextMeshProUGUI (C#)
        │       └── Cash_Container
        │             ● CanvasRenderer
        │             ● Image
        │           ├── CashLabel_Txt
        │           │     ● CanvasRenderer
        │           │     ● TextMeshProUGUI (C#)
        │           └── CashAmount_Txt
        │                 ● CanvasRenderer
        │                 ● TextMeshProUGUI (C#)
        └── Content_Panel
              ● CanvasRenderer
              ● Image
            ├── CityTable_Panel
            │     ● CanvasRenderer
            │     ● Image
            │   └── CityTable_Img
            │         ● CanvasRenderer
            │         ● Image
            ├── Vs_img
            │     ● CanvasRenderer
            │     ● Image
            ├── BetAmount_Panel
            │     ● CanvasRenderer
            │     ● Image
            │   ├── CoinPackIcton_Img
            │   │     ● CanvasRenderer
            │   │     ● Image
            │   └── BetAmount_txt
            │         ● CanvasRenderer
            │         ● TextMeshProUGUI (C#)
            ├── Player1_Panel
            │     ● CanvasRenderer
            │     ● Image
            │   ├── PlayerProfileFrame_img
            │   │     ● CanvasRenderer
            │   │     ● Image
            │   │   └── PlayerProfile
            │   │         ● CanvasRenderer
            │   │         ● Image
            │   ├── PlayerDisplayname_txt
            │   │     ● CanvasRenderer
            │   │     ● TextMeshProUGUI (C#)
            │   ├── PlayerLevelContainer_Panel
            │   │     ● CanvasRenderer
            │   │     ● Image
            │   │   ├── PlayerLevel_img
            │   │   │     ● CanvasRenderer
            │   │   │     ● Image
            │   │   │   └── PlayerLevel_txt
            │   │   │         ● CanvasRenderer
            │   │   │         ● TextMeshProUGUI (C#)
            │   │   └── PlayerLevel_Slider
            │   │         ● Slider
            │   │       ├── Background
            │   │       │     ● CanvasRenderer
            │   │       │     ● Image
            │   │       ├── Fill Area
            │   │       │   └── Fill
            │   │       │         ● CanvasRenderer
            │   │       │         ● Image
            │   │       └── Handle Slide Area
            │   │           └── Handle
            │   │                 ● CanvasRenderer
            │   │                 ● Image
            │   ├── PlayerCoinsContribution_Panel
            │   │     ● CanvasRenderer
            │   │     ● Image
            │   │   ├── CoinsIcon_Img
            │   │   │     ● CanvasRenderer
            │   │   │     ● Image
            │   │   └── PlayerCoins_Panel
            │   │       └── Background_img
            │   │             ● CanvasRenderer
            │   │             ● Image
            │   │           └── CoinsAmount_txt
            │   │                 ● CanvasRenderer
            │   │                 ● TextMeshProUGUI (C#)
            │   └── CoinsAnimation_Panel
            │         ● CanvasRenderer
            │         ● Image
            │         ● Animator
            └── Opponent_Panel
                  ● CanvasRenderer
                  ● Image
                ├── OpponentProfileFrame_img
                │     ● CanvasRenderer
                │     ● Image
                │     ● Mask
                │   ├── OpponentSearch_RawImage
                │   │     ● CanvasRenderer
                │   │     ● RawImage
                │   │     ● LayoutElement
                │   ├── OpponentFoundEffect_img
                │   │     ● CanvasRenderer
                │   │     ● Image
                │   └── OpponentFound_img
                │         ● CanvasRenderer
                │         ● Image
                ├── OpponentDisplayname_txt
                │     ● CanvasRenderer
                │     ● TextMeshProUGUI (C#)
                ├── OpponentLevelContainer_Panel
                │     ● CanvasRenderer
                │     ● Image
                │   ├── OpponentLevel_img
                │   │     ● CanvasRenderer
                │   │     ● Image
                │   │   └── OpponentLevel_txt
                │   │         ● CanvasRenderer
                │   │         ● TextMeshProUGUI (C#)
                │   └── OpponentLevel_Slider
                │         ● Slider
                │       ├── Background
                │       │     ● CanvasRenderer
                │       │     ● Image
                │       ├── Fill Area
                │       │   └── Fill
                │       │         ● CanvasRenderer
                │       │         ● Image
                │       └── Handle Slide Area
                │           └── Handle
                │                 ● CanvasRenderer
                │                 ● Image
                ├── OpponentCoinsContribution_Panel
                │     ● CanvasRenderer
                │     ● Image
                │   ├── OpponentCoinsIcon_Img
                │   │     ● CanvasRenderer
                │   │     ● Image
                │   └── OpponentCoins_Panel
                │       └── Background_img
                │             ● CanvasRenderer
                │             ● Image
                │           └── CoinsAmount_txt
                │                 ● CanvasRenderer
                │                 ● TextMeshProUGUI (C#)
                └── CoinsAnimation_Panel
                      ● CanvasRenderer
                      ● Image
                      ● Animator
```
