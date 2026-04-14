// ============================================================
// GameManager.cs — Ana oyun yöneticisi (Singleton pattern)
// OOP: Orchestration, tüm OOP prensiplerinin bir araya geldiği sınıf
// ============================================================
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EGKart.Cards;
using EGKart.Players;
using UnityEngine;

namespace EGKart
{
    /// <summary>
    /// EG-Kart oyununun ana yöneticisi.
    /// Tüm oyun state'ini, sıra sırasını ve oyun akışını kontrol eder.
    /// Singleton pattern — sahnede tek bir instance olması zorunlu.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // --- Singleton ---
        public static GameManager Instance { get; private set; }

        // --- Oyun State ---
        private Deck _deck;
        private List<BasePlayer> _players;
        private int _currentPlayerIndex;
        private bool _clockwise;
        private bool _awaitingHumanInput;
        private bool _gameOver;

        // Skipping ve Stacking için
        private bool _skipNext;
        private int _pendingPenalty; // Biriken kart cezası

        // Olaylar (Events) — UIManager dinler
        public event System.Action<BasePlayer> OnTurnChanged;
        public event System.Action<BasePlayer, BaseCard> OnCardPlayed;
        public event System.Action<BasePlayer> OnCardDrawn;
        public event System.Action<string> OnEGKartAnnounced;
        public event System.Action<BasePlayer> OnGameWon;
        public event System.Action<string> OnInvalidPlay;

        // Inspector'dan ayarlanabilir
        [Header("Oyun Ayarları")]
        [SerializeField] private int aiPlayerCount = 2;
        [SerializeField] private int startHandSize = 7;
        [SerializeField] private float aiTurnDelay = 1.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private IEnumerator Start()
        {
            Debug.Log("[EG-Kart] Oyun baslatiliyor...");
            
            // UI ve sistem tam yüklensin diye 1 saniye bekletiyoruz (Çökmeyi önler)
            yield return new WaitForSeconds(1.0f);
            
            InitializeGame();
        }

        // ========================================
        // Oyun Başlatma
        // ========================================

        private void InitializeGame()
        {
            if (UIManager.Instance == null)
            {
                Debug.LogError("[EG-Kart] UIManager bulunamadi! Sahne kurulumunu kontrol edin.");
                return;
            }

            _clockwise = true;
            _awaitingHumanInput = false;
            _gameOver = false;
            _pendingPenalty = 0;
            _skipNext = false;

            _deck = new Deck();
            _players = new List<BasePlayer>();
            _players.Add(new HumanPlayer("Sen"));

            int aiCount = Mathf.Clamp(aiPlayerCount, 1, 3);
            for (int i = 0; i < aiCount; i++)
            {
                _players.Add(new AIPlayer($"AI {i+1}"));
            }

            foreach (var p in _players)
            {
                for (int i = 0; i < startHandSize; i++)
                {
                    p.DrawCard(_deck.Draw());
                }
            }

            BaseCard startCard = _deck.DealStartCard();
            _currentPlayerIndex = 0;

            OnCardPlayed?.Invoke(_players[0], startCard);
            OnTurnChanged?.Invoke(_players[_currentPlayerIndex]);
            StartTurn();
        }

        // ========================================
        // Stacking Mantığı
        // ========================================

        public void AddPenalty(int amount)
        {
            _pendingPenalty += amount;
            OnEGKartAnnounced?.Invoke($"⚠️ CEZA BİRİKTİ: {_pendingPenalty} KART!");
        }

        private void ResolvePenalty(BasePlayer player)
        {
            if (_pendingPenalty <= 0) return;

            for (int i = 0; i < _pendingPenalty; i++)
            {
                player.DrawCard(_deck.Draw());
            }

            OnEGKartAnnounced?.Invoke($"{player.Name} toplam {_pendingPenalty} kart çekti! 😱");
            OnCardDrawn?.Invoke(player);
            _pendingPenalty = 0;
        }

        // ========================================
        // Tur Yönetimi
        // ========================================

        private void StartTurn()
        {
            if (_gameOver) return;

            BasePlayer current = _players[_currentPlayerIndex];

            // Bekleyen ceza varsa ve oyuncunun elinde +2 veya +4 yoksa çekmek zorundadır
            if (_pendingPenalty > 0)
            {
                bool canStack = current.Hand.GetCards().Any(c => 
                    (c is ActionCard ac && ac.ActionType == CardType.CekIki) || 
                    (c is WildCard wc && wc.WildType == CardType.RenkSecCekDort));

                if (!canStack)
                {
                    Debug.Log($"[EG-Kart] {current.Name} cezayı karşılayamıyor, çekiyor...");
                    ResolvePenalty(current);
                    NextTurn();
                    return;
                }
                else if (!current.IsHuman())
                {
                    // AI cezayı karşılayabiliyorsa stacking kartı atsın
                    var stackCard = current.Hand.GetCards().FirstOrDefault(c => 
                        (c is ActionCard ac && ac.ActionType == CardType.CekIki) || 
                        (c is WildCard wc && wc.WildType == CardType.RenkSecCekDort));
                    
                    if (stackCard != null)
                    {
                        StartCoroutine(AITurnCoroutine((AIPlayer)current));
                        return;
                    }
                }
            }

            if (_skipNext)
            {
                _skipNext = false;
                NextTurn();
                return;
            }

            OnTurnChanged?.Invoke(current);

            if (current.IsHuman())
            {
                current.TakeTurn(this);
            }
            else
            {
                StartCoroutine(AITurnCoroutine((AIPlayer)current));
            }
        }

        private IEnumerator AITurnCoroutine(AIPlayer aiPlayer)
        {
            yield return new WaitForSeconds(aiTurnDelay);
            aiPlayer.TakeTurn(this);
            if (!_gameOver) NextTurn();
        }

        private void NextTurn()
        {
            if (_gameOver) return;
            int direction = _clockwise ? 1 : -1;
            _currentPlayerIndex = (_currentPlayerIndex + direction + _players.Count) % _players.Count;
            StartTurn();
        }

        // ========================================
        // Kart Oynama
        // ========================================

        public void PlayCard(BasePlayer player, BaseCard card)
        {
            // Stacking Kontrolü
            if (_pendingPenalty > 0)
            {
                bool isStackingCard = (card is ActionCard ac && ac.ActionType == CardType.CekIki) || 
                                     (card is WildCard wc && wc.WildType == CardType.RenkSecCekDort);
                if (!isStackingCard)
                {
                    OnInvalidPlay?.Invoke("Ceza birikmişken sadece +2 veya +4 atabilirsin!");
                    return;
                }
            }

            if (!card.CanPlayOn(_deck.TopDiscard))
            {
                OnInvalidPlay?.Invoke("Bu kartı oynayamazsın!");
                return;
            }

            player.Hand.RemoveCard(card);
            _deck.Discard(card);
            OnCardPlayed?.Invoke(player, card);

            if (card is WildCard wildCard && wildCard.ChosenColor == CardColor.Joker)
            {
                if (player.IsHuman())
                {
                    UIManager.Instance.ShowColorPicker(wildCard);
                    return;
                }
            }

            card.ApplyEffect(this);

            if (player.Hand.IsEmpty)
            {
                EndGame(player);
                return;
            }

            if (player.IsHuman()) NextTurn();
        }

        public void OnHumanCardSelected(BaseCard card)
        {
            if (!_awaitingHumanInput || !_players[_currentPlayerIndex].IsHuman()) return;

            _awaitingHumanInput = false;
            HumanPlayer human = (HumanPlayer)_players[_currentPlayerIndex];

            if (_pendingPenalty > 0)
            {
                bool isStackingCard = (card is ActionCard ac && ac.ActionType == CardType.CekIki) || 
                                     (card is WildCard wc && wc.WildType == CardType.RenkSecCekDort);
                if (!isStackingCard)
                {
                    OnInvalidPlay?.Invoke("Şu an sadece +2 veya +4 atabilirsin!");
                    _awaitingHumanInput = true;
                    return;
                }
            }

            if (!card.CanPlayOn(_deck.TopDiscard))
            {
                OnInvalidPlay?.Invoke("Bu kart şu an oynanamaz!");
                _awaitingHumanInput = true;
                return;
            }

            if (card is WildCard wildCard)
            {
                UIManager.Instance.ShowColorPicker(wildCard);
                human.Hand.RemoveCard(card);
                _deck.Discard(card);
                OnCardPlayed?.Invoke(human, card);
                return;
            }

            PlayCard(human, card);
        }

        public void OnColorChosen(WildCard wildCard, CardColor color)
        {
            wildCard.SetChosenColor(color);
            wildCard.ApplyEffect(this);

            HumanPlayer human = (HumanPlayer)_players[_currentPlayerIndex];
            if (human.Hand.IsEmpty) { EndGame(human); return; }
            NextTurn();
        }

        public void OnHumanDrawCard()
        {
            if (!_awaitingHumanInput || _gameOver) return;

            BasePlayer human = _players[_currentPlayerIndex];
            
            // 25 Kart Sınırı Kontrolü
            if (human.Hand.CardCount >= 25)
            {
                OnInvalidPlay?.Invoke("Maksimum kart sınırına (25) ulaştınız!");
                return;
            }

            _awaitingHumanInput = false;
            if (_pendingPenalty > 0) ResolvePenalty(human);
            else DrawCardForPlayer(human);
            NextTurn();
        }

        public BaseCard GetTopCard() => _deck.TopDiscard;
        public void SkipNextPlayer() => _skipNext = true;
        public void ReverseDirection()
        {
            _clockwise = !_clockwise;
            if (_players.Count == 2) _skipNext = true;
        }

        public void DrawCardForPlayer(BasePlayer player)
        {
            BaseCard drawn = _deck.Draw();
            player.DrawCard(drawn);
            OnCardDrawn?.Invoke(player);
        }

        public void OnHumanCallEGKart()
        {
            HumanPlayer human = (HumanPlayer)_players[0];
            if (human.Hand.HasOneCard())
            {
                human.HasCalledEGKart = true;
                OnEGKartAnnounced?.Invoke("UNO! 🎉");
            }
            else
            {
                OnInvalidPlay?.Invoke("UNO! zamanında değil! +2 kart ceza!");
                DrawCardForPlayer(human); DrawCardForPlayer(human);
            }
        }

        public void SetAwaitingHumanInput(bool value)
        {
            _awaitingHumanInput = value;
        }

        public void OnEGKartCalled(BasePlayer player)
        {
            OnEGKartAnnounced?.Invoke($"{player.Name} UNO! bağırdı! 🎴");
        }

        private void EndGame(BasePlayer winner)
        {
            _gameOver = true;
            int winnerScore = _players.Where(p => p != winner).Sum(p => p.Hand.GetTotalValue());
            winner.AddScore(winnerScore);
            OnGameWon?.Invoke(winner);
        }

        public List<BasePlayer> GetPlayers() => _players;
        public BasePlayer GetCurrentPlayer() => _players[_currentPlayerIndex];
        public bool IsClockwise() => _clockwise;
        public int PendingPenalty => _pendingPenalty;
    }
}
