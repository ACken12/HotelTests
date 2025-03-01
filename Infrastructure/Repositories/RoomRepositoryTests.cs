using Hotel.src.Core.Entities;
using Hotel.src.Core.Enums;
using Hotel.src.Core.Interfaces.IRepository;
using Hotel.src.Infrastructure.Data;
using Hotel.src.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HotelTests.Infrastructure.Repositories
{
    public class RoomRepositoryTests
    {
        private Mock<ApplicationDbContext> _mockDbContext;
        private Mock<DbSet<Room>> _mockRoomsDbSet;
        private IRoomRepository _roomRepository;

        [SetUp]
        public void Setup()
        {
            // Arrange: Set up common objects for all tests.
            _mockRoomsDbSet = new Mock<DbSet<Room>>();

            // Simulamos los métodos de DbSet para IQueryable (como FirstOrDefault, Where, etc.)
            _mockRoomsDbSet.As<IQueryable<Room>>().Setup(m => m.Provider).Returns(Enumerable.Empty<Room>().AsQueryable().Provider);
            _mockRoomsDbSet.As<IQueryable<Room>>().Setup(m => m.Expression).Returns(Enumerable.Empty<Room>().AsQueryable().Expression);
            _mockRoomsDbSet.As<IQueryable<Room>>().Setup(m => m.ElementType).Returns(Enumerable.Empty<Room>().AsQueryable().ElementType);
            _mockRoomsDbSet.As<IQueryable<Room>>().Setup(m => m.GetEnumerator()).Returns(Enumerable.Empty<Room>().GetEnumerator());

            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockDbContext.Setup(c => c.Rooms).Returns(_mockRoomsDbSet.Object);

            _roomRepository = new RoomRepository(_mockDbContext.Object);
        }

        [Test]
        public void Add_ShouldAddRoomSuccessfully_WhenRoomNumberIsUnique()
        {
            // Arrange
            var newRoom = new Room("102", RoomType.SIMPLE, 100, 2, RoomStatus.DISPONIBLE);

            // Act
            var result = _roomRepository.Add(newRoom);

            // Assert
            _mockRoomsDbSet.Verify(u => u.Add(newRoom), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(), Times.Once);

            Assert.That(result.ROOMNUMBER, Is.EqualTo("102"));
            Assert.AreEqual(newRoom, result);
        }

        [Test]
        public void Add_ShouldThrowException_WhenRoomNumberAlreadyExists()
        {
            // Arrange
            var existingRoom = new Room("101", RoomType.SIMPLE, 100, 2, RoomStatus.DISPONIBLE);

            // Simulamos que ya existe una habitación con el número "101"
            var rooms = new List<Room> { existingRoom };

            // Configuramos el DbSet simulado para devolver esta lista de habitaciones
            _mockRoomsDbSet.As<IQueryable<Room>>().Setup(m => m.Provider).Returns(rooms.AsQueryable().Provider);
            _mockRoomsDbSet.As<IQueryable<Room>>().Setup(m => m.Expression).Returns(rooms.AsQueryable().Expression);
            _mockRoomsDbSet.As<IQueryable<Room>>().Setup(m => m.ElementType).Returns(rooms.AsQueryable().ElementType);
            _mockRoomsDbSet.As<IQueryable<Room>>().Setup(m => m.GetEnumerator()).Returns(rooms.GetEnumerator());

            _mockDbContext.Setup(c => c.Rooms).Returns(_mockRoomsDbSet.Object);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _roomRepository.Add(existingRoom));
            Assert.That(ex.Message, Is.EqualTo("El número de habitación ya existe."));
        }
    }
}