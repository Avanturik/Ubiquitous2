using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public class YesNoDialog : NotifyPropertyChangeBase
    {
        /// <summary>
        /// The <see cref="QuestionText" /> property's name.
        /// </summary>
        public const string QuestionTextPropertyName = "QuestionText";

        private string _questionText = null;

        /// <summary>
        /// Sets and gets the QuestionText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string QuestionText
        {
            get
            {
                return _questionText;
            }

            set
            {
                if (_questionText == value)
                {
                    return;
                }

                RaisePropertyChanging(QuestionTextPropertyName);
                _questionText = value;
                RaisePropertyChanged(QuestionTextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Result" /> property's name.
        /// </summary>
        public const string ResultPropertyName = "Result";

        private bool _result = false;

        /// <summary>
        /// Sets and gets the Result property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Result
        {
            get
            {
                return _result;
            }

            set
            {
                if (_result == value)
                {
                    return;
                }

                RaisePropertyChanging(ResultPropertyName);
                _result = value;
                RaisePropertyChanged(ResultPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsOpenRequest" /> property's name.
        /// </summary>
        public const string IsOpenRequestPropertyName = "IsOpenRequest";

        private bool _isOpenRequest = false;

        /// <summary>
        /// Sets and gets the IsOpenRequest property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsOpenRequest
        {
            get
            {
                return _isOpenRequest;
            }

            set
            {
                if (_isOpenRequest == value)
                {
                    return;
                }

                RaisePropertyChanging(IsOpenRequestPropertyName);
                _isOpenRequest = value;
                RaisePropertyChanged(IsOpenRequestPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsCloseRequest" /> property's name.
        /// </summary>
        public const string IsCloseRequestPropertyName = "IsCloseRequest";

        private bool _isCloseRequest = false;

        /// <summary>
        /// Sets and gets the IsCloseRequest property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsCloseRequest
        {
            get
            {
                return _isCloseRequest;
            }

            set
            {
                if (_isCloseRequest == value)
                {
                    return;
                }

                RaisePropertyChanging(IsCloseRequestPropertyName);
                _isCloseRequest = value;
                RaisePropertyChanged(IsCloseRequestPropertyName);
            }
        }

        public Action YesAction { get; set; }
        public Action NoAction { get; set; }
    }
}
