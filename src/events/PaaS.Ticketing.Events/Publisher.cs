using CloudNative.CloudEvents;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace PaaS.Ticketing.Events
{
    public class Publisher : IDisposable
    {
        private string _connectionString;
        private readonly IQueueClient _queueClient;

        public Publisher()
        { }

        public Publisher(string queueName, string connectionString)
        {
            if (!String.IsNullOrEmpty(connectionString))
            {
                _connectionString = connectionString;
            }
               
            _queueClient = new QueueClient(_connectionString, queueName);
        }

        public async Task SendMessagesAsync(string cloudEvent)
        {
            var message = new Message(Encoding.UTF8.GetBytes(cloudEvent));
            await _queueClient.SendAsync(message);
        }

        public void Dispose()
        {
            _queueClient.CloseAsync();
        }
    }
}
