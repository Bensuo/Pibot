using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputMessageContents;
using Telegram.Bot.Types.ReplyMarkups;
namespace Pibot
{
    class Program
    {

        private static readonly TelegramBotClient Bot = new TelegramBotClient("378850449:AAGV-GnSAslC5JOVpSX2l8G3W1AeDWnV8vY");

        static void Main(string[] args)
        {
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            var me = Bot.GetMeAsync().Result;
            
            Console.Title = me.Username;

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Debugger.Break();
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received choosen inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            InlineQueryResult[] results = {
                new InlineQueryResultLocation
                {
                    Id = "1",
                    Latitude = 40.7058316f, // displayed result
                    Longitude = -74.2581888f,
                    Title = "New York",
                    InputMessageContent = new InputLocationMessageContent // message if result is selected
                    {
                        Latitude = 40.7058316f,
                        Longitude = -74.2581888f,
                    }
                },

                new InlineQueryResultLocation
                {
                    Id = "2",
                    Longitude = 52.507629f, // displayed result
                    Latitude = 13.1449577f,
                    Title = "Berlin",
                    InputMessageContent = new InputLocationMessageContent // message if result is selected
                    {
                        Longitude = 52.507629f,
                        Latitude = 13.1449577f
                    }
                }
            };

            await Bot.AnswerInlineQueryAsync(inlineQueryEventArgs.InlineQuery.Id, results, isPersonal: true, cacheTime: 0);
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            Console.WriteLine(message.Text);
            if (message == null || message.Type != MessageType.TextMessage) return;

            if (message.Text.StartsWith("@Pibot"))
            {
                string msg = message.Text.ToLower();
                if (msg.Contains("butts"))
                {
                    
                    await Bot.SendTextMessageAsync(message.Chat.Id, String.Format("{0}, I enjoy big butts and frankly cannot lie about it!", message.From.Username));
                    
                }
                else if (msg.Contains("awesome"))
                {
                    await Bot.SendStickerAsync(message.Chat.Id, "CAADBAAD-AEAAjjhGgABRCHT30HsyBgC");
                }
                
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }
    }
}
