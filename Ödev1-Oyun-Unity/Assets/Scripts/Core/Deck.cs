// ============================================================
// Deck.cs — Deste yönetimi
// OOP: Encapsulation (Kapsülleme)
// ============================================================
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EGKart.Cards
{
    /// <summary>
    /// 108 kartlık tam EG-Kart destesini yönetir.
    /// Kapsülleme (Encapsulation): _cards private, draw/shuffle public API.
    /// </summary>
    public class Deck
    {
        // --- Kapsülleme: private alan ---
        private List<BaseCard> _cards;
        private List<BaseCard> _discardPile; // Atık destesi

        public int RemainingCards => _cards.Count;
        public BaseCard TopDiscard => _discardPile.Count > 0 ? _discardPile[^1] : null;

        public Deck()
        {
            _cards = new List<BaseCard>();
            _discardPile = new List<BaseCard>();
            Build();
            Shuffle();
        }

        /// <summary>
        /// 108 kartlık tam EG-Kart destesini oluşturur.
        /// Her renk: 1 adet 0, 2 adet 1-9, 2 Atla, 2 Ters, 2 +2 = 25 kart × 4 renk = 100
        /// 4 Renk Seç + 4 Renk Seç +4 = 8 kart
        /// Toplam: 108 kart
        /// </summary>
        private void Build()
        {
            CardColor[] colors = { CardColor.Kirmizi, CardColor.Mor, CardColor.Mavi, CardColor.Sari };

            foreach (CardColor color in colors)
            {
                // 0 kartı — 1 adet
                _cards.Add(new NumberCard(color, 0));

                // 1-9 kartları — 2 adet her biri
                for (int i = 1; i <= 9; i++)
                {
                    _cards.Add(new NumberCard(color, i));
                    _cards.Add(new NumberCard(color, i));
                }

                // Eylem kartları — 2 adet her biri
                _cards.Add(new ActionCard(color, CardType.Atla));
                _cards.Add(new ActionCard(color, CardType.Atla));
                _cards.Add(new ActionCard(color, CardType.Ters));
                _cards.Add(new ActionCard(color, CardType.Ters));
                _cards.Add(new ActionCard(color, CardType.CekIki));
                _cards.Add(new ActionCard(color, CardType.CekIki));
            }

            // Joker kartlar — 4 adet Renk Seç, 4 adet Renk Seç +4
            for (int i = 0; i < 4; i++)
            {
                _cards.Add(new WildCard(CardType.RenkSec));
                _cards.Add(new WildCard(CardType.RenkSecCekDort));
            }
        }

        /// <summary>Desteyi karıştırır (Fisher-Yates algoritması).</summary>
        public void Shuffle()
        {
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_cards[i], _cards[j]) = (_cards[j], _cards[i]);
            }
        }

        /// <summary>Desteden 1 kart çeker. Deste bitmişse atık destesi karıştırılıp yeniden kullanılır.</summary>
        public BaseCard Draw()
        {
            if (_cards.Count == 0)
                ReshuffleDiscard();

            if (_cards.Count == 0)
                return null;

            BaseCard card = _cards[^1];
            _cards.RemoveAt(_cards.Count - 1);
            return card;
        }

        /// <summary>Oynanan kartı atık destesine ekler.</summary>
        public void Discard(BaseCard card)
        {
            _discardPile.Add(card);
        }

        /// <summary>İlk kartı başlatmak için atık destesine koyar.</summary>
        public BaseCard DealStartCard()
        {
            BaseCard card;
            do
            {
                card = Draw();
            } while (card is WildCard); // İlk kart joker olamaz

            _discardPile.Add(card);
            return card;
        }

        /// <summary>Atık destesini karıştırıp ana desteye ekler (bitmişse).</summary>
        private void ReshuffleDiscard()
        {
            if (_discardPile.Count <= 1) return;

            BaseCard top = _discardPile[^1];
            _discardPile.RemoveAt(_discardPile.Count - 1);
            _cards.AddRange(_discardPile);
            _discardPile.Clear();
            _discardPile.Add(top);
            Shuffle();
        }
    }
}
