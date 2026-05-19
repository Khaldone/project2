```text
Scene_Login/
├── Main Camera
│     ● Camera
│     ● UniversalAdditionalCameraData
├── Directional Light
│     ● Light
│     ● UniversalAdditionalLightData
├── Canvas
│     ● Canvas
│     ● CanvasScaler
│     ● GraphicRaycaster
│     ● LoginPopupView (C#)
│     ● LoginUIHandler (C#)
│   └── MasterPanel
│         ● CanvasRenderer
│         ● Image
│       └── ButtonsContainers_Panel
│             ● CanvasRenderer
│             ● Image
│             ● GridLayoutGroup
│           ├── GuestLogin_btn
│           │     ● CanvasRenderer
│           │     ● Image
│           │     ● Button
│           │   └── GuestLogin_txt
│           │         ● CanvasRenderer
│           │         ● TextMeshProUGUI (C#)
│           └── DeviceLogin_btn
│                 ● CanvasRenderer
│                 ● Image
│                 ● Button
│               └── DeviceLogin_Txt
│                     ● CanvasRenderer
│                     ● TextMeshProUGUI (C#)
├── EventSystem
│     ● EventSystem
│     ● InputSystemUIInputModule
└── LoginLifetimeScope
      ● LoginLifetimeScope (C#)
```
