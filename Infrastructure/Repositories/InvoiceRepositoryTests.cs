using Hotel.src.Core.Entities;
using Hotel.src.Infrastructure.Repositories;
using Hotel.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HotelTests.Infrastructure.Repositories
{
    [TestFixture]
    public class InvoiceRepositoryTests
    {
        private Mock<ApplicationDbContext> _mockDbContext;
        private Mock<DbSet<Invoice>> _mockInvoiceDbSet;
        private InvoiceRepository _invoiceRepository;

        [SetUp]
        public void Setup()
        {
            _mockInvoiceDbSet = new Mock<DbSet<Invoice>>();

            _mockInvoiceDbSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(Enumerable.Empty<Invoice>().AsQueryable().Provider);
            _mockInvoiceDbSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(Enumerable.Empty<Invoice>().AsQueryable().Expression);
            _mockInvoiceDbSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(Enumerable.Empty<Invoice>().AsQueryable().ElementType);
            _mockInvoiceDbSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(Enumerable.Empty<Invoice>().GetEnumerator());

            _mockDbContext = new Mock<ApplicationDbContext>();
            _mockDbContext.Setup(c => c.Invoices).Returns(_mockInvoiceDbSet.Object);

            _invoiceRepository = new InvoiceRepository(_mockDbContext.Object);
        }

        [Test]
        public void AddInvoice_ShouldAddSuccessfully()
        {
            var invoice = new Invoice
            {
                DateIssued = DateTime.UtcNow,
                TotalAmount = 250.00f
            };

            var result = _invoiceRepository.AddInvoice(invoice);

            _mockInvoiceDbSet.Verify(u => u.Add(invoice), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(), Times.Once);

            Assert.That(result, Is.EqualTo(invoice));
        }

        [Test]
        public void AddInvoice_ShouldThrowException_WhenInvoiceIsNull()
        {
            Invoice invoice = null;

            Assert.Throws<ArgumentNullException>(() => _invoiceRepository.AddInvoice(invoice));
        }
    }
}
