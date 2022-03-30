using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace MidlsApi.Database.DAL
{
    /// <summary> Интерфейс UnitOfWork для доступа к данным </summary>
    public interface IDataProvider : IDisposable
    {
        /// <summary> Контекст подключения к базе данных </summary>
        DbContext Context { get; }

        /// <summary> Установить время ожидания при выполнении команды </summary>
        /// <param name="timeout">вермя ожидания в секундах</param>
        void SetCommandTimeout(int timeout);

        /// <summary> Установить время ожидания при выполнении команды </summary>
        /// <param name="timeout">вермя ожидания в секундах</param>
        void SetCommandTimeout(TimeSpan timeout);

        #region CRUD

        /// <summary> Получить IQueryable"EntityType" по указанной сущности </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <returns>Набор сущностей</returns>
        IQueryable<EntityType> Set<EntityType>() where EntityType : class, IEntityBase;

        /// <summary> Получить NoTrackable set по указанной сущности </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <returns>Набор неотслеживаемых сущностей</returns>
        IQueryable<EntityType> SetNQ<EntityType>() where EntityType : class, IEntityBase;

        /// <summary> Получить первую сущность по значению ключевого поля, искать среди всех записей </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="condition">Условие поиска</param>
        /// <returns>Найденная сущность или null</returns>
        EntityType Find<EntityType>(System.Linq.Expressions.Expression<Func<EntityType, bool>> condition) where EntityType : class, IEntityBase;

        /// <summary> Получить первую сущность по значению ключевого поля, искать среди всех записей, асинхронно </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="condition">Условие поиска</param>
        /// <param name="token">Токен остановки</param>
        /// <returns>Найденная сущность или null</returns>
        Task<EntityType> FindAsync<EntityType>(System.Linq.Expressions.Expression<Func<EntityType, bool>> condition, CancellationToken token = default) where EntityType : class, IEntityBase;

        /// <summary> Получить первую сущность по значению ключевого поля, искать среди всех записей (неотслеживаемая) </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="condition">Условие</param>
        /// <returns>Найденная сущность или null</returns>
        EntityType FindNQ<EntityType>(System.Linq.Expressions.Expression<Func<EntityType, bool>> condition) where EntityType : class, IEntityBase;

        /// <summary> Получить первую сущность по значению ключевого поля, искать среди всех записей (неотслеживаемая), асинхронно </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="condition">Условие поиска</param>
        /// <param name="token">Токен остановки</param>
        /// <returns>Найденная сущность или null</returns>
        Task<EntityType> FindNQAsync<EntityType>(System.Linq.Expressions.Expression<Func<EntityType, bool>> condition, CancellationToken token = default) where EntityType : class, IEntityBase;

        /// <summary> Добавить новую запись сущности в контекст </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="saveChanges">Сразу сохранить</param>
        void Add<EntityType>(EntityType entity, bool saveChanges = true) where EntityType : class, IEntityBase;

        /// <summary> Добавить новую запись сущности в контекст, асинхронно </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="saveChanges">Сразу сохранить</param>
        /// <param name="token">Токен System.Threading.CancellationToken для синхронизации прекращения выполнения задачи</param>
        Task AddAsync<EntityType>(EntityType entity, bool saveChanges = true, CancellationToken token = default) where EntityType : class, IEntityBase;

        /// <summary> Метод прикрепляет объект к контексту.
        /// Если существует прикрепленный объект того же типа с тем же Id, то прикрепление не происходит. </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <returns>entity или найденный объект</returns>
        EntityType GetOrAttach<EntityType>(EntityType entity) where EntityType : class, IEntityBase;

        /// <summary> Поменять статус сущности в контексте </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="state">Новый статус</param>
        void StateChange<EntityType>(EntityType entity, EntityState state) where EntityType : class, IEntityBase;

        /// <summary> Обновить сущность в контексте </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="saveChanges">Сразу сохранить</param>
        /// <param name="updateDateChange">обновить дату изменения</param>
        void Update<EntityType>(EntityType entity, bool saveChanges = true, bool updateDateChange = true) where EntityType : class, IEntityBase;

        /// <summary> Обновить сущность в контексте </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="saveChanges">Сразу сохранить</param>
        /// <param name="updateDateChange">обновить дату изменения</param>
        /// <param name="token">Токен System.Threading.CancellationToken для синхронизации прекращения выполнения задачи</param>
        Task UpdateAsync<EntityType>(EntityType entity, bool saveChanges = true, bool updateDateChange = true, CancellationToken token = default) where EntityType : class, IEntityBase;

        /// <summary> Добавить или обновить сущность в контексте (определяется на основании первичных ключей) </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="saveChanges">Сохранить контекст</param>
        /// <param name="updateDateChange">Обновлять дату изменения записи</param>
        void SaveOrUpdate<EntityType>(EntityType entity, bool saveChanges = true, bool updateDateChange = true) where EntityType : class, IEntityBase;

        /// <summary> Добавить или обновить сущность в контексте (определяется на основании первичных ключей), асинхронный метод </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="saveChanges">Сохранить контекст</param>
        /// <param name="updateDateChange">Обновлять дату изменения записи</param>
        /// <param name="token">Токен System.Threading.CancellationToken для синхронизации прекращения выполнения задачи</param>
        Task SaveOrUpdateAsync<EntityType>(EntityType entity, bool saveChanges = true, bool updateDateChange = true, CancellationToken token = default) where EntityType : class, IEntityBase;

        /// <summary> Обновить статус свойства сущности в контексте </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <typeparam name="TProperty">Тип свойства</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="expr">Лямбда-выражение</param>
        void UpdateProperty<EntityType, TProperty>(EntityType entity, Expression<Func<EntityType, TProperty>> expr) where EntityType : class, IEntityBase;

        /// <summary> Удалить сущность (перенести в архив). Если в сущности есть поле IsArchive, будет выставлено IsArchive = 1, 
        /// иначе сущность будет удалена </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        void Delete<EntityType>(EntityType entity) where EntityType : class, IEntityBase;

        /// <summary> Удалить сущность (перенести в архив). Если в сущности есть поле IsArchive, будет выставлено IsArchive = 1, 
        /// иначе сущность будет удалена </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="token">Токен System.Threading.CancellationToken для синхронизации прекращения выполнения задачи</param>
        Task DeleteAsync<EntityType>(EntityType entity, CancellationToken token = default) where EntityType : class, IEntityBase;

        /// <summary> Удалить список сущностей. Если в сущности есть поле IsArchive, будет выставлено IsArchive = 1,  иначе сущность будет удалена </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entities">Список объектов сущностей</param>
        void Delete<EntityType>(IEnumerable<EntityType> entities) where EntityType : class, IEntityBase;

        /// <summary> Удалить список сущностей. Если в сущности есть поле IsArchive, будет выставлено IsArchive = 1,  иначе сущность будет удалена </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entities">Список объектов сущностей</param>
        /// <param name="token">Токен System.Threading.CancellationToken для синхронизации прекращения выполнения задачи</param>
        Task DeleteAsync<EntityType>(IEnumerable<EntityType> entities, CancellationToken token = default) where EntityType : class, IEntityBase;

        /// <summary> Присоединить сущность к контексту </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entity"></param>
        /// <returns>Присоединенная сущность</returns>
        EntityType Attach<EntityType>(EntityType entity) where EntityType : class, IEntityBase;

        /// <summary> Отсоединить сущность от контекста </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        void Detach<EntityType>(EntityType entity) where EntityType : class, IEntityBase;

        /// <summary> Сохранить изменения контекста </summary>
        void SaveChanges();

        /// <summary> Сохранить изменения контекста, асинхронный </summary>
        /// <param name="token">Токен System.Threading.CancellationToken для синхронизации прекращения выполнения задачи</param>
        Task SaveChangesAsync(CancellationToken token = default);

        /// <summary> Выполнить сырой SQL запрос возвращающий данные </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров</param>
        /// <returns>Результаты запроса списком</returns>
        IList<T> SqlQuery<T>(string query, params object[] parameters);

        /// <summary> Выполнить сырой SQL запрос возвращающий данные, асинхронно </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров</param>
        /// <param name="token">Токен прерывания запроса</param>
        /// <returns>Результаты запроса списком</returns>
        Task<IList<T>> SqlQueryAsync<T>(string query, CancellationToken token = default, params object[] parameters);

        /// <summary> Выполнить сырой SQL запрос не возвращающий данных </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров, которые автоматически конвертируются
        /// в параметры типа DbParameter с именами вида @p0, @p1 и т.д.</param>
        /// <returns>Количество затронутых записей</returns>
        int ExecuteSqlCommand(string query, params object[] parameters);

        /// <summary> Выполнить сырой SQL запрос возвращающий данные, асинхронно </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров</param>
        /// <param name="token">Токен прерывания запроса</param>
        /// <returns>Количество затронутых записей</returns>
        Task<int> ExecuteSqlCommandAsync(string query, IEnumerable<object> parameters, CancellationToken token = default);

        /// <summary> Выполнить сырой SQL запрос возвращающий данные, асинхронно </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров</param>
        /// <returns>Количество затронутых записей</returns>
        Task<int> ExecuteSqlCommandAsync(string query, params object[] parameters);
        #endregion

        #region Transaction

        /// <summary> Запустить ReadCommitted транзакцию </summary>
        /// <returns>Транзакция</returns>
        TransactionScope GetTransactionScope(IsolationLevel isolationLevel);

        /// <summary> Запустить транзакцию с указанным уровнем изоляции </summary>
        /// <returns>Транзакция</returns>
        TransactionScope GetTransactionScope();

        /// <summary> Запуск транзакции в базе данных </summary>
        IDbContextTransaction BeginTransaction();

        /// <summary> Подтвердить транзакцию </summary>
        void CommitTransaction();

        /// <summary> Откатить транзакцию </summary>
        void RollbackTransaction();

        /// <summary> Отменить транзакцию в БД с откатом изменений в отслеживаемых сущностях </summary>
        /// <param name="allChanges">Отменить изменения в отслеживаемых сущностях</param>
        void RollbackTransaction(bool allChanges);

        #endregion Transaction

        #region Collections

        /// <summary> Присоединить коллекцию к контексту </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="collection">Коллекция сущностей</param>
        /// <returns>Присоединенную к контексту коллекцию</returns>
        ICollection<EntityType> GetOrAttachCollection<EntityType>(ICollection<EntityType> collection) where EntityType : class, IEntityBase;

        /// <summary> Скопировать одну сущность в другую </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="sourceEntity">Исходный объект</param>
        /// <param name="destinationEntity">Конечный объект</param>
        void CopyDtoToEntity<EntityType>(EntityType sourceEntity, EntityType destinationEntity) where EntityType : class, IEntityBase;

        /// <summary> Скопировать коллекцию </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="sourceCollection">Исходная коллекция</param>
        /// <param name="destinationCollection">Конечная коллекция</param>
        void CopyCollection<EntityType>(ICollection<EntityType> sourceCollection, ICollection<EntityType> destinationCollection) where EntityType : class, IEntityBase;

        /// <summary> Скопировать коллекцию из начальной в конечную </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="sourceCollection">Исходная коллекция</param>
        /// <param name="destinationCollection">Конечная коллекция</param>
        /// <param name="mergFunc">Метод обработки одинаковых элементов</param>
        /// <returns>Обновленная конечная коллекция</returns>
        ICollection<EntityType> MergeCollection<EntityType>(ICollection<EntityType> sourceCollection, ICollection<EntityType> destinationCollection, Action<EntityType, EntityType> mergFunc = null) where EntityType : class, IEntityBase;

        #endregion Collections

    }
}
