using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Net;

namespace IpBlockerNetcore.Services
{
    public class IpBasedRequestSender
    {
        private readonly string _ipAddress;

        public IpBasedRequestSender(string ipAddress)
        {
            _ipAddress = ipAddress;
        }

        public async Task<HttpResponseMessage> SendRequestAsync(string url, string apiKey, string ip)
        {
            var handler = new SocketsHttpHandler
            {
                ConnectCallback = async (context, cancellationToken) =>
                {
                    var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(new IPEndPoint(IPAddress.Parse(_ipAddress), 0));
                    await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Key", apiKey);
          
            // İstek gönderme
            var response = await client.GetAsync($"{url}?ipAddress={ip}&verbose=");

            //esponse.EnsureSuccessStatusCode();
            return response; // HttpResponseMessage döndürülüyor
        }
    }
}
