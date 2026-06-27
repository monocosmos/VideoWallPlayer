# VideoWallPlayer

VideoWallPlayer, video wall ekranlari icin Windows ve Android uygulamalarindan olusan VLC/libVLC tabanli bir oynatici paketidir.

Repo iki ana uygulama klasorune ayrilmistir:

| Klasor | Platform | Aciklama |
| --- | --- | --- |
| [`window_app`](window_app/README.md) | Windows | WinForms + VLC/libVLC tabanli masaustu EXE uygulamasi |
| [`android_app`](android_app/README.md) | Android | Android Activity + VLC/libVLC tabanli APK uygulamasi |

## Dokumanlar

- Windows README: [`window_app/README.md`](window_app/README.md)
- Windows wiki: [`window_app/wiki/Home.md`](window_app/wiki/Home.md)
- Android README: [`android_app/README.md`](android_app/README.md)
- Android wiki: [`android_app/wiki/Home.md`](android_app/wiki/Home.md)

## Amac

- Kenarliksiz, kontrol cubuksuz ve metinsiz tam ekran video oynatma
- Playlist, loop, karisik oynatma ve sessiz mod
- Donanim hizlandirma destekli VLC/libVLC oynatma
- Video wall, kiosk, otel, fuar, showroom ve dijital signage senaryolari

## GitHub

Public repo:

<https://github.com/monocosmos/VideoWallPlayer>

## Visual Studio ile Calisma

Repo kokundeki solution dosyasini acin:

```text
VideoWallPlayer.sln
```

Bu solution Windows uygulamasini ve grafik arayuzlu Windows kurulum projesini
icerir. Android kaynaklari da solution icinde duzenlenebilir dosyalar olarak
listelenir; APK uretimi icin `android_app` klasoru Android Studio veya Gradle
ile kullanilir.

Windows setup projesi Visual Studio icinden build/publish edildiginde gerekli
portable payload'u kendisi uretir ve setup EXE icine gomuler. Bu nedenle repo
icinde hazir EXE/APK/ZIP ciktisi tutulmaz; ciktiklar `releases`, `dist-*` ve
ilgili build klasorlerinde lokal olarak uretilir.
