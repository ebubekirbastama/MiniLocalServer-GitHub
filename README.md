# Mini Local Server

![C#](https://img.shields.io/badge/C%23-12%2B-239120?style=for-the-badge&logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Windows Forms](https://img.shields.io/badge/UI-Windows%20Forms-0078D4?style=for-the-badge&logo=windows&logoColor=white)
![PHP](https://img.shields.io/badge/PHP-Built--in%20Server-777BB4?style=for-the-badge&logo=php&logoColor=white)
![License](https://img.shields.io/badge/License-Apache%202.0-blue?style=for-the-badge)

> Windows üzerinde PHP ve HTML projelerini hızlıca yerel olarak çalıştırmak için geliştirilmiş, C#/.NET 8 tabanlı hafif bir masaüstü local server yöneticisi.

## 📌 Proje Hakkında

**MiniLocalServer**, klasik `php -S` komutunu masaüstü arayüzü üzerinden yönetmeyi kolaylaştırır. PHP bulunan projelerde PHP'nin kendi built-in web server'ını çalıştırır; PHP bulunmayan ve yalnızca statik HTML içeren projelerde ise uygulamanın kendi hafif C# HTTP sunucusunu devreye alır.

Uygulama kaynak koduna göre:

- Proje klasörü seçilebilir.
- `php.exe` yolu otomatik aranabilir veya elle seçilebilir.
- `1024–65535` arasında port seçilebilir; varsayılan port **8080**'dir.
- Portun kullanımda olup olmadığı kontrol edilir.
- PHP dosyaları varsa PHP built-in server kullanılır.
- `index.php` varsa temiz URL/front-controller yönlendirmesi için geçici PHP router oluşturulur.
- PHP yoksa statik HTML için C# `TcpListener` tabanlı sunucu kullanılır.
- Sunucu logları arayüzde gösterilir.
- Çalışan proje tek tuşla varsayılan tarayıcıda açılabilir.
- Durdurma ve uygulama kapanırken temizleme işlemleri desteklenir. fileciteturn265file0

## ✨ Özellikler

### 🐘 PHP Local Server

PHP executable bulunduğunda uygulama aşağıdaki mantıkla PHP'nin built-in server'ını çalıştırır:

```text
php -S 127.0.0.1:<PORT> -t <PROJECT_FOLDER> <ROUTER>
```

PHP process'i ayrı bir child process olarak başlatılır; stdout/stderr çıktıları GUI log alanına aktarılır. fileciteturn265file0

### 🧭 `index.php` Router

Proje klasöründe `index.php` bulunduğunda uygulama geçici bir PHP router dosyası üretir. Bu router mevcut gerçek dosya/dizinleri korurken uygun olmayan yolları `index.php`'ye yönlendirmek için kullanılır.

Bu sayede basit PHP front-controller projeleri için daha temiz URL davranışı sağlanır. fileciteturn265file0

### 🌐 PHP'siz HTML Server

`php.exe` bulunmadığında ve projede `.php` dosyası yoksa uygulama kendi C# statik HTTP sunucusunu başlatır.

Bu sunucu:

- `GET` destekler.
- `HEAD` destekler.
- `index.html` / `index.htm` arar.
- Dizin için basit directory listing oluşturabilir.
- MIME type belirler.
- `404`, `403`, `405` ve `500` yanıtları üretir.
- Path traversal denemelerine karşı root klasör sınırını kontrol eder.

Kaynak kodunda bu sunucu `TcpListener`, `TcpClient` ve `NetworkStream` kullanılarak uygulanmıştır. fileciteturn265file0

### 🖥️ Masaüstü Arayüzü

Arayüzde doğrudan şu kontroller bulunur:

- Proje klasörü seçimi
- PHP executable seçimi
- Port seçimi
- Başlat
- Durdur
- Tarayıcıda Aç
- Canlı log alanı
- Durum göstergesi

## 🧰 Teknoloji Kartları

| Teknoloji | Kullanım |
|---|---|
| 🟢 **C#** | Ana programlama dili |
| 🟣 **.NET 8** | Uygulama platformu |
| 🪟 **Windows Forms** | Masaüstü kullanıcı arayüzü |
| 🐘 **PHP** | PHP projeleri için built-in server |
| 🌐 **TcpListener** | PHP'siz statik HTTP server |
| 🔌 **TcpClient / NetworkStream** | HTTP istemci bağlantıları |
| 📄 **HTTP** | Statik içerik servis protokolü |

Proje `.NET 8` ve `net8.0-windows` hedeflemekte, Windows Forms kullanmaktadır. fileciteturn266file0

## 📁 Proje Yapısı

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

Repository'de ayrıca `docs` ve `scripts` klasörleri bulunmaktadır. fileciteturn262file0

## 🚀 Gereksinimler

### Geliştirme

- Windows
- Visual Studio 2022 veya .NET 8 SDK destekleyen IDE
- .NET 8 SDK

### PHP projeleri için

- PHP 8.x gibi uyumlu bir `php.exe`
- PHP executable'ın sistem PATH'inde bulunması veya uygulama içerisinden seçilmesi

PHP bulunmuyorsa **statik HTML projeleri** yine çalıştırılabilir.

## 📥 Kurulum

Repository'yi klonlayın:

```bash
git clone https://github.com/ebubekirbastama/MiniLocalServer-GitHub.git
cd MiniLocalServer-GitHub
```

Solution'ı açın:

```bash
dotnet build MiniLocalServer.sln
```

Ardından uygulamayı çalıştırın:

```bash
dotnet run --project src/MiniLocalServer/MiniLocalServer.csproj
```

## ▶️ Kullanım

1. **Proje klasörü** alanından PHP/HTML projenizi seçin.
2. PHP kullanıyorsanız `php.exe` yolunu kontrol edin.
3. Portu belirleyin. Varsayılan: `8080`.
4. **Başlat** düğmesine basın.
5. Uygulama uygun server modunu otomatik seçer.
6. Sunucu başladığında **Tarayıcıda Aç** ile `127.0.0.1` adresini açabilirsiniz.
7. İşiniz bittiğinde **Durdur** düğmesine basın.

### Mod seçimi

```text
              Proje Klasörü
                    │
                    ▼
             PHP dosyası var mı?
                /          \
              EVET          HAYIR
               │              │
               ▼              ▼
        php.exe mevcut mu?   C# Static Server
          /        \
        EVET        HAYIR
         │            │
         ▼            ▼
   PHP Built-in    PHP gerekiyorsa
      Server        kullanıcıya uyarı
```

Kaynak kodu PHP dosyaları mevcutken `php.exe` bulunamazsa kullanıcıya açık bir uyarı gösterir; PHP dosyası olmayan projelerde ise statik server'a geçer. fileciteturn265file0

## 🔐 Güvenlik

Bu proje öncelikle **localhost geliştirme ortamı** için tasarlanmıştır.

PHP server `127.0.0.1` üzerinde başlatılır. Statik server da `TcpListener(IPAddress.Loopback, port)` kullanır; bu nedenle amaç ağdaki diğer cihazlara servis açmak değil, yerel geliştirme sunucusu sağlamaktır. fileciteturn265file0

Yine de:

- Üretim web sunucusu olarak kullanmayın.
- Güvenilmeyen PHP kodlarını çalıştırmayın.
- Hassas dosyaların bulunduğu klasörleri server root olarak seçmeyin.
- Özellikle PHP executable'ını güvenilir bir kaynaktan kullanın.
- Portu internete/yerel ağa açacak şekilde kodu değiştirmeden önce erişim kontrolünü değerlendirin.

## 🧪 Proje Durumu

Bu proje **geliştirme/local development tool** niteliğindedir. Amaç Apache/Nginx/IIS gibi üretim web sunucularının yerini almak değil, küçük PHP/HTML projelerini hızlı şekilde yerel ortamda çalıştırmaktır.

### Mevcut mimarinin güçlü tarafları

- Harici PHP server yapılandırmasını kolaylaştırması
- PHP yokken statik HTML için fallback sağlaması
- Port kontrolü
- GUI logları
- Browser açma
- Uygulama kapanırken server temizliği
- Statik server'da temel path traversal kontrolü

### Geliştirme önerileri

- Proje profillerini kaydetme
- Son kullanılan klasör ve portu hatırlama
- PHP sürümü tespiti
- PHP sürüm yönetimi
- HTTPS/development certificate desteği
- MIME type tablosunu genişletme
- Daha gelişmiş access/error log sistemi
- Otomatik port seçimi
- Sistem tray desteği
- Dark mode
- Unit/integration testleri
- Sağlık kontrolü ve server restart özelliği

## 📄 Lisans

Bu repository **Apache License 2.0** lisansını kullanmaktadır. fileciteturn261file0

## 👤 Geliştirici

**Ebubekir Bastama**  
GitHub: [@ebubekirbastama](https://github.com/ebubekirbastama)

---

⭐ Projeyi faydalı bulduysanız repository'ye yıldız bırakabilirsiniz.
