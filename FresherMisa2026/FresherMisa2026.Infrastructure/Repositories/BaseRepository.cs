using Dapper;
using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Department;
using FresherMisa2026.Entities.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FresherMisa2026.Infrastructure.Repositories
{   
    /// <summary>
    /// Type handler for Guid to string conversion in MySQL
    /// </summary>
    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {   
            if (value == null || value is DBNull)
                return Guid.Empty;
            if (value is Guid guid)
                {
                Guid checkne = guid;
                    return guid;
                }
            if (value is string str)
                return Guid.Parse(str);
            if (value is byte[] bytes)
                return new Guid(bytes);
            return Guid.Parse(value.ToString()!);
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.DbType = DbType.String;
            parameter.Value = value.ToString();
        }
    }

    /// <summary>
    /// Base repository
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// Created By: NHoang (18/04/2026)
    public class BaseRepository<TEntity> : IBaseRepository<TEntity>, IDisposable where TEntity : BaseModel
    {
        //Properties
        protected readonly string _connectionString;
        private readonly IConfiguration _configuration;
        protected string _tableName;
        public Type _modelType = null;
        protected readonly IMemoryCache _cache;

        //Static constructor to register Guid type handler once
        static BaseRepository()
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
        }


        //Constructor
        public BaseRepository(IConfiguration configuration,IMemoryCache cache)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
            _modelType = typeof(TEntity);
            _tableName = _modelType.GetTableName();
            _cache = cache;
        }


        /// <summary>
        /// Dispose connection
        /// </summary>
        /// Created By: dvhai (09/04/2026)
        public void Dispose()
        {
           
        }

        /// <summary>
        /// Mở kết nối database
        /// </summary>
        protected async Task OpenConnectionAsync()
        {
            
        }

        #region Method Get
        /// <summary>
        /// Lấy danh sách entity
        /// </summary>
        /// <returns>Danh sách tất cả bản ghi</returns>
        /// Created By: dvhai (09/04/2026)
        /// Updated By: NHoang (18/04/2026)
        public async Task<IEnumerable<BaseModel>> GetEntitiesAsync()
        {
             string cacheKey = $"{_tableName}:All";
        
                // Check cache
                if (_cache.TryGetValue(cacheKey, out IEnumerable<BaseModel> entities))
                {
                    return entities;
                }
                
                // Query DB
                entities = await GetEntitiesUsingCommandTextAsync();
                
                // Cache 5 phút
                _cache.Set(cacheKey, entities.ToList(), TimeSpan.FromMinutes(5));
                
                return entities;
        }

        /// <summary>
        /// Lấy tất cả theo command text
        /// </summary>
        /// <returns></returns>
        /// CREATED BY: NHoang (18/04/2026)
        private async Task<IEnumerable<TEntity>> GetEntitiesUsingCommandTextAsync()
        {
            var query = new StringBuilder($"select * from {_tableName}");

            if (_modelType.GetHasDeletedColumn())
            {
                query.Append($" where IsDeleted = FALSE");
            }

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var entities = await connection.QueryAsync<TEntity>(query.ToString(), commandType: CommandType.Text);
            return entities.ToList();
        }

        /// <summary>
        /// Lấy bản ghi theo id
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Bản ghi tìm thấy hoặc null</returns>
        /// CREATED BY: NHoang (18/04/2026)
        public async Task<TEntity> GetEntityByIDAsync(Guid entityId)
        {
            string cacheKey = $"{_tableName}:ID:{entityId}";
        
                // Check cache
                if (_cache.TryGetValue(cacheKey, out TEntity entity))
                {
                    return entity;
                }
                
                // Query DB
                entity = await GetEntitieByIdUsingCommandTextAsync(entityId.ToString());
                
                if (entity != null)
                {
                    _cache.Set(cacheKey, entity, TimeSpan.FromMinutes(5));
                }
                
                return entity;
        }

        /// <summary>
        /// Lấy bản ghi theo id dùng command text
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// CREATED BY: NHoang (18/04/2026)
        private async Task<TEntity> GetEntitieByIdUsingCommandTextAsync(string id)
        {
            var query = new StringBuilder($"select * from {_tableName}");
            int whereCount = 0;
            Func<StringBuilder, bool> AppendWhere = (query) => { 
                if (query.ToString().Contains(" where ")) return false; 
                query.Append(" where "); 
                return true; 
            };
            var primaryKey = _modelType.GetKeyName();
            if (primaryKey != null)
            {
                if (AppendWhere(query))
                    query.Append($"{primaryKey} = @Id");
                    whereCount++;
            }
            if (_modelType.GetHasDeletedColumn())
            {
                
                    query.Append(whereCount > 0 
                    ? " AND IsDeleted = FALSE"    
                    : " where IsDeleted = FALSE");
                    whereCount++;
            }
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var entity = await connection.QueryFirstOrDefaultAsync<TEntity>(
                query.ToString(), 
                new { Id = id }, 
                commandType: CommandType.Text
            );
            
            return entity;
        }

        /// <summary>
        /// Xóa bản ghi theo id
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Số bản ghi bị xóa</returns>
        /// CREATED BY: DVHAI (11/07/2021)
        /// UPDATED BY: NHoang (18/04/2026)
        public async Task<int> DeleteAsync(Guid entityId)
        {
            
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var transaction = connection.BeginTransaction();
            try
            {
                var keyName = _modelType.GetKeyName();
                var dynamicParams = new DynamicParameters();
                dynamicParams.Add($"@v_{keyName}", entityId.ToString());
                var rowAffects = await connection.ExecuteAsync(
                    $"Proc_Delete{_tableName}ById", 
                    param: dynamicParams, 
                    transaction: transaction, 
                    commandType: CommandType.StoredProcedure
                );
                transaction.Commit();
                
                ClearCache(entityId);
                
                return rowAffects;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

           
        }


        /// <summary>
        /// Thêm bản ghi mới
        /// </summary>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>Số bản ghi thêm mới</returns>
        /// CREATED BY: DVHAI (11/07/2021)
        /// UPDATED BY: NHoang (18/04/2026)
        public async Task<int> InsertAsync(TEntity entity)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var transaction = connection.BeginTransaction();
            try
            {
                var parameters = MappingDbType(entity);
                var rowAffects = await connection.ExecuteAsync(
                    $"Proc_Insert{_tableName}", 
                    param: parameters, 
                    transaction: transaction, 
                    commandType: CommandType.StoredProcedure
                );
                transaction.Commit();
                
                ClearCache();
                
                return rowAffects;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Cập nhật thông tin bản ghi
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// CREATED BY: DVHAI (11/07/2021)
        /// UPDATE BY:HNGUYEN(17/4/2026)
        public async Task<int> UpdateAsync(Guid entityId, TEntity entity)
        {
           using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var transaction = connection.BeginTransaction();
            try
            {
                var keyName = _modelType.GetKeyName();
                entity.GetType().GetProperty(keyName).SetValue(entity, entityId);
                var parameters = MappingDbType(entity);
                var rowAffects = await connection.ExecuteAsync(
                    $"Proc_Update{_tableName}", 
                    param: parameters, 
                    transaction: transaction, 
                    commandType: CommandType.StoredProcedure
                );
                transaction.Commit();
                
               
                ClearCache(entityId);
                
                return rowAffects;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Lấy danh sách thực thể paging
        /// </summary>
        /// <param name="pageSize">Số bản ghi mỗi trang</param>
        /// <param name="pageIndex">Chỉ số trang</param>
        /// <param name="search">Từ khóa tìm kiếm</param>
        /// <param name="searchFields">Danh sách trường tìm kiếm</param>
        /// <param name="sort">Sắp xếp theo</param>
        /// <returns>Tổng số bản ghi và danh sách dữ liệu</returns>
        /// CREATED BY: DVHAI (07/07/2026)
        /// UPDATED BY: NHoang (18/04/2026)
        public async Task<(long Total,
            IEnumerable<TEntity> Data)> GetFilterPagingAsync(
            int pageSize,
            int pageIndex,
            string search,
            List<string> searchFields,
            string sort)
        {
            long total = 0;
            var data = Enumerable.Empty<TEntity>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string store = string.Format("Proc_{0}_FilterPaging", _tableName);
            var parameters = new DynamicParameters();
            parameters.Add("@v_pageIndex", pageIndex);
            parameters.Add("@v_pageSize", pageSize);
            parameters.Add("@v_search", search);
            parameters.Add("@v_sort", sort);
            parameters.Add("@v_searchFields", JsonSerializer.Serialize(searchFields));

            using var reader = await connection.QueryMultipleAsync(
                new CommandDefinition(store, parameters, commandType: CommandType.StoredProcedure));

            data = (await reader.ReadAsync<TEntity>()).ToList();
            total = await reader.ReadFirstAsync<long>();

            return (total, data);
        }

        /// <summary>
        /// Ánh xạ các thuộc tính sang kiểu dynamic
        /// </summary>
        /// <param name="entity">Thực thể</param>
        /// <returns>Dan sách các biến động</returns>
            private DynamicParameters MappingDbType(TEntity entity)
        {
            var parameters = new DynamicParameters();
            try
            {
                //1. Duyệt các thuộc tính trên entity và tạo parameters
                var properties = entity.GetType().GetProperties();

                foreach (var property in properties)
                {
                    var propertyName = property.Name;
                    var propertyValue = property.GetValue(entity);
                    var propertyType = property.PropertyType;

                    if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                        parameters.Add($"@v_{propertyName}", propertyValue?.ToString(), DbType.String);
                    else
                        parameters.Add($"@v_{propertyName}", propertyValue);
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with empty parameters
                Console.WriteLine($"Error mapping entity properties: {ex.Message}");
            }
            //2. Trả về danh sách các parameter
            return parameters;
        }

        #endregion
         #region Cache Clear Methods
        /// <summary>
        /// Xóa cache toàn bộ danh sách
        /// </summary>
        private void ClearCache()
        {
            _cache.Remove($"{_tableName}:All");
        }
        /// <summary>
        /// Xóa cache theo ID và toàn bộ danh sách
        /// </summary>
        private void ClearCache(Guid entityId)
        {
            _cache.Remove($"{_tableName}:ID:{entityId}");
            _cache.Remove($"{_tableName}:All");
        }
        #endregion
    }
}
