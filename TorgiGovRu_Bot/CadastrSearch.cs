using System;
using System.Collections.Generic;
using System.Text;

namespace TorgiGovRu_Bot
{
    public class BotCommandTask
    {
     
        public enum CadastrCommand
        {
            cadastr, 
            region, 
            publishdate
        }
        
        public static string[] CadastrCommands = new[] { nameof(CadastrCommand.cadastr), nameof(CadastrCommand.region), nameof(CadastrCommand.publishdate) };

        public string Guid { get; set; }
        public string chatId { get; set; }
        public string command { get; set; }
        public int Complite { get; set; }

    }
}
