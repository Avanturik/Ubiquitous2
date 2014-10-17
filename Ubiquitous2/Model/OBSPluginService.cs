using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using UB.Utils;

namespace UB.Model
{
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    //[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    //[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    
    public class OBSPluginService : IOBSPluginService   
    {
        private ServiceHost serviceHost;
        private object lockSave = new object();
        
        [DataMember]
        private static RenderTargetBitmap renderTarget;
        
        public byte[] GetImage()
        {
            //Log.WriteInfo("ElementToOBSPlugin: image requested");
            lock (lockSave)
            {
                if (RenderTarget != null)
                {
                    WriteableBitmap wb = new WriteableBitmap(RenderTarget);
                    return wb.ConvertToByteArray();
                }
                else
                {
                    return null;
                }

            }
        }
        [DataMember]
        public RenderTargetBitmap RenderTarget {
            get { 
                lock(lockSave)
                    return renderTarget;             
            }
            set
            {
                lock (lockSave)
                    renderTarget = value;
            }
        
        }
        public void Start()
        {
            serviceHost = new ServiceHost(
              typeof(OBSPluginService),
              new Uri[]{
                new Uri("net.pipe://localhost")
              });

            serviceHost.AddServiceEndpoint(typeof(IOBSPluginService),
              new NetNamedPipeBinding(),
              "ImageSource");

            serviceHost.Open();
        }

        public void Stop()
        {
            serviceHost.Close();
        }
    }
}
