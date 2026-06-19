# VideoWallPlayer Windows Wiki

## Genel Bakis

Windows uygulamasi WinForms + VLC/libVLC tabanlidir. Amaci video wall ekranlarda videolari tam ekran, penceresiz, kontrolsuz ve metinsiz oynatmaktir.

## Ana Is Akisi

1. `window_app\VideoWallPlayer.sln` Visual Studio ile acilir.
2. Arayuzden videolar veya klasor eklenir.
3. Tekrar, karisik oynatma, hedef ekran, donanim hizlandirma ve GPU tercihi ayarlanir.
4. `Videoyu Baslat` ile tam ekran oynatma baslar.
5. Oynatma sirasinda:
   - `Space`: duraklat/devam ettir
   - `Esc`: arayuze don
   - `F11`: arayuze don

## Build

```powershell
dotnet build window_app\VideoWallPlayer.sln -c Release
```

## Publish

```powershell
dotnet publish window_app\native\VideoWallPlayer.Native\VideoWallPlayer.Native.csproj -c Release -r win-x64 --self-contained true -o window_app\dist-native
```

## Cikti

```text
window_app\dist-native\VideoWallPlayer.exe
```

## Notlar

- Video oynarken imlec gizlenir.
- Pencere kenari veya kontrol cubugu gosterilmez.
- VLC/libVLC codec destegi kullanilir.
- Cift oynaticili gecis, siradaki videoyu onceden hazirlayarak gecis boslugunu azaltir.
