using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Operations.Classification.GererMesComptes
{
    public class GererMesComptesClient : IDisposable
    {
        public GererMesComptesClient(Func<HttpMessageHandler, HttpMessageHandler> wrapClientHandler)
        {
            var cookieContainer = new CookieContainer();
            HttpMessageHandler handler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true
            };

            if (wrapClientHandler != null)
            {
                handler = wrapClientHandler(handler);
            }

            Transport = new HttpClient(handler) { BaseAddress = new Uri("https://www.gerermescomptes.com/") };
            Transport.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json,text/javascript,*/*;q=0.01");

            ////_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            Transport.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            Transport.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "fr-FR,fr;q=0.8,en-US;q=0.6,en;q=0.4,nl;q=0.2");
            Transport.DefaultRequestHeaders.TryAddWithoutValidation(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36");
            Transport.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
            Transport.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://www.gerermescomptes.com/");  
        }

        public GererMesComptesClient()
            : this(null)
        {
        }

        public HttpClient Transport { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<bool> Connect(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            var getResponse = await Transport.GetAsync("/fr/connexion.html");
            getResponse.EnsureSuccessStatusCode();

            var dico = new Dictionary<string, string>
                           {
                               ["action_form"] = "connect",
                               ["goto"] = string.Empty,
                               ["email"] = userName,
                               ["pass"] = password,
                               ["connection"] = "Se connecter >"
                           };

            var response = await Transport.PostAsync("/fr/connexion.html", new FormUrlEncodedContent(dico));
            response.EnsureSuccessStatusCode();

            return true;
        }

        public async Task<bool> Disconnect()
        {
            var getResponse = await Transport.GetAsync("/fr/deconnexion.html");
            getResponse.EnsureSuccessStatusCode();
            var unused = getResponse.RequestMessage.RequestUri.AbsolutePath;

            var isConnected = await IsConnected();
            return !isConnected;
        }

        public async Task<bool> IsConnected()
        {
            var getResponse = await Transport.GetAsync("/fr/connexion.html");
            getResponse.EnsureSuccessStatusCode();
            var location = getResponse.RequestMessage.RequestUri.AbsolutePath;

            return location.Equals("/fr/u/finances/");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Transport?.Dispose();
            }
        }
    }
}