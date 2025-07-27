using ApiReputation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiReputation.Application.Interfaces
{
    public interface IScannerService
    {
        Task<ScanResult> PerformScanAsync(int apiInfoId);
    }
}
