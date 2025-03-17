using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

namespace SimpleAcceptor
{
    class Program
    {

        public Program() 
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
        }

        [STAThread]
        static void Main(string[] args)
        {

            try
            {
                SessionSettings settings = new SessionSettings("sampleacc.cfg");
                IApplication app = new FixReceiverApp();
                IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
                ILogFactory logFactory = new FileLogFactory(settings);
                IAcceptor acceptor = new ThreadedSocketAcceptor(app, storeFactory, settings, logFactory);
                acceptor.Start();
                Console.WriteLine("Conexão FIX iniciada. Aguardando requisições...");
                Console.WriteLine("Pressione <Enter> para encerrar a aplicação.");
                Console.ReadLine();
                acceptor.Stop();
                Console.WriteLine("Conexão FIX finalizada.");          
            }
            catch (Exception e)
            {
                Console.WriteLine("==FATAL ERROR==");
                Console.WriteLine(e.ToString());
            }
        }
    }
}