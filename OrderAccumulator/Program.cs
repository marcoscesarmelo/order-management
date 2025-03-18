using Microsoft.Extensions.Logging;
using QuickFix;
using QuickFix.Logger;
using QuickFix.Store;

namespace SimpleAcceptor
{
    class Program
    {
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
                Console.WriteLine("Pressione <Enter> para encerrar a aplicação.");
                Console.ReadLine();
                acceptor.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine("==FATAL ERROR==");
                Console.WriteLine(e.ToString());
            }
        }
    }
}