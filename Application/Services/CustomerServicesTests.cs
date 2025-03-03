using Hotel.src.Application.Services;
using Hotel.src.Core.Entities;
using Hotel.src.Core.Interfaces.IRepository;
using Moq;

namespace HotelTests.Application.Services
{
    [TestFixture]
    [Author("Kendall Angulo", "kendallangulo01.com")]
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
                .Setup(repo => repo.AddClient(testUser))
                .Returns(testUser);


            // Act: Call the RegisterCustomer method
            var result = _customerServices.RegisterCustomer(testUser);

            // Assert: Verify the result matches the expected user
            Assert.That(result, Is.EqualTo(testUser)); /// <test type="Pass successfully">
        }

        /// <summary>
        /// Tests that RegisterCustomer returns null when the repository returns null.
        /// </summary>
        [Test]
        public void RegisterCustomer_ShouldReturnNull_WhenRepositoryReturnsNull()
        {
            // Arrange: Configure the mock repository to return null
            User testUser = null;
           
            _mockCustomerRepository
                .Setup(repo => repo.AddClient(testUser))
                .Returns(testUser);

            // Act: Call the RegisterCustomer method
            var result = _customerServices.RegisterCustomer(testUser);

            // Assert: Verify that the result is null
            Assert.That(result, Is.Null);  /// <test type="fail">
        }

    }
}
