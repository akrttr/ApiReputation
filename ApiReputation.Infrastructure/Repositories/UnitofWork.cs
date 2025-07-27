using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ApiReputation.Application.Interfaces;
using ApiReputation.Application.Repositories;
using ApiReputation.Domain.Entities;
using ApiReputation.Infrastructure.Contexts;

namespace ApiReputation.Infrastructure.Repositories
{
    public class UnitofWork : IUnitofWork
    {

        private readonly ApplicationDbContext _context;
        private IRepository<ApiInfo> _apiInfoRepository;
        private IRepository<ScanResult> _scanResultRepository;


        public UnitofWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<ApiInfo> ApiInfoRepository =>
           _apiInfoRepository ??= new Repository<ApiInfo>(_context);

        public IRepository<ScanResult> ScanResultRepository =>
            _scanResultRepository ??= new Repository<ScanResult>(_context);

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
