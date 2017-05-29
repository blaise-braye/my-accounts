using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Operations.Classification.GererMesComptes
{
    public class GererMesComptesClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public GererMesComptesClient()
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
                              {
                                  CookieContainer = cookieContainer,
                                  AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                                  AllowAutoRedirect = true
                              };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://www.gerermescomptes.com/") };
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json,text/javascript,*/*;q=0.01");
            
            ////_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "fr-FR,fr;q=0.8,en-US;q=0.6,en;q=0.4,nl;q=0.2");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://www.gerermescomptes.com/");
        }

        public HttpClient Transport => _httpClient;

        public async Task<bool> Connect(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            var getResponse = await _httpClient.GetAsync("/fr/connexion.html");
            getResponse.EnsureSuccessStatusCode();

            var dico = new Dictionary<string, string>();
            dico["action_form"] = "connect";
            dico["goto"] = string.Empty;
            dico["email"] = userName;
            dico["pass"] = password;
            dico["connection"] = "Se connecter >";

            var response = await _httpClient.PostAsync("/fr/connexion.html", new FormUrlEncodedContent(dico));
            response.EnsureSuccessStatusCode();

            return true;
        }

        public async Task<bool> Disconnect()
        {
            var getResponse = await _httpClient.GetAsync("/fr/deconnexion.html");
            getResponse.EnsureSuccessStatusCode();
            var location = getResponse.RequestMessage.RequestUri.AbsolutePath;

            var isConnected = await IsConnected();
            return !isConnected;
        }

        public async Task<bool> IsConnected()
        {
            var getResponse = await _httpClient.GetAsync("/fr/connexion.html");
            getResponse.EnsureSuccessStatusCode();
            var location = getResponse.RequestMessage.RequestUri.AbsolutePath;
            
            return location.Equals("/fr/u/finances/");
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
        }
    }
}
