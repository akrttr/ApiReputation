using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiReputation.Domain.Entities
{
    public class ScanResult
    {
        public int Id { get; set; }

        public DateTime ScanDate { get; set; }
        public int ApiInfoId { get; set; }
        [ForeignKey("ApiInfoId")]
        public ApiInfo ApiInfo { get; set; }

        public bool IsSslValid { get; set; }
        public string SslIssuer { get; set; }
        public DateTime SslExpiryDate { get; set; }

        public bool HasHstsHeader { get; set; }
        public bool HasXFrameOptionsHeader { get; set; }
        public bool HasXContentTypeOptionsHeader { get; set; }

        public string ServerInfoLeakDetails { get; set; } 

        public string OverallResult { get; set; }
        public int ResponseTimeMs { get; set; }
        public bool IsSuccessStatusCode { get; set; }
        public int HttpStatusCode { get; set; }
    }
}
