using QuickFix;
using QuickFix.Fields;
using OrderAccumulator.Models;
using Microsoft.Extensions.Logging;

namespace SimpleAcceptor    
{
    public class FixReceiverApp : IApplication
    {
        static Dictionary<string, StockSymbol> allSymbols = new Dictionary<string, StockSymbol>();
        private readonly ILogger<FixReceiverApp> _logger;
        
        public FixReceiverApp() 
        {
            fillSymbols();
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _logger = loggerFactory.CreateLogger<FixReceiverApp>();
        }

        public void fillSymbols() 
        {
            allSymbols.Add("PETR4", new StockSymbol { BusinessCode = "PETR4", Exposition = 0, Limit = 100000000m });
            allSymbols.Add("VALE3", new StockSymbol { BusinessCode = "VALE3", Exposition = 0, Limit = 1000m });
            allSymbols.Add("VIIA4", new StockSymbol { BusinessCode = "VIIA4", Exposition = 0, Limit = 100000000m });
        }        
  
        public void FromApp(Message message, SessionID sessionID)
        {
            QuickFix.FIX44.ExecutionReport exReport = new QuickFix.FIX44.ExecutionReport();
            _logger.LogInformation("FromApp:  " + message);
            try 
            {
                if(message is QuickFix.FIX44.NewOrderSingle newOrderMessage) {
                    bool isOrderValid = validateOrder(newOrderMessage);
                    exReport = new QuickFix.FIX44.ExecutionReport(
                        new OrderID(((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString()),
                        new ExecID(((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString()),
                        isOrderValid ? new ExecType(ExecType.NEW) : new ExecType(ExecType.REJECTED),
                        isOrderValid ? new OrdStatus(OrdStatus.NEW) :  new OrdStatus(OrdStatus.REJECTED),
                        exReport.Symbol = newOrderMessage.Symbol,
                        new Side(Side.BUY),
                        exReport.LeavesQty = new LeavesQty(1),
                        new CumQty(newOrderMessage.OrderQty.Value),
                        new AvgPx(newOrderMessage.Price.Value));
                    Session.SendToTarget(exReport, sessionID);
                    if(isOrderValid) 
                    {
                        allSymbols[newOrderMessage.Symbol.Value].Exposition += defineNewLimit(newOrderMessage);
                    }
  
                }
            }
            catch(Exception e) 
            {
                _logger.LogInformation("Erro ao tratar mensagem: " + e.Message);
            }


        }

        public Decimal calculateOrderAmount(QuickFix.FIX44.NewOrderSingle order)
        {
            return order.Price.Value * order.OrderQty.Value;
        }

        public Decimal defineNewLimit(QuickFix.FIX44.NewOrderSingle order)
        {
            return allSymbols[order.Symbol.Value].Exposition + calculateOrderAmount(order);
        }

        public bool validateOrder(QuickFix.FIX44.NewOrderSingle order) 
        {
            if (!allSymbols.ContainsKey(order.Symbol.Value)) 
            {
                return false;
            }
          

            if (order.Side.Value == Side.BUY && defineNewLimit(order) > allSymbols[order.Symbol.Value].Limit)
            {
                return false;
            }            

            return true;

        }

        public void ToApp(Message message, SessionID sessionID)
        {
            _logger.LogInformation("ToApp: " + message);
        }

        public void FromAdmin(Message message, SessionID sessionID) 
        {
            _logger.LogInformation("FromAdmin:  " + message);
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            _logger.LogInformation("ToAdmin:  " + message);
        }

        public void OnCreate(SessionID sessionID) { }
        public void OnLogout(SessionID sessionID) { }
        public void OnLogon(SessionID sessionID) { }
    }
}