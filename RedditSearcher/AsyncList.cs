using Reddit.Controllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedditSearcher
{
	public class AsyncList<T> : List<T>
	{
        public event EventHandler OnAdd;

        public new void Add(T item) // "new" to avoid compiler-warnings, because we're hiding a method from base-class
        {
            if (null != OnAdd)
            {
                OnAdd(this, null);
            }
            base.Add(item);
        }
    }
}
