
using Hotel.src.Core.Entities;
using Hotel.src.Core.Enums;
using Hotel.src.Core.Interfaces.IRepository;
using Hotel.src.Infrastructure.Data;
using Hotel.src.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;


namespace HotelTests.Infrastructure.Repositories
{
    /// <summary>
    /// Unit tests for CustomerRepository using the AAA (Arrange, Act, Assert) pattern.
    /// </summary>
    [TestFixture]
    [Author("Kendall Angulo", "kendallangulo01.com")]
    public class CustomerRepositoryTests
    {
        private Mock<ApplicationDbContext> _mockDbContext;
        private Mock<DbSet<User>> _mockUsersDbSet;
        private ICustomerRepository _repository;

        /// <summary>
        /// Setup method that runs before each test to initialize common objects.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Arrange: Set up common objects for all tests.
            _mockUsersDbSet = new Mock<DbSet<User>>();
            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockDbContext.Setup(c => c.Users).Returns(_mockUsersDbSet.Object);
            _repository = new CustomerRepository(_mockDbContext.Object);
        }
        /// <summary>
        /// Success test: Verifies that AddCliente successfully adds a user and returns the added user.
        /// </summary>
        [Test]
        public void AddCliente_Should_AddUserAndReturnUser_OnSuccess()
        {
            // Arrange: Create a test user and configure SaveChanges to succeed.
            var user = new User {NAME = "Camila" , EMAIL = "camila@example", PASSWORD = "12345", ROLE = RoleUser.User };
            _mockDbContext.Setup(c => c.SaveChanges()).Returns(1);

            // Act: Call the AddCliente method through the repository interface.
            var result = _repository.AddClient(user);

            // Assert: Verify that the user was added and that SaveChanges was called exactly once.
            _mockUsersDbSet.Verify(u => u.Add(user), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.That(result, Is.EqualTo(user));
        }

        /// <summary>
        /// Failure test: Verifies that AddCliente throws an exception when SaveChanges fails.
        /// </summary>
        [Test]
        public void AddCliente_Should_ThrowException_WhenSaveChangesFails()
        {
            // Arrange: Create a test user and configure SaveChanges to throw an exception.
            var user = new User { ID = 1, NAME = "Falle" };
            _mockDbContext.Setup(c => c.SaveChanges()).Throws(new Exception("Database error"));

            // Act Execute the action under test
            var exception = Assert.Throws<Exception>(() => _repository.AddClient(user));

            // Assert: Verify that an exception is thrown.
            Assert.That(exception.Message, Is.EqualTo("Database error"));
            _mockUsersDbSet.Verify(u => u.Add(user), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(), Times.Once);
        }

    }
}
