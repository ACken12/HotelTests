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


    }
}
