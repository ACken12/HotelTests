using Hotel.src.Application.Services;
using Hotel.src.Core.Entities;
using Hotel.src.Core.Interfaces.IRepository;
using Moq;

namespace HotelTests.Application.Services
{
    public class CustomerServicesTests
    {

        private Mock<ICustomerRepository> _mockCustomerRepository;
        private CustomerServices _customerServices;

        [SetUp]
        public void Setup()
        {
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _customerServices = new CustomerServices(_mockCustomerRepository.Object);
        }
        /// <summary>
        /// Tests that RegisterCustomer returns the user when the repository successfully adds it.
        /// </summary>
        [Test]
        public void RegisterCustomer_ShouldReturnUser_WhenRepositoryAddsSuccessfully()
        {
            // Arrange: Create a test user and configure the mock to return it
            var testUser = new User { ID = 1, NAME = "John Doe" };
            _mockCustomerRepository
                .Setup(repo => repo.AddCliente(testUser))
                .Returns(testUser);

            // Act: Call the RegisterCustomer method
            var result = _customerServices.RegisterCustomer(testUser);

            // Assert: Verify the result matches the expected user
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ID, Is.EqualTo(testUser.ID));
            Assert.That(result.NAME, Is.EqualTo(testUser.NAME));
        }

        /// <summary>
        /// Tests that RegisterCustomer returns null when the repository returns null.
        /// </summary>
        [Test]
        public void RegisterCustomer_ShouldReturnNull_WhenRepositoryReturnsNull()
        {
            // Arrange: Configure the mock repository to return null
            var testUser = new User { ID = 2, NAME = "Jane Doe" };
            _mockCustomerRepository
                .Setup(repo => repo.AddCliente(testUser))
                .Returns((User)null);

            // Act: Call the RegisterCustomer method
            var result = _customerServices.RegisterCustomer(testUser);

            // Assert: Verify that the result is null
            Assert.That(result, Is.Null);
        }

    }
}
