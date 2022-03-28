using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace $ext_safeprojectname$.DAL.Model
{
    public partial class TestEntities : DbContext
    {
        private string _connectionString = string.Empty;

        public TestEntities()
        {
            _connectionString = GetConnectionString();
        }

        public TestEntities(IConfiguration config)
        {
            _connectionString = GetConnectionString(config);
        }

        public TestEntities(DbContextOptions<TestEntities> options)
            : base(options)
        {
            _connectionString = GetConnectionString();
        }

        public TestEntities(IConfiguration config, DbContextOptions<TestEntities> options)
            : base(options)
        {
            _connectionString = GetConnectionString(config);
        }

        public TestEntities(string connectionString)
        {
            _connectionString = connectionString ?? GetConnectionString();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (string.IsNullOrEmpty(_connectionString))
                    throw new OperationCanceledException("Connection string is empty for TestEntities.");
                optionsBuilder.UseLazyLoadingProxies().UseSqlServer(_connectionString, o => o.UseRelationalNulls(false));

            }
        }

        /// <summary> Получение строки подключения из файла настроек. Если через DI, то IConfiguration приходит в параметрах </summary>
        /// <param name="config">Конфигурация</param>
        /// <returns>Строка подключения</returns>
        private string GetConnectionString(IConfiguration config = null)
        {
            if (config == null)
            {
                config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
            }
            return config.GetConnectionString(ConnectionStringNames.Get(DatabaseProviderEnum.test));
        }

        public virtual DbSet<TestTable> TestTable { get; set; }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestTable>(entity =>
            {

            });
            OnModelCreatingPartial(modelBuilder);
        }
    }
}
