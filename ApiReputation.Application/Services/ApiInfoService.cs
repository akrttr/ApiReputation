using ApiReputation.Application.Repositories;
using ApiReputation.Domain.Entities;
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
       // public async Task<ApiInfo> GetApiByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task<ApiInfo> GetApiByIdAsync(int id)
        {
           
            var api = await _repository.GetByIdAsync(id);
            if (api == null)
                throw new KeyNotFoundException("API bulunamadı.");

            return api;
        }

        public async Task AddApiAsync(ApiInfo api) => await _repository.AddAsync(api);
        public async Task DeleteApiAsync(int id)
        {
            var api = await _repository.GetByIdAsync(id);
            if (api == null)
                throw new KeyNotFoundException("API bulunamadı.");

            await _repository.DeleteAsync(api);
        }



        public async Task UpdateApiAsync(ApiInfo api)
        {
            var existingApi = await _repository.GetByIdAsync(api.Id);
            if (existingApi == null)
                throw new KeyNotFoundException("API bulunamadı.");

            existingApi.Name = api.Name;
            existingApi.BaseUrl = api.BaseUrl;
            existingApi.Category = api.Category;

            await _repository.UpdateAsync(existingApi);
        }

    }
}
