using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface IChatChannel
    {
        Action<CybergameChannel> JoinCallback { get; set; }
        Action<CybergameChannel> LeaveCallback { get; set; }
        Action<ChatMessage> ReadMessage { get; set; }
        string ChannelName { get; set; }
        IChat Chat { get; set; }
        ChannelStats ChannelStats { get; set; }
        string NickName { get; set; }

        void SetupStatsWatcher();
        void Join(Action<IChatChannel> callback, string channel);
        void Leave();
        void SendMessage(ChatMessage message);
    }
}
