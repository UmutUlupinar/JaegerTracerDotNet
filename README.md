# Jaeger Tracer .NET 10 API Projesi

Bu proje, .NET 10 ile geliştirilmiş iki API projesinden oluşmaktadır. API'ler arasındaki iletişim ve tüm işlemler Jaeger Tracer ile izlenmektedir.

## Proje Yapısı

- **QuestionApi**: Soru yönetimi için API (Port: 5000)
- **AnswerApi**: Cevap yönetimi için API (Port: 5001)
- **Shared**: Ortak sınıflar ve yapılar

## Özellikler

- ✅ .NET 10 framework
- ✅ Jaeger Tracer entegrasyonu (HTTP endpoint: 14268)
- ✅ Merkezi HTTP client yapısı
- ✅ Service katmanı (Controller'da logic yok)
- ✅ Client sınıfları (API'ler arası iletişim için)
- ✅ Ortak hata yapısı (ApiException)
- ✅ Swagger desteği
- ✅ OpenTelemetry ile distributed tracing

## Gereksinimler

- .NET 10 SDK
- Docker (Jaeger container için)

## Kurulum

### 1. Jaeger Container'ı Başlatma

```bash
docker run -d --name jaeger \
  -e COLLECTOR_ZIPKIN_HTTP_PORT=9411 \
  -p 6831:6831/udp \
  -p 6832:6832/udp \
  -p 5778:5778 \
  -p 16686:16686 \
  -p 14268:14268 \
  -p 9411:9411 \
  jaegertracing/all-in-one:latest
```

### 2. Projeyi Çalıştırma

Her iki API'yi de IDE üzerinden ayrı ayrı çalıştırabilirsiniz:

- **QuestionApi**: `http://localhost:5000/swagger`
- **AnswerApi**: `http://localhost:5001/swagger`

### 3. Jaeger UI'ya Erişim

Jaeger UI'ya şu adresten erişebilirsiniz:
- `http://localhost:16686`

## API Endpoints

### QuestionApi

- `POST /api/question` - Yeni soru oluştur
- `GET /api/question/{id}` - Soru getir
- `POST /api/question/process-with-answer` - Soru oluştur ve cevap al

### AnswerApi

- `POST /api/answer` - Yeni cevap oluştur
- `GET /api/answer/{id}` - Cevap getir
- `GET /api/answer/question/{questionId}` - Soruya göre cevap getir

## Mimari

### Katman Yapısı

1. **Controllers**: Sadece HTTP isteklerini alır ve service'e yönlendirir
2. **Services**: İş mantığı burada bulunur
3. **Clients**: Diğer API'lere istek atmak için kullanılır
4. **Shared**: Ortak yapılar (HttpClientService, ApiException, vb.)

### Tracing Yapısı

- Her HTTP isteği otomatik olarak trace edilir
- Service metodları ActivitySource ile trace edilir
- API'ler arası iletişimde trace context korunur
- Hatalar tracer'da görüntülenir

## Örnek Kullanım

1. QuestionApi'ye `POST /api/question/process-with-answer` endpoint'ine istek atın
2. Bu istek QuestionService'i çağırır
3. QuestionService, AnswerApi'ye istek atar (AnswerApiClient üzerinden)
4. Tüm bu adımlar Jaeger'da görüntülenir

## Yapılandırma

`appsettings.json` dosyalarında Jaeger endpoint'i ve API base URL'leri yapılandırılabilir:

```json
{
  "Jaeger": {
    "Endpoint": "http://localhost:14268/api/traces"
  },
  "AnswerApi": {
    "BaseUrl": "http://localhost:5001"
  }
}
```

## Notlar

- Proje Docker Compose'a ihtiyaç duymaz, direkt IDE'den çalıştırılabilir
- Authorization/Token yapısı yoktur
- Database kullanılmamaktadır (örnek veriler simüle edilir)

## Kullanılan Yapay Zekâ Modelleri

Cursor IDE içinde Auto (agent router) desteğinden yararlanılarak proje adımları planlandı ve kod üretildi

