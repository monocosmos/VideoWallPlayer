namespace VideoWallPlayer.Native;

internal sealed record LanguageOption(string Code, string Name);

internal sealed class Localizer
{
    private static readonly Dictionary<string, string> Turkish = new()
    {
        ["app.title"] = "VideoWallPlayer",
        ["brand.subtitle"] = "Kenar çizgisi, kontrol çubuğu ve metin olmadan profesyonel video wall oynatma",
        ["playlist"] = "Oynatma Listesi",
        ["settings"] = "Ayarlar",
        ["language"] = "Dil",
        ["add.videos"] = "Video Ekle",
        ["add.folder"] = "Klasör Ekle",
        ["remove.selected"] = "Seçileni Sil",
        ["clear"] = "Temizle",
        ["move.up"] = "Yukarı",
        ["move.down"] = "Aşağı",
        ["repeat.mode"] = "Tekrar modu",
        ["repeat.all"] = "Liste bitince başa dön",
        ["repeat.one"] = "Aynı videoyu tekrarla",
        ["repeat.none"] = "Liste bitince dur",
        ["shuffle"] = "Karışık oynat",
        ["fullscreen"] = "Tam ekran oynat",
        ["kiosk"] = "Kiosk modu",
        ["muted"] = "Sessiz",
        ["target.display"] = "Hedef ekran",
        ["display"] = "Ekran",
        ["hardware.acceleration"] = "Donanım hızlandırma",
        ["auto"] = "Otomatik",
        ["disabled"] = "Kapalı",
        ["video.output"] = "Video çıkışı",
        ["gpu.preference"] = "GPU tercihi",
        ["gpu.default"] = "Windows varsayılanı",
        ["gpu.power"] = "Güç tasarrufu GPU",
        ["gpu.high"] = "Yüksek performans GPU",
        ["gpu.detected"] = "Algılanan GPU",
        ["cache"] = "Önbellek (ms)",
        ["gpu.note"] = "GPU tercihi Windows'a uygulama tercihi olarak yazılır. Fiziksel GPU seçimini Windows ve ekran kartı sürücüsü kesinleştirir; değişiklik için uygulamayı yeniden açmak gerekebilir.",
        ["play"] = "Videoyu Başlat",
        ["save.settings"] = "Ayar Kaydet",
        ["exit"] = "Çıkış",
        ["video.dialog.title"] = "Video dosyaları seç",
        ["video.files"] = "Video dosyaları",
        ["all.files"] = "Tüm dosyalar",
        ["folder.dialog.title"] = "Video klasörü seç",
        ["status.added"] = "{0} video eklendi.",
        ["status.count"] = "{0} video listede.",
        ["settings.saved"] = "Ayarlar kaydedildi.",
        ["empty.playlist.message"] = "Oynatma listesi boş. Uygulama exe yanındaki videos klasörünü izleyecek. Devam edilsin mi?",
        ["empty.playlist.title"] = "Oynatma listesi boş",
        ["gpu.message.default"] = "GPU tercihi Windows varsayılanına alındı.",
        ["gpu.message.high"] = "Windows için yüksek performans GPU tercihi kaydedildi.",
        ["gpu.message.power"] = "Windows için güç tasarrufu GPU tercihi kaydedildi."
    };

    private static readonly Dictionary<string, string> English = new()
    {
        ["app.title"] = "VideoWallPlayer",
        ["brand.subtitle"] = "Professional video wall playback without borders, controls, or on-screen text",
        ["playlist"] = "Playlist",
        ["settings"] = "Settings",
        ["language"] = "Language",
        ["add.videos"] = "Add Videos",
        ["add.folder"] = "Add Folder",
        ["remove.selected"] = "Remove Selected",
        ["clear"] = "Clear",
        ["move.up"] = "Up",
        ["move.down"] = "Down",
        ["repeat.mode"] = "Repeat mode",
        ["repeat.all"] = "Loop playlist",
        ["repeat.one"] = "Repeat current video",
        ["repeat.none"] = "Stop at end",
        ["shuffle"] = "Shuffle",
        ["fullscreen"] = "Fullscreen",
        ["kiosk"] = "Kiosk mode",
        ["muted"] = "Muted",
        ["target.display"] = "Target display",
        ["display"] = "Display",
        ["hardware.acceleration"] = "Hardware acceleration",
        ["auto"] = "Auto",
        ["disabled"] = "Disabled",
        ["video.output"] = "Video output",
        ["gpu.preference"] = "GPU preference",
        ["gpu.default"] = "Windows default",
        ["gpu.power"] = "Power saving GPU",
        ["gpu.high"] = "High performance GPU",
        ["gpu.detected"] = "Detected GPU",
        ["cache"] = "Cache (ms)",
        ["gpu.note"] = "GPU preference is written to Windows per-app graphics settings. Windows and the display driver make the final physical GPU decision; restarting the app may be required.",
        ["play"] = "Start Video",
        ["save.settings"] = "Save Settings",
        ["exit"] = "Exit",
        ["video.dialog.title"] = "Select video files",
        ["video.files"] = "Video files",
        ["all.files"] = "All files",
        ["folder.dialog.title"] = "Select video folder",
        ["status.added"] = "{0} video(s) added.",
        ["status.count"] = "{0} video(s) in playlist.",
        ["settings.saved"] = "Settings saved.",
        ["empty.playlist.message"] = "The playlist is empty. The app will watch the videos folder next to the exe. Continue?",
        ["empty.playlist.title"] = "Empty playlist",
        ["gpu.message.default"] = "GPU preference was reset to Windows default.",
        ["gpu.message.high"] = "Windows high performance GPU preference was saved.",
        ["gpu.message.power"] = "Windows power saving GPU preference was saved."
    };

    private static readonly Dictionary<string, Dictionary<string, string>> Packs = new()
    {
        ["tr"] = Turkish,
        ["en"] = English,
        ["es"] = Make("Reproducir", "Ajustes", "Lista de reproducción", "Idioma"),
        ["zh-Hans"] = Make("播放", "设置", "播放列表", "语言"),
        ["hi"] = Make("चलाएँ", "सेटिंग्स", "प्लेलिस्ट", "भाषा"),
        ["ar"] = Make("تشغيل", "الإعدادات", "قائمة التشغيل", "اللغة"),
        ["bn"] = Make("চালান", "সেটিংস", "প্লেলিস্ট", "ভাষা"),
        ["pt"] = Make("Reproduzir", "Configurações", "Lista de reprodução", "Idioma"),
        ["ru"] = Make("Воспроизвести", "Настройки", "Плейлист", "Язык"),
        ["ja"] = Make("再生", "設定", "プレイリスト", "言語"),
        ["de"] = Make("Abspielen", "Einstellungen", "Wiedergabeliste", "Sprache"),
        ["fr"] = Make("Lire", "Paramètres", "Liste de lecture", "Langue"),
        ["id"] = Make("Putar", "Pengaturan", "Daftar putar", "Bahasa"),
        ["ur"] = Make("چلائیں", "ترتیبات", "پلے لسٹ", "زبان"),
        ["vi"] = Make("Phát", "Cài đặt", "Danh sách phát", "Ngôn ngữ"),
        ["ko"] = Make("재생", "설정", "재생 목록", "언어"),
        ["it"] = Make("Riproduci", "Impostazioni", "Playlist", "Lingua"),
        ["pl"] = Make("Odtwórz", "Ustawienia", "Lista odtwarzania", "Język"),
        ["fa"] = Make("پخش", "تنظیمات", "فهرست پخش", "زبان"),
        ["nl"] = Make("Afspelen", "Instellingen", "Afspeellijst", "Taal")
    };

    public static readonly LanguageOption[] Languages =
    [
        new("tr", "Türkçe"),
        new("en", "English"),
        new("es", "Español"),
        new("zh-Hans", "中文"),
        new("hi", "हिन्दी"),
        new("ar", "العربية"),
        new("bn", "বাংলা"),
        new("pt", "Português"),
        new("ru", "Русский"),
        new("ja", "日本語"),
        new("de", "Deutsch"),
        new("fr", "Français"),
        new("id", "Bahasa Indonesia"),
        new("ur", "اردو"),
        new("vi", "Tiếng Việt"),
        new("ko", "한국어"),
        new("it", "Italiano"),
        new("pl", "Polski"),
        new("fa", "فارسی"),
        new("nl", "Nederlands")
    ];

    public Localizer(string? languageCode)
    {
        LanguageCode = Languages.Any(language => language.Code == languageCode) ? languageCode! : "tr";
    }

    public string LanguageCode { get; }

    public bool IsRightToLeft => LanguageCode is "ar" or "ur" or "fa";

    public string T(string key)
    {
        if (Packs.TryGetValue(LanguageCode, out var pack) && pack.TryGetValue(key, out var value))
        {
            return value;
        }

        return English.TryGetValue(key, out var fallback) ? fallback : key;
    }

    public string Format(string key, params object[] args)
    {
        return string.Format(T(key), args);
    }

    private static Dictionary<string, string> Make(string play, string settings, string playlist, string language)
    {
        var pack = new Dictionary<string, string>(English)
        {
            ["play"] = play,
            ["settings"] = settings,
            ["playlist"] = playlist,
            ["language"] = language
        };

        return pack;
    }
}
