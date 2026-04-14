#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace EGKart.Editor
{
    public static class SceneBuilder
    {
        // --- Neon & Premium Renk Paleti ---
        private static Color COL_DARK_BG  = new Color(0.04f, 0.04f, 0.06f); // Derin Siyah
        private static Color COL_PURPLE   = new Color(0.69f, 0.15f, 1.00f); // Neon Mor (#B026FF)
        private static Color COL_RED      = new Color(1.00f, 0.19f, 0.19f); // Neon Kırmızı (#FF3131)
        private static Color COL_BLUE     = new Color(0.12f, 0.32f, 1.00f); // Neon Mavi (#1F51FF)
        private static Color COL_YELLOW   = new Color(1.00f, 0.98f, 0.00f); // Neon Sarı (#FFFB00)
        private static Color COL_WHITE    = new Color(0.95f, 0.95f, 1.00f); // Temiz Beyaz

        [MenuItem("EG-Kart/Sahneyi Kur (Scene Setup)")]
        public static void BuildScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Hata!", "Oyun çalışırken (Play Mode) sahne kurulumu yapılamaz. Lütfen önce oyunu durdurun.", "Tamam");
                return;
            }

            var currentScene = EditorSceneManager.GetActiveScene();
            // Eski objeleri temizle
            foreach (var r in currentScene.GetRootGameObjects())
            {
                if (r.name is "Canvas" or "GameManager" or "UIManager" or "EventSystem")
                    Object.DestroyImmediate(r);
            }

            // Kamera arka planını daha derin yap
            if (Camera.main != null)
            {
                Camera.main.backgroundColor = COL_DARK_BG;
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
            }

            // --- EventSystem ---
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();

            // Canvas oluştur
            GameObject canvasGO = CreateCanvas();
            Transform cv = canvasGO.transform;

            // Arka plan
            CreateBackground(cv);

            // --- UI Elemanları (Daha şık konumlandırma) ---
            GameObject discardPile = CreatePanel(cv, "DiscardPile", new Vector2(0, 60), new Vector2(125, 175), COL_PURPLE);
            AddOutline(discardPile, COL_WHITE, 3);
            CreateLabel(discardPile.transform, "DiscardLabel", "?", 72, COL_WHITE);
            
            GameObject deck = CreatePanel(cv, "Deck", new Vector2(-180, 60), new Vector2(125, 175), Color.white);
            // Kart arkalığı PNG'sini zorunlu Sprite yap ve yükle
            string backPath = "Assets/Textures/CardBack.png";
            PrepareSprite(backPath);
            Sprite deckSprite = AssetDatabase.LoadAssetAtPath<Sprite>(backPath);
            if (deckSprite != null) {
                deck.GetComponent<Image>().sprite = deckSprite;
            }
            AddOutline(deck, COL_WHITE, 3);
            
            // --- Yön Göstergesi (Direction Indicator) ---
            GameObject dirInd = CreatePanel(cv, "DirectionIndicator", new Vector2(0, 60), new Vector2(250, 250), new Color(1,1,1,0.1f));
            dirInd.transform.SetAsFirstSibling(); // Masanın/kartların altında kalsın
            if (deckSprite != null) {
                // Şimdilik kart arkalığını düşük alpha ile halka niyetine kullanalım
                var img = dirInd.GetComponent<Image>();
                img.sprite = deckSprite; img.color = new Color(COL_PURPLE.r, COL_PURPLE.g, COL_PURPLE.b, 0.2f);
            }
            
            GameObject playerHand = CreatePanel(cv, "PlayerHandPanel", new Vector2(0, -290), new Vector2(1000, 200), new Color(1,1,1,0.05f));
            // NOT: HorizontalLayoutGroup kaldırıldı; kart dizilimi artık UIManager tarafından milimetrik yönetiliyor.

            // Metinler (Neon parlaması efekti için beyaz + renkli shadow)
            var curPlayerTxt = CreateLabel(cv, "CurrentPlayerText", "SIRA: ...", 32, COL_PURPLE, new Vector2(-380, 230), new Vector2(350, 60));
            var statusTxt = CreateLabel(cv, "StatusText", "Oyun Hazırlanıyor...", 26, COL_WHITE, new Vector2(0, -85), new Vector2(900, 60));

            // AI Panellerini Sade Olarak Geri Getiriyoruz (Yukarıda)
            TextMeshProUGUI[] aiTexts = new TextMeshProUGUI[2];
            for (int i = 0; i < 2; i++) {
                float posX = (i == 0) ? -280 : 280;
                GameObject aiP = CreatePanel(cv, $"AIPanel_{i}", new Vector2(posX, 330), new Vector2(220, 60), new Color(0,0,0,0.75f));
                AddOutline(aiP, COL_PURPLE, 2); 
                aiTexts[i] = CreateLabel(aiP.transform, "Label", $"AI {i+1}: 7 Kart", 18, COL_WHITE);
            }

            // Butonlar (Neon Stil)
            GameObject drawBtn = CreateButton(cv, "DrawCardButton", "KART ÇEK", new Vector2(-440, -125), new Vector2(160, 65), new Color(0.15f, 0.15f, 0.25f));
            AddOutline(drawBtn, COL_BLUE, 3);
            GameObject egBtn = CreateButton(cv, "EGKartButton", "UNO!", new Vector2(440, -125), new Vector2(160, 65), new Color(0.2f, 0.1f, 0.25f));
            AddOutline(egBtn, COL_PURPLE, 3);
            
            // Çıkış Butonu (Sağ Üst Köşe)
            GameObject exitBtn = CreateButton(cv, "ExitButton", "ÇIKIŞ", new Vector2(570, 320), new Vector2(100, 45), COL_RED);
            AddOutline(exitBtn, COL_WHITE, 2);

            // Popuplar
            GameObject cpPanel = CreatePanel(cv, "ColorPickerPanel", Vector2.zero, new Vector2(500, 280), new Color(0.05f, 0.05f, 0.1f, 0.99f));
            cpPanel.SetActive(false);
            AddOutline(cpPanel, COL_WHITE, 4);
            CreateLabel(cpPanel.transform, "Title", "RENK SEÇİN", 26, COL_WHITE, new Vector2(0, 80), new Vector2(400, 50));
            
            Button[] colorBtns = new Button[4];
            string[] cNames = {"Kirmizi", "Mor", "Mavi", "Sari"};
            Color[] cColors = {COL_RED, COL_PURPLE, COL_BLUE, COL_YELLOW};
            for(int i=0; i<4; i++) {
                colorBtns[i] = CreateButton(cpPanel.transform, "btn"+cNames[i], "", new Vector2(-150 + i*100, -20), new Vector2(90, 90), cColors[i]).GetComponent<Button>();
                AddOutline(colorBtns[i].gameObject, COL_WHITE, 3);
            }

            GameObject egNotif = CreatePanel(cv, "EGKartPanel", new Vector2(0, 100), new Vector2(550, 160), new Color(0,0,0,0.9f));
            egNotif.SetActive(false);
            AddOutline(egNotif, COL_PURPLE, 5);
            var egNotifTxt = CreateLabel(egNotif.transform, "Text", "UNO!", 52, COL_PURPLE);

            GameObject goPanel = CreatePanel(cv, "GameOverPanel", Vector2.zero, new Vector2(650, 450), new Color(0,0,0,0.98f));
            goPanel.SetActive(false);
            AddOutline(goPanel, COL_PURPLE, 6);
            var goTxt = CreateLabel(goPanel.transform, "Text", "OYUN BİTTİ", 40, COL_PURPLE, new Vector2(0, 70), new Vector2(600, 120));
            GameObject newGmBtn = CreateButton(goPanel.transform, "NewGameButton", "YENİ OYUN", new Vector2(0, -100), new Vector2(280, 70), COL_PURPLE);
            AddOutline(newGmBtn, COL_WHITE, 3);

            // --- UIManager & Referanslar ---
            GameObject umGO = new GameObject("UIManager");
            UIManager um = umGO.AddComponent<UIManager>();
            
            SerializedObject so = new SerializedObject(um);
            so.FindProperty("playerHandPanel").objectReferenceValue = playerHand.transform;
            so.FindProperty("discardPileTransform").objectReferenceValue = discardPile.transform;
            so.FindProperty("colorPickerPanel").objectReferenceValue = cpPanel;
            so.FindProperty("gameOverPanel").objectReferenceValue = goPanel;
            so.FindProperty("egKartPanel").objectReferenceValue = egNotif;
            so.FindProperty("currentPlayerText").objectReferenceValue = curPlayerTxt;
            so.FindProperty("statusText").objectReferenceValue = statusTxt;
            so.FindProperty("gameOverText").objectReferenceValue = goTxt;
            so.FindProperty("egKartText").objectReferenceValue = egNotifTxt;
            so.FindProperty("egKartButton").objectReferenceValue = egBtn.GetComponent<Button>();
            so.FindProperty("drawCardButton").objectReferenceValue = drawBtn.GetComponent<Button>();
            so.FindProperty("newGameButton").objectReferenceValue = newGmBtn.GetComponent<Button>();
            so.FindProperty("exitButton").objectReferenceValue = exitBtn.GetComponent<Button>();
            so.FindProperty("deckTransform").objectReferenceValue = deck.transform;
            so.FindProperty("directionIndicator").objectReferenceValue = dirInd.transform;
            so.FindProperty("btnKirmizi").objectReferenceValue = colorBtns[0];
            so.FindProperty("btnMor").objectReferenceValue = colorBtns[1];
            so.FindProperty("btnMavi").objectReferenceValue = colorBtns[2];
            so.FindProperty("btnSari").objectReferenceValue = colorBtns[3];
            
            var aiProp = so.FindProperty("aiCardCountTexts"); aiProp.arraySize = 2;
            for(int i=0; i<2; i++) aiProp.GetArrayElementAtIndex(i).objectReferenceValue = aiTexts[i];

            // --- KART PREFAB ŞABLONU (Yeni Görselli & Hover Efektli) ---
            GameObject cardTemplate = CreatePanel(cv, "CardPrefab_Template", Vector2.zero, new Vector2(125, 175), Color.white);
            cardTemplate.SetActive(false);
            cardTemplate.AddComponent<CanvasGroup>();
            cardTemplate.AddComponent<Button>();
            
            // Kart etiketi
            var label = CreateLabel(cardTemplate.transform, "Label", "0", 45, Color.black, Vector2.zero, new Vector2(120, 160));
            label.fontStyle = FontStyles.Bold;
            AddOutline(cardTemplate, COL_WHITE, 4); // Beyaz Çerçeve
            so.FindProperty("cardPrefab").objectReferenceValue = cardTemplate;

            so.ApplyModifiedProperties();

            // GameManager
            GameObject gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();

            EditorSceneManager.MarkSceneDirty(currentScene);
            EditorSceneManager.SaveScene(currentScene);
            Debug.Log("<color=green>[UNO] Sahne ve Yeni Kart Tasarımları Başarıyla Kuruldu!</color>");
        }

        private static void PrepareSprite(string path)
        {
            AssetDatabase.Refresh();
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite) {
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        private static void AddOutline(GameObject go, Color color, float width)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(width, -width);
        }

        // ========================================
        // Yardımcı Oluşturucular
        // ========================================

        private static GameObject CreateCanvas()
        {
            GameObject go = new GameObject("Canvas");
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static void CreateBackground(Transform parent)
        {
            GameObject go = new GameObject("Background");
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            
            Image img = go.AddComponent<Image>();
            
            string path = "Assets/Textures/TableBG.png";
            
            // Asset veritabanını tazele ki yeni kopyalanan resim hemen görünsün
            AssetDatabase.Refresh();

            // Texture'ı Sprite olarak ayarla (Zorunlu import ayarı)
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            Sprite tableSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (tableSprite != null)
            {
                img.sprite = tableSprite;
                img.color = Color.white;
                img.type = Image.Type.Simple;
            }
            else
            {
                img.color = COL_DARK_BG;
                Debug.LogWarning("[UNO] Assets/Textures/TableBG.png bulunamadı veya henüz Sprite değil.");
            }
        }

        private static GameObject CreatePanel(Transform parent, string name,
            Vector2 anchoredPos, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            Image img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name,
            string text, int fontSize, Color color,
            Vector2 pos = default, Vector2 size = default)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            // Etiket alanını milimetrik sığdır (Varsayılan 100x140)
            rt.sizeDelta = size == default ? new Vector2(100, 140) : size;
            
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 12; // Minimum okunaklılık sınırı
            tmp.fontSizeMax = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            return tmp;
        }

        private static GameObject CreateButton(Transform parent, string name,
            string labelText, Vector2 pos, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image img = go.AddComponent<Image>();
            img.color = color;
            Button btn = go.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = color * 1.2f;
            cb.pressedColor = color * 0.8f;
            btn.colors = cb;
            // Buton etiketi için alanı yayalım
            CreateLabel(go.transform, name + "Label", labelText, 16, Color.white, Vector2.zero, size);
            return go;
        }
    }
}
#endif
