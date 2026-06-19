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

Bu makinede Android Studio JBR ve Android SDK kullaniliyor:

```powershell
$env:JAVA_HOME='C:\Program Files\Android\Android Studio\jbr'
$env:ANDROID_HOME="$env:LOCALAPPDATA\Android\Sdk"
$env:ANDROID_SDK_ROOT=$env:ANDROID_HOME
& "$env:USERPROFILE\.gradle\wrapper\dists\gradle-9.4.1-bin\arn2x92ynaizyzdaamcbpbhtj\gradle-9.4.1\bin\gradle.bat" :app:assembleDebug
```

Cikti:

```text
app\build\outputs\apk\debug\app-debug.apk
```

Repo kokune kopyalanmis hazir cikti:

```text
dist-android\VideoWallPlayer-android-debug.apk
```
