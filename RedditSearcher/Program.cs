using Discord;
using Discord.WebSocket;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Reddit;

namespace RedditSearcher
{
	public class Program
	{
		private DiscordSocketClient client;
		public static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();
		 
		public async Task MainAsync()
		{
			client = new DiscordSocketClient();
			client.Log += Log;
			client.MessageReceived += SendTestMessage;

			var reddit = new RedditClient("SubScraper", "ZUgLWsr12zKnsH_p2BksRaI7VHI");

			var token = Environment.GetEnvironmentVariable("BotToken");

			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			await Task.Delay(-1);
		}
		private Task Log(LogMessage msg) 
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
		private Task SendTestMessage(SocketMessage message)
		{
			if (message.Author.IsBot)
				return Task.CompletedTask;
			if (message.Channel.Name != "bot-things")
				return Task.CompletedTask;

			message.Channel.SendMessageAsync($@"Fuck you {message.Author.Mention}.");
			return Task.CompletedTask;
		}
	}
}
