using Hotel.src.Application.Services;
using Hotel.src.Core.Entities;
using Hotel.src.Core.Enums;
using Hotel.src.Core.Interfaces.IRepository;
using Moq;

namespace HotelTests.Application.Services
{
    [TestFixture]
    class ReservationServiceTests
    {
        private Mock<IReservationRepository> _reservationRepositoryMock;
        private ReservationService _reservationService;

        [SetUp]
        public void Setup()
        {
            _reservationRepositoryMock = new Mock<IReservationRepository>();
            _reservationService = new ReservationService(_reservationRepositoryMock.Object);
        }
        [Test]
        public void RegisterReservation_ShouldRegisterSuccessfully_WhenValidData_Success()
        {
            // Arrange
            var reservation = new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada);
            _reservationRepositoryMock.Setup(repo => repo.Add(It.IsAny<Reservation>())).Returns(reservation);

            // Act
            _reservationService.RegisterReservation(reservation);

            // Assert
            _reservationRepositoryMock.Verify(repo => repo.Add(reservation), Times.Once);
        }    
        [Test]
        public void RegisterReservation_CreateReservation_InvalidDates_Error()
        {
            var reservation = new Reservation(1, DateTime.Today.AddDays(3), DateTime.Today, ReservationStatus.Confirmada);
            var ex = Assert.Throws<Exception>(() => _reservationService.RegisterReservation(reservation));
            Assert.That(ex.Message, Is.EqualTo("Las fechas de la reserva son inválidas."));
        }
        [Test]
        public void CancelRoomInReservation_Success()
        {
            var reservation = new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada);
            var room = new ReservationRoom { ReservationID = reservation.ID, RoomID = 1, Reservation = reservation };
            reservation.ReservationRooms.Add(room);

            _reservationRepositoryMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns(reservation);

            _reservationService.CancelRoomInReservation(reservation.ID, room.RoomID);

            Assert.That(reservation.ReservationRooms.Count, Is.EqualTo(0));
            _reservationRepositoryMock.Verify(repo => repo.Update(reservation), Times.Once);
        }
        [Test]
        public void CancelRoom_NonExistentReservation_Error()
        {
            _reservationRepositoryMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns((Reservation)null);

            var ex = Assert.Throws<Exception>(() => _reservationService.CancelRoomInReservation(1, 1));

            Assert.That(ex.Message, Is.EqualTo("La reserva no existe."));
        }
        [Test]
        public void ViewBookingHistory_UserHasBookings_Success()
        {
            var reservations = new List<Reservation> { new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada) };
            _reservationRepositoryMock.Setup(repo => repo.GetByClientId(1)).Returns(reservations);

            var result = _reservationService.GetReservationsByClientId(1);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ViewBookingHistory_NoBookings_MessageDisplayed()
        {
            _reservationRepositoryMock.Setup(repo => repo.GetByClientId(1)).Returns(new List<Reservation>());

            var result = _reservationService.GetReservationsByClientId(1);

            Assert.That(result.Count, Is.EqualTo(0));
        }
        [Test]
        public void AddRoomToReservation_ShouldAddRoomSuccessfully()
        {
            var reservationRoom = new ReservationRoom { ReservationID = 1, RoomID = 101 };
            _reservationRepositoryMock.Setup(repo => repo.AddRoomInReservation(It.IsAny<ReservationRoom>()))
                                      .Returns(reservationRoom);

            var result = _reservationService.AddRoomToReservation(reservationRoom);

            _reservationRepositoryMock.Verify(repo => repo.AddRoomInReservation(reservationRoom), Times.Once);
            Assert.That(result, Is.EqualTo(reservationRoom));
        }

        [Test]
        public void AddRoomToReservation_NullReservationRoom_ShouldThrowArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => _reservationService.AddRoomToReservation(null));
            Assert.That(ex.Message, Does.Contain("La relación de reserva y habitación no puede ser nula."));
        }

        [Test]
        public void UpdateReservation_ShouldCallRepositoryUpdateSuccessfully()
        {
            var reservation = new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada);

            var reservationRoom = new ReservationRoom { ReservationID = 1, RoomID = 101 };
            reservation.ReservationRooms.Add(reservationRoom);
            
            _reservationService.UpdateReservation(reservation);

            _reservationRepositoryMock.Verify(repo => repo.Update(reservation), Times.Once);
        }

        [Test]
        public void UpdateReservation_NullReservation_ShouldThrowArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => _reservationService.UpdateReservation(null));
            Assert.That(ex.Message, Does.Contain("La reserva no puede ser nula."));
        }

        [Test]
        public void GetReservationsByDateRange_ShouldReturnReservationsWithinRange()
        {
            var reservations = new List<Reservation>
            {
                new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada)
            };
            _reservationRepositoryMock.Setup(repo => repo.GetByDateRange(DateTime.Today, DateTime.Today.AddDays(5)))
                                      .Returns(reservations);

            var result = _reservationService.GetReservationsByDateRange(DateTime.Today, DateTime.Today.AddDays(5));

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetReservationsByDateRange_InvalidDates_ShouldThrowArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => _reservationService.GetReservationsByDateRange(DateTime.Today.AddDays(5), DateTime.Today));
            Assert.That(ex.Message, Does.Contain("La fecha de fin no puede ser menor a la fecha de inicio."));
        }

        [Test]
        public void GetActiveReservationsByClientId_ShouldReturnOnlyConfirmedReservations()
        {
            var reservations = new List<Reservation>
            {
                new Reservation(1, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Confirmada) { USERID = 1 },
                new Reservation(2, DateTime.Today, DateTime.Today.AddDays(3), ReservationStatus.Cancelada) { USERID = 1 }
            };
            _reservationRepositoryMock.Setup(repo => repo.GetByClientId(1)).Returns(reservations);

            var result = _reservationService.GetActiveReservationsByClientId(1);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().STATUS, Is.EqualTo(ReservationStatus.Confirmada));
        }
    }
}
