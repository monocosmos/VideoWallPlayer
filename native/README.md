# VideoWallPlayer Native

Bu klasor Windows native WinForms + VLC/libVLC tabanli VideoWallPlayer uygulamasini icerir.

## Calistirma

```powershell
dotnet run --project native\VideoWallPlayer.Native\VideoWallPlayer.Native.csproj
```

## Derleme

```powershell
dotnet build VideoWallPlayer.sln -c Release
```

## EXE yayinlama

```powershell
dotnet publish native\VideoWallPlayer.Native\VideoWallPlayer.Native.csproj -c Release -r win-x64 --self-contained true -o dist-native
```

Cikti:

```text
dist-native\VideoWallPlayer.exe
```
