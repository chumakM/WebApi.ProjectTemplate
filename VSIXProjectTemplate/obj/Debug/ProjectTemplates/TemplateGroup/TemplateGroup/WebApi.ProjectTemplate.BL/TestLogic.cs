using Microsoft.Extensions.Logging;
using System;
using $ext_safeprojectname$.DAL.Repositories;

namespace $ext_safeprojectname$.BL
{
    /// <summary>
    /// Слой бизнес логики для теста
    /// </summary>
    public class TestLogic : BaseLogic<TestLogic>, ITestLogic
    {
        /// <summary>
        /// Работа с тестовой базой
        /// </summary>
        private readonly ITestRepository _testRepository;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="testRepository"></param>
        /// <param name="logger"></param>
        public TestLogic(ITestRepository testRepository, ILogger<TestLogic> logger) : base(logger)
        {
            _testRepository = testRepository;
        }
        /// <summary>
        /// Тестовый метод логики
        /// </summary>
        /// <returns></returns>
        public string DataFromTest()
        {
            _logger.LogInformation("Test Method Call");
            return _testRepository.DataFromTest();
        }
    }
}
