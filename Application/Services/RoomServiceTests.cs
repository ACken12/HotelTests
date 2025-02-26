using Hotel.src.Application.Services;
using Hotel.src.Core.Entities;
using Hotel.src.Core.Enums;
using Hotel.src.Core.Interfaces.IRepository;
using Moq;


namespace HotelTests.Application.Services
{
    public class RoomServiceTests
    {
        private Mock<IRoomRepository> _roomRepositoryMock;
        private RoomService _roomService;

        [SetUp]
        public void Setup()
        {
            _roomRepositoryMock = new Mock<IRoomRepository>();
            _roomService = new RoomService(_roomRepositoryMock.Object);
        }

        [Test]
        public void RegisterRoom_ShouldRegisterSuccessfully_WhenDataIsValid()
        {
            // Arrange
            var room = new Room("101", RoomType.SIMPLE, 100, 2, RoomStatus.DISPONIBLE);
            _roomRepositoryMock.Setup(repo => repo.Add(It.IsAny<Room>())).Returns(room);

            // Act
            var result = _roomService.RegisterRoom("101", RoomType.SIMPLE, 100, 2);

            // Assert
            Assert.NotNull(result);
            Assert.That(result.ROOMNUMBER, Is.EqualTo("101"));
        }

        [Test]
        public void RegisterRoom_ShouldThrowException_WhenRoomNumberIsEmpty()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _roomService.RegisterRoom("", RoomType.SIMPLE, 100, 2));
            Assert.That(ex.Message, Is.EqualTo("El número de habitación es obligatorio"));
        }

        [Test]
        public void SearchRooms_ShouldReturnMatchingRooms_WhenCriteriaMatches()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room("101", RoomType.SIMPLE, 100, 2, RoomStatus.DISPONIBLE),
                new Room("102", RoomType.DOBLE, 150, 4, RoomStatus.DISPONIBLE)
            };
            _roomRepositoryMock.Setup(repo => repo.GetAll()).Returns(rooms);

            // Act
            var result = _roomService.SearchRooms(RoomType.SIMPLE, null, null);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().ROOMNUMBER, Is.EqualTo("101"));
        }

        [Test]
        public void SearchRooms_ShouldReturnEmptyList_WhenNoMatches()
        {
            // Arrange
            var rooms = new List<Room>();
            _roomRepositoryMock.Setup(repo => repo.GetAll()).Returns(rooms);

            // Act
            var result = _roomService.SearchRooms(RoomType.SUITE, 300, 500);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void CheckAvailability_ShouldReturnAvailableRooms_WhenDatesAreValid()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room("101", RoomType.SIMPLE, 100, 2, RoomStatus.DISPONIBLE),
                new Room("102", RoomType.DOBLE, 150, 4, RoomStatus.DISPONIBLE)
            };
            _roomRepositoryMock.Setup(repo => repo.GetByStatus(RoomStatus.DISPONIBLE)).Returns(rooms);
            _roomRepositoryMock.Setup(repo => repo.HasReservationsInDateRange(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(false);

            // Act
            var result = _roomService.CheckAvailability(DateTime.Today, DateTime.Today.AddDays(2));

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public void CheckAvailability_ShouldReturnEmptyList_WhenNoRoomsAvailable()
        {
            // Arrange
            var rooms = new List<Room>();
            _roomRepositoryMock.Setup(repo => repo.GetByStatus(RoomStatus.DISPONIBLE)).Returns(rooms);

            // Act
            var result = _roomService.CheckAvailability(DateTime.Today, DateTime.Today.AddDays(2));

            // Assert
            Assert.That(result, Is.Empty);
        }
    }
}