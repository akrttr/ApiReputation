using ApiReputation.Application.Interfaces;
using ApiReputation.Application.Repositories;
using ApiReputation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
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
                ScanDate = DateTime.Now,
            };

            var client = _httpClientFactory.CreateClient();

            try
            {
                // === ANA HTTP İSTEĞİ ===
                // Tüm kontrolleri bu tek isteğin yanıtı üzerinden yapacağız.
                var response = await client.GetAsync(apiInfo.BaseUrl);

                result.HasHstsHeader = response.Headers.Contains("Strict-Transport-Security");
                result.HasXFrameOptionsHeader = response.Headers.Contains("X-Frame-Options");
                result.HasXContentTypeOptionsHeader = response.Headers.Contains("X-Content-Type-Options");

                // === BİLGİ SIZINTISI KONTROLÜ ===
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

                // === SSL SERTİFİKASI KONTROLÜ ===
                // Bu basit kontrol, sertifikanın varlığını ve temel geçerliliğini test eder.
                // Not: Bu kısım hata verebilecek sunucular için daha da iyileştirilebilir.
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
                if (result.IsSslValid && result.HasHstsHeader)
                {
                    result.OverallResult = "Güvenli";
                }
                else if (result.IsSslValid == false)
                {
                    result.OverallResult = "Yüksek Risk";
                }
                else
                {
                    result.OverallResult = "Orta Risk";
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
