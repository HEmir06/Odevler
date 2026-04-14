// ============================================================
// WildCard.cs — Joker kartı (Kalıtım + Polymorphism)
// OOP: Inheritance, Polymorphism
// ============================================================
namespace EGKart.Cards
{
    /// <summary>
    /// Renk Seç (Joker) ve Renk Seç +4 kartlarını temsil eder.
    /// Renksiz kartlar — oynanınca oyuncu renk seçer.
    /// </summary>
    public class WildCard : BaseCard
    {
        public CardType WildType { get; private set; }
        
        // Oynanırken seçilen renk
        public CardColor ChosenColor { get; private set; }

        public WildCard(CardType wildType)
            : base(CardColor.Joker, wildType == CardType.RenkSecCekDort ? 50 : 40)
        {
            WildType = wildType;
            ChosenColor = CardColor.Joker;
        }

        /// <summary>Oyuncu/AI renk seçtiğinde çağrılır.</summary>
        public void SetChosenColor(CardColor color)
        {
            ChosenColor = color;
        }

        // --- Polymorphism: BaseCard soyut metotlarını override eder ---
        public override string GetDisplayName()
        {
            return WildType switch
            {
                CardType.RenkSec        => "Renk Seç",
                CardType.RenkSecCekDort => "Renk Seç +4",
                _                       => "Joker"
            };
        }

        public override void ApplyEffect(GameManager gameManager)
        {
            if (WildType == CardType.RenkSecCekDort)
            {
                // Sonraki oyuncu için cezayı 4 artırır (Stacking)
                gameManager.AddPenalty(4);
            }
            // Renk değişimi UI Manager üzerinden yapılır (GameManager koordine eder)
        }

        public override bool CanPlayOn(BaseCard topCard)
        {
            // Joker kartlar her zaman oynanabilir
            return true;
        }

        public override bool IsWild() => true;
    }
}
