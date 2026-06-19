# VideoWallPlayer Native

Bu klasor Windows native WinForms + VLC/libVLC tabanli VideoWallPlayer uygulamasini icerir.

## Calistirma

```powershell
dotnet run --project window_app\native\VideoWallPlayer.Native\VideoWallPlayer.Native.csproj
```

## Derleme

```powershell
dotnet build window_app\VideoWallPlayer.sln -c Release
```

## EXE yayinlama

```powershell
dotnet publish window_app\native\VideoWallPlayer.Native\VideoWallPlayer.Native.csproj -c Release -r win-x64 --self-contained true -o window_app\dist-native
```

Cikti:

```text
window_app\dist-native\VideoWallPlayer.exe
```
