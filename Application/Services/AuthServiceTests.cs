using Hotel.src.Application.Services;
using Hotel.src.Core.Entities;
using Hotel.src.Core.Enums;
using Hotel.src.Core.Interfaces.IRepository;
using Moq;


namespace HotelTests.Application.Services
{
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

            // Setup del mock para el repositorio
            _userRepositoryMock
                .Setup(repo => repo.GetUserByEmailAndRole("admin@example.com", "admin123"))
                .Returns(testUser);

            // Act 🔹 Llamamos al método que queremos probar
            var token = _authService.Authenticate("admin@example.com", "admin123");

            // Assert 🔹 Verificamos que el token no sea null o vacío
            Assert.That(token, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void Authenticate_WrongPassword_ReturnsNull()
        {
            // Arrange 🔹 Simulamos que el usuario existe, pero la contraseña es incorrecta
            var testUser = new User { EMAIL = "admin@example.com", PASSWORD = "admin123", ROLE = RoleUser.Admin };
            _userRepositoryMock
                .Setup(repo => repo.GetUserByEmailAndRole("admin@example.com", "wrongpassword"))
                .Returns(testUser);

            // Act 🔹 Llamamos al método con contraseña incorrecta
            var token = _authService.Authenticate("admin@example.com", "wrongpassword");

            // Assert 🔹 El token debe ser null
            Assert.Null(token);
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
            Assert.That(Enum.Parse<RoleUser>(actualRole), Is.EqualTo(RoleUser.Admin));
        }

        [Test]
        public void GetUserIdFromToken_ValidToken_ReturnsCorrectUserId()
        {
            // Arrange: Crear un usuario dummy con Id conocido
            int expectedUserId = 1;
            var testUser = new User { ID = expectedUserId, EMAIL = "admin@example.com", PASSWORD = "admin123", ROLE = RoleUser.Admin };

            // Setup del mock para el repositorio
            
            _userRepositoryMock
                .Setup(repo => repo.GetUserByEmailAndRole("admin@example.com", "admin123"))
                .Returns(testUser);
            // Act 🔹 Llamamos al método que queremos probar
            string token = _authService.Authenticate("admin@example.com", "admin123");
            JwtService jwtService = new JwtService();
            int userId = jwtService.GetUserIdFromToken(token);



            // Assert: Se espera que el ID obtenido sea el esperado
            Assert.That(expectedUserId, Is.EqualTo(userId));
        }
    }
}