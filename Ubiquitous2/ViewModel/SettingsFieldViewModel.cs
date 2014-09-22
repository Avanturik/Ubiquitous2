using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Devart.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using UB.Model;

namespace UB.ViewModel
{
    public class SettingsFieldViewModel : ViewModelBase, IHeightMeasurer
    {

        /// <summary>
        /// The <see cref="DataType" /> property's name.
        /// </summary>
        public const string DataTypePropertyName = "DataType";
        private ConfigField _configField;
        private String _dataType;

        [PreferredConstructor]
        public SettingsFieldViewModel()
        {
        }
        public SettingsFieldViewModel(ConfigField configField)
        {
            _configField = configField;
            _labelText = configField.Label;
            _dataType = configField.DataType;
            switch (configField.DataType.ToLower())
            {
                case "range":
                    var textRange = configField.Value as string;
                    var fields = textRange.Split(';').ToArray();
                    if( fields.Length == 4 )
                    {
                        _minimumRange = double.Parse(fields[0], CultureInfo.InvariantCulture);
                        _maximumRange = double.Parse(fields[1], CultureInfo.InvariantCulture);
                        _smallRangeStep = double.Parse(fields[2], CultureInfo.InvariantCulture);
                        _largeRangeStep = double.Parse(fields[3], CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        _minimumRange = 0;
                        _maximumRange = 1;
                        _smallRangeStep = 0.1;
                        _largeRangeStep = 1;
                        if (_configField != null)
                            _configField.Value = RangeToString();
                    }
                    break;
                case "filesave":
                    _text = configField.Value as string;
                    break;
                case "bool":
                    _isTrue = (bool)configField.Value;
                    break;
                case "text":
                    _text = configField.Value as string;
                    break;
                case "password":
                    _text = configField.Value as string;
                    break;
            }
        }

        private string RangeToString()
        {
            return String.Format("{0};{1};{2};{3}", _minimumRange, _maximumRange, _smallRangeStep, _largeRangeStep);
        }

        private RelayCommand _saveFileDialog;

        /// <summary>
        /// Gets the SaveFileDialog.
        /// </summary>
        public RelayCommand SaveFileDialog
        {
            get
            {
                return _saveFileDialog
                    ?? (_saveFileDialog = new RelayCommand(
                                          () =>
                                          {
                                              Microsoft.Win32.SaveFileDialog fileDialog = new Microsoft.Win32.SaveFileDialog();
                                              fileDialog.FileName = "ubiquitouschat";
                                              fileDialog.DefaultExt = ".png";
                                              fileDialog.Filter = "PNG image (.png)|*.png";
                                              var result = fileDialog.ShowDialog();
                                              if( result == true )
                                              {
                                                  Text = fileDialog.FileName;
                                              }
                                          }));
            }
        }

        /// <summary>
        /// The <see cref="SmallRangeStep" /> property's name.
        /// </summary>
        public const string SmallRangeStepPropertyName = "SmallRangeStep";

        private double _smallRangeStep = 0;

        /// <summary>
        /// Sets and gets the SmallRangeStep property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double SmallRangeStep
        {
            get
            {
                return _smallRangeStep;
            }

            set
            {
                if (_smallRangeStep == value)
                {
                    return;
                }

                RaisePropertyChanging(SmallRangeStepPropertyName);
                _smallRangeStep = value;
                if (_configField != null)
                    _configField.Value = RangeToString();
                RaisePropertyChanged(SmallRangeStepPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="LargeRangeStep" /> property's name.
        /// </summary>
        public const string LargeRangeStepPropertyName = "LargeRangeStep";

        private double _largeRangeStep = 1;

        /// <summary>
        /// Sets and gets the LargeRangeStep property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double LargeRangeStep
        {
            get
            {
                return _largeRangeStep;
            }

            set
            {
                if (_largeRangeStep == value)
                {
                    return;
                }

                RaisePropertyChanging(LargeRangeStepPropertyName);
                _largeRangeStep = value;
                if (_configField != null)
                    _configField.Value = RangeToString();
                RaisePropertyChanged(LargeRangeStepPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="MaximumRange" /> property's name.
        /// </summary>
        public const string MaximumRangePropertyName = "MaximumRange";

        private double _maximumRange = 0;

        /// <summary>
        /// Sets and gets the MaximumRange property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double MaximumRange
        {
            get
            {
                return _maximumRange;
            }

            set
            {
                if (_maximumRange == value)
                {
                    return;
                }

                RaisePropertyChanging(MaximumRangePropertyName);
                _maximumRange = value;
                if (_configField != null)
                    _configField.Value = RangeToString();
                RaisePropertyChanged(MaximumRangePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="MinimumRange" /> property's name.
        /// </summary>
        public const string MinimumRangePropertyName = "MinimumRange";

        private double _minimumRange = 0;

        /// <summary>
        /// Sets and gets the MinimumRange property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double MinimumRange
        {
            get
            {
                return _minimumRange;
            }

            set
            {
                if (_minimumRange == value)
                {
                    return;
                }

                RaisePropertyChanging(MinimumRangePropertyName);
                _minimumRange = value;
                if (_configField != null)
                    _configField.Value = RangeToString();
                RaisePropertyChanged(MinimumRangePropertyName);
            }
        }

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
                if( _configField != null )
                    _configField.Value = value;

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

        public double GetEstimatedHeight(double availableWidth)
        {
            return 30;
        }

        /// <summary>
        /// The <see cref="IsTrue" /> property's name.
        /// </summary>
        public const string IsTruePropertyName = "IsTrue";

        private bool _isTrue= false;

        /// <summary>
        /// Sets and gets the IsTrue property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsTrue
        {
            get
            {
                return _isTrue;
            }

            set
            {
                if (_isTrue == value)
                {
                    return;
                }

                RaisePropertyChanging(IsTruePropertyName);                
                if (_configField != null)
                    _configField.Value = value;
                _isTrue = value;
                RaisePropertyChanged(IsTruePropertyName);
            }
        }
    }
}
