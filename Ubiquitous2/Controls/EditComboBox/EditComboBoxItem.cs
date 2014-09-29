using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using UB.Model;

namespace UB.Controls
{
    public class EditComboBoxItem : NotifyPropertyChangeBase
    {

        /// <summary>
        /// The <see cref="SelectAction" /> property's name.
        /// </summary>
        public const string SelectActionPropertyName = "SelectAction";

        private Action _selectAction = null;

        /// <summary>
        /// Sets and gets the SelectAction property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Action SelectAction
        {
            get
            {
                return _selectAction;
            }

            set
            {
                if (_selectAction == value)
                {
                    return;
                }

                RaisePropertyChanging(SelectActionPropertyName);
                _selectAction = value;
                RaisePropertyChanged(SelectActionPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private string _title = null;

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }

            set
            {
                if (_title == value)
                {
                    return;
                }

                RaisePropertyChanging(TitlePropertyName);
                _title = value;
                RaisePropertyChanged(TitlePropertyName);
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
