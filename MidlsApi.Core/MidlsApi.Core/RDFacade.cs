using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary> Расширения DbContext, DatabaseFacade, позволяющие выполнять запросы напрямую через контекст </summary>
    public static class RDFacadeExtensions
    {
        /// <summary> Выполнить запрос напрямую </summary>
        /// <param name="databaseFacade">Расширяемый объект</param>
        /// <param name="sql">Текст запроса</param>
        /// <param name="parameters">Список параметров, которые автоматически конвертируются
        /// в параметры типа DbParameter с именами вида @p0, @p1 и т.д.</param>
        /// <returns>DataReader</returns>
        public static RelationalDataReader ExecuteSqlQuery(this DatabaseFacade databaseFacade, string sql, params object[] parameters)
        {
            var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();

            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = databaseFacade
                    .GetService<IRawSqlCommandBuilder>()
                    .Build(sql, parameters);

                RelationalCommandParameterObject ro =
                    new RelationalCommandParameterObject(
                          databaseFacade.GetService<IRelationalConnection>(),
                          rawSqlCommand.ParameterValues,
                          readerColumns: null,
                          context: null,
                          logger: null
                        );

                return rawSqlCommand
                                    .RelationalCommand
                                    .ExecuteReader(parameterObject: ro);
            }
        }

        /// <summary> Выполнить запрос напрямую, асинхронно </summary>
        /// <param name="databaseFacade">Расширяемый объект</param>
        /// <param name="sql">Текст запроса</param>
        /// <param name="cancellationToken">Токен System.Threading.CancellationToken для синхронизации прекращения выполнения задачи</param>
        /// <returns>Количество затронутых записей</returns>
        /// <param name="parameters">Список параметров, которые автоматически конвертируются
        /// в параметры типа DbParameter с именами вида @p0, @p1 и т.д.</param>
        /// <returns>DataReader</returns>
        public static async Task<RelationalDataReader> ExecuteSqlQueryAsync(this DatabaseFacade databaseFacade,
                                                             string sql,
                                                             CancellationToken cancellationToken = default(CancellationToken),
                                                             params object[] parameters)
        {

            var concurrencyDetector = databaseFacade.GetService<IConcurrencyDetector>();

            using (concurrencyDetector.EnterCriticalSection())
            {
                var rawSqlCommand = databaseFacade
                    .GetService<IRawSqlCommandBuilder>()
                    .Build(sql, parameters);

                RelationalCommandParameterObject ro =
                    new RelationalCommandParameterObject(
                          databaseFacade.GetService<IRelationalConnection>(),
                          rawSqlCommand.ParameterValues,
                          readerColumns: null,
                          context: null,
                          logger: null
                        );
                return await rawSqlCommand
                                    .RelationalCommand
                                    .ExecuteReaderAsync(
                                        parameterObject: ro,
                                        cancellationToken: cancellationToken);
            }
        }

        /// <summary> Выполнить сырой SQL запрос не возвращающий данных </summary>
        /// <param name="context">Расширяемый объект, контекст подключения к БД</param>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров, которые автоматически конвертируются
        /// в параметры типа DbParameter с именами вида @p0, @p1 и т.д.</param>
        /// <returns>Количество затронутых записей</returns>
        public static int ExecuteSqlCommand(this DbContext context, string query, params object[] parameters) => 
            context.Database.ExecuteSqlRaw(query, parameters);

        /// <summary> Выполнить сырой SQL запрос не возвращающий данных, асинхронно </summary>
        /// <param name="context">Расширяемый объект, контекст подключения к БД</param>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров, которые автоматически конвертируются
        /// в параметры типа DbParameter с именами вида @p0, @p1 и т.д.</param>
        /// <returns>Количество затронутых записей</returns>
        public static async Task<int> ExecuteSqlCommandAsync(this DbContext context, string query, params object[] parameters) =>
            await context.Database.ExecuteSqlRawAsync(query, parameters);

        /// <summary> Выполнить сырой SQL запрос не возвращающий данных, асинхронно </summary>
        /// <param name="context">Расширяемый объект, контекст подключения к БД</param>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров, которые автоматически конвертируютсяЮ
        /// в параметры типа DbParameter с именами вида @p0, @p1 и т.д.</param>
        /// <param name="token">Токен System.Threading.CancellationToken для синхронизации прекращения выполнения задачи</param>
        /// <returns>Количество затронутых записей</returns>
        public static async Task<int> ExecuteSqlCommandAsync(this DbContext context, string query, IEnumerable<object> parameters, CancellationToken token = default) =>
            await context.Database.ExecuteSqlRawAsync(query, parameters, token);

        /// <summary> Выполнить сырой SQL запрос возвращающий данные </summary>
        /// <param name="context">Расширяемый объект, контекст подключения к БД</param>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров</param>
        public static IList<T> SqlQuery<T>(this DbContext context, string query, params object[] parameters)
        {
            var result = new List<T>();
            using (var rr = context.Database.ExecuteSqlQuery(query, parameters))
            {
                var dbReader = rr.DbDataReader;
                try
                {
                    if (dbReader.HasRows)
                    {
                        while (dbReader.Read())
                            result.Add(ParseFields<T>(dbReader));
                    }
                }
                finally
                {
                    dbReader.Close();
                }
            }
            return result;
        }

        /// <summary> Выполнить сырой SQL запрос возвращающий данные, асинхронно </summary>
        /// <param name="context">Расширяемый объект, контекст подключения к БД</param>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров</param>
        public static async Task<IList<T>> SqlQueryAsync<T>(this DbContext context, string query, CancellationToken token = default, params object[] parameters)
        {
            var result = new List<T>();
            using (var rr = await context.Database.ExecuteSqlQueryAsync(query, token, parameters))
            {
                var dbReader = rr.DbDataReader;
                try
                {
                    if (dbReader.HasRows)
                    {
                        while (await dbReader.ReadAsync(token))
                            result.Add(ParseFields<T>(dbReader));
                    }
                }
                finally
                {
                    dbReader.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// Создать объект, если у типа есть конструктор по-умолчанию или вернуть значение по-умолчанию,
        /// если тип является производным от System.ValueType (int, string, double and so on).
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <returns>Объект или дефолтное значение для базовых типов</returns>
        private static T GetObjectOfType<T>()
        {
            Type objType = typeof(T);
            if (objType.IsValueType)
                return default(T);
            ConstructorInfo cnstr = objType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null);
            if (cnstr != null)
                return (T)cnstr.Invoke(new object[] { });
            return default(T);
        }

        /// <summary> Заполнить свойства объекта значениями из датаридера </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="reader">датаридер</param>
        /// <returns>Заполненный объект или значение для базовых типов</returns>
        public static T ParseFields<T>(System.Data.Common.DbDataReader reader)
        {
            T result = GetObjectOfType<T>();
            Type resultType = typeof(T);
            if (!resultType.IsValueType && resultType != typeof(string) && result == null)
                return default(T);
            for (int fieldNum = reader.FieldCount - 1; fieldNum >= 0; fieldNum--)
            {
                Type valueType = reader.GetFieldType(fieldNum);
                string fieldName = reader.GetName(fieldNum);
                object value = reader.GetValue(fieldNum);
                value = value is System.DBNull ? null : value;
                if (resultType.IsValueType || resultType == typeof(string))
                {
                    if (resultType.IsAssignableFrom(valueType))
                    {
                        result = (T)value;
                        continue;
                    }
                    try
                    {
                        result = (T)Convert.ChangeType(value, resultType);
                    }
                    catch
                    {
                        throw new FormatException($"Ожидаемый результат и результат запроса имеют разные форматы: {resultType.ToString()} и {valueType.ToString()}");
                    }
                }
                if (result != null)
                {
                    PropertyInfo prop = resultType.GetProperty(fieldName);
                    if (prop != null)
                    {
                        if (!prop.PropertyType.IsAssignableFrom(valueType))
                        {
                            try
                            {
                                prop.SetValue(result, ChangeType(value, prop.PropertyType));
                            }
                            catch
                            {
                            }
                        }
                        else
                            prop.SetValue(result, value);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// достать из типа nullable базовый тип
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conversion"></param>
        /// <returns></returns>
        private static  object ChangeType(object value, Type conversion)
        {
            var t = conversion;
            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }
                t = Nullable.GetUnderlyingType(t);
            }
            return Convert.ChangeType(value, t);
        }
    }
}
