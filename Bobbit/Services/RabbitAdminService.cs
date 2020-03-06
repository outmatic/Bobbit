using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Bobbit.Services.Model;
using RestSharp;
using RestSharp.Authenticators;

namespace Bobbit.Services
{
    public interface IRabbitAdminService
    {
        Task<bool> CanConnectAsync(ConnectOptions connectOptions);
        Task<List<VirtualHost>> GetVirtualHosts(ConnectOptions connectOptions);
        Task<List<Queue>> GetQueues(ConnectOptions connectOptions, string vhost = null);
        List<Message> FetchMessages(ConnectOptions connectOptions, string vhost, string queue, HashSet<ulong> messagesToBeDeleted = null);
    }

    public class RabbitAdminService : IRabbitAdminService
    {
        public async Task<bool> CanConnectAsync(ConnectOptions connectOptions)
        {
            var client = BuildRestClient(connectOptions);
            var request = new RestRequest("overview");
            var response = await client.ExecuteGetAsync(request);

            return response.IsSuccessful;
        }

        public async Task<List<VirtualHost>> GetVirtualHosts(ConnectOptions connectOptions)
        {
            var client = BuildRestClient(connectOptions);
            var request = new RestRequest("vhosts");
            var response = await client.ExecuteGetAsync<List<VirtualHost>>(request);

            return response.Data;
        }

        public async Task<List<Queue>> GetQueues(ConnectOptions connectOptions, string vhost)
        {
            var client = BuildRestClient(connectOptions);
            var request = new RestRequest("queues");

            var response = await client.ExecuteGetAsync<List<Queue>>(request);

            return response.Data;
        }

        public List<Message> FetchMessages(ConnectOptions connectOptions, string vhost, string queue, HashSet<ulong> messagesToBeDeleted)
        {
            var factory = new ConnectionFactory
            {
                UserName = connectOptions.UserName,
                Password = connectOptions.Password,
                VirtualHost = vhost,
                HostName = connectOptions.Host,
                Port = connectOptions.AmqpPort,
            };

            using var connection = factory.CreateConnection();

            var messages = CreateChannelAndFetch(connection, queue, messagesToBeDeleted);

            return messages;
        }

        private IRestClient BuildRestClient(ConnectOptions connectOptions)
        {
            var baseUri = new UriBuilder("http", connectOptions.Host, connectOptions.HttpApiPort).Uri;

            return new RestClient
            {
                BaseUrl = new Uri(baseUri, "api"),
                Authenticator = new HttpBasicAuthenticator(connectOptions.UserName, connectOptions.Password)
            };
        }

        private List<Message> CreateChannelAndFetch(IConnection connection, string queue, HashSet<ulong> messagesToBeDeleted)
        {
            using var channel = connection.CreateModel();
            var result = channel.BasicGet(queue, false);

            var messages = new List<Message>();
            while (result != null && messages.Count <= 100)
            {
                if (messagesToBeDeleted?.Contains(result.DeliveryTag) == true)
                    channel.BasicAck(result.DeliveryTag, false);
                else
                {
                    Encoding encoding;
                    try
                    {
                        encoding = Encoding.GetEncoding(result.BasicProperties.ContentEncoding);
                    }
                    catch (ArgumentException)
                    {
                        encoding = Encoding.Default;
                    }

                    messages.Add(new Message
                    {
                        Body = encoding.GetString(result.Body),
                        Type = result.BasicProperties.Type,
                        DeliveryTag = result.DeliveryTag
                    });
                }

                result = channel.BasicGet(queue, false);
            }

            return messages;
        }
    }
}
