// ============================================================
// CardType.cs — Kart türü enum'ı
// ============================================================
namespace EGKart.Cards
{
    public enum CardType
    {
        Sayi = 0,       // 0-9 sayı kartları
        Atla = 10,      // Skip
        Ters = 11,      // Reverse
        CekIki = 12,    // Draw 2
        RenkSec = 13,   // Wild
        RenkSecCekDort = 14  // Wild Draw 4
    }
}
