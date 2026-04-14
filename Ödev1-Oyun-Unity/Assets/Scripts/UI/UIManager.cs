// ============================================================
// UIManager.cs — UI yöneticisi (MonoBehaviour)
// Kart görsel oluşturma, panel yönetimi, animasyonlar
// ============================================================
using System.Collections;
using System.Collections.Generic;
using EGKart.Cards;
using EGKart.Players;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EGKart
{
    /// <summary>
    /// Tüm UI elemanlarını yönetir ve GameManager olaylarını dinler.
    /// Kart prefab'larını oluşturur, animasyonları oynatır.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        // --- Inspector Referansları ---
        [Header("Kart Prefab")]
        [SerializeField] private GameObject cardPrefab;

        [Header("Referanslar")]
        [SerializeField] private RectTransform playerHandPanel;
        [SerializeField] private Transform discardPileTransform;
        [SerializeField] private RectTransform deckTransform;
        [SerializeField] private Transform directionIndicator;

        [Header("Paneller")]
        [SerializeField] private GameObject colorPickerPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject egKartPanel;

        [Header("AI El Panelleri")]
        [SerializeField] private Transform[] aiHandPanels; // 0,1,2 = AI oyuncular

        [Header("UI Text")]
        [SerializeField] private TextMeshProUGUI currentPlayerText;
        [SerializeField] private TextMeshProUGUI directionText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private TextMeshProUGUI egKartText;
        [SerializeField] private TextMeshProUGUI[] aiCardCountTexts;

        [Header("Butonlar")]
        [SerializeField] private Button egKartButton;
        [SerializeField] private Button drawCardButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button exitButton;

        // Renk seçici butonlar
        [SerializeField] private Button btnKirmizi;
        [SerializeField] private Button btnMor;
        [SerializeField] private Button btnMavi;
        [SerializeField] private Button btnSari;

        // Aktif el kartı nesneleri
        private List<GameObject> _playerCardObjects = new List<GameObject>();
        private WildCard _pendingWildCard;

        // --- Neon & Premium Renk Tanımları ---
        private static readonly Dictionary<CardColor, Color> CardColors = new()
        {
            { CardColor.Kirmizi, new Color(1.00f, 0.00f, 0.10f) }, // Tam Neon Kırmızı
            { CardColor.Mor,     new Color(0.75f, 0.00f, 1.00f) }, // Canlı Neon Mor
            { CardColor.Mavi,    new Color(0.00f, 0.45f, 1.00f) }, // Elektrik Mavisi
            { CardColor.Sari,    new Color(1.00f, 0.95f, 0.00f) }, // Lazer Sarısı
            { CardColor.Joker,   new Color(1.00f, 0.00f, 0.60f) }, // Cosmic Magenta (Siyahtan Pembeden Mora Geçişli Parlak Bir Ton)
        };

        private void Update()
        {
            if (directionIndicator != null && GameManager.Instance != null)
            {
                float speed = GameManager.Instance.IsClockwise() ? -100f : 100f;
                directionIndicator.Rotate(Vector3.forward, speed * Time.deltaTime);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            SubscribeToEvents(); // Artık daha erken abone oluyoruz
        }

        private void Start()
        {
            colorPickerPanel?.SetActive(false);
            gameOverPanel?.SetActive(false);
            egKartPanel?.SetActive(false);
        }

        private void SubscribeToEvents()
        {
            StartCoroutine(DelayedSubscribe());
        }

        private IEnumerator DelayedSubscribe()
        {
            while (GameManager.Instance == null) yield return null;

            GameManager.Instance.OnTurnChanged    += HandleTurnChanged;
            GameManager.Instance.OnCardPlayed     += HandleCardPlayed;
            GameManager.Instance.OnCardDrawn      += HandleCardDrawn;
            GameManager.Instance.OnEGKartAnnounced += HandleEGKart;
            GameManager.Instance.OnGameWon        += HandleGameWon;
            GameManager.Instance.OnInvalidPlay    += HandleInvalidPlay;

            // GameManager hazır olduktan SONRA butonları bağla
            SetupButtons();
            Debug.Log("[UI] Tum sistemler baglandi!");
        }

        private void SetupButtons()
        {
            // Lambda kullanıyoruz: tıklandığı anda GameManager.Instance'a bakıyor
            if (egKartButton != null)
                egKartButton.onClick.AddListener(() => GameManager.Instance?.OnHumanCallEGKart());

            if (drawCardButton != null)
                drawCardButton.onClick.AddListener(() => GameManager.Instance?.OnHumanDrawCard());

            if (newGameButton != null)
                newGameButton.onClick.AddListener(() =>
                    UnityEngine.SceneManagement.SceneManager.LoadScene(
                        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));

            if (exitButton != null)
                exitButton.onClick.AddListener(QuitGame);

            if (btnKirmizi != null) btnKirmizi.onClick.AddListener(() => OnColorButtonPressed(CardColor.Kirmizi));
            if (btnMor != null)     btnMor.onClick.AddListener(() => OnColorButtonPressed(CardColor.Mor));
            if (btnMavi != null)    btnMavi.onClick.AddListener(() => OnColorButtonPressed(CardColor.Mavi));
            if (btnSari != null)    btnSari.onClick.AddListener(() => OnColorButtonPressed(CardColor.Sari));

            Debug.Log("[UI] Butonlar basariyla baglandi.");
        }

        // ========================================
        // Event Handlers
        // ========================================

        private void HandleTurnChanged(BasePlayer player)
        {
            if (player == null || currentPlayerText == null) return;
            
            currentPlayerText.text = $"SIRA: {player.Name.ToUpper()}";

            bool isHuman = player.IsHuman();
            if (drawCardButton != null) drawCardButton.interactable = isHuman;
            if (egKartButton != null) egKartButton.interactable = isHuman;

            UpdateTurnGlow(player);

            if (statusText != null)
                statusText.text = isHuman ? "<color=#B026FF>SENIN SIRAN!</color> Bir kart at veya cek." : $"{player.Name.ToUpper()} HAMLE YAPIYOR...";

            RefreshPlayerHand(GameManager.Instance.GetPlayers()[0], GameManager.Instance.GetTopCard());
            RefreshAICardCounts();
        }

        private void UpdateTurnGlow(BasePlayer currentPlayer)
        {
            // İnsan oyuncu ışığı (playerHandPanel üzerindeki Outline)
            if (playerHandPanel != null)
            {
                var outline = playerHandPanel.GetComponent<Outline>();
                if (outline != null) outline.effectDistance = currentPlayer.IsHuman() ? new Vector2(8, -8) : Vector2.zero;
            }

            // AI oyuncu ışıkları
            var allPlayers = GameManager.Instance.GetPlayers();
            for (int i = 1; i < allPlayers.Count; i++)
            {
                if (i - 1 < aiCardCountTexts.Length && aiCardCountTexts[i - 1] != null)
                {
                    // AI paneli etiketin parent'ıdır
                    var outline = aiCardCountTexts[i - 1].transform.parent.GetComponent<Outline>();
                    if (outline != null)
                    {
                        bool isThisAI = (allPlayers[i] == currentPlayer);
                        outline.effectDistance = isThisAI ? new Vector2(6, -6) : Vector2.zero;
                    }
                }
            }
        }

        private void HandleCardPlayed(BasePlayer player, BaseCard card)
        {
            RefreshDiscardPile(card);
            // Kart atıldığında kendi elimizi hemen tazele
            RefreshPlayerHand(GameManager.Instance.GetPlayers()[0], card);
            RefreshAICardCounts();
            SetStatus($"{player.Name} oynadı: {card.GetDisplayName()}");
        }

        private void HandleCardDrawn(BasePlayer player)
        {
            // Kart çekildiğinde kendi elimizi hemen tazele
            RefreshPlayerHand(GameManager.Instance.GetPlayers()[0], GameManager.Instance.GetTopCard());
            RefreshAICardCounts();
            SetStatus($"{player.Name} kart çekti.");
        }

        private void HandleEGKart(string message)
        {
            StartCoroutine(ShowEGKartAnimation(message));
        }

        private void HandleGameWon(BasePlayer winner)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (gameOverText != null) gameOverText.text = $"🏆 {winner.Name} KAZANDI!\nPuan: {winner.Score}";
        }

        private void HandleInvalidPlay(string message)
        {
            SetStatus($"⚠️ {message}");
        }

        // ========================================
        // Kart El Görünümü & Animasyonlar
        // ========================================

        private void RefreshPlayerHand(BasePlayer player, BaseCard topCard)
        {
            if (playerHandPanel == null || cardPrefab == null || player == null) return;

            foreach (Transform child in playerHandPanel) Destroy(child.gameObject);
            _playerCardObjects.Clear();

            var cards = player.Hand.GetCards();
            int count = cards.Count;

            // MATEMATIKSEL GARANTİ: (N-1)*Aralık + KartGenisliği asla panelWidth'i geçemez.
            float panelWidth = 820f; 
            float cardWidth = 125f;
            float spacing = count <= 1 ? 0 : Mathf.Min(110f, (panelWidth - cardWidth) / (count - 1));
            float startX = -(count - 1) * spacing * 0.5f;

            // Kart sayısı arttıkça kartları küçülterek üst üste aşırı binmeyi önleyelim
            float dynamicScale = count > 8 ? Mathf.Lerp(1.0f, 0.65f, (count - 8) / 17f) : 1.0f;

            for (int i = 0; i < count; i++)
            {
                try {
                    GameObject cardGO = Instantiate(cardPrefab, playerHandPanel);
                    RectTransform rt = cardGO.GetComponent<RectTransform>();
                    
                    float x = startX + (i * spacing);
                    rt.localPosition = new Vector3(x, 0, 0);
                    rt.localRotation = Quaternion.identity;
                    rt.localScale = Vector3.one * dynamicScale;

                    SetupCardVisual(cardGO, cards[i], topCard);
                    _playerCardObjects.Add(cardGO);
                } catch { continue; }
            }
        }
        
        public void QuitGame()
        {
            Debug.Log("[EG-Kart] Oyundan cikiliyor...");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private IEnumerator AnimateCardEntry(Transform cardT, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            Vector3 targetPos = cardT.localPosition;
            Vector3 startPos;

            // ORTADAN (DESTEDEN) GELME MANTIĞI
            if (deckTransform != null)
            {
                // Deste pozisyonunu el panelinin local koordinatına çeviriyoruz
                startPos = cardT.parent.InverseTransformPoint(deckTransform.position);
            }
            else
            {
                startPos = targetPos + new Vector3(0, -400, 0); // Yedek: Aşağıdan başla
            }
            
            cardT.localPosition = startPos;
            cardT.localScale = Vector3.zero;
            
            float elapsed = 0;
            float duration = 0.5f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float ease = 1f - Mathf.Pow(1f - t, 3f); // Ease Out Cubic
                
                cardT.localPosition = Vector3.LerpUnclamped(startPos, targetPos, ease);
                cardT.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, ease);
                
                yield return null;
            }
            
            cardT.localPosition = targetPos;
            cardT.localScale = Vector3.one;
        }

        private void SetupCardVisual(GameObject cardGO, BaseCard card, BaseCard topCard)
        {
            cardGO.SetActive(true);
            // Arka plan rengi
            Image bg = cardGO.GetComponent<Image>();
            if (bg != null)
                bg.color = CardColors.ContainsKey(card.Color) ? CardColors[card.Color] : Color.gray;

            // Kart yazısı
            TextMeshProUGUI label = cardGO.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = GetCardLabel(card);

            // Oynanabilirlik kontrolü - Stacking (Birikme) varsa kısıtlarız
            int pending = GameManager.Instance.PendingPenalty;
            bool playable = false;

            if (pending > 0)
            {
                // Ceza varken sadece +2 veya +4 oynanabilir
                playable = (card is ActionCard ac && ac.ActionType == CardType.CekIki) || 
                           (card is WildCard wc && wc.WildType == CardType.RenkSecCekDort);
            }
            else
            {
                playable = card.CanPlayOn(topCard);
            }

            CanvasGroup cg = cardGO.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = playable ? 1.0f : 0.65f; // Bir tık daha belirgin olsun

            // Tıklama event'i
            Button btn = cardGO.GetComponent<Button>();
            if (btn != null)
            {
                BaseCard capturedCard = card; // Değişkeni buraya geri ekliyoruz
                // Stacking varken geçersiz karta basılması durumunda GameManager uyarı verir
                btn.interactable = true; 
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => GameManager.Instance.OnHumanCardSelected(capturedCard));
            }
        }

        private string GetCardLabel(BaseCard card)
        {
            // Daha anlaşılır ve şık "logo-su" metinler
            return card switch
            {
                NumberCard nc => $"<b>{nc.Number}</b>",
                ActionCard ac => ac.ActionType switch
                {
                    CardType.Atla   => "<b>[ATLA]</b>",
                    CardType.Ters   => "<b><< TERS >></b>",
                    CardType.CekIki => "<b>+2</b>",
                    _               => "<b>ACT</b>"
                },
                WildCard wc => wc.WildType == CardType.RenkSecCekDort ? "<b>+4 *</b>" : "<b>JOKER *</b>",
                _ => "?"
            };
        }

        private void RefreshDiscardPile(BaseCard topCard)
        {
            if (topCard == null || discardPileTransform == null) return;

            // Arka plan rengini güncelle
            Image discardImg = discardPileTransform.GetComponent<Image>();
            if (discardImg != null && CardColors.ContainsKey(topCard.Color))
                discardImg.color = CardColors[topCard.Color];

            // Rakamı/ikonu güncelle
            TextMeshProUGUI discardLabel = discardPileTransform.GetComponentInChildren<TextMeshProUGUI>();
            if (discardLabel != null)
            {
                discardLabel.text = GetCardLabel(topCard);
                discardLabel.color = (topCard.Color == CardColor.Sari) ? Color.black : Color.white;
            }

            // Hafif rotasyon ekleyelim (Juice effect)
            discardPileTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
        }

        private void RefreshAICardCounts()
        {
            var players = GameManager.Instance.GetPlayers();
            for (int i = 1; i < players.Count && i - 1 < aiCardCountTexts.Length; i++)
            {
                aiCardCountTexts[i - 1].text = $"{players[i].Name}\n{players[i].Hand.CardCount} Kart";
            }
        }

        // ========================================
        // Renk Seçici
        // ========================================

        public void ShowColorPicker(WildCard wildCard)
        {
            _pendingWildCard = wildCard;
            colorPickerPanel.SetActive(true);
        }

        private void OnColorButtonPressed(CardColor color)
        {
            colorPickerPanel.SetActive(false);
            if (_pendingWildCard != null)
                GameManager.Instance.OnColorChosen(_pendingWildCard, color);
            _pendingWildCard = null;
        }

        // ========================================
        // UNO! Animasyonu
        // ========================================

        private IEnumerator ShowEGKartAnimation(string message)
        {
            egKartPanel.SetActive(true);
            egKartText.text = message.Replace("EG-KART", "UNO!");
            yield return new WaitForSeconds(2f);
            egKartPanel.SetActive(false);
        }

        private void SetStatus(string message)
        {
            statusText.text = $"<color=#B026FF>{message}</color>";
        }
    }
}
