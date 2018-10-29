﻿using Guts.Client.Shared.Utility;

namespace Guts.Client.Core
{
    public class LoginWindowFactory : ILoginWindowFactory
    {
        private readonly IHttpHandler _httpHandler;
        private readonly string _webAppBaseUrl;

        public LoginWindowFactory(IHttpHandler httpHandler, string webAppBaseUrl)
        {
            _httpHandler = httpHandler;
            _webAppBaseUrl = webAppBaseUrl;
        }

        public ILoginWindow Create()
        {
            return new LoginWindow(_httpHandler, _webAppBaseUrl);
        }
    }
}