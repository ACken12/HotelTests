using Moq;
using Hotel.src.Application.Services;
using Hotel.src.Core.Entities;
using Hotel.src.Core.Interfaces.IRepository;

namespace Hotel.Tests
{
    [TestFixture]
    public class BillingServiceTests
    {
        private BillingService _billingService;
        private Mock<IReservationRepository> _reservationRepositoryMock;
        private Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private Mock<IInvoiceDetailsRepository> _invoiceDetailsRepositoryMock;

        /// <summary>
        /// Inicializa los mocks y la instancia de BillingService antes de cada prueba.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _reservationRepositoryMock = new Mock<IReservationRepository>();
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _invoiceDetailsRepositoryMock = new Mock<IInvoiceDetailsRepository>();

            _billingService = new BillingService(
                _reservationRepositoryMock.Object,
                _invoiceRepositoryMock.Object,
                _invoiceDetailsRepositoryMock.Object);
        }

        /// <summary>
        /// Verifica que GenerateInvoice retorne una factura con detalles correctos cuando la reserva es válida.
        /// </summary>
        [Test]
        public void GenerateInvoice_WithValidReservation_ReturnsInvoiceWithDetails()
        {
            int reservationId = 1;
            var reservation = new Reservation
            {
                ID = reservationId,
                TOTALPRICE = 200.0,
                STARTDATE = DateTime.UtcNow.AddDays(-2),
                ENDDATE = DateTime.UtcNow,
                ReservationRooms = new List<ReservationRoom>
                {
                    new ReservationRoom
                    {
                        Room = new Room { ID = 101, PRICEPERNIGHT = 50.0f }
                    }
                }
            };

            _reservationRepositoryMock.Setup(r => r.GetById(reservationId))
                .Returns(reservation);

            _invoiceRepositoryMock.Setup(i => i.AddInvoice(It.IsAny<Invoice>()))
                .Callback<Invoice>(inv => inv.ID = 123) 
                .Returns((Invoice inv) => inv);

            _invoiceDetailsRepositoryMock.Setup(d => d.AddInvoiceDetail(It.IsAny<InvoiceDetail>()))
                .Returns((InvoiceDetail detail) => detail);

            var invoice = _billingService.GenerateInvoice(reservationId);

            Assert.IsNotNull(invoice);
            Assert.AreEqual(123, invoice.ID);
            Assert.AreEqual((float)reservation.TOTALPRICE, invoice.TotalAmount);
            Assert.IsNotEmpty(invoice.InvoiceDetails);

            double expectedNights = (reservation.ENDDATE - reservation.STARTDATE).TotalDays;
            decimal expectedPrice = (decimal)reservation.ReservationRooms.First().Room.PRICEPERNIGHT * (decimal)expectedNights;
            var detail = invoice.InvoiceDetails.First();
            Assert.AreEqual(expectedPrice, detail.Price);
        }

        /// <summary>
        /// Verifica que GenerateInvoice lance una excepción cuando la reserva no existe.
        /// </summary>
        [Test]
        public void GenerateInvoice_ReservationDoesNotExist_ThrowsException()
        {
            int reservationId = 99;
            _reservationRepositoryMock.Setup(r => r.GetById(reservationId))
                .Returns((Reservation)null);

            var ex = Assert.Throws<Exception>(() => _billingService.GenerateInvoice(reservationId));
            Assert.AreEqual("La reserva no existe.", ex.Message);
        }

        /// <summary>
        /// Verifica que GenerateInvoice lance una excepción cuando la reserva no tiene habitaciones asociadas.
        /// </summary>
        [Test]
        public void GenerateInvoice_ReservationWithoutRooms_ThrowsException()
        {
            int reservationId = 2;
            var reservation = new Reservation
            {
                ID = reservationId,
                TOTALPRICE = 100.0,
                STARTDATE = DateTime.UtcNow.AddDays(-1),
                ENDDATE = DateTime.UtcNow,
                ReservationRooms = new List<ReservationRoom>()
            };

            _reservationRepositoryMock.Setup(r => r.GetById(reservationId))
                .Returns(reservation);

            var ex = Assert.Throws<Exception>(() => _billingService.GenerateInvoice(reservationId));
            Assert.AreEqual("No hay habitaciones asociadas a esta reserva.", ex.Message);
        }
    }
}
