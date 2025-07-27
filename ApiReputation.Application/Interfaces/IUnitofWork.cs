using ApiReputation.Application.Repositories;
using ApiReputation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiReputation.Application.Interfaces
{
    public interface IUnitofWork :IDisposable
    {
        IRepository<ApiInfo> ApiInfoRepository { get; }
        IRepository<ScanResult> ScanResultRepository { get; }

        Task<int> SaveAsync();

    }
}
