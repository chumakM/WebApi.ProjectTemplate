using System;
using System.Collections.Generic;
using System.Text;

namespace MidlsApi.Database.DAL
{
    /// <summary> Названия подключений в настройках для провайдеров </summary>
    public static class ConnectionStringNames
    {
        private static readonly IDictionary<DatabaseProviderEnum, string> connectionNames = new Dictionary<DatabaseProviderEnum, string>() {
            { DatabaseProviderEnum.Default, "DefaultEntities" }
        };

        /// <summary> Получить название подключения </summary>
        /// <param name="provider">Тип DataProvider</param>
        /// <returns>Название подключения</returns>
        public static string Get(DatabaseProviderEnum provider)
        {
            if (!connectionNames.ContainsKey(provider) || string.IsNullOrWhiteSpace(connectionNames[provider]))
                throw new ArgumentOutOfRangeException(nameof(provider), $"Not found connection name for {provider}");
            return connectionNames[provider];
        }

    }
}
