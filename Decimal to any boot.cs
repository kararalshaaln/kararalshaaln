using System;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;




namespace DecimalToAny_Bot
{
    class Program
    {
        private static TelegramBotClient? Bot;
        static string msg;

        public static async Task Main()
        {
            Bot = new TelegramBotClient("5009290783:AAGfaF1gz--hNX5fby4GucXZ4tMQQyIoRz4");

            User me = await Bot.GetMeAsync();
            Console.Title = me.Username ?? "Decimal To Any Base Bot";
            using var cts = new CancellationTokenSource();

            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Bot.StartReceiving(HandleUpdateAsync,
                               HandleErrorAsync,
                               receiverOptions,
                               cancellationToken: cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
                UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
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

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");
            if (message.Type != MessageType.Text)
                return;


            var action = message.Text switch
            {
                "/help" or "/start" => help(botClient, message),
                _ => SendInlineKeyboard(botClient, message)
            };
            Message sentMessage = await action;
            Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");


            static async Task<Message> help(ITelegramBotClient botClient, Message message)
            {


                return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                            text:
                                                                  "/help - Get help\n" +
                                                                  "Enter the decimal number you want to convert.\n",
                                                            replyMarkup: new ReplyKeyboardRemove());
            }



            static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
            {
                msg = message.Text;

                var isNumeric = int.TryParse(message.Text, out int n);
                if (isNumeric)
                {

                    await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);


                    // Simulate longer running task
                    await Task.Delay(500);

                    InlineKeyboardMarkup inlineKeyboard = new(
                        new[]
                        {
                    // first row
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData(text: "Binary", callbackData: "2"),
                    },
                    // second row
                    new []
                        {
                            InlineKeyboardButton.WithCallbackData(text: "Octal", callbackData: "8"),
                        },

                        new []
                        {
                            InlineKeyboardButton.WithCallbackData(text: "hexadecimal", callbackData: "16"),
                        },
                        });

                    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                                text: "Choose the target base:",
                                                                replyMarkup: inlineKeyboard);

                }
               
                    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                                                                text: "Make sure you write the number correctly !!!\nPlease Try again.");

                
            }



            
        }



        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }



        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }


        private static async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery)
        {
            Console.WriteLine($"Received inline query from: {inlineQuery.From.Id}");

            InlineQueryResult[] results = {
            // displayed result
            new InlineQueryResultArticle(
                id: "3",
                title: "TgBots",
                inputMessageContent: new InputTextMessageContent(
                    "hello"
                )
            )
        };

            await botClient.AnswerInlineQueryAsync(inlineQueryId: inlineQuery.Id,
                                                   results: results,
                                                   isPersonal: true,
                                                   cacheTime: 0);
        }

        private static Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId}");
            return Task.CompletedTask;
        }


        // Process Inline Keyboard callback data
        private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            Console.WriteLine(callbackQuery.Message.Text);
            string res = calculateBase(callbackQuery.Data);


            /*
            await botClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQuery.Id,
                text: $"Received {res}");*/

            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.Message.Chat.Id,
                text: "("+msg + ") in base (" + callbackQuery.Data +") = " +res);
        }


        static string calculateBase(string b)
        {
            string res = "Error";

            int Im = Convert.ToInt32(msg);
            switch (b)
            {
                case "2":
                    Console.WriteLine(Im);
                    return Convert.ToString(Im, 2);
                case "8":
                    return Convert.ToString(Im, 8);
                case "16":
                    return Convert.ToString(Im, 16);
                default:
                    return "Make sure you write the number correctly !!!\nPlease Try again.";

            }

            return res;
        }

    }
}
