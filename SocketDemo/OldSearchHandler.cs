using Microsoft.Extensions.Configuration;
using SocketDemo.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WebSocketManager;
using WebSocketManager.Common;

namespace SocketDemo
{
    public class OldSearchHandler : WebSocketHandler
    {
        private IConfiguration _configuration;

        public OldSearchHandler(IWebSocketConnectionManager socketManager,
                             IConfiguration configuration) : base(socketManager)
        {
            _configuration = configuration;
        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);

            var socketId = _webSocketConnectionManager.GetId(socket);

            var message = new Message()
            {
                MessageType = MessageType.Text,
                Data = $"{socketId} is now connected"
            };

            await SendMessageToAllAsync(message);
        }

        public async Task SendMessage(string socketId, string message)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("WordBag")))
            {
                var sb = new StringBuilder();

                sb.Append("SELECT TOP 5 ValuationId,UpdateDate,DataSource,WordBag FROM dbo.OldWordBagCurrent with(nolock) WHERE 1 = 1");
                string[] parsedSearch = message.Split(' ');
                foreach (string term in parsedSearch)
                {
                    sb.Append(" AND CONTAINS(WordBag, '" + term + "*')");
                }
                sb.Append(" ORDER BY UpdateDate DESC; ");
                var command = sb.ToString();

                try
                {
                    var results = new List<SearchResult>();

                    SqlCommand cmd = new SqlCommand()
                    {
                        CommandText = command,
                        CommandType = System.Data.CommandType.Text,
                        Connection = connection
                    };

                    connection.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var match = new SearchResult()
                            {
                                DataSource = (string)reader["DataSource"],
                                UpdateDate = (DateTime)reader["UpdateDate"],
                                ValuationId = (int)reader["ValuationId"],
                                WordBag = (string)reader["WordBag"]
                            };


                            await InvokeClientMethodToAllAsync("receiveMessage", socketId, match.ToString());
                            //results.Add(match);
                        }
                    }

                    //foreach (var result in results)
                    //{
                    //    await InvokeClientMethodToAllAsync("receiveMessage", socketId, result.ToString());
                    //}

                }
                catch (Exception ex)
                {

                }
            }

        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            var socketId = _webSocketConnectionManager.GetId(socket);

            await base.OnDisconnected(socket);

            var message = new Message()
            {
                MessageType = MessageType.Text,
                Data = $"{socketId} disconnected"
            };

            await SendMessageToAllAsync(message);
        }
    }
}
