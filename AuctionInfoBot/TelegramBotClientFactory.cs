using System.Net;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace AuctionInfoBot
{
    public class TelegramBotClientFactory
    {
        private readonly Options _options;

        public TelegramBotClientFactory(IOptions<Options> options)
        {
            _options = options.Value;
        }

        public TelegramBotClient CreateTelegramBotClient()
        {
            if (!_options.Proxy.UseProxy)
                return new TelegramBotClient(_options.Token);

            var credentials = !string.IsNullOrWhiteSpace(_options.Proxy.UserName) && !string.IsNullOrWhiteSpace(_options.Proxy.Password)
                ? new NetworkCredential(_options.Proxy.UserName, _options.Proxy.Password)
                : CredentialCache.DefaultCredentials;

            var proxy = new WebProxy(_options.Proxy.Url, true, null, credentials);
            return new TelegramBotClient(_options.Token, proxy);
        }
    }
}