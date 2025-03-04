using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Hotel.src.Application.Services;
using Hotel.src.Core.Entities;
using Hotel.src.Core.Interfaces.IRepositories;

namespace Hotel.Tests
{
    [TestFixture]
    public class OccupancyReportServiceTests
    {
        private Mock<IOccupancyRepository> _mockRepository;
        private OccupancyReportService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IOccupancyRepository>();
            _service = new OccupancyReportService(_mockRepository.Object);
        }

        [Test]
        public void EmptyDatabase_NoRooms_ReturnsZeroOccupancy()
        {
            // Arrange
            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddDays(5);

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(0);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(0);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(0);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate))
                           .Returns(new List<(DateTime, int)>());

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);

            // Assert
            Assert.AreEqual(0, report.TotalRooms, "TotalRooms debe ser 0.");
            Assert.AreEqual(0, report.TotalIncome, "TotalIncome debe ser 0.");
            Assert.AreEqual(0, report.OccupancyRate, "La tasa de ocupación global debe ser 0.");
            Assert.IsEmpty(report.OccupancyByType, "La lista de ocupación por tipo debe estar vacía.");
            Assert.IsEmpty(report.DailyOccupancy, "La lista de ocupación diaria debe estar vacía.");
        }

        [Test]
        public void FullPeriodOccupancy_AllRoomsOccupied_Returns100Percent()
        {
            // Arrange
            DateTime startDate = new DateTime(2024, 1, 1);
            DateTime endDate = new DateTime(2024, 1, 7); // 7 días
            int totalRooms = 10;
            int daysInPeriod = (endDate - startDate).Days + 1; // 7
            int totalOccupiedDays = totalRooms * daysInPeriod; // 10 * 7 = 70

            // Para ocupación por tipo: suponemos dos tipos, cada uno con ocupación completa.
            var occupancyByTypeData = new List<(string RoomType, int ReservationsCount, int OccupiedDays, int TotalRoomsType)>
            {
                ("SIMPLE", 5, 40, 5), // 5*7=35, pero aquí lo dejamos en 40 para ilustrar (podría venir de varias reservas)
                ("DOBLE", 5, 30, 5)   // 5*7=35, usamos 30 para este ejemplo
            };

            // Daily occupancy: cada día todas las habitaciones ocupadas
            var dailyOccupancyData = Enumerable.Range(0, daysInPeriod)
                                               .Select(i => (startDate.AddDays(i), totalRooms))
                                               .ToList();

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(5000);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(totalOccupiedDays);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate)).Returns(occupancyByTypeData);
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate)).Returns(dailyOccupancyData);

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);

            // Assert
            Assert.AreEqual(1.0, report.OccupancyRate, 0.0001, "La tasa global de ocupación debe ser 100%.");
            Assert.AreEqual(daysInPeriod, report.DailyOccupancy.Count, "Debe haber un reporte diario por cada día.");
            foreach (var daily in report.DailyOccupancy)
            {
                Assert.AreEqual(totalRooms, daily.OccupiedRooms, "Cada día debe tener todas las habitaciones ocupadas.");
                Assert.AreEqual(1.0m, daily.OccupancyRateDay, "La tasa diaria debe ser 100%.");
            }
        }

        [Test]
        public void DateBoundaryValidation_ReservationSpansReportRange_ReturnsOccupiedDaysWithinRange()
        {
            // Arrange
            // Reporte: 2024-01-05 al 2024-01-10 (6 días)
            DateTime startDate = new DateTime(2024, 1, 5);
            DateTime endDate = new DateTime(2024, 1, 10);
            int totalRooms = 5;
            int daysInPeriod = (endDate - startDate).Days + 1; // 6 días
            // Reserva real: 2024-01-03 a 2024-01-07, solo se cuentan los días dentro del rango: 2024-01-05 a 2024-01-07 = 3 días
            int totalOccupiedDays = 3;

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(1000);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(totalOccupiedDays);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate))
                           .Returns(new List<(DateTime, int)>());

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);
            double expectedRate = totalRooms > 0 ? (double)totalOccupiedDays / (totalRooms * daysInPeriod) : 0;

            // Assert
            Assert.AreEqual(expectedRate, report.OccupancyRate, 0.0001, "La tasa global de ocupación debe considerar solo los días dentro del rango.");
        }

        [Test]
        public void RoomTypeMapping_ReturnsCorrectRoomTypeLabels()
        {
            // Arrange
            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddDays(3);
            int totalRooms = 20;
            int totalOccupiedDays = 50;

            var occupancyByTypeData = new List<(string RoomType, int ReservationsCount, int OccupiedDays, int TotalRoomsType)>
            {
                ("SIMPLE", 3, 10, 8),
                ("DOBLE", 2, 15, 6),
                ("SUITE", 1, 5, 6)
            };

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(3000);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(totalOccupiedDays);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate)).Returns(occupancyByTypeData);
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate))
                           .Returns(new List<(DateTime, int)>());

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);

            // Assert
            Assert.AreEqual(3, report.OccupancyByType.Count, "Debe haber tres tipos de habitación.");
            CollectionAssert.AreEquivalent(new[] { "SIMPLE", "DOBLE", "SUITE" },
                report.OccupancyByType.Select(r => r.RoomType),
                "Los tipos de habitación deben ser SIMPLE, DOBLE y SUITE.");
        }

        [Test]
        public void SingleDayReport_ReportForOneDay_ReturnsOneDailyEntry()
        {
            // Arrange
            DateTime startDate = new DateTime(2024, 1, 1);
            DateTime endDate = startDate; // mismo día
            int totalRooms = 5;
            int totalOccupiedDays = 3; // ejemplo

            var dailyOccupancyData = new List<(DateTime, int)>
            {
                (startDate, 3)
            };

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(800);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(totalOccupiedDays);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate)).Returns(dailyOccupancyData);

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);

            // Assert
            Assert.AreEqual(1, report.DailyOccupancy.Count, "Debe haber exactamente un reporte diario para un solo día.");
            Assert.AreEqual(startDate, report.DailyOccupancy[0].Day, "El reporte diario debe corresponder al día de la consulta.");
        }

        [Test]
        public void ZeroTotalRooms_WithReservations_ReturnsZeroOccupancyRates()
        {
            // Arrange
            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddDays(3);
            // Aunque existan reservas, si GetTotalRooms retorna 0, las tasas deben ser 0
            int totalRooms = 0;
            int totalOccupiedDays = 10;

            var dailyOccupancyData = new List<(DateTime, int)>
            {
                (startDate, 0),
                (startDate.AddDays(1), 0),
                (startDate.AddDays(2), 0),
                (startDate.AddDays(3), 0)
            };

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(1000);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(totalOccupiedDays);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate)).Returns(dailyOccupancyData);

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);

            // Assert
            Assert.AreEqual(0, report.OccupancyRate, "La tasa global debe ser 0 cuando no hay habitaciones.");
            foreach (var daily in report.DailyOccupancy)
            {
                Assert.AreEqual(0, daily.TotalRooms, "El total de habitaciones en el reporte diario debe ser 0.");
                Assert.AreEqual(0m, daily.OccupancyRateDay, "La tasa diaria debe ser 0 cuando no hay habitaciones.");
            }
        }

        [Test]
        public void OverlappingReservations_ForSameRoom_ReturnsCorrectOccupiedDays()
        {
            // Arrange
            // Para un periodo de 5 días, dos reservas que se solapan en la misma habitación.
            DateTime startDate = new DateTime(2024, 2, 1);
            DateTime endDate = new DateTime(2024, 2, 5); // 5 días
            int totalRooms = 5;
            int daysInPeriod = (endDate - startDate).Days + 1; // 5
            // Aunque existan dos reservas, al solaparse, se cuenta solo una vez la ocupación por día.
            int totalOccupiedDays = 5;

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(1200);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(totalOccupiedDays);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            // Daily occupancy: cada día 1 habitación ocupada (sin duplicar por overlap)
            var dailyOccupancyData = Enumerable.Range(0, daysInPeriod)
                                               .Select(i => (startDate.AddDays(i), 1))
                                               .ToList();
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate)).Returns(dailyOccupancyData);

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);
            double expectedGlobalRate = totalOccupiedDays / (double)(totalRooms * daysInPeriod);

            // Assert
            Assert.AreEqual(expectedGlobalRate, report.OccupancyRate, 0.0001, "La tasa global debe considerar la superposición correctamente.");
        }

        [Test]
        public void MultiRoomReservations_ReservationWithMultipleRooms_ReturnsCorrectOccupiedRoomDays()
        {
            // Arrange
            // Para un periodo de 7 días, una reserva que involucra 3 habitaciones durante 2 días aporta 6 ocupaciones.
            DateTime startDate = new DateTime(2024, 3, 1);
            DateTime endDate = new DateTime(2024, 3, 7); // 7 días
            int totalRooms = 10;
            int daysInPeriod = (endDate - startDate).Days + 1;
            int totalOccupiedDays = 6; // 3 habitaciones * 2 días

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(2500);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(totalOccupiedDays);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            // Daily occupancy: distribuidos en dos días
            var dailyOccupancyData = new List<(DateTime, int)>
            {
                (startDate, 3),
                (startDate.AddDays(1), 3),
                (startDate.AddDays(2), 0),
                (startDate.AddDays(3), 0),
                (startDate.AddDays(4), 0),
                (startDate.AddDays(5), 0),
                (startDate.AddDays(6), 0)
            };
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate)).Returns(dailyOccupancyData);

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);
            double expectedGlobalRate = totalOccupiedDays / (double)(totalRooms * daysInPeriod);

            // Assert
            Assert.AreEqual(expectedGlobalRate, report.OccupancyRate, 0.0001, "La tasa global debe reflejar las reservas multi-room correctamente.");
        }

        [Test]
        public void PartialPeriodOccupancy_RoomOccupiedOnlyPartOfPeriod_ReturnsPartialOccupancyRate()
        {
            // Arrange
            // Periodo de 7 días, una habitación ocupada solo 3 días.
            DateTime startDate = new DateTime(2024, 4, 1);
            DateTime endDate = new DateTime(2024, 4, 7); // 7 días
            int totalRooms = 1;
            int totalOccupiedDays = 3;

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(500);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(totalOccupiedDays);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            // Daily occupancy: 3 días ocupados, 4 no ocupados
            var dailyOccupancyData = new List<(DateTime, int)>
            {
                (startDate, 1),
                (startDate.AddDays(1), 1),
                (startDate.AddDays(2), 1),
                (startDate.AddDays(3), 0),
                (startDate.AddDays(4), 0),
                (startDate.AddDays(5), 0),
                (startDate.AddDays(6), 0)
            };
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate)).Returns(dailyOccupancyData);

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);
            double expectedRate = totalOccupiedDays / (double)(totalRooms * 7);

            // Assert
            Assert.AreEqual(expectedRate, report.OccupancyRate, 0.0001, "La tasa de ocupación parcial debe ser correcta.");
        }

        [Test]
        public void NoDailyOccupancyEntries_ReturnsEmptyDailyListButCorrectGlobalRate()
        {
            // Arrange
            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddDays(4);
            int totalRooms = 8;
            int totalOccupiedDays = 16; // ejemplo

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(1800);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(totalOccupiedDays);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            // Simulamos que GetDailyOccupancy retorna una lista vacía
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate)).Returns(new List<(DateTime, int)>());

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);

            // Assert
            Assert.IsEmpty(report.DailyOccupancy, "La lista de ocupación diaria debe estar vacía.");
            double expectedGlobalRate = totalOccupiedDays / (double)(totalRooms * ((endDate - startDate).Days + 1));
            Assert.AreEqual(expectedGlobalRate, report.OccupancyRate, 0.0001, "La tasa global debe calcularse correctamente aun sin datos diarios.");
        }

        [Test]
        public void RandomOrderDailyOccupancy_OrderIsPreservedAsReturnedByRepository()
        {
            // Arrange
            DateTime startDate = new DateTime(2024, 5, 1);
            DateTime endDate = new DateTime(2024, 5, 4); // 4 días
            int totalRooms = 10;
            int totalOccupiedDays = 20;

            var dailyOccupancyData = new List<(DateTime, int)>
            {
                (new DateTime(2024, 5, 3), 8),
                (new DateTime(2024, 5, 1), 10),
                (new DateTime(2024, 5, 4), 7),
                (new DateTime(2024, 5, 2), 9)
            };

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(4000);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(totalOccupiedDays);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate)).Returns(dailyOccupancyData);

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);

            // Assert
            // Se verifica que el orden de los elementos en DailyOccupancy es el mismo que el de dailyOccupancyData
            CollectionAssert.AreEqual(dailyOccupancyData.Select(d => d.Item1), report.DailyOccupancy.Select(d => d.Day));
        }

        [Test]
        public void HighIncomeCalculation_ReturnsCorrectTotalIncome()
        {
            // Arrange
            DateTime startDate = new DateTime(2024, 6, 1);
            DateTime endDate = new DateTime(2024, 6, 10);
            int totalRooms = 50;
            int totalOccupiedDays = 400;
            double totalIncome = 99999.99;

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(totalIncome);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(totalOccupiedDays);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate))
                           .Returns(new List<(DateTime, int)>());

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);

            // Assert
            Assert.AreEqual(totalIncome, report.TotalIncome, "El total de ingresos debe ser el esperado.");
        }

        [Test]
        public void RoomTypeMultipleEntries_ReturnsEachEntrySeparately()
        {
            // Arrange
            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddDays(2);
            int totalRooms = 15;
            int totalOccupiedDays = 20;

            // Se simulan dos entradas para el mismo tipo de habitación "SIMPLE"
            var occupancyByTypeData = new List<(string RoomType, int ReservationsCount, int OccupiedDays, int TotalRoomsType)>
            {
                ("SIMPLE", 2, 8, 7),
                ("SIMPLE", 1, 4, 3)
            };

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(2200);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(totalOccupiedDays);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate)).Returns(occupancyByTypeData);
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate))
                           .Returns(new List<(DateTime, int)>());

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);

            // Assert
            Assert.AreEqual(2, report.OccupancyByType.Count, "Deben existir dos entradas para el tipo SIMPLE.");
            foreach (var entry in report.OccupancyByType)
            {
                Assert.AreEqual("SIMPLE", entry.RoomType, "El tipo de habitación debe ser SIMPLE.");
            }
        }

        [Test]
        public void DailyOccupancyPercentages_CorrectlyCalculated()
        {
            // Arrange
            DateTime startDate = new DateTime(2024, 7, 1);
            DateTime endDate = new DateTime(2024, 7, 3); // 3 días
            int totalRooms = 4;
            // Supongamos: día 1: 0 ocupadas, día 2: 2 ocupadas, día 3: 4 ocupadas (completa)
            var dailyOccupancyData = new List<(DateTime, int)>
            {
                (startDate, 0),
                (startDate.AddDays(1), 2),
                (startDate.AddDays(2), 4)
            };

            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(1000);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(6);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate)).Returns(dailyOccupancyData);

            // Act
            OccupancyReport report = _service.GenerateOccupancyReport(startDate, endDate);

            // Assert
            Assert.AreEqual(0m, report.DailyOccupancy[0].OccupancyRateDay, "Día 1: Tasa debe ser 0%.");
            Assert.AreEqual((decimal)2 / totalRooms, report.DailyOccupancy[1].OccupancyRateDay, "Día 2: Tasa debe calcularse correctamente.");
            Assert.AreEqual(1.0m, report.DailyOccupancy[2].OccupancyRateDay, "Día 3: Tasa debe ser 100%.");
        }
        [Test]
        public void ExceptionInGetTotalRooms_ThrowsException()
        {
            // Arrange
            DateTime startDate = new DateTime(2024, 8, 1);
            DateTime endDate = new DateTime(2024, 8, 7);
            _mockRepository.Setup(r => r.GetTotalRooms())
                           .Throws(new Exception("Database error in GetTotalRooms"));
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(1000);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(20);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate))
                           .Returns(new List<(DateTime, int)>());

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GenerateOccupancyReport(startDate, endDate));
            Assert.That(ex.Message, Is.EqualTo("Database error in GetTotalRooms"));
        }

        [Test]
        public void ExceptionInGetTotalIncome_ThrowsException()
        {
            // Arrange
            DateTime startDate = new DateTime(2024, 8, 8);
            DateTime endDate = new DateTime(2024, 8, 14);
            int totalRooms = 10;
            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate))
                           .Throws(new Exception("Database error in GetTotalIncome"));
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(30);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate))
                           .Returns(new List<(DateTime, int)>());

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GenerateOccupancyReport(startDate, endDate));
            Assert.That(ex.Message, Is.EqualTo("Database error in GetTotalIncome"));
        }

        [Test]
        public void ExceptionInGetDailyOccupancy_ThrowsException()
        {
            // Arrange
            DateTime startDate = new DateTime(2024, 8, 15);
            DateTime endDate = new DateTime(2024, 8, 20);
            int totalRooms = 8;
            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(2000);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(25);
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns(new List<(string, int, int, int)>());
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate))
                           .Throws(new Exception("Database error in GetDailyOccupancy"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GenerateOccupancyReport(startDate, endDate));
            Assert.That(ex.Message, Is.EqualTo("Database error in GetDailyOccupancy"));
        }

        [Test]
        public void NullOccupancyByType_ThrowsException()
        {
            // Arrange
            DateTime startDate = new DateTime(2024, 8, 21);
            DateTime endDate = new DateTime(2024, 8, 25);
            int totalRooms = 12;
            _mockRepository.Setup(r => r.GetTotalRooms()).Returns(totalRooms);
            _mockRepository.Setup(r => r.GetTotalIncome(startDate, endDate)).Returns(3000);
            _mockRepository.Setup(r => r.GetTotalOccupiedDays(startDate, endDate)).Returns(40);
            // Simulamos que el repositorio retorna null en lugar de una lista
            _mockRepository.Setup(r => r.GetOccupancyByType(startDate, endDate))
                           .Returns((List<(string, int, int, int)>)null);
            _mockRepository.Setup(r => r.GetDailyOccupancy(startDate, endDate))
                           .Returns(new List<(DateTime, int)>());

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => _service.GenerateOccupancyReport(startDate, endDate));
        }

        [Test]
        public void InvalidDateRange_StartDateAfterEndDate_ThrowsException()
        {
            // Arrange
            DateTime startDate = new DateTime(2024, 9, 10);
            DateTime endDate = new DateTime(2024, 9, 5);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _service.GenerateOccupancyReport(startDate, endDate),
                "Debe lanzar excepción si startDate > endDate.");
        }
        

    }
}

