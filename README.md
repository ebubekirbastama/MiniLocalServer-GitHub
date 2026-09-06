# Mini Local Server

![C#](https://img.shields.io/badge/C%23-12%2B-239120?style=for-the-badge&logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Windows Forms](https://img.shields.io/badge/UI-Windows%20Forms-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![PHP](https://img.shields.io/badge/PHP-Built--in%20Server-777BB4?style=for-the-badge&logo=php&logoColor=white)
![License](https://img.shields.io/badge/License-Apache%202.0-blue?style=for-the-badge)

> Windows üzerinde PHP ve HTML projelerini hızlıca yerel olarak çalıştırmak için geliştirilmiş, C#/.NET 8 tabanlı hafif bir local server yöneticisi.

## Proje Hakkında

**MiniLocalServer**, PHP'nin built-in web server'ını Windows Forms arayüzünden yönetmeyi kolaylaştırır. PHP içeren projelerde `php -S` çalıştırılır; yalnızca statik HTML içeren projelerde ise uygulamanın kendi C# HTTP sunucusu kullanılır.

Kaynak koduna göre uygulama; proje klasörü seçimi, `php.exe` seçimi/otomatik bulunması, 1024–65535 arası port seçimi, port kontrolü, PHP process yönetimi, canlı loglar, browser açma ve temiz kapatma işlemlerini destekler. `index.php` bulunan projelerde geçici router oluşturularak front-controller kullanımına da destek verir. fileciteturn262file0

## Özellikler

### PHP Built-in Server

PHP bulunduğunda uygulama şu yapıda server başlatır:

```text
php -S 127.0.0.1:<PORT> -t <PROJECT_FOLDER> <ROUTER>
```

PHP process'i ayrı bir child process olarak çalıştırılır; stdout ve stderr çıktıları uygulamanın log alanına aktarılır. fileciteturn262file0

### `index.php` Router

Proje kökünde `index.php` bulunduğunda uygulama geçici bir PHP router oluşturur. Gerçek dosya ve dizinler korunurken uygun olmayan yollar `index.php` front-controller'ına yönlendirilir. Router dosyası geçici dizinde oluşturulur.

### PHP Olmadan Statik HTML

PHP bulunmayan ve `.php` dosyası içermeyen projelerde C# tabanlı statik HTTP server devreye girer.

Bu sunucu kaynak kodunda `TcpListener`, `TcpClient` ve `NetworkStream` kullanır. `GET` ve `HEAD` istekleri, `index.html` / `index.htm`, basit directory listing, MIME type belirleme ve temel HTTP hata yanıtları desteklenir. Ayrıca dosya yolunun server root dışına taşmasını engelleyen kontrol bulunur. fileciteturn262file0

### Windows Forms Arayüzü

Arayüzde:

- Proje klasörü seçimi
- `php.exe` seçimi
- Port seçimi
- Başlat
- Durdur
- Tarayıcıda Aç
- Canlı log alanı
- Durum göstergesi

bulunur. Varsayılan port `8080`, izin verilen port aralığı `1024–65535`'tir. fileciteturn262file0

## Teknoloji Kartları

| Teknoloji | Kullanım |
|---|---|
| **C# 12+** | Ana programlama dili |
| **.NET 8** | Uygulama platformu |
| **Windows Forms** | Masaüstü arayüzü |
| **PHP Built-in Server** | PHP projelerinin çalıştırılması |
| **TcpListener** | PHP'siz statik HTTP server |
| **TcpClient / NetworkStream** | HTTP bağlantıları |
| **HTTP** | Statik içerik servis protokolü |

## Proje Yapısı

```text
MiniLocalServer-GitHub/
├── MiniLocalServer.sln
├── src/
│   └── MiniLocalServer/
│       ├── MainForm.cs
│       ├── MiniLocalServer.csproj
│       └── Program.cs
├── docs/
├── scripts/
├── .gitignore
├── LICENSE
└── README.md
```

Repository'de ayrıca `docs` ve `scripts` klasörleri bulunmaktadır. fileciteturn261file0

## Gereksinimler

### Geliştirme ortamı

- Windows
- .NET 8 SDK
- Visual Studio 2022 veya .NET 8 destekleyen başka bir IDE

### PHP projeleri için

- Uyumlu bir `php.exe`
- PHP'nin PATH üzerinde olması veya uygulama içerisinden `php.exe` seçilmesi

PHP bulunmadığında statik HTML projeleri yine C# server ile çalıştırılabilir.

## Kurulum

```bash
git clone https://github.com/ebubekirbastama/MiniLocalServer-GitHub.git
cd MiniLocalServer-GitHub
dotnet build MiniLocalServer.sln
dotnet run --project src/MiniLocalServer/MiniLocalServer.csproj
```

## Kullanım

1. Proje klasörünü seçin.
2. PHP projesi kullanıyorsanız `php.exe` yolunu kontrol edin.
3. Portu belirleyin. Varsayılan değer `8080`'dir.
4. `Başlat` düğmesine basın.
5. Uygulama uygun server modunu seçer.
6. Server başladıktan sonra `Tarayıcıda Aç` ile localhost adresini açabilirsiniz.
7. İşiniz bittiğinde `Durdur` düğmesini kullanın.

### Server seçim mantığı

```text
                 Proje Klasörü
                       |
                       v
               PHP dosyası var mı?
                  /          \
                EVET          HAYIR
                 |              |
                 v              v
          php.exe mevcut mu?  C# Static Server
             /       \
           EVET       HAYIR
            |            |
            v            v
      PHP Built-in    PHP gerekli
         Server        uyarısı
```

Kaynak kodunda PHP dosyaları bulunup `php.exe` bulunamazsa kullanıcıya açık uyarı verilir; PHP dosyası olmayan projelerde statik server'a geçilir. fileciteturn262file0

## Yerel Sunucu Adresi

Server loopback adresine bağlanır:

```text
http://127.0.0.1:<port>/
```

PHP server `127.0.0.1` üzerinde başlatılır. Statik server da `IPAddress.Loopback` kullanır. Bu nedenle mevcut tasarımın amacı servisi internete veya LAN'a açmak değil, yerel geliştirme ortamı sağlamaktır. fileciteturn262file0

## Güvenlik

Bu proje bir **development/local server tool** olarak tasarlanmıştır; Apache, Nginx veya IIS gibi üretim web sunucularının yerine geçmesi amaçlanmamıştır.

Dikkat edilmesi gerekenler:

- Güvenilmeyen PHP kodlarını çalıştırmayın.
- Hassas dosyaların bulunduğu klasörleri server root olarak seçmeyin.
- Güvenilir bir `php.exe` kullanın.
- Uygulamayı LAN/internet üzerinde yayınlamak için kodu değiştirmeden önce erişim kontrolü ve authentication ekleyin.
- Statik server'daki root/path kontrolünün üretim güvenliği olarak değerlendirilmemesi gerekir.

## Teknik Durum

Proje güncel bir geliştirme aracı olmakla birlikte mimarisi bilinçli olarak küçük tutulmuştur.

### Güçlü taraflar

- PHP server yönetimini basitleştirmesi
- PHP yokken HTML fallback'i
- Port kullanım kontrolü
- Canlı PHP/server logları
- Browser entegrasyonu
- Server kapanırken temizleme
- Statik server'da temel path traversal koruması

### Geliştirme önerileri

- Proje profillerini kaydetme
- Son kullanılan klasör ve portu hatırlama
- PHP sürüm tespiti
- Birden fazla PHP sürümünü yönetme
- Otomatik port seçimi
- Daha gelişmiş HTTP access/error logları
- HTTPS development certificate desteği
- Sistem tray desteği
- Unit ve integration testleri
- Server health check ve otomatik restart

## Lisans

Repository'deki `LICENSE` dosyasına göre proje **Apache License 2.0** lisansına sahiptir. fileciteturn261file0

## Geliştirici

**Ebubekir Bastama**  
GitHub: https://github.com/ebubekirbastama

---

Proje yerel PHP/HTML geliştirme iş akışınızı kolaylaştırıyorsa repository'ye yıldız bırakabilirsiniz.
