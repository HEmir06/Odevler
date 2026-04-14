// ============================================================
// AIPlayer.cs — Yapay Zeka oyuncu (Kalıtım + Polymorphism)
// OOP: Inheritance, Method Override, Polymorphism
// ============================================================
using System.Collections.Generic;
using System.Linq;
using EGKart.Cards;
using UnityEngine;

namespace EGKart.Players
{
    /// <summary>
    /// Kural tabanlı yapay zeka oyuncuyu temsil eder.
    /// BasePlayer'dan türer — Polymorphism örneği:
    /// HumanPlayer'dan farklı TakeTurn() ve ChooseCard() davranışı sergiler.
    /// </summary>
    public class AIPlayer : BasePlayer
    {
        // AI güçlük seviyesi — ileride genişletilebilir
        private AIDifficulty _difficulty;

        public AIPlayer(string name, AIDifficulty difficulty = AIDifficulty.Normal)
            : base(name)
        {
            _difficulty = difficulty;
        }

        // --- Polymorphism: BasePlayer'dan tamamen farklı davranış ---
        public override void TakeTurn(GameManager gameManager)
        {
            BaseCard topCard = gameManager.GetTopCard();
            BaseCard chosen = ChooseCard(topCard);

            if (chosen != null)
            {
                // Wild kart seçildiyse önce renk belirle
                if (chosen is WildCard wildCard)
                {
                    CardColor bestColor = ChooseColor();
                    wildCard.SetChosenColor(bestColor);
                }

                gameManager.PlayCard(this, chosen);
            }
            else
            {
                // Oynanabilir kart yoksa desteden çek
                gameManager.DrawCardForPlayer(this);
            }

            // Tek kart kaldıysa otomatik EG-KART bağır
            if (_hand.HasOneCard() && !_hasCalledEGKart)
            {
                _hasCalledEGKart = true;
                gameManager.OnEGKartCalled(this);
            }
        }

        // --- Polymorphism: Kural tabanlı kart seçimi ---
        public override BaseCard ChooseCard(BaseCard topCard)
        {
            List<BaseCard> playable = _hand.GetPlayableCards(topCard);
            if (playable.Count == 0) return null;

            // Öncelik sırası (strateji):
            // 1. Normal kartlar (Joker'i sakla)
            // 2. Eylem kartları (rakibi zorla)
            // 3. Wild Draw 4 (en güçlü, en son çare)

            // Eylem kartlarını önce oyna
            var actionCards = playable.Where(c => c is ActionCard).ToList();
            if (actionCards.Count > 0 && _difficulty != AIDifficulty.Easy)
                return actionCards[Random.Range(0, actionCards.Count)];

            // Wild olmayan kartları tercih et
            var nonWild = playable.Where(c => !c.IsWild()).ToList();
            if (nonWild.Count > 0)
                return nonWild[Random.Range(0, nonWild.Count)];

            // Son çare: Wild kart
            return playable[0];
        }

        // --- Polymorphism: En çok sahip olduğu rengi seçer ---
        public override CardColor ChooseColor()
        {
            var colorCounts = new Dictionary<CardColor, int>
            {
                { CardColor.Kirmizi, 0 },
                { CardColor.Mor,     0 },
                { CardColor.Mavi,    0 },
                { CardColor.Sari,    0 }
            };

            foreach (var card in _hand.GetCards())
            {
                if (card.Color != CardColor.Joker)
                    colorCounts[card.Color]++;
            }

            return colorCounts.OrderByDescending(kv => kv.Value).First().Key;
        }

        public override bool IsHuman() => false;
    }

    public enum AIDifficulty { Easy, Normal, Hard }
}
