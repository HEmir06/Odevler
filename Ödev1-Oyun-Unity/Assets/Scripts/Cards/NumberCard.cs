// ============================================================
// NumberCard.cs — Sayı kartı (Kalıtım: BaseCard'dan türer)
// OOP: Inheritance, Method Override (Polymorphism)
// ============================================================
namespace EGKart.Cards
{
    /// <summary>
    /// 0-9 arası sayı kartlarını temsil eder.
    /// BaseCard'dan türer — Kalıtım (Inheritance) örneği.
    /// </summary>
    public class NumberCard : BaseCard
    {
        public int Number { get; private set; }

        public NumberCard(CardColor color, int number)
            : base(color, number)
        {
            Number = number;
        }

        // --- Polymorphism: BaseCard'ın soyut metotlarını override eder ---
        public override string GetDisplayName()
        {
            return $"{Color} {Number}";
        }

        public override void ApplyEffect(GameManager gameManager)
        {
            // Sayı kartının özel etkisi yoktur — sıra sadece geçer
        }

        public override bool CanPlayOn(BaseCard topCard)
        {
            if (topCard == null) return true; // Oyun başında null olabilir

            if (topCard is NumberCard numCard)
                return Color == topCard.Color || Number == numCard.Number;

            if (topCard is ActionCard actCard)
                return Color == topCard.Color;

            if (topCard is WildCard wildCard)
                return wildCard.ChosenColor == Color;

            return false;
        }

        public override bool IsWild() => false;
    }
}
