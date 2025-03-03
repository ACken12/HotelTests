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
        private Mock<DbSet<ReservationRoom>> _mockReservationRoomsDbSet;


        [SetUp]
        public void Setup()
        {
            // Configuración del mock para DbSet<Reservation>
            _mockReservationsDbSet = new Mock<DbSet<Reservation>>();
            var emptyReservations = Enumerable.Empty<Reservation>().AsQueryable();
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Provider).Returns(emptyReservations.Provider);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Expression).Returns(emptyReservations.Expression);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.ElementType).Returns(emptyReservations.ElementType);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.GetEnumerator()).Returns(emptyReservations.GetEnumerator());

            // Configuración del mock para DbSet<ReservationRoom>
            _mockReservationRoomsDbSet = new Mock<DbSet<ReservationRoom>>();

            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);
            _mockDbContext.Setup(c => c.ReservationRooms).Returns(_mockReservationRoomsDbSet.Object);

            _reservationRepository = new ReservationRepository(_mockDbContext.Object);
        }

        [Test]
        public void Add_ShouldAddReservationSuccessfully()
        {
            var newReservation = new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada);

            var result = _reservationRepository.Add(newReservation);

            _mockReservationsDbSet.Verify(u => u.Add(newReservation), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(), Times.Once);

            Assert.That(result, Is.EqualTo(newReservation));
        }

        [Test]
        public void GetById_ShouldReturnNull_WhenReservationDoesNotExist()
        {
            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);

            var result = _reservationRepository.GetById(99);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetByClientId_ShouldReturnReservations_WhenClientHasReservations()
        {
            var reservations = new List<Reservation>
            {
                new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada) { USERID = 1 }
            };

            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Provider).Returns(reservations.AsQueryable().Provider);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Expression).Returns(reservations.AsQueryable().Expression);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.ElementType).Returns(reservations.AsQueryable().ElementType);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.GetEnumerator()).Returns(reservations.GetEnumerator());

            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);

            var result = _reservationRepository.GetByClientId(1);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetByClientId_ShouldReturnEmptyList_WhenNoReservationsExist()
        {
            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);

            var result = _reservationRepository.GetByClientId(1);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetByDateRange_ShouldReturnReservationsWithinRange()
        {
            var reservations = new List<Reservation>
            {
                new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada)
            };

            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Provider).Returns(reservations.AsQueryable().Provider);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Expression).Returns(reservations.AsQueryable().Expression);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.ElementType).Returns(reservations.AsQueryable().ElementType);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.GetEnumerator()).Returns(reservations.GetEnumerator());

            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);

            var result = _reservationRepository.GetByDateRange(DateTime.Today, DateTime.Today.AddDays(5));

            Assert.That(result.Count, Is.EqualTo(1));
        }
        [Test]
        public void GetConfirmedReservations_ShouldReturnOnlyConfirmedReservationsWithinRange()
        {
            var reservations = new List<Reservation>
            {
                new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada)
            };
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Provider).Returns(reservations.AsQueryable().Provider);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Expression).Returns(reservations.AsQueryable().Expression);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.ElementType).Returns(reservations.AsQueryable().ElementType);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.GetEnumerator()).Returns(reservations.GetEnumerator());

            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);

            var result = _reservationRepository.GetConfirmedReservations(DateTime.Today, DateTime.Today.AddDays(3));

            Assert.That(result.All(r => r.STATUS == ReservationStatus.Confirmada), Is.True);
        }
        [Test]
        public void GetConfirmedReservations_ShouldReturnEmptyList_WhenOnlyPaidReservationsExist()
        {
            var reservations = new List<Reservation>
            {
                new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Pagada)
            };
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Provider).Returns(reservations.AsQueryable().Provider);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Expression).Returns(reservations.AsQueryable().Expression);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.ElementType).Returns(reservations.AsQueryable().ElementType);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.GetEnumerator()).Returns(reservations.GetEnumerator());
            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);
            var result = _reservationRepository.GetConfirmedReservations(DateTime.Today, DateTime.Today.AddDays(3));
            Assert.That(result, Is.Empty);
        }
        [Test]
        public void GetByDateRange_ShouldReturnEmptyList_WhenNoReservationsMatch()
        {
            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);

            var result = _reservationRepository.GetByDateRange(DateTime.Today.AddDays(10), DateTime.Today.AddDays(15));

            Assert.That(result, Is.Empty);
        }
        [Test]
        public void AddRoomInReservation_ShouldAddRoomSuccessfully()
        {
            var reservationRoom = new ReservationRoom { ReservationID = 1, RoomID = 101 };

            var result = _reservationRepository.AddRoomInReservation(reservationRoom);

            _mockReservationRoomsDbSet.Verify(r => r.Add(reservationRoom), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.That(result, Is.EqualTo(reservationRoom));
        }
        [Test]
        public void AddRoomInReservation_NullReservationRoom_ShouldThrowArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => _reservationRepository.AddRoomInReservation(null));
            Assert.That(ex.Message, Does.Contain("La relación de reserva y habitación no puede ser nula."));
        }
        [Test]
        public void Update_ValidReservation_ShouldUpdateSuccessfully()
        {
            var reservation = new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada);

            _reservationRepository.Update(reservation);

            _mockReservationsDbSet.Verify(r => r.Update(reservation), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(), Times.Once);
        }
        [Test]
        public void Update_NullReservation_ShouldThrowArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => _reservationRepository.Update(null));
            Assert.That(ex.Message, Does.Contain("La reserva no puede ser nula."));
        }
        [Test]
        public void GetUpcomingReservations_ShouldReturnUpcomingReservations()
        {
            int daysAhead = 2;
            DateTime today = DateTime.UtcNow.Date;
            var upcomingReservation = new Reservation(1, today.AddDays(daysAhead), today.AddDays(daysAhead + 3), ReservationStatus.Confirmada);
            var reservations = new List<Reservation> { upcomingReservation };
            var queryableReservations = reservations.AsQueryable();
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Provider).Returns(queryableReservations.Provider);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.Expression).Returns(queryableReservations.Expression);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.ElementType).Returns(queryableReservations.ElementType);
            _mockReservationsDbSet.As<IQueryable<Reservation>>().Setup(m => m.GetEnumerator()).Returns(queryableReservations.GetEnumerator());
            _mockDbContext.Setup(c => c.Reservations).Returns(_mockReservationsDbSet.Object);

            var result = _reservationRepository.GetUpcomingReservations(daysAhead);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().ID, Is.EqualTo(upcomingReservation.ID));
        }

        [Test]
        public void GetUpcomingReservations_NegativeDaysAhead_ShouldThrowArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => _reservationRepository.GetUpcomingReservations(-1));
            Assert.That(ex.Message, Does.Contain("El parámetro daysAhead no puede ser negativo."));
        }
    }
}