using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Claims;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens;

using System.Net;
using System.Net.Http;

// The following using statements were added for this sample.
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using ArticleWebApp.Utils;
using System.IO;

namespace ArticleWebApp.Controllers
{
    public class HomeController : Controller
    {
        private IDistributedCache DistributedCache = null;

        public HomeController(IDistributedCache distributedCache)
        {
            DistributedCache = distributedCache;
        }

        public IActionResult Index()
        {
            var nameIdentifier = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var authContext = new AuthenticationContext("https://adfs2016.contoso.local/adfs", false, new DistributedTokenCache(DistributedCache, nameIdentifier));
            var clientId = "Your_Client_ID";
            var clientSecret = "Your_Client_Secret";
            var credential = new ClientCredential(clientId, clientSecret);
            
            var backendtokenTask = authContext.AcquireTokenSilentAsync("https://webapi.contoso.local", credential, new UserIdentifier(nameIdentifier,
                UserIdentifierType.UniqueId));
            backendtokenTask.Wait();

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://webapi.contoso.local");
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", backendtokenTask.Result.AccessToken);

            var requestTask = httpClient.GetAsync("api/values");
            requestTask.Wait();
            var result = requestTask.Result;
            var streamTask = result.Content.ReadAsStreamAsync();
            streamTask.Wait();

            using (StreamReader reader = new StreamReader(streamTask.Result))
            {
                var values = reader.ReadToEnd();

                return View(model: values);
            }
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
