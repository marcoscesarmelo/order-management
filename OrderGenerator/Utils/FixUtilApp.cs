using QuickFix;
using QuickFix.Fields;
using ApplicationException = System.ApplicationException;
using Exception = System.Exception;

namespace Utils
{
    public class FixUtilApp : MessageCracker, IApplication
    {
        private Session? _session = null;

        public QuickFix.FIX44.NewOrderSingle? sampleMessage;

        public static char orderStatus = ' ';

        public IInitiator? MyInitiator = null;

        public static bool firstMessage = true;

        #region IApplication interface overrides

        public void OnCreate(SessionID sessionId)
        {
            _session = Session.LookupSession(sessionId);
            if (_session is null)
                throw new ApplicationException("Somehow session is not found");
        }

        public void OnLogon(SessionID sessionId) { 
            Console.WriteLine("Logon - " + sessionId);
        }
        public void OnLogout(SessionID sessionId) { Console.WriteLine("Logout - " + sessionId); }

        public void FromAdmin(Message message, SessionID sessionId) { 
            Console.WriteLine("Mensagem adm recebida do receiver: " + message);
        }
        public void ToAdmin(Message message, SessionID sessionId) { }

        public void FromApp(Message message, SessionID sessionId)
        {
            Console.WriteLine("Mensagem recebida do receiver: " + message);
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

            Console.WriteLine();
        }
        #endregion


        #region MessageCracker handlers
        public void OnMessage(QuickFix.FIX44.ExecutionReport m, SessionID s)
        {
            string report =  m.ExecType.getValue().ToString();
            Console.WriteLine("Received execution report: " + report);
        }

        public void OnMessage(QuickFix.FIX44.OrderCancelReject m, SessionID s)
        {
            Console.WriteLine("Received order cancel reject");
        }
        #endregion


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
                Console.WriteLine("Can't send message: session not created.");
            }
            else 
            {
                _session.Send(m);
                if(firstMessage) 
                {
                    Thread.Sleep(3000);
                    _session.Send(m);
                }
                firstMessage = false;
            }

        }

        private static string ReadCommand() {
            string? inp = Console.ReadLine();
            if (inp is null)
                throw new ApplicationException("Input no longer available");
            return inp.Trim();
        }

        private void QueryEnterOrder()
        {

            QuickFix.FIX44.NewOrderSingle m = QueryNewOrderSingle44();

            if (m is not null && QueryConfirm("Send order"))
            {
                m.Header.GetString(Tags.BeginString);

                SendMessage(m);
            }
        }

        private bool QueryConfirm(string query)
        {
            Console.WriteLine();
            string line = "y";
            return (line[0].Equals('y') || line[0].Equals('Y'));
        }

        #region Message creation functions
        private QuickFix.FIX44.NewOrderSingle QueryNewOrderSingle44()
        {
            OrdType ordType = QueryOrdType();

            QuickFix.FIX44.NewOrderSingle newOrderSingle = new QuickFix.FIX44.NewOrderSingle(
                QueryClOrdId(),
                QuerySymbol(),
                QuerySide(),
                new TransactTime(DateTime.Now),
                ordType);

            newOrderSingle.Set(new HandlInst('1'));
            newOrderSingle.Set(QueryOrderQty());
            newOrderSingle.Set(QueryTimeInForce());
            if (ordType.Value == OrdType.LIMIT || ordType.Value == OrdType.STOP_LIMIT)
                newOrderSingle.Set(QueryPrice());
            if (ordType.Value == OrdType.STOP || ordType.Value == OrdType.STOP_LIMIT)
                newOrderSingle.Set(QueryStopPx());

            return newOrderSingle;
        }

        private QuickFix.FIX44.OrderCancelRequest QueryOrderCancelRequest44()
        {
            QuickFix.FIX44.OrderCancelRequest orderCancelRequest = new QuickFix.FIX44.OrderCancelRequest(
                QueryOrigClOrdId(),
                QueryClOrdId(),
                QuerySymbol(),
                QuerySide(),
                new TransactTime(DateTime.Now));

            orderCancelRequest.Set(QueryOrderQty());
            return orderCancelRequest;
        }

        private QuickFix.FIX44.OrderCancelReplaceRequest QueryCancelReplaceRequest44()
        {
            QuickFix.FIX44.OrderCancelReplaceRequest ocrr = new QuickFix.FIX44.OrderCancelReplaceRequest(
                QueryOrigClOrdId(),
                QueryClOrdId(),
                QuerySymbol(),
                QuerySide(),
                new TransactTime(DateTime.Now),
                QueryOrdType());

            ocrr.Set(new HandlInst('1'));
            if (QueryConfirm("New price"))
                ocrr.Set(QueryPrice());
            if (QueryConfirm("New quantity"))
                ocrr.Set(QueryOrderQty());

            return ocrr;
        }

        #endregion

        #region field query private methods
        private ClOrdID QueryClOrdId()
        {
            Console.WriteLine();
            return new ClOrdID(((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString());
        }

        private OrigClOrdID QueryOrigClOrdId()
        {
            Console.WriteLine();
            return new OrigClOrdID(ReadCommand());
        }

        private Symbol QuerySymbol()
        {
            Console.WriteLine();
            return new Symbol(sampleMessage.Symbol.Value);
        }

        private Side QuerySide()
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

        private OrdType QueryOrdType()
        {
            string s = "2";

            char c = ' ';
            switch (s)
            {
                case "1": c = OrdType.MARKET; break;
                case "2": c = OrdType.LIMIT; break;
                case "3": c = OrdType.STOP; break;
                case "4": c = OrdType.STOP_LIMIT; break;
                default: throw new Exception("unsupported input");
            }
            return new OrdType(c);
        }

        private OrderQty QueryOrderQty()
        {
            Console.WriteLine();
            return new OrderQty(Convert.ToDecimal(sampleMessage.OrderQty.Value));
        }

        private TimeInForce QueryTimeInForce()
        {
            string s = "1";

            char c = ' ';
            switch (s)
            {
                case "1": c = TimeInForce.DAY; break;
                case "2": c = TimeInForce.IMMEDIATE_OR_CANCEL; break;
                case "3": c = TimeInForce.AT_THE_OPENING; break;
                case "4": c = TimeInForce.GOOD_TILL_CANCEL; break;
                case "5": c = TimeInForce.GOOD_TILL_CROSSING; break;
                default: throw new Exception("unsupported input");
            }
            return new TimeInForce(c);
        }

        private Price QueryPrice()
        {
            Console.WriteLine();
            return new Price(Convert.ToDecimal(sampleMessage.Price.Value));
        }

        private StopPx QueryStopPx()
        {
            Console.WriteLine();
            return new StopPx(Convert.ToDecimal(ReadCommand()));
        }

        #endregion
    }
}