using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Reddit;
using Reddit.Controllers;
using System.Collections.Generic;
using Reddit.Controllers.EventArgs;
using System.Threading;

namespace RedditSearcher
{
	public class Program
	{
		private DiscordSocketClient client;
		private String refreshToken;
		private String appSecret;
		private String id;
		private RedditClient reddit;
		public static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			#region Init
			client = new DiscordSocketClient();
			client.Log += Log;
			client.MessageReceived += DiscordHelper.Instance.SendPostMessage;
			refreshToken = "55241894-8eAx-kCjXtm2dyDSosVf22w2KFd0uA";
			appSecret = "N884os551RMO8KTrZnRIylt4G6lg5g";
			id = "JS1knwXwC_epBg";

			reddit = new RedditClient(id, refreshToken, appSecret);
			DiscordHelper.reddit = reddit;
			#endregion

			var token = Environment.GetEnvironmentVariable("BotToken");
			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			var we = reddit.Subreddit("WatchExchange");
			var pc = reddit.Subreddit("Buildapcsales");
			we.Posts.GetNew(); 
			we.Posts.MonitorNew();
			we.Posts.NewUpdated += RedditHelper.Instance.NewPostUpdate;



			await Task.Delay(Timeout.Infinite);
		}
		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
	}
}
