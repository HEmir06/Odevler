// ============================================================
// ActionCard.cs — Özel eylem kartı (Kalıtım + Polymorphism)
// OOP: Inheritance, Polymorphism, Interface kullanımı
// ============================================================
namespace EGKart.Cards
{
    /// <summary>
    /// Atla, Ters ve +2 gibi eylem kartlarını temsil eder.
    /// BaseCard'dan türer — Kalıtım + Çok Biçimlilik (Polymorphism) örneği.
    /// </summary>
    public class ActionCard : BaseCard
    {
        public CardType ActionType { get; private set; }
        private ICardEffect _effect;

        public ActionCard(CardColor color, CardType actionType)
            : base(color, 20) // Eylem kartları değer olarak 20 puan taşır
        {
            ActionType = actionType;
            _effect = CardEffectFactory.Create(actionType);
        }

        // --- Polymorphism: BaseCard'ın soyut metotlarını override eder ---
        public override string GetDisplayName()
        {
            return ActionType switch
            {
                CardType.Atla    => $"{Color} Atla",
                CardType.Ters    => $"{Color} Ters",
                CardType.CekIki  => $"{Color} +2",
                _                => $"{Color} Eylem"
            };
        }

        public override void ApplyEffect(GameManager gameManager)
        {
            // ICardEffect interface üzerinden polymorphic çağrı
            _effect.Execute(gameManager);
        }

        public override bool CanPlayOn(BaseCard topCard)
        {
            if (topCard == null) return true; // Oyun başında null olabilir

            if (topCard is WildCard wildCard)
                return wildCard.ChosenColor == Color;

            if (topCard is ActionCard actCard)
                return Color == topCard.Color || ActionType == actCard.ActionType;

            return Color == topCard.Color;
        }

        public override bool IsWild() => false;
    }
}
