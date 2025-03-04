
using System.Linq.Expressions;
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
            var user = new User { NAME = "Camila", EMAIL = "camila@example", PASSWORD = "12345", ROLE = RoleUser.User };
            // Create an empty list to simulate an empty database
            var usersList = new List<User>().AsQueryable();

            // Mock the DbSet behavior
            var mockUsersDbSet = new Mock<DbSet<User>>();
            mockUsersDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(usersList.Provider);
            mockUsersDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(usersList.Expression);
            mockUsersDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(usersList.ElementType);
            mockUsersDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(usersList.GetEnumerator());

            // Mock Users property in DbContext
            _mockDbContext.Setup(c => c.Users).Returns(mockUsersDbSet.Object);

            // Mock SaveChanges to return 1 (indicating success)
            _mockDbContext.Setup(c => c.SaveChanges()).Returns(1);

            // Act: Call the AddClient method
            var result = _repository.AddClient(user);

            // Assert: Verify that the user was added and SaveChanges was called exactly once
            mockUsersDbSet.Verify(u => u.Add(user), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.That(result, Is.EqualTo(user));
        }

        /// <summary>
        /// Failure test: Verifies that AddCliente throws an exception when SaveChanges fails.
        /// </summary>
        [Test]
        public void AddCliente_Should_ReturnNull_When_EmailAlreadyExists()
        {
            // Arrange: Create a test user
            var existingUser = new User { NAME = "Camila", EMAIL = "camila@example", PASSWORD = "12345", ROLE = RoleUser.User };
            var newUser = new User { NAME = "New User", EMAIL = "camila@example", PASSWORD = "67890", ROLE = RoleUser.User };
            var usersList = new List<User> { existingUser }.AsQueryable(); // Simulating that the email already exists

            // Mock the DbSet behavior
            var mockUsersDbSet = new Mock<DbSet<User>>();
            mockUsersDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(usersList.Provider);
            mockUsersDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(usersList.Expression);
            mockUsersDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(usersList.ElementType);
            mockUsersDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(usersList.GetEnumerator());

            // Mock Users property in DbContext
            _mockDbContext.Setup(c => c.Users).Returns(mockUsersDbSet.Object);

            // Act: Try to add a user with the same email
            var result = _repository.AddClient(newUser);

            // Assert: Verify that Add() and SaveChanges() were NOT called
            mockUsersDbSet.Verify(u => u.Add(It.IsAny<User>()), Times.Never);
            _mockDbContext.Verify(c => c.SaveChanges(), Times.Never);
            Assert.That(result, Is.Null);
        }

    }
}
