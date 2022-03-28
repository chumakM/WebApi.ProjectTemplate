using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using $ext_safeprojectname$.DAL.Model;

namespace $ext_safeprojectname$.DAL.Repositories
{
    /// <summary>
    /// Слой доступа к данным
    /// </summary>
    public class TestRepository : ITestRepository
    {
        private readonly IDataProviderFactory _providerFactory;
        public TestRepository(IDataProviderFactory dataProviderFactory)
        {
            _providerFactory = dataProviderFactory;
        }
        /// <summary>
        /// Получение тестовых данных
        /// </summary>
        /// <returns></returns>
        public string DataFromTest()
        {
            // Создаем провайдер для доступа к тестовой базе, с помощью Enum указываем провайдер какой базы мы будем использовтаь
            using (var testProvider = _providerFactory.CreateProvider(DatabaseProviderEnum.test))
            {
                return testProvider.Set<TestTable>().FirstOrDefault().Name;
            }
        }
    }
}
