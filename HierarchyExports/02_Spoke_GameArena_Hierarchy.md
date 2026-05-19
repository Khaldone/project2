```text
02_Spoke_GameArena/
├── [Setup]
│   └── ArenaLifetimeScope 
│         ● ArenaLifetimeScope (C#)
├── [Environment]
│   ├── Camera
│   │     ● Camera
│   │     ● AudioListener
│   │     ● UniversalAdditionalCameraData
│   ├── Lighting_Setup
│   │   └── Directional Light
│   │         ● Light
│   │         ● UniversalAdditionalLightData
│   └── Table_Anchor
│       └── 3D_PoolTable_Visual
├── [Gameplay_Systems]
│   ├── Input_Catcher
│   │     ● CueInputView (C#)
│   ├── AimLine_Visualizer
│   │     ● AimLineView (C#)
│   └── VFX_Audio_Pools
│       ├── Spark_Prefab_1
│       └── Spark_Prefab_2
├── [Network_Entities]
│   ├── Fusion_NetworkRunner
│   └── PoolBall
│         ● FusionPoolBall (C#)
├── [UI_Presentation]
│   └── ArenaHUDCanvas
│         ● Canvas
│         ● CanvasScaler
│         ● GraphicRaycaster
│       └── SafeArea_Panel
│             ● CanvasRenderer
│             ● Image
│             ● ArenaNavigationHandler (C#)
│           ├── Top_Scoreboard
│           │     ● CanvasRenderer
│           │     ● Image
│           ├── Power_Meter_Widget
│           │     ● CanvasRenderer
│           │     ● Image
│           └── Btn_Pause_Menu
│                 ● CanvasRenderer
│                 ● Image
│                 ● Button
│               └── Text (Legacy)
│                     ● CanvasRenderer
│                     ● Text
└── EventSystem
      ● EventSystem
      ● InputSystemUIInputModule
```
