# VideoWallPlayer Android

Android icin VLC/libVLC tabanli tam ekran video wall oynatici.

## Ozellikler

- Coklu video secimi
- Playlist saklama
- Liste bitince basa donme, tek video tekrari veya liste sonunda durma
- Karisik oynatma
- Sessiz oynatma
- Donanim hizlandirma ac/kapat
- VLC codec destegi
- Tam ekran, sistem barlarini gizleyen video modu
- Video ekraninda `Space` ile duraklat/devam ettir
- `Back`, `Esc` veya `F11` ile oynatimdan arayuze donme

## APK uretme

Android Studio JBR ve Android SDK kurulu olmalidir. Gradle wrapper repo icinde
geldigi icin makineye ozel Gradle yolu gerekmez.

```powershell
Push-Location android_app
$env:JAVA_HOME='C:\Program Files\Android\Android Studio\jbr'
$env:ANDROID_HOME="$env:LOCALAPPDATA\Android\Sdk"
$env:ANDROID_SDK_ROOT=$env:ANDROID_HOME
.\gradlew.bat :app:assembleDebug
Pop-Location
```

Cikti:

```text
app\build\outputs\apk\debug\app-debug.apk
```

Hazir dagitim klasorune kopyalama gerekiyorsa:

```text
dist-android\VideoWallPlayer-android-debug.apk
```

## Android Studio ile Acma

Android Studio'da su klasoru acin:

```text
android_app
```

## Wiki

Android dokumanlari:

```text
android_app\wiki\Home.md
```
