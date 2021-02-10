using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TradeInformationBot.XsdClasses.TorgiGovRu;


namespace TradeInformationBot
{
    class Program
    {
        private static TelegramBotClient Bot;
        private static readonly string BotToken = "1654340584:AAF-bKl5HKvWblYRTADEuMdJAAclqK72L1g";
        public static async Task Main()
        {

#if USE_PROXY
            var Proxy = new WebProxy(Configuration.Proxy.Host, Configuration.Proxy.Port) { UseDefaultCredentials = true };
            Bot = new TelegramBotClient(Configuration.BotToken, webProxy: Proxy);
#else
            Bot = new TelegramBotClient(BotToken);
#endif

            var me = await Bot.GetMeAsync();
            Console.Title = me.Username;

            var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            Bot.StartReceiving(
                new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                cts.Token
            );

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
        }

        static async Task SendText(long chatId,string text)
        {
            await Bot.SendTextMessageAsync(
                chatId: chatId,
                text: text);
        }

        
        
        public static  void LoadOdDetail(openDataNotification openDataNotification,string cadastrNum)
        {


            XmlClient xmlClient=new XmlClient();


            var notifications =  xmlClient.GetObjectFromXml<fullNotification>(openDataNotification.odDetailedHref);
            openDataNotification.NotificationGround = notifications;
          
        }

        public static void LoadOdDetailSafe(openDataNotification openDataNotification, string cadastrNum)
        {
            int countRun = 5;
            

            while (true)
            {
                try
                {
                    LoadOdDetail(openDataNotification, cadastrNum);
                    return;
                }
                catch
                {

                    Task.Delay(5000);

                }

                if (--countRun == 0)
                {

                    break;
                    
                }
            }

            
        }

        static async Task FindCadastre(Message message,IEnumerable<string> cadastreArgs)
        {
            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            
            var publishDataFromString= cadastreArgs.FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? "";
            var cadastrenumber = cadastreArgs.FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? "";
            if (!DateTime.TryParse(publishDataFromString, out var publishDataFrom))
            {
                publishDataFrom = DateTime.Now.AddDays(-1);
            }
            else
            {
                cadastrenumber=cadastreArgs.Skip(1).FirstOrDefault(x => !string.IsNullOrEmpty(x)) ?? "";
            }
            

            


            if(cadastrenumber!=null) cadastrenumber= Regex.Replace(cadastrenumber, @"\r\n?|\n", "");
            //await SendText(message.Chat.Id, cadastrenumber);
            XmlClient client=new XmlClient();
            publishDataFromString = publishDataFrom.ToString("yyyyMMddTHHmmss"); //Regex.Replace(publishDataFrom.ToString("s"), @"-?:?", "");
            var uriCall =
                $"https://torgi.gov.ru/opendata/7710349494-torgi/data.xml?bidKind=2&publishDateFrom={publishDataFromString}";
           var ret= client.GetObjectFromXml<openData>(uriCall).notification.Where(x=>x.organizationName.Contains("КАМЫШИН", StringComparison.InvariantCultureIgnoreCase)) ;

           if (!ret.Any())
           {
               await SendText(message.Chat.Id, "Обьекты не найдены");
               return;
            }

            Parallel.ForEach(ret, new ParallelOptions(){MaxDegreeOfParallelism = 2}, (openData)=>
                LoadOdDetailSafe(openData, cadastrenumber));

            
           var items=ret.Where(x => 
               x.NotificationGround.notification.lot.Any(x=>x.cadastralNum!=null && x.cadastralNum.Contains(cadastrenumber,StringComparison.InvariantCultureIgnoreCase)));

           if (!items.Any())
           {
               await SendText(message.Chat.Id, "Обьекты не найдены");
               return;
           }

           if (string.IsNullOrEmpty(cadastrenumber))
           {

               foreach (var item in items)
               {
                   await SendText(message.Chat.Id,
                       
                       string.Join(Environment.NewLine,item.NotificationGround.notification.lot.Select(x=> $"{x.cadastralNum}\r\n{x.description}\r\n{x.area}\r\n{x.location}")));
                   

               }
               return;
            }


           foreach (var item in items)
           {
               await SendText(message.Chat.Id, 
                   item.NotificationGround.notification.common.notificationUrl + "\r\n" + string.Join(Environment.NewLine, item.NotificationGround.notification.documents.Select(x => x.docUrl + "\r\n")));
               

           }


        }



        static async Task FindRequest(Message message)
        {
            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            // Simulate longer running task
            await Task.Delay(500);

            var arguments=message.Text.Split(' ').Skip(1);

            if (!arguments.Any())
            {
                await SendText(
                    message.Chat.Id,
                    text: "Синтаксис\r\n" +
                          "Поиск по кадастру /find cadastre 44:22233:222213"
                );
                return;
                
            }

            switch (arguments.First())
            {
                case "cadastre":
                    await FindCadastre(message, arguments.Skip(1));
                    break;
                case "address":
                    break;
                    


            }

            

        }

        private static async Task BotOnMessageReceived(Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;

            var action = (message.Text.Split(' ').First()) switch
            {
                "/platform" => SendInlineKeyboard(message),
                "/keyboard" => SendReplyKeyboard(message),
                "/find" => FindRequest(message),

            };
            await action;

            // Send inline keyboard
            // You can process responses in BotOnCallbackQueryReceived handler
            static async Task SendInlineKeyboard(Message message)
            {
                await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                // Simulate longer running task
                await Task.Delay(500);

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    // first row
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("torgi.gov.ru", "platform_TorgiGov"),
                        //InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    }/*,
                    // second row
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    }*/
                });
                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Выберите платформу торгов",
                    replyMarkup: inlineKeyboard
                );
            }

            static async Task SendReplyKeyboard(Message message)
            {
                var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                    new KeyboardButton[][]
                    {
                        new KeyboardButton[] {"1.1", "1.2"},
                        new KeyboardButton[] {"2.1", "2.2"},
                    },
                    resizeKeyboard: true
                );

                await Bot.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Choose",
                    replyMarkup: replyKeyboardMarkup

                );
            }
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.Message),
               // UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
               // UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery),
               // UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult),
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                _ => UnknownUpdateHandlerAsync(update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }
        private static async Task UnknownUpdateHandlerAsync(Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
        }
    }
}
