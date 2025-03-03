using Hotel.src.Application.Services;
using Hotel.src.Core.Entities;
using Hotel.src.Core.Enums;
using Hotel.src.Core.Interfaces.IRepository;
using Moq;
using NUnit.Framework.Internal;


namespace HotelTests.Application.Services
{
    [TestFixture]
    [Author("Kendall Angulo", "kendallangulo01.com")]
    public class Tests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private AuthService _authService;

        [SetUp]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            var jwtService = new JwtService(); // Instantiate the real implementation of JwtService
            _authService = new AuthService(_userRepositoryMock.Object, jwtService); // Pass the real instance to AuthService
        }

        [Test]
        public void Authenticate_ShouldReturnToken_WhenUserExists()
        {
            // Arrange 🔹 Simulate a user in the DB
            var testUser = new User { EMAIL = "admin@example.com", PASSWORD = "admin123", ROLE = RoleUser.Admin };

            // Setup of the mock for the repository
            _userRepositoryMock
                .Setup(repo => repo.GetUserByEmailAndRole("admin@example.com", "admin123"))
                .Returns(testUser);

            // Act 🔹 We call the method we want to test
            var token = _authService.Authenticate("admin@example.com", "admin123");

            // Assert 🔹 We verify that the token is not null or empty
            Assert.That(token, Is.Not.Null.Or.Empty); /// <test type = "Pass successfully" >
        }

        [Test]
        public void Authenticate_ShouldReturnToken_WhenUserNotExists()
        {
            // Arrange 🔹 We pretend that the user exists, but the password is incorrect
            var testUser = new User { EMAIL = "admin@example.com", PASSWORD = "admin123", ROLE = RoleUser.Admin };
            _userRepositoryMock
                .Setup(repo => repo.GetUserByEmailAndRole("admin@example.com", "wrongpassword"))
                .Returns(testUser);

            // Act 🔹 We call the method with incorrect password
            var token = _authService.Authenticate("admin@example.com", "wrongpassword");

            // Assert 🔹 Token must be null
            Assert.Null(token); /// <test type = "Fail" >
        }
        [Test]
        public void GetRoleFromToken_ValidToken_ReturnsCorrectRole()
        {
            // Arrange
            var testUser = new User { EMAIL = "admin@example.com", PASSWORD = "admin123", ROLE = RoleUser.Admin };
            var jwtService = new JwtService();
            string token = jwtService.GenerateToken(testUser);

            // Act
            string actualRole = jwtService.GetRoleFromToken(token);

            // Assert
            Assert.That(Enum.Parse<RoleUser>(actualRole), Is.EqualTo(RoleUser.Admin)); /// <test type="Pass successfully">
        }
        [Test]
        public void GetRoleFromToken_ValidToken_ReturnsIncorrectRole()
        {
            // Arrange
            var testUser = new User { EMAIL = "admin@example.com", PASSWORD = "admin123", ROLE = RoleUser.Admin };
            var jwtService = new JwtService();
            string token = jwtService.GenerateToken(testUser);

            // Act
            string actualRole = jwtService.GetRoleFromToken(token);

            // Assert
            Assert.That(Enum.Parse<RoleUser>(actualRole), Is.Not.EqualTo(RoleUser.User)); /// <test type="Fail">
        }

        [Test]
        public void GetUserIdFromToken_ValidToken_ReturnsCorrectUserId()
        {
            // Arrange: Create a dummy user with known Id
            int expectedUserId = 1;
            var testUser = new User { ID = expectedUserId, EMAIL = "admin@example.com", PASSWORD = "admin123", ROLE = RoleUser.Admin };

            // Mock setup for the repository

            _userRepositoryMock
                .Setup(repo => repo.GetUserByEmailAndRole("admin@example.com", "admin123"))
                .Returns(testUser);
            // Act 🔹We call the method we want to test
            string token = _authService.Authenticate("admin@example.com", "admin123");
            JwtService jwtService = new JwtService();
            int userId = jwtService.GetUserIdFromToken(token);



            // Assert: The ID obtained is expected to be the expected one
            Assert.That(expectedUserId, Is.EqualTo(userId));
        }

        [Test]
        public void GetUserIdFromToken_ValidToken_ReturnsIncorrectUserId()
        {
            // Arrange: Crear un usuario dummy con Id conocido
            int expectedUserId = 1;
            var testUser = new User { ID = 2, EMAIL = "admin@example.com", PASSWORD = "admin123", ROLE = RoleUser.Admin };

            // Mock setup for the repository

            _userRepositoryMock
                .Setup(repo => repo.GetUserByEmailAndRole("admin@example.com", "admin123"))
                .Returns(testUser);
            // Act 🔹 We call the method we want to test
            string token = _authService.Authenticate("admin@example.com", "admin123");
            JwtService jwtService = new JwtService();
            int userId = jwtService.GetUserIdFromToken(token);

            // Assert: The ID obtained is not expected to be the expected one
            Assert.That(expectedUserId, Is.Not.EqualTo(userId));
        }
    }
}