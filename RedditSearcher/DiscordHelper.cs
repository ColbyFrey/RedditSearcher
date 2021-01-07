using Discord;
using Discord.WebSocket;
using Reddit;
using Reddit.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedditSearcher
{
	public class DiscordHelper
	{
		private static readonly DiscordHelper instance = new DiscordHelper();

		private DiscordHelper() { }

		public static DiscordHelper Instance
		{
			get
			{
				return instance;
			}
		}
		public static RedditClient reddit;
		public Task SendPostMessage(SocketMessage message)
		{
			if (message.Author.IsBot)
				return Task.CompletedTask;
			if (message.Channel.Name != "bot-things")
				return Task.CompletedTask;
			if (message.Content.ToLower() != "!check")
				return Task.CompletedTask;

			#region Watch Exchange
			List<Post> we = RedditHelper.CheckSubbredditByTitle(reddit.Subreddit("WatchExchange"), "Spaceview");

			foreach (Post post in we)
			{
				EmbedBuilder url = new EmbedBuilder();
				url.Url = $@"https://www.reddit.com{post.Permalink}";
				url.Title = post.Title;
				message.Channel.SendMessageAsync(text: $@"Post found on /r/WatchExchange at {post.Created.ToShortTimeString()}: {post.Title}.", embed: url.Build());
			}
			if (we.Count == 0)
				message.Channel.SendMessageAsync("No Posts found on /r/WatchExchange for SpaceView.");
			#endregion

			#region BuildAPcSales

			List<Post> bpc = RedditHelper.CheckSubbredditByTitle(reddit.Subreddit("Buildapcsales"), "Laptop");
			foreach (Post post in bpc)
			{
				EmbedBuilder url = new EmbedBuilder();
				url.Url = $@"https://www.reddit.com{post.Permalink}";
				url.Title = post.Title;
				message.Channel.SendMessageAsync(text: $@"Post found on /r/buildapcsales at {post.Created.ToShortTimeString()}: {post.Title}.", embed: url.Build());
			}
			if (we.Count == 0)
				message.Channel.SendMessageAsync("No Posts found on /r/buildapcsales for Laptop.");
			#endregion

			return Task.CompletedTask;
		}

		public void StopChecking()
		{

		}
		public EmbedBuilder CreateMessage(Post post)
		{
			EmbedBuilder url = new EmbedBuilder();
			url.Url = $@"https://www.reddit.com{post.Permalink}";
			url.Title = post.Title;
			url.Description = $@"Post found on /r/WatchExchange at {post.Created.ToShortTimeString()}";
			return url;
		}

	}
}
