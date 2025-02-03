using ApiReputation.Domain.Entities;
using ApiReputation.Infrastructure.Repositories;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiReputation.Application.Services
{
    public class ApiInfoService
    {
        private readonly IRepository<ApiInfo> _repository;

        public ApiInfoService(IRepository<ApiInfo> repository)
        {
            _repository = repository;
        }


        public async Task<IEnumerable<ApiInfo>> GetAllApiAsync() => await _repository.GetAllAsync();
        public async Task<ApiInfo> GetApiByIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task AddApiAsync(ApiInfo api) => await _repository.AddAsync(api);
        public async Task DeleteApiAsync(int id) => await _repository.DeleteAsync(id);
        public async Task UpdateApiAsync(ApiInfo api) => await _repository.UpdateAsync(api);
        
        

        
    }
}
