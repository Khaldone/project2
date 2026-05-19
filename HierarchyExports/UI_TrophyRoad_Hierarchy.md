```text
UI_TrophyRoad/
├── MasterCanvas
│     ● Canvas
│     ● CanvasScaler
│     ● GraphicRaycaster
│     ● TrophyRoad_NavHandler (C#)
│   ├── MasterPanel
│   │     ● CanvasRenderer
│   │     ● Image
│   │     ● TrophyRoadScreen (C#)
│   │     ● CanvasGroup
│   │   ├── TopPanel
│   │   │     ● CanvasRenderer
│   │   │     ● Image
│   │   │   └── Back_btn
│   │   │         ● CanvasRenderer
│   │   │         ● Image
│   │   │         ● Button
│   │   │       └── Text (TMP)
│   │   │             ● CanvasRenderer
│   │   │             ● TextMeshProUGUI (C#)
│   │   └── Road_ScrollRect
│   │         ● ScrollRect
│   │         ● TrophyTrackSnapper (C#)
│   │       └── Viewport
│   │             ● CanvasRenderer
│   │             ● Image
│   │             ● RectMask2D
│   │           └── Content
│   │                 ● HorizontalLayoutGroup
│   │                 ● ContentSizeFitter
│   │               ├── Track_ProgressBar_Slider
│   │               │     ● LayoutElement
│   │               │     ● Slider
│   │               │   ├── Background
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● Image
│   │               │   └── Fill Area
│   │               │       └── Fill
│   │               │             ● CanvasRenderer
│   │               │             ● Image
│   │               ├── TrophyNodeWidget_Template(Clone)
│   │               │     ● TrophyNodeWidget (C#)
│   │               │     ● LayoutElement
│   │               │     ● Canvas
│   │               │     ● CanvasGroup
│   │               │     ● GraphicRaycaster
│   │               │   ├── Title_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Cups_Required_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Claim_Button
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● Image
│   │               │   │     ● Button
│   │               │   ├── Locked_Group
│   │               │   ├── Claimable_Group
│   │               │   ├── Claimed_Group
│   │               │   └── Reward_image
│   │               │         ● CanvasRenderer
│   │               │         ● Image
│   │               ├── TrophyNodeWidget_Template(Clone)
│   │               │     ● TrophyNodeWidget (C#)
│   │               │     ● LayoutElement
│   │               │     ● Canvas
│   │               │     ● CanvasGroup
│   │               │     ● GraphicRaycaster
│   │               │   ├── Title_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Cups_Required_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Claim_Button
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● Image
│   │               │   │     ● Button
│   │               │   ├── Locked_Group
│   │               │   ├── Claimable_Group
│   │               │   ├── Claimed_Group
│   │               │   └── Reward_image
│   │               │         ● CanvasRenderer
│   │               │         ● Image
│   │               ├── TrophyNodeWidget_Template(Clone)
│   │               │     ● TrophyNodeWidget (C#)
│   │               │     ● LayoutElement
│   │               │     ● Canvas
│   │               │     ● CanvasGroup
│   │               │     ● GraphicRaycaster
│   │               │   ├── Title_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Cups_Required_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Claim_Button
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● Image
│   │               │   │     ● Button
│   │               │   ├── Locked_Group
│   │               │   ├── Claimable_Group
│   │               │   ├── Claimed_Group
│   │               │   └── Reward_image
│   │               │         ● CanvasRenderer
│   │               │         ● Image
│   │               ├── TrophyNodeWidget_Template(Clone)
│   │               │     ● TrophyNodeWidget (C#)
│   │               │     ● LayoutElement
│   │               │     ● Canvas
│   │               │     ● CanvasGroup
│   │               │     ● GraphicRaycaster
│   │               │   ├── Title_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Cups_Required_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Claim_Button
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● Image
│   │               │   │     ● Button
│   │               │   ├── Locked_Group
│   │               │   ├── Claimable_Group
│   │               │   ├── Claimed_Group
│   │               │   └── Reward_image
│   │               │         ● CanvasRenderer
│   │               │         ● Image
│   │               ├── TrophyNodeWidget_Template(Clone)
│   │               │     ● TrophyNodeWidget (C#)
│   │               │     ● LayoutElement
│   │               │     ● Canvas
│   │               │     ● CanvasGroup
│   │               │     ● GraphicRaycaster
│   │               │   ├── Title_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Cups_Required_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Claim_Button
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● Image
│   │               │   │     ● Button
│   │               │   ├── Locked_Group
│   │               │   ├── Claimable_Group
│   │               │   ├── Claimed_Group
│   │               │   └── Reward_image
│   │               │         ● CanvasRenderer
│   │               │         ● Image
│   │               ├── TrophyNodeWidget_Template(Clone)
│   │               │     ● TrophyNodeWidget (C#)
│   │               │     ● LayoutElement
│   │               │     ● Canvas
│   │               │     ● CanvasGroup
│   │               │     ● GraphicRaycaster
│   │               │   ├── Title_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Cups_Required_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Claim_Button
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● Image
│   │               │   │     ● Button
│   │               │   ├── Locked_Group
│   │               │   ├── Claimable_Group
│   │               │   ├── Claimed_Group
│   │               │   └── Reward_image
│   │               │         ● CanvasRenderer
│   │               │         ● Image
│   │               ├── TrophyNodeWidget_Template(Clone)
│   │               │     ● TrophyNodeWidget (C#)
│   │               │     ● LayoutElement
│   │               │     ● Canvas
│   │               │     ● CanvasGroup
│   │               │     ● GraphicRaycaster
│   │               │   ├── Title_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Cups_Required_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Claim_Button
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● Image
│   │               │   │     ● Button
│   │               │   ├── Locked_Group
│   │               │   ├── Claimable_Group
│   │               │   ├── Claimed_Group
│   │               │   └── Reward_image
│   │               │         ● CanvasRenderer
│   │               │         ● Image
│   │               ├── TrophyNodeWidget_Template(Clone)
│   │               │     ● TrophyNodeWidget (C#)
│   │               │     ● LayoutElement
│   │               │     ● Canvas
│   │               │     ● CanvasGroup
│   │               │     ● GraphicRaycaster
│   │               │   ├── Title_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Cups_Required_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Claim_Button
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● Image
│   │               │   │     ● Button
│   │               │   ├── Locked_Group
│   │               │   ├── Claimable_Group
│   │               │   ├── Claimed_Group
│   │               │   └── Reward_image
│   │               │         ● CanvasRenderer
│   │               │         ● Image
│   │               ├── TrophyNodeWidget_Template(Clone)
│   │               │     ● TrophyNodeWidget (C#)
│   │               │     ● LayoutElement
│   │               │     ● Canvas
│   │               │     ● CanvasGroup
│   │               │     ● GraphicRaycaster
│   │               │   ├── Title_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Cups_Required_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Claim_Button
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● Image
│   │               │   │     ● Button
│   │               │   ├── Locked_Group
│   │               │   ├── Claimable_Group
│   │               │   ├── Claimed_Group
│   │               │   └── Reward_image
│   │               │         ● CanvasRenderer
│   │               │         ● Image
│   │               ├── TrophyNodeWidget_Template(Clone)
│   │               │     ● TrophyNodeWidget (C#)
│   │               │     ● LayoutElement
│   │               │     ● Canvas
│   │               │     ● CanvasGroup
│   │               │     ● GraphicRaycaster
│   │               │   ├── Title_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Cups_Required_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Claim_Button
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● Image
│   │               │   │     ● Button
│   │               │   ├── Locked_Group
│   │               │   ├── Claimable_Group
│   │               │   ├── Claimed_Group
│   │               │   └── Reward_image
│   │               │         ● CanvasRenderer
│   │               │         ● Image
│   │               ├── TrophyNodeWidget_Template(Clone)
│   │               │     ● TrophyNodeWidget (C#)
│   │               │     ● LayoutElement
│   │               │     ● Canvas
│   │               │     ● CanvasGroup
│   │               │     ● GraphicRaycaster
│   │               │   ├── Title_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Cups_Required_Txt
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● TextMeshProUGUI (C#)
│   │               │   ├── Claim_Button
│   │               │   │     ● CanvasRenderer
│   │               │   │     ● Image
│   │               │   │     ● Button
│   │               │   ├── Locked_Group
│   │               │   ├── Claimable_Group
│   │               │   ├── Claimed_Group
│   │               │   └── Reward_image
│   │               │         ● CanvasRenderer
│   │               │         ● Image
│   │               └── TrophyNodeWidget_Template(Clone)
│   │                     ● TrophyNodeWidget (C#)
│   │                     ● LayoutElement
│   │                     ● Canvas
│   │                     ● CanvasGroup
│   │                     ● GraphicRaycaster
│   │                   ├── Title_Txt
│   │                   │     ● CanvasRenderer
│   │                   │     ● TextMeshProUGUI (C#)
│   │                   ├── Cups_Required_Txt
│   │                   │     ● CanvasRenderer
│   │                   │     ● TextMeshProUGUI (C#)
│   │                   ├── Claim_Button
│   │                   │     ● CanvasRenderer
│   │                   │     ● Image
│   │                   │     ● Button
│   │                   ├── Locked_Group
│   │                   ├── Claimable_Group
│   │                   ├── Claimed_Group
│   │                   └── Reward_image
│   │                         ● CanvasRenderer
│   │                         ● Image
│   ├── ReturnToCurrentMilestone
│   │     ● CanvasRenderer
│   │     ● Image
│   │     ● Button
│   │     ● CanvasGroup
│   └── JumpToCurrentMilestone
│         ● CanvasRenderer
│         ● Image
│         ● Button
│         ● CanvasGroup
├── TrophyRoadLifetimeScope
│     ● TrophyRoadLifetimeScope (C#)
└── Debugger
      ● TrophyRoadDebugSimulator (C#)
```
