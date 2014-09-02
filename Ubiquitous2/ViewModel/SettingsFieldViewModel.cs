using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;

namespace UB.ViewModel
{
    public class SettingsFieldViewModel : ViewModelBase
    {

        /// <summary>
        /// The <see cref="DataType" /> property's name.
        /// </summary>
        public const string DataTypePropertyName = "DataType";

        private String _dataType = "Text";

        /// <summary>
        /// Sets and gets the DataType property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public String DataType
        {
            get
            {
                return _dataType;
            }

            set
            {
                if (_dataType == value)
                {
                    return;
                }

                RaisePropertyChanging(DataTypePropertyName);
                _dataType = value;
                RaisePropertyChanged(DataTypePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Text" /> property's name.
        /// </summary>
        public const string TextPropertyName = "Text";

        private String _text = "Text";

        /// <summary>
        /// Sets and gets the Text property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public String Text
        {
            get
            {
                return _text;
            }

            set
            {
                if (_text == value)
                {
                    return;
                }

                RaisePropertyChanging(TextPropertyName);
                _text = value;
                RaisePropertyChanged(TextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="LabelText" /> property's name.
        /// </summary>
        public const string LabelTextPropertyName = "LabelText";

        private String _labelText = "Label";

        /// <summary>
        /// Sets and gets the LabelText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public String LabelText
        {
            get
            {
                return _labelText ;
            }

            set
            {
                if (_labelText  == value)
                {
                    return;
                }

                RaisePropertyChanging(LabelTextPropertyName);
                _labelText  = value;
                RaisePropertyChanged(LabelTextPropertyName);
            }
        }
    }
}
