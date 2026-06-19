# VideoWallPlayer Windows App

Windows icin VLC/libVLC tabanli, kenarliksiz ve kontrolsuz tam ekran video wall oynatici.

## Klasor Yapisi

```text
window_app/
  VideoWallPlayer.sln
  native/                  WinForms + libVLC ana uygulama
  src/                     Eski Electron prototipi
  videos/                  Varsayilan video klasoru
  wiki/                    Windows dokumanlari
```

## Ozellikler

- Coklu video secimi ve klasorden toplu ekleme
- Liste bitince basa donme, tek video tekrari veya liste sonunda durma
- Karisik oynatma
- Tam ekran, penceresiz ve kontrolsuz oynatma
- Video sirasinda imleci gizleme
- `Esc` veya `F11` ile oynatimdan cikma
- `Space` ile duraklat/devam ettir
- Donanim hizlandirma secenekleri
- Windows GPU tercihi
- Coklu ekran secimi
- 20 dil secenegi
- VLC/libVLC codec destegi
- Videolar arasi daha puruzsuz gecis icin cift oynaticili on hazirlama

## Visual Studio ile Acma

Visual Studio'da su dosyayi acin:

```text
window_app\VideoWallPlayer.sln
```

Ana proje:

```text
window_app\native\VideoWallPlayer.Native\VideoWallPlayer.Native.csproj
```

## EXE Uretme

Repo kokunden:

```powershell
dotnet publish window_app\native\VideoWallPlayer.Native\VideoWallPlayer.Native.csproj -c Release -r win-x64 --self-contained true -o window_app\dist-native
```

Cikti:

```text
window_app\dist-native\VideoWallPlayer.exe
```

## Portable ve Kurulum Paketi Uretme

Repo kokunden:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File window_app\scripts\build-windows-packages.ps1
```

Ciktilar:

```text
releases\VideoWallPlayer-Windows-Portable-x64.zip
releases\VideoWallPlayer-Windows-Setup-x64.exe
```

Portable paket klasore acilip dogrudan calistirilir. Setup paketi uygulamayi
`C:\Program Files\VideoWallPlayer` altina kurar, masaustu ve Start Menu kisayolu
olusturur, Windows uygulama kaldirma listesine uninstall kaydi ekler.

## Gelistirme

Derleme:

```powershell
dotnet build window_app\VideoWallPlayer.sln -c Release
```

Calistirma:

```powershell
dotnet run --project window_app\native\VideoWallPlayer.Native\VideoWallPlayer.Native.csproj
```

## Wiki

Windows dokumanlari:

```text
window_app\wiki\Home.md
```
