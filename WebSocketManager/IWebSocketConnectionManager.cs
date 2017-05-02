using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebSocketManager
{
    public interface IWebSocketConnectionManager
    {
        void AddSocket(WebSocket socket);
        ConcurrentDictionary<string, WebSocket> GetAll();
        string GetId(WebSocket socket);
        WebSocket GetSocketById(string id);
        Task RemoveSocket(string id);
    }
}