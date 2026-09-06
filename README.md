# MiniLocalServer

**MiniLocalServer**, Windows üzerinde PHP ve statik HTML/CSS/JavaScript projelerini hızlıca localhost'ta açmak için hazırlanmış küçük bir C# / .NET 8 WinForms aracıdır.

XAMPP'ın tamamını kurmadan, seçtiğiniz proje klasörünü birkaç tıklamayla yerel sunucuda görüntülemeyi amaçlar.

## Özellikler

- PHP proje klasörü seçme
- HTML / CSS / JavaScript projelerini PHP olmadan çalıştırma
- PHP projelerini `php -S` built-in server ile çalıştırma
- `index.php`, `index.html`, `index.htm` desteği
- Port seçimi
- Başlat / Durdur
- Tarayıcıda otomatik açma
- Uygulama içinde PHP loglarını görüntüleme
- Kullanılan portun boş olup olmadığını kontrol etme
- PHP executable'ını otomatik arama
- `index.php` bulunan projelerde clean URL / front-controller yönlendirmesi
- Windows x64 için self-contained tek EXE publish desteği

## PHP nerede aranıyor?

MiniLocalServer sırasıyla şu konumları kontrol eder:

1. `MiniLocalServer.exe` yanındaki `php\php.exe`
2. `MiniLocalServer.exe` yanındaki `php.exe`
3. `C:\xampp\php\php.exe`
4. `C:\laragon\bin\php\...\php.exe`
5. Windows `PATH` içindeki `php.exe`

İsterseniz uygulamadaki **PHP Seç** düğmesiyle yolu elle de gösterebilirsiniz.

---

# 1. Hazır programı kullanma

Uygulamayı açın:

```text
MiniLocalServer.exe
```

Ardından:

1. **Proje klasörü** alanından sitenizin klasörünü seçin.
2. PHP projesiyse **PHP yolu** alanında geçerli bir `php.exe` bulunduğunu kontrol edin.
3. Port olarak örneğin `8080` bırakın.
4. **Başlat** düğmesine basın.
5. Tarayıcı otomatik olarak açılır.

Adres:

```text
http://127.0.0.1:8080/
```

Sunucuyu kapatmak için **Durdur** düğmesini kullanın.

## Örnek PHP proje klasörü

```text
C:\Projeler\haber-sitesi\
├─ index.php
├─ application\
├─ system\
├─ assets\
├─ uploads\
└─ ...
```

MiniLocalServer'da proje klasörü olarak:

```text
C:\Projeler\haber-sitesi
```

seçilir.

## Örnek HTML proje klasörü

```text
C:\Projeler\kurumsal-site\
├─ index.html
├─ css\
├─ js\
└─ images\
```

Bu tip projelerde PHP bulunmasına gerek yoktur. MiniLocalServer kendi basit statik HTTP sunucusunu kullanır.

---

# 2. Kaynak kodu çalıştırma

## Gereksinimler

Geliştirme için:

- Windows 10/11
- .NET 8 SDK
- İsteğe bağlı Visual Studio 2022

SDK kontrolü:

```powershell
dotnet --version
```

## Visual Studio ile

Repo içindeki:

```text
MiniLocalServer.sln
```

dosyasını Visual Studio ile açın ve **F5** ile çalıştırın.

## Terminalden

Repo ana klasöründe:

```powershell
dotnet run --project .\src\MiniLocalServer\MiniLocalServer.csproj
```

---

# 3. EXE oluşturma

Temel publish komutu:

```powershell
dotnet publish .\src\MiniLocalServer\MiniLocalServer.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

### Komut ne anlama geliyor?

| Parametre | Görevi |
|---|---|
| `dotnet publish` | Dağıtıma hazır çıktı üretir |
| `-c Release` | Release yapılandırmasıyla derler |
| `-r win-x64` | 64-bit Windows hedefler |
| `--self-contained true` | .NET Runtime'ı uygulamayla birlikte paketler |
| `/p:PublishSingleFile=true` | Uygulamayı mümkün olduğunca tek EXE halinde üretir |

Varsayılan çıktı yaklaşık olarak:

```text
src\MiniLocalServer\bin\Release\net8.0-windows\win-x64\publish\MiniLocalServer.exe
```

## Önerilen publish komutu

Çıktıyı doğrudan `artifacts` klasörüne almak için:

```powershell
dotnet publish .\src\MiniLocalServer\MiniLocalServer.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  /p:PublishSingleFile=true `
  /p:DebugType=None `
  /p:DebugSymbols=false `
  -o .\artifacts\win-x64
```

Sonuç:

```text
artifacts\win-x64\MiniLocalServer.exe
```

Daha ayrıntılı açıklama: [`docs/PUBLISH.md`](docs/PUBLISH.md)

---

# 4. Tek tıkla EXE üretme

Repo içinde hazır script bulunmaktadır:

```text
scripts\publish-win-x64.bat
```

Windows Explorer'da bu dosyaya çift tıklayın.

Alternatif olarak PowerShell:

```powershell
.\scripts\publish-win-x64.ps1
```

İşlem tamamlandığında:

```text
artifacts\win-x64\MiniLocalServer.exe
artifacts\MiniLocalServer-win-x64.zip
```

oluşturulur.

---

# 5. Tamamen portable PHP kullanımı

Hedef bilgisayarda XAMPP veya sistem PHP'si olmasını istemiyorsanız Windows PHP paketini MiniLocalServer'ın yanına koyabilirsiniz:

```text
MiniLocalServer/
├─ MiniLocalServer.exe
└─ php/
   ├─ php.exe
   ├─ php.ini
   ├─ ext/
   └─ diğer PHP dosyaları...
```

Program açıldığında `php\php.exe` otomatik bulunur.

> PHP paketini bu GitHub reposuna dahil etmek önerilmez. PHP binary dosyalarını resmi dağıtım kaynağından ayrıca temin edip release paketine ekleyebilirsiniz.

---

# 6. CodeIgniter / clean URL kullanımı

Kök proje klasöründe `index.php` varsa MiniLocalServer PHP server için geçici bir router oluşturur.

Örneğin:

```text
http://127.0.0.1:8080/haber/123
```

isteğinde gerçek bir `haber/123` dosyası yoksa istek `index.php` üzerinden uygulamaya aktarılabilir.

Bu davranış CodeIgniter benzeri front-controller mimarilerinde yararlıdır.

## Önemli `.htaccess` notu

MiniLocalServer **Apache değildir**. PHP built-in server kullanır.

Bu nedenle Apache'ye özel:

```text
.htaccess
mod_rewrite
AllowOverride
```

kuralları doğrudan çalışmaz.

Basit front-controller yönlendirmesi uygulama tarafından telafi edilir; ancak gelişmiş Apache rewrite/header/security kurallarının tamamı birebir uygulanmaz.

---

# 7. MySQL / MariaDB kullanan projeler

Mevcut sürüm MySQL veya MariaDB sunucusunu kendi içinde başlatmaz.

PHP projeniz veritabanı kullanıyorsa ayrıca çalışan bir MySQL/MariaDB servisine ihtiyacınız vardır.

Örneğin CodeIgniter veritabanı ayarında:

```text
hostname: 127.0.0.1
port:     3306
username: ...
password: ...
database: ...
```

bilgileri çalışan yerel veritabanı sunucusuna göre ayarlanmalıdır.

---

# 8. Sık karşılaşılan sorunlar

## PHP bulunamadı

Uygulamada **PHP Seç** düğmesine basıp doğrudan `php.exe` dosyasını gösterin.

Örnek:

```text
C:\xampp\php\php.exe
```

veya portable:

```text
MiniLocalServer\php\php.exe
```

## 8080 portu kullanımda

Başka bir port deneyin:

```text
8081
8090
9000
```

ve tarayıcı adresi seçtiğiniz porta göre değişir:

```text
http://127.0.0.1:8081/
```

## Sayfa açılıyor fakat CSS/JS dosyaları gelmiyor

HTML/PHP kodundaki asset yollarını kontrol edin. Mutlak domain adresleri veya yanlış base URL ayarları localhost'ta sorun çıkarabilir.

## CodeIgniter localhost URL'si yanlış

Projenizin `base_url` veya benzeri ayarı production domainine sabitlenmiş olabilir. Local geliştirme için `http://127.0.0.1:8080/` adresine göre yapılandırın.

## PHP extension hatası

Örneğin uygulama `mysqli`, `pdo_mysql`, `curl`, `mbstring`, `openssl` gibi extension'lara ihtiyaç duyabilir. Portable PHP kullanıyorsanız `php.ini` içindeki extension ayarlarının etkin olduğundan emin olun.

---

# 9. Repo klasör yapısı

```text
MiniLocalServer/
├─ .github/
│  └─ workflows/
│     └─ build.yml
├─ docs/
│  └─ PUBLISH.md
├─ scripts/
│  ├─ publish-win-x64.bat
│  └─ publish-win-x64.ps1
├─ src/
│  └─ MiniLocalServer/
│     ├─ MainForm.cs
│     ├─ Program.cs
│     └─ MiniLocalServer.csproj
├─ .gitignore
├─ MiniLocalServer.sln
└─ README.md
```

---

# 10. GitHub'a yükleme

Yeni boş bir GitHub repository oluşturduktan sonra repo klasöründe:

```powershell
git init
git add .
git commit -m "Initial MiniLocalServer release"
git branch -M main
git remote add origin https://github.com/KULLANICI_ADIN/MiniLocalServer.git
git push -u origin main
```

`KULLANICI_ADIN` kısmını kendi GitHub kullanıcı adınızla değiştirin.

> GitHub'da repository oluştururken README eklemeyin; bu paketin içinde README zaten bulunmaktadır. Eğer eklediyseniz push öncesinde remote geçmişiyle birleştirmeniz gerekebilir.

---

# 11. GitHub Actions

`.github/workflows/build.yml` dosyası hazırdır.

`main` veya `master` branch'ine push yapıldığında GitHub Actions:

1. Kaynak kodu checkout eder.
2. .NET 8 kurar.
3. Restore yapar.
4. Windows x64, self-contained, single-file publish alır.
5. Oluşan dosyaları **MiniLocalServer-win-x64** adıyla workflow artifact olarak yükler.

GitHub'da:

```text
Repository → Actions → Build Windows x64 → ilgili çalışma → Artifacts
```

bölümünden build çıktısını indirebilirsiniz.

---

# Güvenlik / kullanım notu

MiniLocalServer geliştirme ve yerel test amacıyla hazırlanmıştır. Sunucu `127.0.0.1` üzerinde dinlediği için varsayılan kullanım yerel makineye yöneliktir.

Production web sunucusu yerine kullanılmamalıdır.

---

## Mevcut kapsam

- [x] HTML/CSS/JS localhost
- [x] PHP built-in server
- [x] Portable PHP algılama
- [x] XAMPP PHP algılama
- [x] Laragon PHP algılama
- [x] Clean URL / front controller
- [x] Tek EXE publish
- [x] GitHub Actions build
- [ ] Dahili MariaDB yönetimi
- [ ] phpMyAdmin entegrasyonu
- [ ] Proje profilleri
- [ ] Sistem tepsisi modu
- [ ] SSL / local HTTPS
