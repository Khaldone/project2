```text
UI_CitySelection/
├── CitySelectionLifetimeScope
│     ● CitySelectionLifetimeScope (C#)
└── MasterCanvas
      ● Canvas
      ● CanvasScaler
      ● GraphicRaycaster
      ● CitySelection_NavHandler (C#)
    └── MasterPanel
          ● CanvasRenderer
          ● Image
          ● CitySelectionScreen (C#)
        ├── TopPanel
        │     ● CanvasRenderer
        │     ● Image
        │   └── Back_btn
        │         ● CanvasRenderer
        │         ● Image
        │         ● Button
        │       └── Back_txt
        │             ● CanvasRenderer
        │             ● TextMeshProUGUI (C#)
        ├── CitiesContainer_Panel
        │     ● CanvasRenderer
        │     ● Image
        │   └── Content
        │         ● CanvasRenderer
        │       └── Scroll-Snap
        │             ● CanvasRenderer
        │             ● ScrollRect
        │             ● CitySnapScrollView (C#)
        │           ├── Viewport
        │           │     ● CanvasRenderer
        │           │     ● RectMask2D
        │           │     ● Image
        │           │   └── Content
        │           │         ● GridLayoutGroup
        │           │         ● ContentSizeFitter
        │           │       ├── CitySlot_Panel(Clone)
        │           │       │     ● CanvasRenderer
        │           │       │   └── CityVisual_Panel
        │           │       │         ● CanvasRenderer
        │           │       │         ● Image
        │           │       │         ● Button
        │           │       │         ● Canvas
        │           │       │         ● GraphicRaycaster
        │           │       │         ● CitySlotView (C#)
        │           │       │       ├── Front_Panel
        │           │       │       │     ● CanvasRenderer
        │           │       │       │     ● Image
        │           │       │       │   ├── LockedOverlay_Panel
        │           │       │       │   │     ● CanvasRenderer
        │           │       │       │   │     ● Image
        │           │       │       │   │   ├── PadLock_img
        │           │       │       │   │   │     ● CanvasRenderer
        │           │       │       │   │   │     ● Image
        │           │       │       │   │   └── UnlockReqContainer_Panel
        │           │       │       │   │         ● CanvasRenderer
        │           │       │       │   │         ● Image
        │           │       │       │   │       └── UnlockReq_txt
        │           │       │       │   │             ● CanvasRenderer
        │           │       │       │   │             ● TextMeshProUGUI (C#)
        │           │       │       │   ├── CityNameContainer_Panel
        │           │       │       │   │     ● CanvasRenderer
        │           │       │       │   │     ● Image
        │           │       │       │   │   └── CityName_txt
        │           │       │       │   │         ● CanvasRenderer
        │           │       │       │   │         ● TextMeshProUGUI (C#)
        │           │       │       │   ├── EntryFeeContainer_Panel
        │           │       │       │   │     ● CanvasRenderer
        │           │       │       │   │     ● Image
        │           │       │       │   │   └── EntryFee_txt
        │           │       │       │   │         ● CanvasRenderer
        │           │       │       │   │         ● TextMeshProUGUI (C#)
        │           │       │       │   ├── PrizeContainer_Panel
        │           │       │       │   │     ● CanvasRenderer
        │           │       │       │   │     ● Image
        │           │       │       │   │   └── Prize_txt
        │           │       │       │   │         ● CanvasRenderer
        │           │       │       │   │         ● TextMeshProUGUI (C#)
        │           │       │       │   └── Bg_img
        │           │       │       │         ● CanvasRenderer
        │           │       │       │         ● Image
        │           │       │       ├── Back_Panel
        │           │       │       │     ● CanvasRenderer
        │           │       │       │     ● Image
        │           │       │       │   ├── RulesList_Panel
        │           │       │       │   │     ● CanvasRenderer
        │           │       │       │   │     ● Image
        │           │       │       │   │   └── Rules_txt
        │           │       │       │   │         ● CanvasRenderer
        │           │       │       │   │         ● TextMeshProUGUI (C#)
        │           │       │       │   └── BackBg_img
        │           │       │       │         ● CanvasRenderer
        │           │       │       │         ● Image
        │           │       │       └── Flip_btn
        │           │       │             ● CanvasRenderer
        │           │       │             ● Image
        │           │       │             ● Button
        │           │       │           └── Flip_txt
        │           │       │                 ● CanvasRenderer
        │           │       │                 ● TextMeshProUGUI (C#)
        │           │       └── CitySlot_Panel(Clone)
        │           │             ● CanvasRenderer
        │           │           └── CityVisual_Panel
        │           │                 ● CanvasRenderer
        │           │                 ● Image
        │           │                 ● Button
        │           │                 ● Canvas
        │           │                 ● GraphicRaycaster
        │           │                 ● CitySlotView (C#)
        │           │               ├── Front_Panel
        │           │               │     ● CanvasRenderer
        │           │               │     ● Image
        │           │               │   ├── LockedOverlay_Panel
        │           │               │   │     ● CanvasRenderer
        │           │               │   │     ● Image
        │           │               │   │   ├── PadLock_img
        │           │               │   │   │     ● CanvasRenderer
        │           │               │   │   │     ● Image
        │           │               │   │   └── UnlockReqContainer_Panel
        │           │               │   │         ● CanvasRenderer
        │           │               │   │         ● Image
        │           │               │   │       └── UnlockReq_txt
        │           │               │   │             ● CanvasRenderer
        │           │               │   │             ● TextMeshProUGUI (C#)
        │           │               │   ├── CityNameContainer_Panel
        │           │               │   │     ● CanvasRenderer
        │           │               │   │     ● Image
        │           │               │   │   └── CityName_txt
        │           │               │   │         ● CanvasRenderer
        │           │               │   │         ● TextMeshProUGUI (C#)
        │           │               │   ├── EntryFeeContainer_Panel
        │           │               │   │     ● CanvasRenderer
        │           │               │   │     ● Image
        │           │               │   │   └── EntryFee_txt
        │           │               │   │         ● CanvasRenderer
        │           │               │   │         ● TextMeshProUGUI (C#)
        │           │               │   ├── PrizeContainer_Panel
        │           │               │   │     ● CanvasRenderer
        │           │               │   │     ● Image
        │           │               │   │   └── Prize_txt
        │           │               │   │         ● CanvasRenderer
        │           │               │   │         ● TextMeshProUGUI (C#)
        │           │               │   └── Bg_img
        │           │               │         ● CanvasRenderer
        │           │               │         ● Image
        │           │               ├── Back_Panel
        │           │               │     ● CanvasRenderer
        │           │               │     ● Image
        │           │               │   ├── RulesList_Panel
        │           │               │   │     ● CanvasRenderer
        │           │               │   │     ● Image
        │           │               │   │   └── Rules_txt
        │           │               │   │         ● CanvasRenderer
        │           │               │   │         ● TextMeshProUGUI (C#)
        │           │               │   └── BackBg_img
        │           │               │         ● CanvasRenderer
        │           │               │         ● Image
        │           │               └── Flip_btn
        │           │                     ● CanvasRenderer
        │           │                     ● Image
        │           │                     ● Button
        │           │                   └── Flip_txt
        │           │                         ● CanvasRenderer
        │           │                         ● TextMeshProUGUI (C#)
        │           ├── Next_btn
        │           │     ● CanvasRenderer
        │           │     ● Button
        │           │     ● Image
        │           └── Previous_btn
        │                 ● CanvasRenderer
        │                 ● Button
        │                 ● Image
        └── BottomPanel
              ● CanvasRenderer
              ● Image
              ● GridLayoutGroup
            ├── 8BallFilter_btn
            │     ● CanvasRenderer
            │     ● Image
            │     ● Button
            │   └── 8Ball_txt
            │         ● CanvasRenderer
            │         ● TextMeshProUGUI (C#)
            ├── 9BallFilter_btn
            │     ● CanvasRenderer
            │     ● Image
            │     ● Button
            │   └── 9Ball_txt
            │         ● CanvasRenderer
            │         ● TextMeshProUGUI (C#)
            ├── EvenOddFilter_btn
            │     ● CanvasRenderer
            │     ● Image
            │     ● Button
            │   └── EvenOdd_btn
            │         ● CanvasRenderer
            │         ● TextMeshProUGUI (C#)
            └── CarromFilter_btn
                  ● CanvasRenderer
                  ● Image
                  ● Button
                └── Carrom_txt
                      ● CanvasRenderer
                      ● TextMeshProUGUI (C#)
```
