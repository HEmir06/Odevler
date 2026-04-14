// ============================================================
// BaseCard.cs — Soyut temel kart sınıfı (Abstraction)
// OOP: Abstract Class, Encapsulation, Abstraction
// ============================================================
using UnityEngine;

namespace EGKart.Cards
{
    /// <summary>
    /// Tüm kart türleri için soyut temel sınıf.
    /// Soyutlama (Abstraction) ve Kapsülleme (Encapsulation) prensiplerini uygular.
    /// </summary>
    public abstract class BaseCard
    {
        // --- Kapsülleme: private alanlar, public property'ler ---
        private CardColor _color;
        private int _value;
        private string _displayName;

        public CardColor Color
        {
            get { return _color; }
            protected set { _color = value; }
        }

        public int Value
        {
            get { return _value; }
            protected set { _value = value; }
        }

        public string DisplayName
        {
            get { return _displayName; }
            protected set { _displayName = value; }
        }

        // --- Soyutlama: Alt sınıfların implement etmesi zorunlu ---
        /// <summary>Kartın ekranda gösterilecek adını döndürür.</summary>
        public abstract string GetDisplayName();

        /// <summary>Kartın oyun üzerindeki özel etkisini uygular.</summary>
        public abstract void ApplyEffect(GameManager gameManager);

        /// <summary>Bu kartın üst kart üzerine oynanabilir olup olmadığını kontrol eder.</summary>
        public abstract bool CanPlayOn(BaseCard topCard);

        /// <summary>Kartın joker (renksiz) olup olmadığını döndürür.</summary>
        public abstract bool IsWild();

        // --- Base constructor ---
        protected BaseCard(CardColor color, int value)
        {
            _color = color;
            _value = value;
        }

        public override string ToString()
        {
            return GetDisplayName();
        }
    }
}
