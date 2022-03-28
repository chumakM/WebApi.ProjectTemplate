using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace $ext_safeprojectname$.DAL
{
    /// <summary> Фабрика создания провайдеров для БД </summary>
    public class DataProviderFactory : IDataProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly ConcurrentDictionary<DatabaseProviderEnum, Tuple<Type, string>> providerList = new ConcurrentDictionary<DatabaseProviderEnum, Tuple<Type, string>>();

        /// <summary> Конструктор </summary>
        /// <param name="serviceProvider">Сервис-локатор</param>
        public DataProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary> Зарегистрировать новый тип </summary>
        /// <param name="provider">Тип DataProvider</param>
        /// <param name="implementationType">Тип реализующий DataProvider</param>
        /// <param name="connectionString">Строка подключения. Если не указано, будет использоваться IConfiguration</param>
        public static void Register(DatabaseProviderEnum provider, Type implementationType, string connectionString = null)
        {

            providerList.TryAdd(provider, new Tuple<Type,string> (implementationType,connectionString));
        }

        /// <summary> Создать провайдера </summary>
        /// <param name="provider">Тип провайдера</param>
        /// <returns>Созданный объект-провайдер</returns>
        public IDataProvider CreateProvider(DatabaseProviderEnum provider)
        {
            if (providerList.TryGetValue(provider, out Tuple<Type,string> providerData))
            {
                string connectionString = providerData.Item2;
                IDataProvider service;
                if (string.IsNullOrEmpty(connectionString))
                {
                    IConfiguration config = _serviceProvider.GetRequiredService<IConfiguration>();
                    if (config == null)
                        throw new Exception("Configuration service not found");
                    connectionString = config.GetConnectionString(ConnectionStringNames.Get(provider));
                    if (string.IsNullOrWhiteSpace(connectionString))
                        throw new ArgumentOutOfRangeException(nameof(provider), $"Connection string not found for {provider}");
                    if (providerList.TryRemove(provider, out _))
                        providerList.TryAdd(provider, new Tuple<Type, string>(providerData.Item1, connectionString));

                }
                service = ActivatorUtilities.CreateInstance(_serviceProvider, providerData.Item1, connectionString) as IDataProvider;
                if (service != null)
                    return service;
            }
            throw new ArgumentOutOfRangeException(nameof(provider), $"Unknown database provider {provider}");
        }
    }
}
