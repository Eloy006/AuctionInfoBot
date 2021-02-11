using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommandTools;
using NoSqlTorgiGovRu;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TorgiGovRu_Bot;


namespace TradeInformationBot
{
    public class TradeBot:IDisposable
    {
        private static TelegramBotClient Bot;
        private static readonly string BotToken = "1654340584:AAF-bKl5HKvWblYRTADEuMdJAAclqK72L1g";
        private CancellationTokenSource BotCancellationToken;
        public async Task Start()
        {

#if USE_PROXY
            var Proxy = new WebProxy(Configuration.Proxy.Host, Configuration.Proxy.Port) { UseDefaultCredentials = true };
            Bot = new TelegramBotClient(Configuration.BotToken, webProxy: Proxy);
#else
            Bot = new TelegramBotClient(BotToken);
#endif

            var me = await Bot.GetMeAsync();
            Console.Title = me.Username;

            BotCancellationToken = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            Bot.StartReceiving(
                new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                BotCancellationToken.Token
            );

        }

        static async Task SendText(long chatId,string text)
        {
            
            await Bot.SendTextMessageAsync(
                chatId: chatId,
                text: text);
        }

        static async Task SendTextReplay(long chatId,int messageId, string text)
        {
            await Bot.SendTextMessageAsync(
                chatId: chatId,
                replyToMessageId:messageId,
                text: text
                
                );
        }


        public List<fullNotification> FindCadastr(string command)
        {

           
            CommandParser commandParser = new CommandParser();

            var parseData = commandParser.ParseCommand(BotCommandTask.CadastrCommands,
                command);
            using (var notification = new FullNotificationModel())
            {
                var findNotifications= notification.FindByСadastralNubmer(parseData[nameof(BotCommandTask.CadastrCommand.cadastr)]).ToList();
                return findNotifications;


            }

        }

        private async Task FindRequest(Message message,string command)
        {
            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            

            if (!command.Any())
            {
                await SendText(
                    message.Chat.Id,
                    text: "Синтаксис\r\n" +
                          "Поиск по кадастру 44:22233:222213"
                );
                return;
                
            }

            var ret=await Task.Run<List<fullNotification>>(() => FindCadastr(command));
            if (!ret.Any())
            {
                SendText(message.Chat.Id, "не найден");
                return;
            }
            
            foreach (var item in ret)
            {

               var lot= string.Join(Environment.NewLine,
                    item.notification.lot.Select(x =>
                        $"Кадастровый номер: {x.cadastralNum}{Environment.NewLine}" +
                        $"Площадь: {x.area*0.01} сот.{Environment.NewLine}" +
                        $"Местоположение: {x.location}{Environment.NewLine}"
                    
                        ));

                
                var sbBuilder = GetCommonText(item);


                var url= item.notification.common.notificationUrl;

            await SendTextReplay(message.Chat.Id, message.MessageId, $"{lot}{Environment.NewLine}{sbBuilder}{Environment.NewLine}{url}");


            }

            

        }

        private static StringBuilder GetCommonText(fullNotification item)
        {
            var sbBuilder = new StringBuilder();
            if (DateTime.TryParse(item.notification.common.startDateRequest, out var startDateRequest))
            {
                sbBuilder.AppendLine(
                    $"Дата и время начала приема заявок: {startDateRequest:dd.MM.yyyy hh:mm}");
            }

            if (DateTime.TryParse(item.notification.common.expireDate, out var expireDate))
            {
                sbBuilder.AppendLine($"Дата и время окончания приема заявок: {expireDate:dd.MM.yyyy hh:mm}");
            }


            if (DateTime.TryParse(item.notification.common.bidAuctionDate, out var bidAuctionDate))
            {
                sbBuilder.AppendLine($"Дата и время проведения аукциона: {bidAuctionDate:dd.MM.yyyy hh:mm}");
            }

            return sbBuilder;
        }


        public class MessageCommand
        {
            public string message { get; set; }
            public string command { get; set; }

            public MessageCommand(string message, string command)
            {
                this.message = message;
                this.command = command;
            }

        }

        private List<MessageCommand> MessageCommands=new List<MessageCommand>()
        {
            new MessageCommand("найти кадастровый номер","find cadastr"),
            new MessageCommand("найти","find cadastr"),
            new MessageCommand("оповестить кадастр","shedulle cadastr"),

        } ;

        

        private string СonvertMessageToCommand(string message)
        {
            var toConvert= MessageCommands.OrderByDescending(x=>x.message.Length);
            
            foreach (var command in toConvert)
            {
                message=message.Replace(command.message, command.command);

            }

            return message;
        }


        private  async Task BotOnMessageReceived(Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;
            
            var textMessage = СonvertMessageToCommand(message.Text);

            var action = (textMessage.Split(' ').First()) switch
            {
                
                "platform" => SendInlineKeyboard(message),
                "keyboard" => SendReplyKeyboard(message),
                "find" => FindRequest(message, textMessage),

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

        public  async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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

        public void Dispose()
        {
            BotCancellationToken.Cancel();
        }
    }
}
