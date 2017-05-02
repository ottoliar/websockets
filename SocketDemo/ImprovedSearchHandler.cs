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
    public class ImprovedSearchHandler : WebSocketHandler
    {
        private IConfiguration _configuration;

        public ImprovedSearchHandler(IWebSocketConnectionManager socketManager,
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
                connection.Open();
                try
                {
                    var results = new List<SearchResult>();
                    int monthCounter = 1;
                    int queryMonth = DateTime.Now.Month;
                    var tableId = Guid.NewGuid().ToString();

                    string[] parsedSearch = message.Split(' ');

                    while (true)
                    {
                        var sb = new StringBuilder();
                        sb.Append(" SELECT TOP 5 ValuationId,UpdateDate,DataSource,WordBag FROM dbo.WordBagCurrent with(nolock) WHERE dbo.WordBagCurrent.Month=");
                        sb.Append(queryMonth);

                        foreach (string term in parsedSearch)
                        {
                            sb.Append(" AND WordBag like '%" + term + "%'");
                        }
                        sb.Append(" ORDER BY UpdateDate DESC; ");

                        SqlCommand cmd2 = new SqlCommand()
                        {
                            CommandText = sb.ToString(),
                            CommandType = System.Data.CommandType.Text,
                            Connection = connection
                        };
                        using (var reader = cmd2.ExecuteReader())
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

                                var result = match.ToString();
                                await InvokeClientMethodToAllAsync("receiveMessage", socketId, result);
                            }
                        }

                        if (monthCounter == 12 || results.Count >= 5)
                        {
                            break;
                        }
                        else
                        {
                            monthCounter++;
                            queryMonth--;
                            if (queryMonth == 0) queryMonth = 12;
                        }
                    }

                    connection.Close();

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
