```text
CoinShop/
├── CoinShopLifetimeScope
│     ● CoinShopLifetimeScope (C#)
└── Canvas
      ● Canvas
      ● CanvasScaler
      ● GraphicRaycaster
      ● CoinShop_NavHandler (C#)
    └── MasterPanel
          ● CanvasRenderer
          ● Image
          ● CoinShopScreen (C#)
        ├── Top Panel
        │     ● CanvasRenderer
        │     ● Image
        │   └── Back_btn
        │         ● CanvasRenderer
        │         ● Image
        │         ● Button
        │       └── Text (Legacy)
        │             ● CanvasRenderer
        │             ● Text
        ├── Left Panel
        │     ● CanvasRenderer
        │     ● Image
        │     ● VerticalLayoutGroup
        │   ├── Tab1_btn
        │   │     ● CanvasRenderer
        │   │     ● Image
        │   │     ● Button
        │   │   └── Text (Legacy)
        │   │         ● CanvasRenderer
        │   │         ● Text
        │   ├── Tab2_btn
        │   │     ● CanvasRenderer
        │   │     ● Image
        │   │     ● Button
        │   │   └── Text (Legacy)
        │   │         ● CanvasRenderer
        │   │         ● Text
        │   └── Tab3_btn
        │         ● CanvasRenderer
        │         ● Image
        │         ● Button
        │       └── Text (Legacy)
        │             ● CanvasRenderer
        │             ● Text
        └── Panel_Container
              ● CanvasRenderer
              ● Image
            └── CoinsPackPanel(Clone)
                  ● CanvasRenderer
                  ● Image
                  ● CoinsPanelView (C#)
                └── Scroll View
                      ● CanvasRenderer
                      ● Image
                      ● ScrollRect
                    └── Viewport
                          ● CanvasRenderer
                          ● Image
                          ● Mask
                        └── Content
                              ● HorizontalLayoutGroup
                            ├── Deal1
                            │     ● CanvasRenderer
                            │     ● Image
                            │     ● DealColumnView (C#)
                            │     ● LayoutElement
                            │   └── Deal1_scrollview
                            │         ● CanvasRenderer
                            │         ● Image
                            │         ● ScrollRect
                            │       └── Viewport
                            │             ● CanvasRenderer
                            │             ● Image
                            │             ● Mask
                            │           └── Content
                            │                 ● VerticalLayoutGroup
                            │                 ● ContentSizeFitter
                            │               ├── squareshopproduct(Clone)
                            │               │     ● CanvasRenderer
                            │               │     ● Image
                            │               │     ● ShopProductButton (C#)
                            │               │   ├── purchase_btn
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● Image
                            │               │   │     ● Button
                            │               │   │   └── Text (TMP)
                            │               │   │         ● CanvasRenderer
                            │               │   │         ● TextMeshProUGUI (C#)
                            │               │   ├── product_img
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● Image
                            │               │   ├── product_txt
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● TextMeshProUGUI (C#)
                            │               │   └── prodcut_price_txt
                            │               │         ● CanvasRenderer
                            │               │         ● TextMeshProUGUI (C#)
                            │               ├── squareshopproduct(Clone)
                            │               │     ● CanvasRenderer
                            │               │     ● Image
                            │               │     ● ShopProductButton (C#)
                            │               │   ├── purchase_btn
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● Image
                            │               │   │     ● Button
                            │               │   │   └── Text (TMP)
                            │               │   │         ● CanvasRenderer
                            │               │   │         ● TextMeshProUGUI (C#)
                            │               │   ├── product_img
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● Image
                            │               │   ├── product_txt
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● TextMeshProUGUI (C#)
                            │               │   └── prodcut_price_txt
                            │               │         ● CanvasRenderer
                            │               │         ● TextMeshProUGUI (C#)
                            │               ├── squareshopproduct(Clone)
                            │               │     ● CanvasRenderer
                            │               │     ● Image
                            │               │     ● ShopProductButton (C#)
                            │               │   ├── purchase_btn
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● Image
                            │               │   │     ● Button
                            │               │   │   └── Text (TMP)
                            │               │   │         ● CanvasRenderer
                            │               │   │         ● TextMeshProUGUI (C#)
                            │               │   ├── product_img
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● Image
                            │               │   ├── product_txt
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● TextMeshProUGUI (C#)
                            │               │   └── prodcut_price_txt
                            │               │         ● CanvasRenderer
                            │               │         ● TextMeshProUGUI (C#)
                            │               ├── squareshopproduct(Clone)
                            │               │     ● CanvasRenderer
                            │               │     ● Image
                            │               │     ● ShopProductButton (C#)
                            │               │   ├── purchase_btn
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● Image
                            │               │   │     ● Button
                            │               │   │   └── Text (TMP)
                            │               │   │         ● CanvasRenderer
                            │               │   │         ● TextMeshProUGUI (C#)
                            │               │   ├── product_img
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● Image
                            │               │   ├── product_txt
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● TextMeshProUGUI (C#)
                            │               │   └── prodcut_price_txt
                            │               │         ● CanvasRenderer
                            │               │         ● TextMeshProUGUI (C#)
                            │               ├── squareshopproduct(Clone)
                            │               │     ● CanvasRenderer
                            │               │     ● Image
                            │               │     ● ShopProductButton (C#)
                            │               │   ├── purchase_btn
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● Image
                            │               │   │     ● Button
                            │               │   │   └── Text (TMP)
                            │               │   │         ● CanvasRenderer
                            │               │   │         ● TextMeshProUGUI (C#)
                            │               │   ├── product_img
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● Image
                            │               │   ├── product_txt
                            │               │   │     ● CanvasRenderer
                            │               │   │     ● TextMeshProUGUI (C#)
                            │               │   └── prodcut_price_txt
                            │               │         ● CanvasRenderer
                            │               │         ● TextMeshProUGUI (C#)
                            │               └── squareshopproduct(Clone)
                            │                     ● CanvasRenderer
                            │                     ● Image
                            │                     ● ShopProductButton (C#)
                            │                   ├── purchase_btn
                            │                   │     ● CanvasRenderer
                            │                   │     ● Image
                            │                   │     ● Button
                            │                   │   └── Text (TMP)
                            │                   │         ● CanvasRenderer
                            │                   │         ● TextMeshProUGUI (C#)
                            │                   ├── product_img
                            │                   │     ● CanvasRenderer
                            │                   │     ● Image
                            │                   ├── product_txt
                            │                   │     ● CanvasRenderer
                            │                   │     ● TextMeshProUGUI (C#)
                            │                   └── prodcut_price_txt
                            │                         ● CanvasRenderer
                            │                         ● TextMeshProUGUI (C#)
                            └── Deal2
                                  ● CanvasRenderer
                                  ● Image
                                  ● DealColumnView (C#)
                                  ● LayoutElement
                                └── Deal2_scrollview
                                      ● CanvasRenderer
                                      ● Image
                                      ● ScrollRect
                                    └── Viewport
                                          ● CanvasRenderer
                                          ● Image
                                          ● Mask
                                        └── Content
                                              ● VerticalLayoutGroup
                                              ● ContentSizeFitter
                                            ├── squareshopproduct(Clone)
                                            │     ● CanvasRenderer
                                            │     ● Image
                                            │     ● ShopProductButton (C#)
                                            │   ├── purchase_btn
                                            │   │     ● CanvasRenderer
                                            │   │     ● Image
                                            │   │     ● Button
                                            │   │   └── Text (TMP)
                                            │   │         ● CanvasRenderer
                                            │   │         ● TextMeshProUGUI (C#)
                                            │   ├── product_img
                                            │   │     ● CanvasRenderer
                                            │   │     ● Image
                                            │   ├── product_txt
                                            │   │     ● CanvasRenderer
                                            │   │     ● TextMeshProUGUI (C#)
                                            │   └── prodcut_price_txt
                                            │         ● CanvasRenderer
                                            │         ● TextMeshProUGUI (C#)
                                            ├── squareshopproduct(Clone)
                                            │     ● CanvasRenderer
                                            │     ● Image
                                            │     ● ShopProductButton (C#)
                                            │   ├── purchase_btn
                                            │   │     ● CanvasRenderer
                                            │   │     ● Image
                                            │   │     ● Button
                                            │   │   └── Text (TMP)
                                            │   │         ● CanvasRenderer
                                            │   │         ● TextMeshProUGUI (C#)
                                            │   ├── product_img
                                            │   │     ● CanvasRenderer
                                            │   │     ● Image
                                            │   ├── product_txt
                                            │   │     ● CanvasRenderer
                                            │   │     ● TextMeshProUGUI (C#)
                                            │   └── prodcut_price_txt
                                            │         ● CanvasRenderer
                                            │         ● TextMeshProUGUI (C#)
                                            ├── squareshopproduct(Clone)
                                            │     ● CanvasRenderer
                                            │     ● Image
                                            │     ● ShopProductButton (C#)
                                            │   ├── purchase_btn
                                            │   │     ● CanvasRenderer
                                            │   │     ● Image
                                            │   │     ● Button
                                            │   │   └── Text (TMP)
                                            │   │         ● CanvasRenderer
                                            │   │         ● TextMeshProUGUI (C#)
                                            │   ├── product_img
                                            │   │     ● CanvasRenderer
                                            │   │     ● Image
                                            │   ├── product_txt
                                            │   │     ● CanvasRenderer
                                            │   │     ● TextMeshProUGUI (C#)
                                            │   └── prodcut_price_txt
                                            │         ● CanvasRenderer
                                            │         ● TextMeshProUGUI (C#)
                                            ├── squareshopproduct(Clone)
                                            │     ● CanvasRenderer
                                            │     ● Image
                                            │     ● ShopProductButton (C#)
                                            │   ├── purchase_btn
                                            │   │     ● CanvasRenderer
                                            │   │     ● Image
                                            │   │     ● Button
                                            │   │   └── Text (TMP)
                                            │   │         ● CanvasRenderer
                                            │   │         ● TextMeshProUGUI (C#)
                                            │   ├── product_img
                                            │   │     ● CanvasRenderer
                                            │   │     ● Image
                                            │   ├── product_txt
                                            │   │     ● CanvasRenderer
                                            │   │     ● TextMeshProUGUI (C#)
                                            │   └── prodcut_price_txt
                                            │         ● CanvasRenderer
                                            │         ● TextMeshProUGUI (C#)
                                            ├── squareshopproduct(Clone)
                                            │     ● CanvasRenderer
                                            │     ● Image
                                            │     ● ShopProductButton (C#)
                                            │   ├── purchase_btn
                                            │   │     ● CanvasRenderer
                                            │   │     ● Image
                                            │   │     ● Button
                                            │   │   └── Text (TMP)
                                            │   │         ● CanvasRenderer
                                            │   │         ● TextMeshProUGUI (C#)
                                            │   ├── product_img
                                            │   │     ● CanvasRenderer
                                            │   │     ● Image
                                            │   ├── product_txt
                                            │   │     ● CanvasRenderer
                                            │   │     ● TextMeshProUGUI (C#)
                                            │   └── prodcut_price_txt
                                            │         ● CanvasRenderer
                                            │         ● TextMeshProUGUI (C#)
                                            └── squareshopproduct(Clone)
                                                  ● CanvasRenderer
                                                  ● Image
                                                  ● ShopProductButton (C#)
                                                ├── purchase_btn
                                                │     ● CanvasRenderer
                                                │     ● Image
                                                │     ● Button
                                                │   └── Text (TMP)
                                                │         ● CanvasRenderer
                                                │         ● TextMeshProUGUI (C#)
                                                ├── product_img
                                                │     ● CanvasRenderer
                                                │     ● Image
                                                ├── product_txt
                                                │     ● CanvasRenderer
                                                │     ● TextMeshProUGUI (C#)
                                                └── prodcut_price_txt
                                                      ● CanvasRenderer
                                                      ● TextMeshProUGUI (C#)
```
