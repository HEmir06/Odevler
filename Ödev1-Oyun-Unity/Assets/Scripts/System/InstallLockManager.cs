// ============================================================
// InstallLockManager.cs — Tek seferlik kurulum kilit mekanizması
// ============================================================
using UnityEngine;
using System.IO;

namespace EGKart
{
    public static class InstallLockManager
    {
        public static bool IsValidInstall()
        {
            // Kurulum kısıtlaması artık tamamen Inno Setup (Installer) tarafından yönetiliyor.
            // Oyunun yetki hatası alıp çökmesini önlemek için uygulama içi kilit kaldırıldı.
            return true;
        }

        public static void WriteLock()
        {
            // Yetki hatalarını önlemek için boş bırakıldı.
        }
    }
}
