using Discord;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedditSearcher
{
	public sealed class RedditHelper
	{
		private static readonly RedditHelper instance = new RedditHelper();

		private RedditHelper() { }

		public static RedditHelper Instance
		{
			get
			{
				return instance;
			}
		}
		public static List<Post> CheckSubbredditByTitle(Subreddit subreddit, string keyword)
		{
			List<Post> posts = new List<Post>();
			string after = "";
			DateTime start = DateTime.Now;
			DateTime today = DateTime.Today;
			bool outdated = false;
			do
			{
				foreach (Post post in subreddit.Posts.GetNew(after: after))
				{
					if (post.Created >= today)
					{
						if (post.Title.Contains(keyword))
							posts.Add(post);
					}
					else
					{
						outdated = true;
						break;
					}

					after = post.Fullname;
				}
			} while (!outdated
				&& start.AddMinutes(5) > DateTime.Now
				&& subreddit.Posts.New.Count > 0);
			return posts;
		}
		public void NewPostUpdate(object sender, PostsUpdateEventArgs e)
		{
			foreach (Post p in e.NewPosts)
			{
				if (p.Title.Contains("Spaceview"))
				{
					EmbedBuilder eb = DiscordHelper.Instance.CreateMessage(p);
					message.Channel.SendMessageAsync(text: eb.Description, embed: eb.Build());

				}
			}

		}


	}
}
