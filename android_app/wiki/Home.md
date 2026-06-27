# VideoWallPlayer Android Wiki

## Genel Bakis

Android uygulamasi VLC/libVLC tabanlidir. Amaci Android tabanli medya oynatici, TV box veya signage cihazlarinda videolari tam ekran ve kontrolsuz oynatmaktir.

## Ana Is Akisi

1. Android Studio'da `android_app` klasoru acilir.
2. APK Gradle wrapper ile uretilir ve cihaza yuklenir.
3. Uygulamada `Video Ekle` ile sistem dosya secicisinden videolar secilir.
4. Tekrar modu, karisik oynatma, sessiz mod, donanim hizlandirma ve onbellek ayarlanir.
5. `Videoyu Baslat` ile tam ekran oynatma baslar.
6. Oynatma sirasinda:
   - `Space`: duraklat/devam ettir
   - `Back`: arayuze don
   - `Esc`: arayuze don
   - `F11`: arayuze don

## Build

```powershell
Push-Location android_app
$env:JAVA_HOME='C:\Program Files\Android\Android Studio\jbr'
$env:ANDROID_HOME="$env:LOCALAPPDATA\Android\Sdk"
$env:ANDROID_SDK_ROOT=$env:ANDROID_HOME
.\gradlew.bat :app:assembleDebug
Pop-Location
```

## Cikti

```text
android_app\app\build\outputs\apk\debug\app-debug.apk
android_app\dist-android\VideoWallPlayer-android-debug.apk
```

## Notlar

- Dosya secimi Android sistem dosya secicisiyle yapilir.
- Secilen videolar kalici URI izniyle saklanir.
- VLC/libVLC Android native kutuphaneleri APK icine dahil edilir.
- APK boyutu buyuktur; bu nedenle APK GitHub'a commitlenmez.
