﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public interface IStreamPageDataService
    {
        void GetStreamTopics( Action<List<IStreamTopic>> callback);
        void GetPresets(Action<List<StreamInfoPreset>> callback);
        void LoadTopicsFromWeb();
        void UpdateTopicsOnWeb();
        void RemovePreset(StreamInfoPreset preset);
        StreamInfoPreset AddPreset(string presetName);
    }
}
