# VideoWallPlayer

Windows video wall ekranlari icin kenarliksiz, yazisiz, kontrol cubuksuz tam ekran video oynatici.

Ana uygulama `native` klasorundeki Windows native VLC/libVLC tabanli surumdur. `src` klasoru eski Electron prototipini icerir.
Android APK projesi `android/VideoWallPlayer.Android` klasorundedir.

## Ozellikler

- Coklu video secimi ve klasorden toplu ekleme
- Liste bitince basa donme, tek video tekrari veya liste sonunda durma
- Karisik oynatma
- Tam ekran, penceresiz ve kontrolsuz oynatma
- Video sirasinda imleci gizleme
- `Esc` veya `F11` ile oynatimdan cikma
- Donanim hizlandirma secenekleri
- Windows GPU tercihi
- Coklu ekran secimi
- 20 dil secenegi
- VLC/libVLC codec destegi
- Videolar arasi daha puruzsuz gecis icin cift oynaticili on hazirlama

## Android APK

Android uygulamasi da VLC/libVLC tabanlidir. Sistem dosya secicisiyle coklu video secimi yapar, playlisti saklar, tam ekran oynatir, liste bitince basa donebilir, karisik oynatabilir ve video ekraninda `Space` tusuyla duraklat/devam ettir yapar.

Hazir debug APK:

```text
dist-android\VideoWallPlayer-android-debug.apk
```

Android Studio ile acilacak proje:

```text
android\VideoWallPlayer.Android
```

APK yeniden uretmek icin:

```powershell
$env:JAVA_HOME='C:\Program Files\Android\Android Studio\jbr'
$env:ANDROID_HOME="$env:LOCALAPPDATA\Android\Sdk"
$env:ANDROID_SDK_ROOT=$env:ANDROID_HOME
& "$env:USERPROFILE\.gradle\wrapper\dists\gradle-9.4.1-bin\arn2x92ynaizyzdaamcbpbhtj\gradle-9.4.1\bin\gradle.bat" :app:assembleDebug
```

APK ciktisi:

```text
android\VideoWallPlayer.Android\app\build\outputs\apk\debug\app-debug.apk
```

## Visual Studio ile acma

Visual Studio'da su dosyayi acin:

```text
VideoWallPlayer.sln
```

Ana proje:

```text
native\VideoWallPlayer.Native\VideoWallPlayer.Native.csproj
```

## Native Windows EXE

Hazir calistirilacak dosya:

```text
dist-native\VideoWallPlayer.exe
```

Exe ciktisini yeniden almak:

```powershell
dotnet publish native\VideoWallPlayer.Native\VideoWallPlayer.Native.csproj -c Release -r win-x64 --self-contained true -o dist-native
```

Sahada kullanirken videolari exe yanindaki `videos` klasorune koyabilirsiniz veya uygulama arayuzunden playlist olusturabilirsiniz.

## Gelistirme

Derleme:

```powershell
dotnet build VideoWallPlayer.sln -c Release
```

Calistirma:

```powershell
dotnet run --project native\VideoWallPlayer.Native\VideoWallPlayer.Native.csproj
```

## Eski Electron prototipi

Electron prototipini calistirmak icin:

```powershell
npm install
npm start
```

Electron portable cikti almak icin:

```powershell
npm.cmd run build:exe
```
