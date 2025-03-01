using Hotel.src.Core.Entities;
using Hotel.src.Core.Enums;
using Hotel.src.Infrastructure.Repositories;
using Hotel.src.Core.Interfaces.IRepository;
using Hotel.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;


namespace HotelTests.Infrastructure.Repositories
{
    class ReservationRepositoryTests
    {
        private Mock<ApplicationDbContext> _mockDbContext;
        private Mock<DbSet<Reservation>> _mockReservationsDbSet;
        private IReservationRepository _reservationRepository;

        [SetUp]
        public void Setup()
        {
            // Configuración del mock para DbSet<Reservation>
            _mockReservationsDbSet = new Mock<DbSet<Reservation>>();

            // Configurar los métodos para simular IQueryable
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Provider).Returns(Enumerable.Empty<Reservation>().AsQueryable().Provider);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Expression).Returns(Enumerable.Empty<Reservation>().AsQueryable().Expression);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.ElementType).Returns(Enumerable.Empty<Reservation>().AsQueryable().ElementType);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.GetEnumerator()).Returns(Enumerable.Empty<Reservation>().GetEnumerator());

            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);

            _reservationRepository = new ReservationRepository(_mockDbContext.Object);
        }

        [Test]
        public void Add_ShouldAddReservationSuccessfully()
        {
            // Arrange
            var newReservation = new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada);

            // Act
            var result = _reservationRepository.Add(newReservation);

            // Assert
            _mockReservationsDbSet.Verify(u => u.Add(newReservation), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(), Times.Once);

            Assert.That(result, Is.EqualTo(newReservation));
        }

        [Test]
        public void GetById_ShouldReturnNull_WhenReservationDoesNotExist()
        {
            // Arrange
            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);

            // Act
            var result = _reservationRepository.GetById(99);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetByClientId_ShouldReturnReservations_WhenClientHasReservations()
        {
            // Arrange
            var reservations = new List<Reservation>
            {
                new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada) { USERID = 1 }
            };

            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Provider).Returns(reservations.AsQueryable().Provider);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Expression).Returns(reservations.AsQueryable().Expression);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.ElementType).Returns(reservations.AsQueryable().ElementType);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.GetEnumerator()).Returns(reservations.GetEnumerator());

            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);

            // Act
            var result = _reservationRepository.GetByClientId(1);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetByClientId_ShouldReturnEmptyList_WhenNoReservationsExist()
        {
            // Arrange
            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);

            // Act
            var result = _reservationRepository.GetByClientId(1);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetByDateRange_ShouldReturnReservationsWithinRange()
        {
            // Arrange
            var reservations = new List<Reservation>
            {
                new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada)
            };

            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Provider).Returns(reservations.AsQueryable().Provider);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Expression).Returns(reservations.AsQueryable().Expression);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.ElementType).Returns(reservations.AsQueryable().ElementType);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.GetEnumerator()).Returns(reservations.GetEnumerator());

            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);

            // Act
            var result = _reservationRepository.GetByDateRange(DateTime.Today, DateTime.Today.AddDays(5));

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetByDateRange_ShouldReturnEmptyList_WhenNoReservationsMatch()
        {
            // Arrange
            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);

            // Act
            var result = _reservationRepository.GetByDateRange(DateTime.Today.AddDays(10), DateTime.Today.AddDays(15));

            // Assert
            Assert.That(result, Is.Empty);
        }
    }
}