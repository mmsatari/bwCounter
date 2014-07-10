using System;
using System.Net;

namespace bwCounter
{
	/// <summary>
	///     A custom WebClient featuring a cookie container
	/// </summary>
	internal class WebClientEx : WebClient
	{
		public WebClientEx()
		{
			CookieContainer = new CookieContainer();
		}

		public CookieContainer CookieContainer { get; private set; }

		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest request = base.GetWebRequest(address);
			if (request is HttpWebRequest)
			{
				(request as HttpWebRequest).CookieContainer = CookieContainer;
			}
			return request;
		}
	}
}