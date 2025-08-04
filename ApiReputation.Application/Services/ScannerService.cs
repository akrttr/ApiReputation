using ApiReputation.Application.Interfaces;
using ApiReputation.Application.Repositories;
using ApiReputation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ApiReputation.Application.Services
{
    public class ScannerService : IScannerService
    {
        private readonly IUnitofWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;

        public ScannerService(IUnitofWork unitOfWork, IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ScanResult> PerformScanAsync(int apiInfoId)
        {
            var apiInfo = await _unitOfWork.ApiInfoRepository.GetByIdAsync(apiInfoId);
            if (apiInfo == null)

            {
                throw new KeyNotFoundException($"Api with ID {apiInfoId} not found!");
            }

            var result = new ScanResult
            {
                ApiInfoId = apiInfoId,
                ScanDate = DateTime.UtcNow,
            };

            var client = _httpClientFactory.CreateClient();
            var stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();
                var response = await client.GetAsync(apiInfo.BaseUrl);
                stopwatch.Stop();

                result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;
                result.IsSuccessStatusCode = response.IsSuccessStatusCode;
                result.HttpStatusCode = (int)response.StatusCode;


                result.HasHstsHeader = response.Headers.Contains("Strict-Transport-Security");
                result.HasXFrameOptionsHeader = response.Headers.Contains("X-Frame-Options");
                result.HasXContentTypeOptionsHeader = response.Headers.Contains("X-Content-Type-Options");

                string serverLeak = "";
                if (response.Headers.TryGetValues("Server", out var serverValues))
                {
                    serverLeak += $"Server: {string.Join(", ", serverValues)}. ";
                }
                if (response.Headers.TryGetValues("X-Powered-By", out var poweredByValues))
                {
                    serverLeak += $"X-Powered-By: {string.Join(", ", poweredByValues)}";
                }
                result.ServerInfoLeakDetails = string.IsNullOrEmpty(serverLeak) ? "No obvious leaks found." : serverLeak;
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
                    {
                        if (cert != null)
                        {
                            result.IsSslValid = errors == SslPolicyErrors.None;
                            result.SslIssuer = cert.Issuer;
                            result.SslExpiryDate = cert.NotAfter; // NotAfter, ExpirationDate'in karşılığıdır.
                        }
                        else
                        {
                            result.IsSslValid = false;
                        }
                        return true;
                    }
                };
                using var sslClient = new HttpClient(handler);
                await sslClient.GetAsync(apiInfo.BaseUrl);


                // === GENEL SONUÇ DEĞERLENDİRMESİ (BASİT) ===
                if (!result.IsSuccessStatusCode)
                {
                    // Eğer istek başarılı değilse (4xx, 5xx hatası varsa), güvenlik kontrollerinin bir önemi kalmaz.
                    // Öncelikli olarak erişim sorununu bildir.
                    if (result.HttpStatusCode >= 500)
                    {
                        result.OverallResult = $"Sunucu Hatası ({result.HttpStatusCode})";
                    }
                    else if (result.HttpStatusCode >= 400)
                    {
                        result.OverallResult = $"Erişim Sorunu ({result.HttpStatusCode})";
                    }
                    else
                    {
                        // Diğer başarısız durum kodları için genel bir mesaj (örn: 3xx yönlendirmeleri)
                        result.OverallResult = $"Başarısız İstek ({result.HttpStatusCode})";
                    }
                }
                else if (result.IsSslValid == false)
                {
                    // İstek başarılı ama SSL geçersiz. Bu en yüksek risktir.
                    result.OverallResult = "Yüksek Risk (SSL Geçersiz)";
                }
                else if (result.HasHstsHeader == false || result.HasXFrameOptionsHeader == false || result.HasXContentTypeOptionsHeader == false)
                {
                    // SSL geçerli ama temel güvenlik başlıklarından en az biri eksik.
                    result.OverallResult = "Orta Risk (Başlık Eksik)";
                }
                else
                {
                    // Hem istek başarılı, hem SSL geçerli, hem de temel başlıklar tamam.
                    result.OverallResult = "Güvenli";
                }
            }
            catch (Exception ex)
            {
                result.OverallResult = "Hata Oluştu";
                result.ServerInfoLeakDetails = $"Scan failed: {ex.Message}";
                result.IsSslValid = false;
            }

            await _unitOfWork.ScanResultRepository.AddAsync(result);

            await _unitOfWork.SaveAsync();

            return result;


        }

    }
}
