using Microsoft.Extensions.Configuration;
using System;

namespace MidlsApi.Database.DAL
{
    /// <summary> Фабрика DataProvider'ов </summary>
    public interface IDataProviderFactory
    {
        /// <summary> Создать объект, если провайдер не зарегистрирован будет ArgumentOutOfRangeException</summary>
        /// <param name="provider">Тип DataProvider</param>
        /// <returns>DataProvider для определенного типа или ArgumentOutOfRangeException</returns>
        IDataProvider CreateProvider(DatabaseProviderEnum provider);
    }
}