using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace UB.Model
{
    [ServiceContract]
    public interface IOBSPluginService
    {
        [OperationContract]
        ImageData GetImage();
        [OperationContract]
        ImageData GetFirstImage();
    }
}
