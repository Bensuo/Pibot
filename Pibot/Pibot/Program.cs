using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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
        private static Dictionary<long, VirtualPet> pets;
        private static int updateInterval = 10000;
        private static DateTime lastUpdate;
        static void Main(string[] args)
        {
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            //Bot.OnInlineQuery += BotOnInlineQueryReceived;
            //Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;
            lastUpdate = DateTime.Now;
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;
            bool running = true;
            string textInput = "";
            string[] splitInput;
            VirtualPet petToCheck;
            
            var t = new Timer(TimerCallback, null, 0, updateInterval);
            
            pets = new Dictionary<long, VirtualPet>();
            Bot.StartReceiving();
            while (running)
            {
                textInput = Console.ReadLine();
                splitInput = textInput.Split(' ');
                if (splitInput[0] == "checkpet")
                {
                    bool petFound = false;
                    foreach (var pet in pets)
                    {
                        if (pet.Value.Name == splitInput[1])
                        {
                            Console.Write("Name: " + pet.Value.Name);
                            Console.Write("\tHunger: " + pet.Value.Hunger);
                            Console.Write("\tHappiness: " + pet.Value.Happiness);
                            TimeSpan timeTillUpdate = new TimeSpan(0,0, 0,0, updateInterval) - (DateTime.Now - lastUpdate);
                            Console.Write("\tNextUpdate: " + timeTillUpdate.Minutes + "m" + timeTillUpdate.Seconds + "s");
                            Console.Write("\n");
                            petFound = true;
                        }
                    }
                    if (!petFound)
                    {
                        Console.WriteLine("No pet found with that name!");
                    }
                }
                else if (splitInput[0] == "quit")
                {
                    running = false;
                }
                else if (splitInput[0] == "status")
                {
                    TimeSpan timeTillUpdate = new TimeSpan(0, 0, 0, 0, updateInterval) - (DateTime.Now - lastUpdate);
                    Console.WriteLine(String.Format("There are currently {0} active pets! The next update will occur in {1}m{2}s", pets.Count, timeTillUpdate.Minutes, timeTillUpdate.Seconds));
                }
                else
                {
                    Console.WriteLine("Command not recognised!");
                }
            }
            Bot.StopReceiving();
        }

        private static async void TimerCallback(Object o)
        {
            foreach (var pet in pets)
            {
                string msg = "";
                pet.Value.Update();
                if (pet.Value.Happiness == 0 && pet.Value.Hunger == 10)
                {
                    msg = "I'm miserable and starving!";
                }
                else if (pet.Value.Happiness == 0)
                {
                    msg = "I'm miserable, play with me!";
                }
                else if (pet.Value.Hunger == 10)
                {
                    msg = "I'm starving, feed me!";
                }
                if (msg != "")
                {
                    await Bot.SendTextMessageAsync(pet.Value.ChatId, msg);
                }
            }


            lastUpdate = DateTime.Now;
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
            
            if (message == null || message.Type != MessageType.TextMessage) return;
            Console.WriteLine(message.Text);
            if (message.Text.StartsWith("@Pibot "))
            {
                string msg = message.Text;
                msg = msg.Remove(0, 7);
                string[] splitMessage = msg.Split(' ');
                //Check for commands
                if (splitMessage[0].Contains("!HatchEgg"))
                {
                    if (pets.ContainsKey(message.Chat.Id) == false)
                    {
                        if (splitMessage.Length > 1 && splitMessage[1] != "")
                        {
                            pets.Add(message.Chat.Id, new VirtualPet(splitMessage[1], message.Chat.Id));
                            await Bot.SendTextMessageAsync(message.Chat.Id, splitMessage[1] + " was born! Hooray!");
                        }
                        else
                        {
                            await Bot.SendTextMessageAsync(message.Chat.Id,
                                String.Format(
                                    "{0}, You must give your virtual pet a name!\n Type @Pibot !HatchEgg namehere",
                                    message.From.Username));
                        }

                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id,
                            String.Format(
                                "{0}, You already have a pet that loves you (probably)!",
                                message.From.Username));
                    }
                }
                else if (pets.ContainsKey(message.Chat.Id))
                {
                    if (splitMessage[0].Contains("!FeedPet"))
                    {
                        pets[message.Chat.Id].Feed(5);
                    }
                    else if (splitMessage[0].Contains("!PlayWithPet"))
                    {
                        pets[message.Chat.Id].PlayWith();
                    }
                }
                else
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Sorry, I don't understand that command!");
                }
                //if (msg.Contains("butts"))
                //{
                    
                //    await Bot.SendTextMessageAsync(message.Chat.Id, String.Format("{0}, I enjoy big butts and frankly cannot lie about it!", message.From.Username));
                    
                //}
                //else if (msg.Contains("awesome"))
                //{
                //    await Bot.SendStickerAsync(message.Chat.Id, "CAADBAAD-AEAAjjhGgABRCHT30HsyBgC");
                //}
                
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
                $"Received {callbackQueryEventArgs.CallbackQuery.Data}");
        }
    }
}
