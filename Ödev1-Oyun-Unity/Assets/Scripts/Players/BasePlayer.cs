// ============================================================
// BasePlayer.cs — Soyut oyuncu sınıfı
// OOP: Abstract Class, Encapsulation, Abstraction
// ============================================================
using EGKart.Cards;

namespace EGKart.Players
{
    /// <summary>
    /// Tüm oyuncu türleri için soyut temel sınıf.
    /// Soyutlama (Abstraction) ve Kapsülleme (Encapsulation) prensibini uygular.
    /// </summary>
    public abstract class BasePlayer
    {
        // --- Kapsülleme: protected alanlar ---
        protected string _name;
        protected Hand _hand;
        protected int _score;
        protected bool _hasCalledEGKart; // "EG-KART!" bağırıldı mı?

        // Public property'ler (Encapsulation)
        public string Name => _name;
        public Hand Hand => _hand;
        public int Score => _score;
        public bool HasCalledEGKart
        {
            get => _hasCalledEGKart;
            set => _hasCalledEGKart = value;
        }

        protected BasePlayer(string name)
        {
            _name = name;
            _hand = new Hand();
            _score = 0;
            _hasCalledEGKart = false;
        }

        // --- Soyutlama: Alt sınıfların implement etmesi zorunlu ---
        
        /// <summary>
        /// Oyuncunun sırası geldiğinde çağrılır.
        /// HumanPlayer ve AIPlayer farklı davranışlar sergiler (Polymorphism).
        /// </summary>
        public abstract void TakeTurn(GameManager gameManager);

        /// <summary>
        /// Üst karta göre oynanacak kartı seçer.
        /// Alt sınıflarda farklı mantıkla override edilir.
        /// </summary>
        public abstract BaseCard ChooseCard(BaseCard topCard);

        /// <summary>
        /// Wild kart oynanınca renk seçer.
        /// HumanPlayer UI'dan seçer, AIPlayer otomatik seçer.
        /// </summary>
        public abstract CardColor ChooseColor();

        // --- Shared (ortak) metotlar ---

        /// <summary>Ele kart ekler.</summary>
        public void DrawCard(BaseCard card)
        {
            if (card != null)
            {
                _hand.AddCard(card);
                _hasCalledEGKart = false; // El değişirse EG-KART durumu sıfırlanır
            }
        }

        /// <summary>Puan ekler (tur sonunda).</summary>
        public void AddScore(int points)
        {
            _score += points;
        }

        /// <summary>Yeni tura hazırlık — eli temizler.</summary>
        public void ResetHand()
        {
            _hand.Clear();
            _hasCalledEGKart = false;
        }

        /// <summary>Oyuncu human mı AI mı?</summary>
        public abstract bool IsHuman();

        public override string ToString() => _name;
    }
}
