﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using HeatWave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace HeatWave.Tests
{
    [TestClass()]
    public class TempRepoDBTests
    {
        private const bool useDatabase = true;
        private static DbContextOptions<TempDBContext> _options;

        [ClassInitialize]

        public static void InitializeOnce(TestContext context)
        {
            if (useDatabase)
            {
                var optionsBuilder = new DbContextOptionsBuilder<TempDBContext>();
                optionsBuilder.UseSqlServer(DBSecret.ConnectionStringSimply);
                _options = optionsBuilder.Options;

                using var _tempDBContext = new TempDBContext(_options);
                _tempDBContext.Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.TemperatureMeasurement");
            }
        }

        // Tømmer databasen før hver test
        [TestInitialize]
        public void TestInitialize()
        {
            using var _tempDBContext = new TempDBContext(_options);
            _tempDBContext.Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.TemperatureMeasurement");
        }

        // Tester GetTempList metoden
        [TestMethod()]
        public void GetTempListTest()
        {
            // Arrange - opretter en TempRepoDB og en TempDBContext.
            using var _tempDBContext = new TempDBContext(_options);
            var _repoDB = new TempRepoDB(_tempDBContext);

            // Act - kalder GetTempList metoden
            IEnumerable<TemperatureMeasurement> result = _repoDB.GetTempList();
            Assert.IsTrue(result.Count() == 0);

            // Assert - tester at der ikke er nogen elementer i listen
            TemperatureMeasurement temperatureMeasurement = new TemperatureMeasurement { Date = new DateTime(2021, 5, 10, 8, 38, 0), InDoorTemperature = 11, OutDoorTemperature = 12 };
            _repoDB.Add(temperatureMeasurement);

            // Act - kalder GetTempList metoden igen
            result = _repoDB.GetTempList();

            // Assert - tester at der er et element i listen og at det er det rigtige elementer
            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.First().InDoorTemperature == 11);
            Assert.IsTrue(result.First().OutDoorTemperature == 12);
            Assert.IsTrue(result.First().Date == new DateTime(2021, 5, 10, 8, 38, 0));
        }

        [TestMethod()]
        public void GetIDTest()
        {
            using var _tempDBContext = new TempDBContext(_options);
            var _repoDB = new TempRepoDB(_tempDBContext);

            // Act - adder et element til listen og kalder GetID metoden
            TemperatureMeasurement tmAdd = _repoDB.Add(new TemperatureMeasurement { Date = new DateTime(2022, 6, 11, 7, 32, 0), InDoorTemperature = 9, OutDoorTemperature = 8 });
            TemperatureMeasurement? tm = _repoDB.GetID(tmAdd.Id);

            // Assert - tester at der er et element i listen og at det er det rigtige elementer
            Assert.IsNotNull(tm);
            Assert.IsTrue(tm.Id == tmAdd.Id);
            Assert.IsTrue(tm.InDoorTemperature == 9);
            Assert.IsTrue(tm.OutDoorTemperature == 8);
            Assert.IsTrue(tm.Date == new DateTime(2022, 6, 11, 7, 32, 0));
        }

        [TestMethod()]
        public void AddTest()
        {
            using var _tempDBContext = new TempDBContext(_options);
            var _repoDB = new TempRepoDB(_tempDBContext);

            // Act - adder et element til listen
            TemperatureMeasurement temperatureMeasurementMinus = new TemperatureMeasurement { Date = new DateTime(2021, 5, 10, 8, 38, 0), InDoorTemperature = -17, OutDoorTemperature = -17 };
            _repoDB.Add(temperatureMeasurementMinus);

            // Assert - tester at der er et element i listen og at det er det rigtige elementer
            Assert.IsTrue(temperatureMeasurementMinus.InDoorTemperature < 0);
            Assert.IsTrue(temperatureMeasurementMinus.OutDoorTemperature < 0);
            Assert.IsTrue(temperatureMeasurementMinus.Id > 0);
            IEnumerable<TemperatureMeasurement> result = _repoDB.GetTempList();
            Assert.IsTrue(result.Count() == 1);
        }

        [TestMethod()]
        public void DeleteTest()
        {
            using var _tempDBContext = new TempDBContext(_options);
            var _repoDB = new TempRepoDB(_tempDBContext);

            // Act - adder et element til listen og sletter det igen
            TemperatureMeasurement temperatureMeasurement = new TemperatureMeasurement { Date = new DateTime(2021, 5, 10, 8, 38, 0), InDoorTemperature = 11, OutDoorTemperature = 12 };
            _repoDB.Add(temperatureMeasurement);
            IEnumerable<TemperatureMeasurement> result = _repoDB.GetTempList();
            Assert.IsTrue(result.Count() == 1);
            _repoDB.Delete(temperatureMeasurement.Id);
            result = _repoDB.GetTempList();
            Assert.IsTrue(result.Count() == 0);
        }

        [TestMethod()]
        public void UpdateTest()
        {
            using var _tempDBContext = new TempDBContext(_options);
            var _repoDB = new TempRepoDB(_tempDBContext);

            // Act - adder et element til listen og updater det igen med nye værdier
            TemperatureMeasurement tM = new TemperatureMeasurement { Date = new DateTime(2021, 5, 10, 8, 38, 0), InDoorTemperature = 11, OutDoorTemperature = 12 };
            _repoDB.Add(tM);
            var result = _repoDB.GetTempList();
            Assert.IsTrue(result.Count() == 1);
            TemperatureMeasurement tMUpdated = new TemperatureMeasurement { Id = tM.Id, Date = new DateTime(2021, 5, 10, 8, 38, 0), InDoorTemperature = 13, OutDoorTemperature = 14 };
            _repoDB.Update(tM.Id, tMUpdated);
            result = _repoDB.GetTempList();
            Assert.IsTrue(result.Count() == 1);
            Assert.IsTrue(result.First().InDoorTemperature == 13);
            Assert.IsTrue(result.First().OutDoorTemperature == 14);
        }


    }
}
