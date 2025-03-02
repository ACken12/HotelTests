using Hotel.src.Core.Entities;
using Hotel.src.Infrastructure.Repositories;
using Hotel.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HotelTests.Infrastructure.Repositories
{
    [TestFixture]
    public class InvoiceDetailsRepositoryTests
    {
        private Mock<ApplicationDbContext> _mockDbContext;
        private Mock<DbSet<InvoiceDetail>> _mockInvoiceDetailsDbSet;
        private InvoiceDetailsRepository _invoiceDetailsRepository;

        [SetUp]
        public void Setup()
        {
            _mockInvoiceDetailsDbSet = new Mock<DbSet<InvoiceDetail>>();

            _mockInvoiceDetailsDbSet.As<IQueryable<InvoiceDetail>>().Setup(m => m.Provider).Returns(Enumerable.Empty<InvoiceDetail>().AsQueryable().Provider);
            _mockInvoiceDetailsDbSet.As<IQueryable<InvoiceDetail>>().Setup(m => m.Expression).Returns(Enumerable.Empty<InvoiceDetail>().AsQueryable().Expression);
            _mockInvoiceDetailsDbSet.As<IQueryable<InvoiceDetail>>().Setup(m => m.ElementType).Returns(Enumerable.Empty<InvoiceDetail>().AsQueryable().ElementType);
            _mockInvoiceDetailsDbSet.As<IQueryable<InvoiceDetail>>().Setup(m => m.GetEnumerator()).Returns(Enumerable.Empty<InvoiceDetail>().GetEnumerator());

            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockDbContext.Setup(c => c.InvoiceDetails).Returns(_mockInvoiceDetailsDbSet.Object);

            _invoiceDetailsRepository = new InvoiceDetailsRepository(_mockDbContext.Object);
        }

        [Test]
        public void AddInvoiceDetail_ShouldAddSuccessfully()
        {
            var invoiceDetail = new InvoiceDetail
            {
                InvoiceID = 1,
                RoomID = 101,
                Price = 100
            };

            var result = _invoiceDetailsRepository.AddInvoiceDetail(invoiceDetail);

            _mockInvoiceDetailsDbSet.Verify(u => u.Add(invoiceDetail), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(), Times.Once);

            Assert.That(result, Is.EqualTo(invoiceDetail));
        }

        [Test]
        public void AddInvoiceDetail_ShouldThrowException_WhenInvoiceDetailIsNull()
        {
            InvoiceDetail invoiceDetail = null;

            Assert.Throws<ArgumentException>(() => _invoiceDetailsRepository.AddInvoiceDetail(invoiceDetail));
        }
    }
}
