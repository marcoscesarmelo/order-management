using QuickFix;
using QuickFix.Fields;
using ApplicationException = System.ApplicationException;
using Exception = System.Exception;

namespace Utils
{
    public class FixUtilApp : MessageCracker, IApplication
    {
        private Session? _session = null;

        public QuickFix.FIX44.NewOrderSingle sampleMessage = new QuickFix.FIX44.NewOrderSingle();

        public static char orderStatus = ' ';

        public IInitiator? MyInitiator = null;

        private readonly ILogger<FixUtilApp> _logger;

        public FixUtilApp()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _logger = loggerFactory.CreateLogger<FixUtilApp>();
        }

        public void OnCreate(SessionID sessionId)
        {
            _session = Session.LookupSession(sessionId);
            if (_session is null)
                throw new ApplicationException("Somehow session is not found");
        }

        public void OnLogon(SessionID sessionId) 
        { 
            _logger.LogInformation("Logon - " + sessionId);
        }
        public void OnLogout(SessionID sessionId) 
        { 
            _logger.LogInformation("Logout - " + sessionId); 
        }

        public void FromAdmin(Message message, SessionID sessionId) { 
            _logger.LogInformation("Mensagem adm recebida do receiver: " + message);
        }
        public void ToAdmin(Message message, SessionID sessionId) { }

        public void FromApp(Message message, SessionID sessionId)
        {
            _logger.LogInformation("Mensagem recebida do receiver: " + message);
            QuickFix.FIX44.ExecutionReport execReport = (QuickFix.FIX44.ExecutionReport)message;
            orderStatus = execReport.OrdStatus.Value;
        }

        public void ToApp(Message message, SessionID sessionId)
        {
            try
            {
                bool possDupFlag = false;
                if (message.Header.IsSetField(Tags.PossDupFlag))
                {
                    possDupFlag = message.Header.GetBoolean(Tags.PossDupFlag);
                }
                if (possDupFlag)
                    throw new DoNotSend();
            }
            catch (FieldNotFoundException)
            { }

        }

        public void Run()
        {
            if (MyInitiator is null)
                throw new ApplicationException("Somehow this.MyInitiator is not set");
            
            QueryEnterOrder();
        }

        private void SendMessage(Message m)
        {
            if (_session is  null)  
            {
                _logger.LogInformation("Can't send message: session not created.");
            }
            else 
            {
                _session.Send(m);
                Thread.Sleep(3000);
                _session.Send(m);
            }
        }

        private void QueryEnterOrder()
        {
            QuickFix.FIX44.NewOrderSingle m = QueryNewOrderSingle44();

            if (m is not null)
            {
                SendMessage(m);
            }
        }

        private QuickFix.FIX44.NewOrderSingle QueryNewOrderSingle44()
        {
            OrdType ordType = SetOrdType();
            QuickFix.FIX44.NewOrderSingle newOrderSingle = new QuickFix.FIX44.NewOrderSingle(
                SetClOrdId(),
                SetSymbol(),
                SetSide(),
                new TransactTime(DateTime.Now),
                ordType);

            newOrderSingle.Set(new HandlInst('1'));
            newOrderSingle.Set(SetOrderQty());
            newOrderSingle.Set(SetTimeInForce());
            if (ordType.Value == OrdType.LIMIT || ordType.Value == OrdType.STOP_LIMIT)
                newOrderSingle.Set(SetPrice());
            return newOrderSingle;
        }

        private ClOrdID SetClOrdId()
        {
            return new ClOrdID(((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString());
        }

        private Symbol SetSymbol()
        {
            return new Symbol(sampleMessage.Symbol.Value);
        }

        private Side SetSide()
        {
            string s =  sampleMessage.Side.Value.ToString();
            char c = ' ';
            switch (s)
            {
                case "1": c = Side.BUY; break;
                case "2": c = Side.SELL; break;
                case "3": c = Side.SELL_SHORT; break;
                case "4": c = Side.SELL_SHORT_EXEMPT; break;
                case "5": c = Side.CROSS; break;
                case "6": c = Side.CROSS_SHORT; break;
                case "7": c = 'A'; break;
                default: throw new Exception("unsupported input");
            }
            return new Side(c);
        }

        private OrdType SetOrdType()
        {
            return new OrdType(OrdType.LIMIT);
        }

        private OrderQty SetOrderQty()
        {
            return new OrderQty(Convert.ToDecimal(sampleMessage.OrderQty.Value));
        }

        private TimeInForce SetTimeInForce()
        {
            return new TimeInForce(TimeInForce.DAY);
        }

        private Price SetPrice()
        {
            return new Price(Convert.ToDecimal(sampleMessage.Price.Value));
        }
    }
}