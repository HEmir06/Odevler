// ============================================================
// ICardEffect.cs — Kart efekti arayüzü (Abstraction + Interface)
// OOP: Interface (Soyutlama), Polymorphism
// ============================================================
namespace EGKart.Cards
{
    /// <summary>
    /// Tüm kart efektleri bu arayüzü uygular.
    /// Bağımlılığı soyutlayarak farklı efektlerin aynı şekilde çağrılmasını sağlar.
    /// Soyutlama (Abstraction) ve Polimorfizm (Polymorphism) prensibi.
    /// </summary>
    public interface ICardEffect
    {
        /// <summary>Kart efektini oyun yöneticisi üzerinde uygular.</summary>
        void Execute(GameManager gameManager);
    }

    // ---- Concrete Effect Implementations ----

    /// <summary>Atla efekti: Sonraki oyuncunun sırası geçer.</summary>
    public class SkipEffect : ICardEffect
    {
        public void Execute(GameManager gameManager)
        {
            gameManager.SkipNextPlayer();
        }
    }

    /// <summary>Ters efekti: Oyun yönü tersine döner.</summary>
    public class ReverseEffect : ICardEffect
    {
        public void Execute(GameManager gameManager)
        {
            gameManager.ReverseDirection();
        }
    }

    /// <summary>+2 efekti: Sonraki oyuncu için cezayı 2 kart artırır.</summary>
    public class DrawTwoEffect : ICardEffect
    {
        public void Execute(GameManager gameManager)
        {
            gameManager.AddPenalty(2);
        }
    }

    /// <summary>Fabrika: CardType'a göre doğru ICardEffect örneği üretir.</summary>
    public static class CardEffectFactory
    {
        public static ICardEffect Create(CardType cardType)
        {
            return cardType switch
            {
                CardType.Atla   => new SkipEffect(),
                CardType.Ters   => new ReverseEffect(),
                CardType.CekIki => new DrawTwoEffect(),
                _               => new SkipEffect()
            };
        }
    }
}
