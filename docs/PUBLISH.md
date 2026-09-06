# EXE Oluşturma / Publish Rehberi

Bu belge `dotnet publish` komutunun ne yaptığını ve MiniLocalServer için nasıl kullanılacağını ayrıntılı biçimde açıklar.

## Kullanılan komut

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

Projeyi repo kökünden çalıştırıyorsanız proje yolunu açıkça vermek daha güvenlidir:

```powershell
dotnet publish .\src\MiniLocalServer\MiniLocalServer.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## Parametrelerin anlamı

### `dotnet publish`

Projeyi son kullanıcıya dağıtılabilecek hale getirir. `dotnet build` yalnızca derleme içindir; `publish` ise çalıştırılabilir yayın çıktısını hazırlar.

### `-c Release`

Release yapılandırmasıyla derler. Günlük geliştirmede kullanılan Debug sürümüne göre dağıtım için daha uygundur.

### `-r win-x64`

Hedef platformu Windows 64-bit olarak belirler.

- `win-x64`: 64-bit Windows
- `win-x86`: 32-bit Windows
- `win-arm64`: ARM64 Windows

MiniLocalServer için önerilen hedef çoğu Windows bilgisayarda `win-x64`'tır.

### `--self-contained true`

.NET çalışma zamanını uygulamayla birlikte paketler. Böylece EXE'yi kullanacak bilgisayarda ayrıca .NET 8 Desktop Runtime kurulu olması gerekmez.

Dezavantajı: çıktı boyutu framework-dependent yayına göre daha büyük olur.

### `/p:PublishSingleFile=true`

.NET dosyalarını mümkün olduğunca tek bir çalıştırılabilir EXE içinde paketler.

> Not: Uygulamaya sonradan eklediğiniz `php` klasörü bu EXE'nin içine otomatik gömülmez. Portable PHP kullanacaksanız `php\` klasörünü yayın klasörüne ayrıca koymanız gerekir.

## Çıktı nerede oluşur?

Varsayılan komutla yaklaşık olarak şu klasörde oluşur:

```text
src\MiniLocalServer\bin\Release\net8.0-windows\win-x64\publish\
```

İçeride ana dosya:

```text
MiniLocalServer.exe
```

Çıktıyı istediğiniz klasöre yönlendirmek için `-o` kullanabilirsiniz:

```powershell
dotnet publish .\src\MiniLocalServer\MiniLocalServer.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  /p:PublishSingleFile=true `
  -o .\artifacts\win-x64
```

Bu durumda:

```text
artifacts\win-x64\MiniLocalServer.exe
```

oluşur.

## En kolay yöntem: hazır script

Repo içinde:

```text
scripts\publish-win-x64.bat
```

bulunur. Dosyaya çift tıklamanız yeterlidir.

PowerShell üzerinden:

```powershell
.\scripts\publish-win-x64.ps1
```

Script şu işlemleri yapar:

1. Eski `artifacts\win-x64` çıktısını temizler.
2. Release / Windows x64 / self-contained / single-file publish alır.
3. EXE'yi `artifacts\win-x64` klasörüne koyar.
4. `artifacts\MiniLocalServer-win-x64.zip` arşivini oluşturur.

## Geliştirme bilgisayarında gerekenler

Kaynak kodu derlemek için:

- Windows 10 veya Windows 11
- .NET 8 SDK
- İsteğe bağlı: Visual Studio 2022

Kontrol:

```powershell
dotnet --version
```

Örnek olarak `8.x.x` benzeri bir sonuç görmelisiniz.

## Son kullanıcı bilgisayarında gerekenler

`--self-contained true` ile oluşturulan EXE için ayrıca .NET Runtime kurulması gerekmez.

PHP projeleri çalıştırılacaksa MiniLocalServer'ın erişebileceği bir `php.exe` gerekir. HTML/CSS/JS projeleri için PHP gerekmez.

## Portable PHP paketi

Tamamen taşınabilir kullanım için yayın klasörü şu şekilde olabilir:

```text
MiniLocalServer/
├─ MiniLocalServer.exe
└─ php/
   ├─ php.exe
   ├─ php.ini
   ├─ php8ts.dll
   ├─ ext/
   └─ ...
```

Uygulama açıldığında önce kendi klasöründeki `php\php.exe` dosyasını arar.

## Sık yapılan hatalar

### `'dotnet' is not recognized...`

.NET SDK kurulu değildir veya PATH'e eklenmemiştir. Terminali kapatıp yeniden açmayı da deneyin.

### `NETSDK...` hataları

Projede hedeflenen SDK sürümü bilgisayarda yoktur. `dotnet --list-sdks` ile yüklü SDK'ları kontrol edin.

### EXE açılıyor ama PHP projesi çalışmıyor

Uygulama ekranındaki **PHP yolu** alanını kontrol edin. `php.exe` bulunmalı.

### Port kullanımda

8080 başka bir uygulama tarafından kullanılıyorsa 8081, 8090 veya başka boş bir port seçin.

### `.htaccess` çalışmıyor

Bu araç Apache kullanmaz. PHP'nin built-in server'ını kullanır. Kök klasörde `index.php` varsa MiniLocalServer front-controller yönlendirmesi uygular; ancak Apache'ye özel `.htaccess` direktiflerinin tamamı desteklenmez.
