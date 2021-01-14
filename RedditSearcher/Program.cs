using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Reddit;
using Reddit.Controllers;
using System.Collections.Generic;
using System.Threading;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;

namespace RedditSearcher
{
	public class Program
	{
		public static DiscordSocketClient client;
		public static RedditClient reddit;
		public static List<string> users = new List<string>();
		public static List<string> parameters = new List<string>();
		public static List<string> subreddits = new List<string>();
		public static ObservableCollection<Subreddit> sbObjs = new ObservableCollection<Subreddit>();
		public static SocketMessage lastMessage;
		public static ulong channelId;
		public static List<Post> NewPosts;
		public static DateTime uptime;

		public static void Main(string[] args)
			=> new Program().MainAsync().GetAwaiter().GetResult();

		public async Task MainAsync()
		{
			#region Init
			NewPosts = new List<Post>();
			sbObjs.CollectionChanged += Subreddit_added; ;
			client = new DiscordSocketClient();
			client.Log += Log;
			client.MessageReceived += SendPostMessage;
			uptime = DateTime.Now;

			reddit = new RedditClient(ConfigurationManager.AppSettings["appId"], ConfigurationManager.AppSettings["refreshToken"], ConfigurationManager.AppSettings["appSecret"]);
			DiscordHelper.reddit = reddit;
			#endregion

			await client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings["BotToken"]);
			await client.StartAsync();
			await Task.Delay(Timeout.Infinite);
		}
		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
		public Task SendPostMessage(SocketMessage message)
		{
			if (message.Author.IsBot)
				return Task.CompletedTask;
			if (message.Channel.Name != "bot-things")
				return Task.CompletedTask;
			if (message.Content.ToLower() == "!init")
			{
				channelId = message.Channel.Id;
				message.Channel.SendMessageAsync(text: $@"Search initalized in this channel by {message.Author.Mention}.");
				users.Add(message.Author.Mention);
				lastMessage = message;
				return Task.CompletedTask;
			}
			else if (message.Content.ToLower() == "!uptime")
			{
				channelId = message.Channel.Id;
				message.Channel.SendMessageAsync(text: $@"I have been running since {uptime.ToLongDateString()} {uptime.ToShortTimeString()}.");
				users.Add(message.Author.Mention);
				lastMessage = message;
				return Task.CompletedTask;
			}
			else if (message.Content.ToLower() == "!notify")
			{
				if (users.Contains(message.Author.Mention))
				{
					message.Channel.SendMessageAsync(text: $@"You have already been added to the list");
					lastMessage = message;
					return Task.CompletedTask;
				}
				else
				{
					message.Channel.SendMessageAsync(text: $@"{message.Author.Mention} has been added to the notify list.");
					users.Add(message.Author.Mention);
					lastMessage = message;
					return Task.CompletedTask;
				}
			}
			else if (message.Content.ToLower() == "!remove")
			{
				users.Remove(message.Author.Mention);
				message.Channel.SendMessageAsync(text: $@"You have been removed from the list");
				lastMessage = message;
				return Task.CompletedTask;
			}
			else if (DiscordHelper.Instance.SubString(message.Content.ToLower(),0,8) == "!addterm")
			{
				if (parameters.Contains(message.Content.Substring(9).Trim()))
				{
					message.Channel.SendMessageAsync(text: $@"Term ""{message.Content.Substring(8).Trim()}"" is already in the list.");
					lastMessage = message;
					return Task.CompletedTask;
				}
				else
				{
					message.Channel.SendMessageAsync(text: $@"Term ""{message.Content.Substring(8).Trim()}"" has been added as a search term.");
					if (!users.Contains(message.Author.Mention))
						users.Add(message.Author.Mention);
					parameters.Add(message.Content.Substring(9).Trim());
					lastMessage = message;
					return Task.CompletedTask;
				}
			}
			else if (DiscordHelper.Instance.SubString(message.Content.ToLower(), 0, 6) == "!addsb")
			{
				if (subreddits.Contains(message.Content.Substring(6).Trim()))
				{
					message.Channel.SendMessageAsync(text: $@"Subreddit ""{message.Content.Substring(6).Trim()}"" is already in the list.");
					lastMessage = message;
					return Task.CompletedTask;
				}
				else
				{
					message.Channel.SendMessageAsync(text: $@"Subreddit ""{message.Content.Substring(6).Trim()}"" has been added.");
					if (!users.Contains(message.Author.Mention))
						users.Add(message.Author.Mention);
					subreddits.Add(message.Content.Substring(6).Trim());
					sbObjs.Add(RedditHelper.Instance.AddSubbreddit(message.Content.Substring(7).Trim()));					
					lastMessage = message;
					return Task.CompletedTask;
				}
			}
			else if (DiscordHelper.Instance.SubString(message.Content.ToLower(), 0, 9) == "!removesb")
			{
				string sb = message.Content.Substring(10).Trim();
				if (subreddits.Contains(sb))
				{
					subreddits.Remove(sb);
					for (int i = sbObjs.Count - 1; i >= 0; i--)
					{
						Subreddit subreddit = sbObjs[i];
						if (subreddit.Name == sb)
						{
							sbObjs.Remove(subreddit);
							subreddit.Posts.MonitorNew();
							subreddit.Posts.NewUpdated -= RedditHelper.NewPostUpdate;
						}
					}
					message.Channel.SendMessageAsync(text: $@"Subreddit ""{sb}"" has been deleted.");
					return Task.CompletedTask;
				}
				else
					message.Channel.SendMessageAsync(text: $@"Subreddit ""{sb}"" is not in the list.");
			}
			else if (DiscordHelper.Instance.SubString(message.Content.ToLower(), 0, 11) == "!removeterm")
			{
				string sb = message.Content.Substring(12).Trim();
				if (parameters.Contains(sb))
				{
					parameters.Remove(sb);
					message.Channel.SendMessageAsync(text: $@"Term ""{sb}"" has been deleted.");
					return Task.CompletedTask;
				}
				else
					message.Channel.SendMessageAsync(text: $@"Term ""{sb}"" is not in the list.");
			}
			else if (message.Content.ToLower() == "!terms")
			{
				message.Channel.SendMessageAsync(text: $"The current search terms are:\n{String.Join(",\n", parameters)}");
			}
			else if (message.Content.ToLower() == "!subreddits")
			{
				message.Channel.SendMessageAsync(text: $"The current search terms are:\n{String.Join(",\n", subreddits)}");
			}
			return Task.CompletedTask;
		}
		public async void Subreddit_added(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(e.NewItems != null)
			{
				foreach (Subreddit sb in e.NewItems)
				{
					sb.Posts.GetNew();
					sb.Posts.MonitorNew();
					sb.Posts.NewUpdated += RedditHelper.NewPostUpdate;
				}
			}
			await Task.Delay(Timeout.Infinite);
		}
	}
}
