using QuickFix;
using QuickFix.FIX44;
using QuickFix.Fields;
using QuickFix.Store;
using OrderGenerator.Models;
using QuickFix.Logger;
using Utils;
using QuickFix.Transport;
using OrderGenerator.Exceptions;

namespace Fix
{
    public class FixUtil
    {
        public const char STATUS_REJECTED = '8';

        private readonly ILogger<FixUtil> _logger;

        public FixUtil()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _logger = loggerFactory.CreateLogger<FixUtil>();
        }

        public void SendOrder(Order order)
        {
        try
                {
                    SessionSettings settings = new SessionSettings("config.cfg");
                    FixUtilApp application = new FixUtilApp();
                    IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
                    ILogFactory logFactory = new ScreenLogFactory(settings);
                    SocketInitiator initiator = new SocketInitiator(application, new FileStoreFactory(settings), settings);

                    application.MyInitiator = initiator;
                    application.sampleMessage = GenerateFixMessage(order);

                    initiator.Start();
                    application.Run();
                    initiator.Stop();
                    if(FixUtilApp.orderStatus == STATUS_REJECTED) 
                    {
                        throw new OrderAccumulatorException("Excedido limite de exposições para símbolo");
                    }
                }
                catch (OrderAccumulatorException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.Message);
                }
        }

        private NewOrderSingle GenerateFixMessage(Order order)
        {
            NewOrderSingle newOrderSingle = new NewOrderSingle(
                setClOrdId(),
                new Symbol(order.Symbol),
                new Side(order.Side == "Compra" ? Side.BUY : Side.SELL),
                new TransactTime(DateTime.Now),
                new OrdType(OrdType.LIMIT));

            newOrderSingle.Set(new HandlInst('1'));
            newOrderSingle.Set(new OrderQty(Convert.ToDecimal(order.Quantity)));
            newOrderSingle.Set(new TimeInForce(TimeInForce.DAY));
            newOrderSingle.Set(new Price(order.Price));

            return newOrderSingle;
        }

        public ClOrdID setClOrdId() {        
            DateTime currentDateTime = DateTime.Now;
            long timestamp = ((DateTimeOffset)currentDateTime).ToUnixTimeSeconds();       
            return new ClOrdID(timestamp.ToString());
        }

    }
}
