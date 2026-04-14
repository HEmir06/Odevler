// ============================================================
// HumanPlayer.cs — İnsan oyuncu (Kalıtım + Polymorphism)
// OOP: Inheritance, Method Override
// ============================================================
using System.Collections.Generic;
using EGKart.Cards;

namespace EGKart.Players
{
    /// <summary>
    /// Kullanıcı tarafından kontrol edilen oyuncuyu temsil eder.
    /// BasePlayer'dan türer — Kalıtım (Inheritance) örneği.
    /// TakeTurn() ve ChooseCard() metotları UI aracılığıyla çalışır.
    /// </summary>
    public class HumanPlayer : BasePlayer
    {
        // İnsan oyuncunun seçtiği kart (UI ile set edilir)
        private BaseCard _pendingCard;
        private CardColor _pendingColor;
        private bool _waitingForColorChoice;

        public bool WaitingForColorChoice => _waitingForColorChoice;

        public HumanPlayer(string name) : base(name) { }

        // --- Polymorphism: BasePlayer soyut metodunu override eder ---
        public override void TakeTurn(GameManager gameManager)
        {
            // İnsan oyuncunun sırası geldiğinde UI'yı aktif eder
            // Kart seçimi UI'dan gelir — GameManager bu metodu çağırır,
            // UI Manager ise oyuncunun tıklamasını bekler
            gameManager.SetAwaitingHumanInput(true);
        }

        /// <summary>UI'dan kart seçildiğinde GameManager tarafından çağrılır.</summary>
        public void SelectCard(BaseCard card)
        {
            _pendingCard = card;
        }

        /// <summary>Wild kart seçimi için UI bekleme moduna alır.</summary>
        public void BeginColorChoice()
        {
            _waitingForColorChoice = true;
        }

        /// <summary>UI'dan renk seçildiğinde çağrılır.</summary>
        public void SelectColor(CardColor color)
        {
            _pendingColor = color;
            _waitingForColorChoice = false;
        }

        // --- Polymorphism ---
        public override BaseCard ChooseCard(BaseCard topCard)
        {
            // İnsan için bu metot; UI'dan gelen seçimi iletir
            return _pendingCard;
        }

        public override CardColor ChooseColor()
        {
            return _pendingColor;
        }

        public override bool IsHuman() => true;
    }
}
