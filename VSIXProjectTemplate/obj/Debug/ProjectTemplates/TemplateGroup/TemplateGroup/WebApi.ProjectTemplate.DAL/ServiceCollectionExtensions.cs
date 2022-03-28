using $safeprojectname$;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary> Расширение методов для DataProvider </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary> Добавить новый DataProvider к фабрике </summary>
        /// <param name="services"></param>
        /// <param name="provider"></param>
        /// <param name="builder"></param>
        public static void AddDataProviderFactory(this IServiceCollection services, DatabaseProviderEnum provider, Func<IConfiguration, IDataProvider> builder)
        {
            if (!services.Contains(new ServiceDescriptor(typeof(IDataProviderFactory), typeof(DataProviderFactory), ServiceLifetime.Singleton)))
            {
                services.AddSingleton<IDataProviderFactory, DataProviderFactory>();
            }

        }

        /// <summary> Добавить новый DataProvider к фабрике </summary>
        /// <param name="services">Расширяемый объект</param>
        /// <param name="provider">Тип DataProvider</param>
        /// <param name="implementationType">Класс, реализующий DataProvider</param>
        /// <param name="connectionString">Строка подключения</param>
        public static void AddDataProviderFactory(this IServiceCollection services, DatabaseProviderEnum provider, Type implementationType, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is empty", nameof(connectionString));
            if (!services.Contains(new ServiceDescriptor(typeof(IDataProviderFactory), typeof(DataProviderFactory), ServiceLifetime.Singleton)))
                services.AddSingleton<IDataProviderFactory, DataProviderFactory>();
            DataProviderFactory.Register(provider, implementationType, connectionString);
        }

        /// <summary> Добавить новый DataProvider к фабрике </summary>
        /// <param name="services">Расширяемый объект</param>
        /// <param name="provider">Тип DataProvider</param>
        /// <param name="implementationType">Класс, реализующий DataProvider</param>
        public static void AddDataProviderFactory(this IServiceCollection services, DatabaseProviderEnum provider, Type implementationType)
        {
            if (!services.Contains(new ServiceDescriptor(typeof(IDataProviderFactory), typeof(DataProviderFactory), ServiceLifetime.Singleton)))
                services.AddSingleton<IDataProviderFactory, DataProviderFactory>();
            DataProviderFactory.Register(provider, implementationType, null);
        }



    }
}
