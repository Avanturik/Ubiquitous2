using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Model;

namespace UB.Model
{
    public class ChatToFileService : NotifyPropertyChangeBase, IService
    {
        public ChatToFileService(ServiceConfig config)
        {
            Config = config;
            Status = new StatusBase();
            AppendText(String.Format("Log started: {0}", DateTime.Now.ToString()));
        }

        public bool Start()
        {
            return true;
        }

        public bool Stop()
        {
            return true;
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        public Action AfterStart
        {
            get;
            set;

        }

        public ServiceConfig Config
        {
            get;
            set;

        }

        public StatusBase Status
        {
            get;
            set;
        }

        public void GetData(Action<object> callback)
        {

        }

        public void AddToHistory( ChatMessage message )
        {
            if (!Config.Enabled)
                return;

            var text = String.Format("[{0}] {1} {2}, {3}: {4}",message.TimeStamp, message.ChatName, message.Channel, message.FromUserName, message.OriginalText);
            AppendText(text);
        }

        private void AppendText( string text )
        {
            Status.LastError = String.Empty;
            var fileName = Config.GetParameterValue("Filename") as string;
            if( String.IsNullOrWhiteSpace(fileName))
            {
                Status.LastError = "Error: filename missing";
                return;
            }
            try
            {
                File.AppendAllText(fileName, text + Environment.NewLine);
            }
            catch (Exception e)
            {
                Status.LastError = String.Format("Error: {0}", e.Message);
                Log.WriteError("History couldn't be saved: {0}", e.Message);
            }
        }
    }
}
