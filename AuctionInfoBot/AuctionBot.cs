using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandTools;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NoSqlTorgiGovRu;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TorgiGovRu_Bot;

namespace AuctionInfoBot
{
    public class AuctionBot
    {
        private readonly ILogger<AuctionBot> _logger;

        private readonly List<MessageCommand> _messageCommands = new List<MessageCommand>
        {
            new MessageCommand("найти кадастровый номер", "find cadastr"),
            new MessageCommand("найти", "find cadastr"),
            new MessageCommand("оповестить кадастр", "shedulle cadastr")
        };

        private readonly Options _options;
        private TelegramBotClient _bot;

        public AuctionBot(IOptions<Options> options, ILogger<AuctionBot> logger)
        {
            _logger = logger;
            _options = options.Value;
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            if (_options.Proxy.UseProxy)
            {
                var credentials = !string.IsNullOrWhiteSpace(_options.Proxy.UserName) && !string.IsNullOrWhiteSpace(_options.Proxy.Password)
                    ? new NetworkCredential(_options.Proxy.UserName, _options.Proxy.Password)
                    : CredentialCache.DefaultCredentials;

                var proxy = new WebProxy(_options.Proxy.Url, true, null, credentials);
                _bot = new TelegramBotClient(_options.Token, proxy);
            }
            else
            {
                _bot = new TelegramBotClient(_options.Token);
            }

            var me = await _bot.GetMeAsync(cancellationToken);
            _logger.LogInformation($"ME: {me.Username}");

            _bot.ReceiveAsync(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), cancellationToken);
        }

        private async Task SendText(long chatId, string text)
        {
            await _bot.SendTextMessageAsync(
                chatId,
                text);
        }

        private async Task SendTextReplay(long chatId, int messageId, string text)
        {
            await _bot.SendTextMessageAsync(
                chatId,
                replyToMessageId: messageId,
                text: text
            );
        }


        public List<fullNotificationNotificationLot> FindCadastr(string command)
        {
            var commandParser = new CommandParser();

            var parseData = commandParser.ParseCommand(BotCommandTask.CadastrCommands, command);
            if (parseData.Count == 0) return new List<fullNotificationNotificationLot>();
            using (var notification = new NotificationLotModel())
            {
                var findNotifications = notification.FindByСadastralNum(parseData[nameof(BotCommandTask.CadastrCommand.cadastr)]).ToList();
                return findNotifications;
            }
        }

        private async Task FindRequest(Message message, string command)
        {
            await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);


            if (!command.Any())
            {
                await SendText(
                    message.Chat.Id,
                    "Синтаксис\r\n" +
                    "Поиск по кадастру 44:22233:222213"
                );
                return;
            }

            var ret = await Task.Run(() => FindCadastr(command));
            if (!ret.Any())
            {
                SendText(message.Chat.Id, "не найден");
                return;
            }

            foreach (var item in ret)
            {
                var lot = string.Join(Environment.NewLine,
                    $"Кадастровый номер: {item.cadastralNum}{Environment.NewLine}" +
                    $"Площадь: {item.area * 0.01} сот.{Environment.NewLine}" +
                    $"Местоположение: {item.location}{Environment.NewLine}"
                );


                var sbBuilder = GetCommonText(item.fullNotification);


                var url = item.fullNotification.notification.common.notificationUrl;

                await SendTextReplay(message.Chat.Id, message.MessageId, $"{lot}{Environment.NewLine}{sbBuilder}{Environment.NewLine}{url}");
            }
        }

        private static StringBuilder GetCommonText(fullNotification item)
        {
            var sbBuilder = new StringBuilder();
            if (DateTime.TryParse(item.notification.common.startDateRequest, out var startDateRequest))
                sbBuilder.AppendLine(
                    $"Дата и время начала приема заявок: {startDateRequest:dd.MM.yyyy hh:mm}");

            if (DateTime.TryParse(item.notification.common.expireDate, out var expireDate))
                sbBuilder.AppendLine($"Дата и время окончания приема заявок: {expireDate:dd.MM.yyyy hh:mm}");


            if (DateTime.TryParse(item.notification.common.bidAuctionDate, out var bidAuctionDate))
                sbBuilder.AppendLine($"Дата и время проведения аукциона: {bidAuctionDate:dd.MM.yyyy hh:mm}");

            return sbBuilder;
        }


        private string СonvertMessageToCommand(string message)
        {
            var toConvert = _messageCommands.OrderByDescending(x => x.message.Length);

            foreach (var command in toConvert)
                message = message.Replace(command.message, command.command);

            return message;
        }


        private async Task BotOnMessageReceived(Message message)
        {
            if (message == null) return;

            Console.WriteLine($"Receive message type: {message.Type}");

            if (message.Type != MessageType.Text)
                return;

            var textMessage = СonvertMessageToCommand(message.Text);

            var action = textMessage.Split(' ').First() switch
            {
                "platform" => SendInlineKeyboard(message),
                "keyboard" => SendReplyKeyboard(message),
                "find" => FindRequest(message, textMessage),
                _ => SendText(message.Chat.Id, "Команда не найдена")
            };
            await action;

            // Send inline keyboard
            // You can process responses in BotOnCallbackQueryReceived handler
            async Task SendInlineKeyboard(Message message)
            {
                await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                // Simulate longer running task
                await Task.Delay(500);

                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    // first row
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("torgi.gov.ru", "platform_TorgiGov")
                        //InlineKeyboardButton.WithCallbackData("1.2", "12"),
                    } /*,
                    // second row
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22"),
                    }*/
                });
                await _bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "Выберите платформу торгов",
                    replyMarkup: inlineKeyboard
                );
            }

            async Task SendReplyKeyboard(Message message)
            {
                var replyKeyboardMarkup = new ReplyKeyboardMarkup(
                    new[]
                    {
                        new KeyboardButton[] {"1.1", "1.2"},
                        new KeyboardButton[] {"2.1", "2.2"}
                    },
                    true
                );

                await _bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "Choose",
                    replyMarkup: replyKeyboardMarkup
                );
            }
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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

        //public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        //{
        //    var ErrorMessage = exception switch
        //    {
        //        ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        //        _ => exception.ToString()
        //    };

        //    Console.WriteLine(ErrorMessage);
        //}


        public class MessageCommand
        {
            public MessageCommand(string message, string command)
            {
                this.message = message;
                this.command = command;
            }

            public string message { get; set; }
            public string command { get; set; }
        }
    }
}