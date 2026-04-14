// ============================================================
// CardColor.cs — Kart renk enum'ı
// Klasik UNO renkleri: Kırmızı, Mor (yeşil yerine), Mavi, Sarı
// ============================================================
namespace EGKart.Cards
{
    /// <summary>
    /// EG-Kart oyunundaki kart renkleri.
    /// Geleneksel yeşil renk, mora (Mor) dönüştürülmüştür.
    /// </summary>
    public enum CardColor
    {
        Kirmizi = 0,   // Kırmızı  (#E8344E)
        Mor     = 1,   // Mor      (#8B34E8) — Yeşil yerine
        Mavi    = 2,   // Mavi     (#3461E8)
        Sari    = 3,   // Sarı     (#E8C234)
        Joker   = 4    // Renksiz (Wild kartlar)
    }
}
