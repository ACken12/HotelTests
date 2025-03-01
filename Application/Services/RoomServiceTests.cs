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
            _roomRepositoryMock.Setup(repo => repo.GetByRoomNumber("101")).Returns((Room)null);
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

        // CheckAvailability Method

        [Test]
        public void CheckAvailability_ShouldReturnAvailableRooms_WhenDatesAreValid()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room("101", RoomType.SIMPLE, 100, 2, RoomStatus.DISPONIBLE),
                new Room("102", RoomType.DOBLE, 150, 4, RoomStatus.DISPONIBLE)
            };
            _roomRepositoryMock.Setup(repo => repo.HasReservationsInDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(rooms);

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
            _roomRepositoryMock.Setup(repo => repo.HasReservationsInDateRange(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(rooms);

            // Act
            var result = _roomService.CheckAvailability(DateTime.Today, DateTime.Today.AddDays(2));

            // Assert
            Assert.That(result, Is.Empty);
        }

        // SearchRooms Method

        [Test]
        public void SearchRooms_ShouldReturnRoomsFilteredByType()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room("101", RoomType.SIMPLE, 100, 2, RoomStatus.DISPONIBLE),
                new Room("102", RoomType.DOBLE, 150, 4, RoomStatus.DISPONIBLE)
            };
            _roomRepositoryMock.Setup(repo => repo.GetByType(RoomType.SIMPLE)).Returns(rooms.Where(r => r.TYPE == RoomType.SIMPLE));

            // Act
            var result = _roomService.SearchRooms(RoomType.SIMPLE);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().ROOMNUMBER, Is.EqualTo("101"));
        }

        [Test]
        public void SearchRooms_ShouldReturnEmptyList_WhenNoRoomsMatchTypeFilter()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room("101", RoomType.DOBLE, 100, 2, RoomStatus.DISPONIBLE),
                new Room("102", RoomType.DOBLE, 150, 4, RoomStatus.DISPONIBLE)
            };
            _roomRepositoryMock.Setup(repo => repo.GetByType(RoomType.SIMPLE)).Returns(rooms.Where(r => r.TYPE == RoomType.SIMPLE));

            // Act
            var result = _roomService.SearchRooms(RoomType.SIMPLE);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void SearchRooms_ShouldReturnRoomsFilteredByPrice()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room("101", RoomType.SIMPLE, 100, 2, RoomStatus.DISPONIBLE),
                new Room("102", RoomType.DOBLE, 150, 4, RoomStatus.DISPONIBLE)
            };
            _roomRepositoryMock.Setup(repo => repo.GetByPriceRange(90, 120)).Returns(rooms.Where(r => r.PRICEPERNIGHT >= 90 && r.PRICEPERNIGHT <= 120));

            // Act
            var result = _roomService.SearchRooms(minPrice: 90, maxPrice: 120);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().ROOMNUMBER, Is.EqualTo("101"));
        }

        [Test]
        public void SearchRooms_ShouldReturnEmptyList_WhenNoRoomsMatchPriceFilter()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room("101", RoomType.SIMPLE, 100, 2, RoomStatus.DISPONIBLE),
                new Room("102", RoomType.DOBLE, 150, 4, RoomStatus.DISPONIBLE)
            };
            _roomRepositoryMock.Setup(repo => repo.GetByPriceRange(200, 250)).Returns(rooms.Where(r => r.PRICEPERNIGHT >= 200 && r.PRICEPERNIGHT <= 250));

            // Act
            var result = _roomService.SearchRooms(minPrice: 200, maxPrice: 250);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(0));
        }

        [Test]
        public void SearchRooms_ShouldReturnRoomsFilteredByDateRange()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room("101", RoomType.SIMPLE, 100, 2, RoomStatus.DISPONIBLE),
                new Room("102", RoomType.DOBLE, 150, 4, RoomStatus.DISPONIBLE)
            };
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(2);
            _roomRepositoryMock.Setup(repo => repo.HasReservationsInDateRange(startDate, endDate)).Returns(rooms);

            // Act
            var result = _roomService.SearchRooms(startDate: startDate, endDate: endDate);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().ROOMNUMBER, Is.EqualTo("101"));
            Assert.That(result.Last().ROOMNUMBER, Is.EqualTo("102"));
        }
        [Test]
        public void SearchRooms_ShouldReturnRoomsAvailableToday_WhenNoDateParametersProvided()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room("101", RoomType.SIMPLE, 100, 2, RoomStatus.DISPONIBLE),
                new Room("102", RoomType.DOBLE, 150, 4, RoomStatus.DISPONIBLE),
                new Room("103", RoomType.SIMPLE, 80, 1, RoomStatus.OCUPADO)
            };

            var today = DateTime.Today;
            var endDate = today.AddDays(1);

            // Mocking that the method should return rooms that are available today
            _roomRepositoryMock.Setup(repo => repo.HasReservationsInDateRange(today, endDate)).Returns(rooms.Where(r => r.STATUS == RoomStatus.DISPONIBLE));

            // Act
            var result = _roomService.SearchRooms();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().ROOMNUMBER, Is.EqualTo("101"));
            Assert.That(result.Last().ROOMNUMBER, Is.EqualTo("102"));
        }
    }
}