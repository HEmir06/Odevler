// ============================================================
// Hand.cs — El (eldeki kartlar) yönetimi
// OOP: Encapsulation (Kapsülleme)
// ============================================================
using System.Collections.Generic;
using System.Linq;

namespace EGKart.Cards
{
    /// <summary>
    /// Bir oyuncunun elindeki kartları yönetir.
    /// Kapsülleme (Encapsulation) prensibi: _cards private, public API üzerinden erişim.
    /// </summary>
    public class Hand
    {
        // --- Kapsülleme: private alan ---
        private List<BaseCard> _cards;

        public int CardCount => _cards.Count;
        public bool IsEmpty => _cards.Count == 0;

        public Hand()
        {
            _cards = new List<BaseCard>();
        }

        /// <summary>Ele kart ekler.</summary>
        public void AddCard(BaseCard card)
        {
            if (card != null)
                _cards.Add(card);
        }

        /// <summary>Elden kart çıkarır ve döndürür.</summary>
        public bool RemoveCard(BaseCard card)
        {
            return _cards.Remove(card);
        }

        /// <summary>Eldeki tüm kartların salt okunur görünümünü döndürür.</summary>
        public IReadOnlyList<BaseCard> GetCards()
        {
            return _cards.AsReadOnly();
        }

        /// <summary>Belirtilen üst karta oynanabilecek kartları döndürür.</summary>
        public List<BaseCard> GetPlayableCards(BaseCard topCard)
        {
            return _cards.Where(c => c.CanPlayOn(topCard)).ToList();
        }

        /// <summary>Eldeki toplam puan değerini hesaplar (kazanan skoru için).</summary>
        public int GetTotalValue()
        {
            return _cards.Sum(c => c.Value);
        }

        /// <summary>Elde sadece 1 kart kaldı mı? (EG-KART durumu)</summary>
        public bool HasOneCard() => _cards.Count == 1;

        /// <summary>Eli tamamen temizler.</summary>
        public void Clear()
        {
            _cards.Clear();
        }
    }
}
