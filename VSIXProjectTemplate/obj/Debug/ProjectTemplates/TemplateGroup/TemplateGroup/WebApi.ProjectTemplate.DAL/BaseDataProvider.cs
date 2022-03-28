using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace $ext_safeprojectname$.DAL
{
    /// <summary> UnitOfWork для доступа к данным БД. </summary>
    public abstract class BaseDataProvider : IDataProvider
    {
        private bool disposed = false;
        private readonly DbContext _context;
        private IDbContextTransaction _transaction = null;

        /// <summary> Контекст подключения к базе </summary>
        public DbContext Context => _context;

        /// <summary>Установить время ожидания при выполнении команды </summary>
        /// <param name="timeout">вермя ожидания</param>
        public void SetCommandTimeout(int timeout)
        {
            _context.Database.SetCommandTimeout(timeout);
        }

        /// <summary>Установить время ожидания при выполнении команды </summary>
        /// <param name="timeout">вермя ожидания</param>
        public void SetCommandTimeout(TimeSpan timeout)
        {
            _context.Database.SetCommandTimeout(timeout);
        }

        #region .ctor
        /// <summary> Конструктор на основании строки подключения </summary>
        public BaseDataProvider(string connectionString)
        {
            _context = CreateContext(connectionString);
        }

        /// <summary> Создание контекста подключения к БД, используя строку подключения </summary>
        /// <param name="connectionString">Строка подключения к БД</param>
        /// <returns>Контекст</returns>
        protected abstract DbContext CreateContext(string connectionString);

        #endregion .ctor

        #region CRUD

        /// <summary> Получить DBSet по указанной сущности (по-умолчанию только актуальные) </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <returns>Набор сущностей</returns>
        public IQueryable<EntityType> Set<EntityType>() where EntityType : class, IEntityBase
        {
            return _context.Set<EntityType>();
        }

        /// <summary> Получить NoTrackable set по указанной сущности </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <returns>Набор неотслеживаемых сущностей</returns>
        public IQueryable<EntityType> SetNQ<EntityType>() where EntityType : class, IEntityBase
        {
            return Set<EntityType>().AsNoTracking();
        }

        /// <summary> Получить первую сущность по значению ключевого поля, искать среди всех записей </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="condition">Условие</param>
        /// <returns>Найденная сущность или null</returns>
        public EntityType Find<EntityType>(Expression<Func<EntityType, bool>> condition) where EntityType : class, IEntityBase
        {
            return Set<EntityType>().Where(condition).FirstOrDefault();
        }

        /// <summary> Получить первую сущность по значению ключевого поля, искать среди всех записей, асинхронно </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="condition">Условие</param>
        /// <param name="token">Токен остановки</param>
        /// <returns>Найденная сущность или null</returns>
        public async Task<EntityType> FindAsync<EntityType>(Expression<Func<EntityType, bool>> condition, CancellationToken token = default) where EntityType : class, IEntityBase
        {
            return await Set<EntityType>().Where(condition).FirstOrDefaultAsync(token);
        }

        /// <summary> Получить первую сущность по значению ключевого поля, искать среди всех записей (неотслеживаемая) </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="condition">Условие</param>
        /// <returns>Найденная сущность или null</returns>
        public EntityType FindNQ<EntityType>(System.Linq.Expressions.Expression<Func<EntityType, bool>> condition) where EntityType : class, IEntityBase
        {
            return SetNQ<EntityType>().Where(condition).FirstOrDefault();
        }

        /// <summary> Получить первую сущность по значению ключевого поля, искать среди всех записей (неотслеживаемая), асинхронно </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="condition">Условие</param>
        /// <param name="token">Токен остановки</param>
        /// <returns>Найденная сущность или null</returns>
        public async Task<EntityType> FindNQAsync<EntityType>(Expression<Func<EntityType, bool>> condition, CancellationToken token = default) where EntityType : class, IEntityBase
        {
            return await SetNQ<EntityType>().Where(condition).FirstOrDefaultAsync(token);
        }

        /// <summary> Добавить новую запись сущности в контекст </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="saveChanges">Сразу сохранить</param>
        public void Add<EntityType>(EntityType entity, bool saveChanges = true) where EntityType : class, IEntityBase
        {
            CheckExtendEntityGuid(entity);
            SetPropertyValue(entity, "DateCreate", DateTime.Now);
            _context.Set<EntityType>().Add(entity);
            if (saveChanges)
                SaveChanges();
        }

        /// <inheritdoc />
        public async Task AddAsync<EntityType>(EntityType entity, bool saveChanges = true, CancellationToken token = default) where EntityType : class, IEntityBase
        {
            CheckExtendEntityGuid(entity);
            SetPropertyValue(entity, "DateCreate", DateTime.Now);
            _context.Set<EntityType>().Add(entity);
            if (saveChanges)
                await SaveChangesAsync(token);
        }

        /// <summary> Метод прикрепляет объект к контексту.
        /// Если существует прикрепленный объект того же типа с тем же Id, то прикрепление не происходит. </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <returns>entity или найденный объект</returns>
        public EntityType GetOrAttach<EntityType>(EntityType entity) where EntityType : class, IEntityBase
        {
            var localEntity = _context.Set<EntityType>().Local.FirstOrDefault(x => GetEntityKey(x) == GetEntityKey(entity))
                              ?? _context.Set<EntityType>().Attach(entity).Entity;
            return localEntity;
        }

        /// <summary> Поменять статус сущности в контексте </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="state">Новый статус</param>
        public void StateChange<EntityType>(EntityType entity, EntityState state) where EntityType : class, IEntityBase
        {
            _context.Entry(entity).State = state;
        }

        /// <summary> Обновить сущность в контексте </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="saveChanges">Сохранить контекст</param>
        /// <param name="updateDataChange">обновить дату изменения</param>
        public void Update<EntityType>(EntityType entity, bool saveChanges = true, bool updateDataChange = true) where EntityType : class, IEntityBase
        {
            CheckExtendEntityGuid(entity);
            if (updateDataChange) SetPropertyValue(entity, "DateChange", DateTime.Now);
            _context.Entry(entity).State = EntityState.Modified;
            if (saveChanges)
                SaveChanges();
        }

        /// <inheritdoc />
        public async Task UpdateAsync<EntityType>(EntityType entity, bool saveChanges = true, bool updateDataChange = true, CancellationToken token = default) where EntityType : class, IEntityBase
        {
            CheckExtendEntityGuid(entity);
            if (updateDataChange) SetPropertyValue(entity, "DateChange", DateTime.Now);
            _context.Entry(entity).State = EntityState.Modified;
            if (saveChanges)
                await SaveChangesAsync(token);
        }

        /// <summary> Добавить или обновить сущность в контексте (определяется на основании первичных ключей) </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="saveChanges">Сохранить контекст</param>
        /// <param name="updateDateChange">Обновлять дату изменения записи</param>
        public void SaveOrUpdate<EntityType>(EntityType entity, bool saveChanges = true, bool updateDateChange = true) where EntityType : class, IEntityBase
        {
            if (IsPrimaryKeyEmpty(entity))
            {
                Add(entity, saveChanges);
            }
            else
            {
                Update(entity, saveChanges, updateDateChange);
            }
        }

        /// <inheritdoc />
        public async Task SaveOrUpdateAsync<EntityType>(EntityType entity, bool saveChanges, bool updateDateChange, CancellationToken token = default) where EntityType : class, IEntityBase
        {
            if (IsPrimaryKeyEmpty(entity))
            {
                await AddAsync(entity, saveChanges, token);
            }
            else
            {
                await UpdateAsync(entity, saveChanges, updateDateChange, token);
            }
        }


        /// <summary> Обновить статус свойства сущности в контексте </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <typeparam name="TProperty">Тип свойства</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="expr">Лямбда-выражение</param>
        public void UpdateProperty<EntityType, TProperty>(EntityType entity, Expression<Func<EntityType, TProperty>> expr) where EntityType : class, IEntityBase
        {
            _context.Entry(entity).Property(expr).IsModified = true;
        }


        /// <summary> Удалить сущность (перенести в архив). Если в сущности есть поле IsArchive, будет выставлено IsArchive = 1, 
        /// иначе сущность будет удалена </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        public void Delete<EntityType>(EntityType entity) where EntityType : class, IEntityBase
        {
            if (typeof(EntityType).GetProperty("IsArchive") == null)
            {
                _context.Entry(entity).State = EntityState.Deleted;
            }
            else
            {
                SetPropertyValue(entity, "DateChange", DateTime.Now);
                SetPropertyValue(entity, "IsArchive", true);
                _context.Entry(entity).State = EntityState.Modified;
            }
            SaveChanges();
        }

        /// <inheritdoc />
        public async Task DeleteAsync<EntityType>(EntityType entity, CancellationToken token = default) where EntityType : class, IEntityBase
        {
            if (typeof(EntityType).GetProperty("IsArchive") == null)
            {
                _context.Entry(entity).State = EntityState.Deleted;
            }
            else
            {
                SetPropertyValue(entity, "DateChange", DateTime.Now);
                SetPropertyValue(entity, "IsArchive", true);
                _context.Entry(entity).State = EntityState.Modified;
            }
            await SaveChangesAsync(token);
        }

        /// <summary> Удалить список сущностей. Если в сущности есть поле IsArchive, будет выставлено IsArchive = 1,  иначе сущность будет удалена </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entities">Список объектов сущностей</param>
        public void Delete<EntityType>(IEnumerable<EntityType> entities) where EntityType : class, IEntityBase
        {
            var dbSet = _context.Set<EntityType>();
            bool hasIsArchiveProp = typeof(EntityType).GetProperty("IsArchive") != null;
            foreach (var entity in entities)
            {
                if (hasIsArchiveProp)
                {
                    SetPropertyValue(entity, "DateChange", DateTime.Now);
                    SetPropertyValue(entity, "IsArchive", true);
                    _context.Entry(entity).State = EntityState.Modified;
                }
                else
                {
                    dbSet.Attach(entity);
                    _context.Entry(entity).State = EntityState.Deleted;
                }
            }
            SaveChanges();
        }

        /// <inheritdoc />
        public async Task DeleteAsync<EntityType>(IEnumerable<EntityType> entities, CancellationToken token = default) where EntityType : class, IEntityBase
        {
            var dbSet = _context.Set<EntityType>();
            bool hasIsArchiveProp = typeof(EntityType).GetProperty("IsArchive") != null;
            foreach (var entity in entities)
            {
                if (hasIsArchiveProp)
                {
                    SetPropertyValue(entity, "DateChange", DateTime.Now);
                    SetPropertyValue(entity, "IsArchive", true);
                    _context.Entry(entity).State = EntityState.Modified;
                }
                else
                {
                    dbSet.Attach(entity);
                    _context.Entry(entity).State = EntityState.Deleted;
                }
            }
            await SaveChangesAsync(token);
        }

        /// <summary> Присоединить сущность к контексту </summary>
        /// <typeparam name="EntityType"></typeparam>
        /// <param name="entity"></param>
        /// <returns>Присоединенная сущность</returns>
        public EntityType Attach<EntityType>(EntityType entity) where EntityType : class, IEntityBase
        {
            return _context.Set<EntityType>().Attach(entity).Entity;
        }

        /// <summary> Отсоединить сущность от контекста </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        public void Detach<EntityType>(EntityType entity) where EntityType : class, IEntityBase
        {
            _context.Entry(entity).State = EntityState.Detached;
        }

        /// <summary> Сохранить изменения контекста </summary>
        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync(CancellationToken token = default)
        {
            await _context.SaveChangesAsync(token);
        }

        /// <summary> Выполнить сырой SQL запрос возвращающий данные </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров</param>
        public IList<T> SqlQuery<T>(string query, params object[] parameters)
            => _context.SqlQuery<T>(query, parameters);

        /// <summary> Выполнить сырой SQL запрос возвращающий данные, асинхронно </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров</param>
        /// <param name="token">Токен прерывания запроса</param>
        /// <returns>Результаты запроса списком</returns>
        public async Task<IList<T>> SqlQueryAsync<T>(string query, CancellationToken token = default, params object[] parameters)
            => await _context.SqlQueryAsync<T>(query, token, parameters);


        /// <summary> Выполнить сырой SQL запрос не возвращающий данных </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров, которые автоматически конвертируются
        /// в параметры типа DbParameter с именами вида @p0, @p1 и т.д.</param>
        /// <returns>Количество затронутых записей</returns>
        public int ExecuteSqlCommand(string query, params object[] parameters)
            => _context.ExecuteSqlCommand(query, parameters);

        /// <summary> Выполнить сырой SQL запрос возвращающий данные, асинхронно </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров</param>
        /// <param name="token">Токен прерывания запроса</param>
        /// <returns>Количество затронутых записей</returns>
        public async Task<int> ExecuteSqlCommandAsync(string query, IEnumerable<object> parameters, CancellationToken token = default)
            => await _context.ExecuteSqlCommandAsync(query, parameters ?? Enumerable.Empty<object>(), token);

        /// <summary> Выполнить сырой SQL запрос возвращающий данные, асинхронно </summary>
        /// <param name="query">Текст запроса</param>
        /// <param name="parameters">Список параметров</param>
        /// <returns>Количество затронутых записей</returns>
        public async Task<int> ExecuteSqlCommandAsync(string query, params object[] parameters)
            => await _context.ExecuteSqlCommandAsync(query, parameters);

        #endregion CRUD

        #region Transaction

        /// <summary> Запустить ReadCommitted транзакцию </summary>
        /// <returns>Транзакция</returns>
        public TransactionScope GetTransactionScope()
        {
            return GetTransactionScope(IsolationLevel.ReadCommitted);
        }

        /// <summary> Запустить транзакцию с указанным уровнем изоляции </summary>
        /// <param name="isolationLevel">Уровень изоляции транзакции</param>
        /// <returns>Транзакция</returns>
        public TransactionScope GetTransactionScope(IsolationLevel isolationLevel)
        {
            return new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = isolationLevel });
        }

        /// <summary> Запуск транзакции в базе данных </summary>
        public IDbContextTransaction BeginTransaction() => _context.Database.BeginTransaction();

        /// <summary> Подтвердить транзакцию </summary>
        public void CommitTransaction() => _context.Database.CommitTransaction();

        /// <summary> Откатить транзакцию </summary>
        public void RollbackTransaction() => _context.Database.RollbackTransaction();


        /// <summary> Отменить транзакцию в БД с откатом изменений в отслеживаемых сущностях </summary>
        /// <param name="allChanges">Отменить изменения в отслеживаемых сущностях</param>
        public void RollbackTransaction(bool allChanges)
        {
            RollbackTransaction();
            if (!allChanges) return;
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                entry.State = EntityState.Detached;
            }
        }

        #endregion Transaction

        #region Collections

        /// <summary> Присоединить коллекцию к контексту </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="collection">Коллекция сущностей</param>
        /// <returns>Присоединенную к контексту коллекцию</returns>
        public ICollection<EntityType> GetOrAttachCollection<EntityType>(ICollection<EntityType> collection) where EntityType : class, IEntityBase
        {
            var result = new List<EntityType>();
            foreach (var element in collection)
            {
                result.Add(GetOrAttach(element));
            }
            return result;
        }

        /// <summary> Скопировать одну сущность в другую </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="sourceEntity">Исходный объект</param>
        /// <param name="destinationEntity">Конечный объект</param>
        public void CopyDtoToEntity<EntityType>(EntityType sourceEntity, EntityType destinationEntity) where EntityType : class, IEntityBase
        {
            var srcProperties = sourceEntity.GetType().GetProperties();
            var dstProperties = destinationEntity.GetType().GetProperties();
            foreach (var srcProperty in srcProperties)
            {
                var dstProperty =
                    dstProperties.FirstOrDefault(x => x.Name == srcProperty.Name && x.MemberType == srcProperty.MemberType);
                if (dstProperty != null)
                {
                    if (typeof(IEntityBase).IsAssignableFrom(dstProperty.PropertyType))
                    {
                        GetType().GetMethod("AttachToSet", BindingFlags.Instance | BindingFlags.NonPublic)
                            .MakeGenericMethod(dstProperty.PropertyType).Invoke(this, new object[] { sourceEntity });
                        dstProperty.SetValue(destinationEntity, srcProperty.GetValue(sourceEntity));
                    }
                    else if (typeof(IEnumerable<IEntityBase>).IsAssignableFrom(dstProperty.PropertyType))
                    {
                        var srcRelations = srcProperty.GetValue(sourceEntity);
                        var dstRelations = dstProperty.GetValue(destinationEntity);
                        var genericArg = dstProperty.PropertyType.GenericTypeArguments.First();
                        if (dstRelations == null)
                            dstProperty.SetValue(destinationEntity, Activator.CreateInstance(genericArg));

                        dstRelations?.GetType().GetMethod("Clear").Invoke(dstRelations, null);
                        if (srcRelations == null)
                            continue;
                        var attached = GetType()
                            .GetMethod("GetOrAttachCollection")
                            .MakeGenericMethod(genericArg)
                            .Invoke(this, new[] { srcRelations });
                        dstRelations?.GetType().GetMethod("AddRange").Invoke(dstRelations, new[] { attached });
                    }
                    else
                    {
                        dstProperty.SetValue(destinationEntity, srcProperty.GetValue(sourceEntity));
                    }
                }
            }
        }

        /// <summary> Скопировать коллекцию </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="sourceCollection">Исходная коллекция</param>
        /// <param name="destinationCollection">Конечная коллекция</param>
        public void CopyCollection<EntityType>(ICollection<EntityType> sourceCollection, ICollection<EntityType> destinationCollection) where EntityType : class, IEntityBase
        {
            if (destinationCollection == null)
                destinationCollection = new List<EntityType>();
            destinationCollection.Clear();
            if (sourceCollection == null)
                return;
            foreach (var item in GetOrAttachCollection(sourceCollection))
            {
                destinationCollection.Add(item);
            }
        }

        /// <summary> Скопировать коллекцию из начальной в конечную </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="sourceCollection">Исходная коллекция</param>
        /// <param name="destinationCollection">Конечная коллекция</param>
        /// <param name="mergFunc">Метод обработки одинаковых элементов</param>
        /// <returns>Обновленная конечная коллекция</returns>
        public ICollection<EntityType> MergeCollection<EntityType>(ICollection<EntityType> sourceCollection, ICollection<EntityType> destinationCollection,
            Action<EntityType, EntityType> mergFunc = null) where EntityType : class, IEntityBase
        {
            return MerdgeCollectionStatic(sourceCollection, destinationCollection, mergFunc);
        }

        /// <summary> Скопировать коллекцию из начальной в конечную </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="sourceCollection">Исходная коллекция</param>
        /// <param name="destinationCollection">Конечная коллекция</param>
        /// <param name="mergFunc">Метод объединения одинаковых элементов</param>
        /// <returns>Обновленная конечная коллекция</returns>
        public static ICollection<EntityType> MerdgeCollectionStatic<EntityType>(ICollection<EntityType> sourceCollection,
            ICollection<EntityType> destinationCollection,
            Action<EntityType, EntityType> mergFunc = null) where EntityType : class, IEntityBase
        {
            if (destinationCollection == null)
            {
                return sourceCollection ?? new Collection<EntityType>();
            }

            if (sourceCollection == null || !sourceCollection.Any())
            {
                destinationCollection.Clear();
                return destinationCollection;
            }

            var idSource = sourceCollection.Select(s => GetEntityKey(s)).ToArray();
            var idDest = destinationCollection.Select(s => GetEntityKey(s)).ToArray();
            var idToMerdge = idSource.Intersect(idDest);

            var destDelete = destinationCollection.Where(d => !idSource.Contains(GetEntityKey(d))).ToList();
            foreach (var item in destDelete)
            {
                destinationCollection.Remove(item);
            }

            var sourceToAdd = sourceCollection.Where(d => !idDest.Contains(GetEntityKey(d))).ToList();
            foreach (var item in sourceToAdd)
            {
                destinationCollection.Add(item);
            }

            if (mergFunc != null)
            {
                foreach (var item in idToMerdge)
                {
                    var id = item;
                    var itemDest = destinationCollection.FirstOrDefault(d => GetEntityKey(d) == id);
                    var itemSource = sourceCollection.FirstOrDefault(d => GetEntityKey(d) == id);
                    mergFunc(itemSource, itemDest);
                }
            }
            return destinationCollection;
        }

        #endregion Collections

        #region Private methods

        /// <summary> Установить свойство объекта </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        /// <param name="propertyName">Название свойства</param>
        /// <param name="value">Новое значение</param>
        private void SetPropertyValue<EntityType>(EntityType entity, string propertyName, object value)
        {
            Type entityType = typeof(EntityType);
            PropertyInfo property = entityType.GetProperty(propertyName);
            if (property == null)
                return;
            property.SetValue(entity, value);
        }

        /// <summary> Получить Primary key value с сущности (считаем, что поле отмеченное [Key] одно) </summary>
        /// <typeparam name="T">Тип сущности</typeparam>
        /// <param name="entity">Сущность</param>
        /// <returns>Значение ключевого поля</returns>
        private static object GetEntityKey<T>(T entity)
        {
            Type entityType = typeof(T);
            PropertyInfo[] props = entityType.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                var key = prop.GetCustomAttribute(typeof(KeyAttribute), true);
                if (key != null)
                    return prop.GetValue(entity);
            }
            return null;
        }

        /// <summary> Проверить первичные ключи на пустое значение </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Сущность</param>
        /// <returns>Да, если пустые</returns>
        private bool IsPrimaryKeyEmpty<EntityType>(EntityType entity) where EntityType : class, IEntityBase
        {
            Type entityType = typeof(EntityType);
            PropertyInfo[] props = entityType.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                var key = prop.GetCustomAttribute(typeof(KeyAttribute), true);
                if (key != null)
                {
                    object value = prop.GetValue(entity);
                    if (value == null)
                        return true;
                    object defaultValue = GetDefaultValue(prop.PropertyType);
                    return value.Equals(defaultValue);
                }
            }
            return false;
        }

        /// <summary> Получить дефолтное значение для типа в рантайм </summary>
        /// <param name="type">Тип</param>
        /// <returns>Дефолтное значение</returns>
        private object GetDefaultValue(Type type)
        {
            // If no Type was supplied, if the Type was a reference type, or if the Type was a System.Void, return null
            if (type == null || !type.IsValueType || type == typeof(void))
                return null;

            // If the supplied Type has generic parameters, its default value cannot be determined
            if (type.ContainsGenericParameters)
                throw new ArgumentException($"[{MethodInfo.GetCurrentMethod()}] Error:\n\nThe supplied value type <{type}> contains generic parameters, so the default value cannot be retrieved");

            // If the Type is a primitive type, or if it is another publicly-visible value type (i.e. struct/enum), return a 
            //  default instance of the value type
            if (type.IsPrimitive || !type.IsNotPublic)
            {
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"[{MethodInfo.GetCurrentMethod()}] Error:\n\nThe Activator.CreateInstance method could not create a default instance of the supplied value type <{type}>", ex);
                }
            }

            // Fail with exception
            throw new ArgumentException($"[{MethodInfo.GetCurrentMethod()}] Error:\n\nThe supplied value type <{type}> is not a publicly-visible type, so the default value cannot be retrieved");
        }

        /// <summary> Проверить наличие свойства ExtendEntityGuid и установить его, если оно есть </summary>
        /// <typeparam name="EntityType">Тип сущности</typeparam>
        /// <param name="entity">Объект сущности</param>
        private void CheckExtendEntityGuid<EntityType>(EntityType entity) where EntityType : class, IEntityBase
        {
            Type entityType = typeof(EntityType);
            PropertyInfo exEnGuidProp = entityType.GetProperty("ExtendEntityGuid");
            if (exEnGuidProp != null)
            {
                //try { extendEntityGuid = Guid.Parse(exEnIdProp.GetValue(entity)); } catch { };
                var v = exEnGuidProp.GetValue(entity)?.ToString();
                if (string.IsNullOrEmpty(v) || !Guid.TryParse(exEnGuidProp.GetValue(entity).ToString(), out Guid extendEntityGuid))
                {
                    if (exEnGuidProp.PropertyType == typeof(string))
                        exEnGuidProp.SetValue(entity, Guid.NewGuid().ToString());
                    else if (exEnGuidProp.PropertyType == typeof(Guid))
                        exEnGuidProp.SetValue(entity, Guid.NewGuid());
                    else
                        Debug.Assert(false, "Тип колонки ExtendEntityGuid не соответствует ожидаемому");
                }
            }
        }

        /// <summary> Присоединить объект к контексту </summary>
        /// <typeparam name="EntityType">Тип объекта</typeparam>
        /// <param name="entity">объект</param>
        protected void AttachToSet<EntityType>(EntityType entity) where EntityType : class
        {
            var et = _context.Set<EntityType>();
            et.Local.Clear();
            et.Attach(entity);
        }

        #endregion Private methods

        #region IDisposable

        /// <summary> Деструктор </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary> Деструктор </summary>
        /// <param name="disposing">Необходимо освободить память</param>
        public virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
            }
            disposed = true;
        }

        #endregion IDisposable
    }
}
