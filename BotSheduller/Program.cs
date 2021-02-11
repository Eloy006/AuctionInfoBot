using System;
using System.Threading.Tasks;

namespace BotSheduller
{
    class Program
    {

        
        static async Task Main(string[] args)
        {
            Console.WriteLine("Start sheduller...");

            await Task.Run(Sheduller);

        }
        public static void Sheduller()
        {

        }
    }


    
}
