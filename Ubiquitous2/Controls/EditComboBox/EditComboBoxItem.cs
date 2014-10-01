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
        /// The <see cref="LinkedObject" /> property's name.
        /// </summary>
        public const string LinkedObjectPropertyName = "LinkedObject";

        private object _linkedObject = null;

        /// <summary>
        /// Sets and gets the LinkedObject property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public object LinkedObject
        {
            get
            {
                return _linkedObject;
            }

            set
            {
                if (_linkedObject == value)
                {
                    return;
                }

                RaisePropertyChanging(LinkedObjectPropertyName);
                _linkedObject = value;
                RaisePropertyChanged(LinkedObjectPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsCurrent" /> property's name.
        /// </summary>
        public const string IsCurrentPropertyName = "IsCurrent";

        private bool _isCurrent = false;

        /// <summary>
        /// Sets and gets the IsCurrent property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsCurrent
        {
            get
            {
                return _isCurrent;
            }

            set
            {
                if (_isCurrent == value)
                {
                    return;
                }

                RaisePropertyChanging(IsCurrentPropertyName);
                _isCurrent = value;
                RaisePropertyChanged(IsCurrentPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsUnselectable" /> property's name.
        /// </summary>
        public const string IsUnselectablePropertyName = "IsUnselectable";

        private bool _isUnselectable = false;

        /// <summary>
        /// Sets and gets the IsUnselectable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUnselectable
        {
            get
            {
                return _isUnselectable;
            }

            set
            {
                if (_isUnselectable == value)
                {
                    return;
                }

                RaisePropertyChanging(IsUnselectablePropertyName);
                _isUnselectable = value;
                RaisePropertyChanged(IsUnselectablePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsUndeletable" /> property's name.
        /// </summary>
        public const string IsUndeletablePropertyName = "IsUndeletable";

        private bool _isUndeletable = false;

        /// <summary>
        /// Sets and gets the IsUndeletable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsUndeletable
        {
            get
            {
                return _isUndeletable;
            }

            set
            {
                if (_isUndeletable == value)
                {
                    return;
                }

                RaisePropertyChanging(IsUndeletablePropertyName);
                _isUndeletable = value;
                RaisePropertyChanged(IsUndeletablePropertyName);
            }
        }
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
