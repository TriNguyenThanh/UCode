using System.ComponentModel;

namespace UCode.Desktop.ViewModels
{
    public class TestCaseItemViewModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _inputRef = string.Empty;
        private string _outputRef = string.Empty;
        private int _indexNo;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public string InputRef
        {
            get => _inputRef;
            set
            {
                if (_inputRef != value)
                {
                    _inputRef = value;
                    OnPropertyChanged(nameof(InputRef));
                    OnPropertyChanged(nameof(InputPreview));
                }
            }
        }

        public string OutputRef
        {
            get => _outputRef;
            set
            {
                if (_outputRef != value)
                {
                    _outputRef = value;
                    OnPropertyChanged(nameof(OutputRef));
                    OnPropertyChanged(nameof(OutputPreview));
                }
            }
        }

        public int IndexNo
        {
            get => _indexNo;
            set
            {
                if (_indexNo != value)
                {
                    _indexNo = value;
                    OnPropertyChanged(nameof(IndexNo));
                }
            }
        }

        public string InputPreview => InputRef.Length > 100 ? InputRef.Substring(0, 100) + "..." : InputRef;
        public string OutputPreview => OutputRef.Length > 100 ? OutputRef.Substring(0, 100) + "..." : OutputRef;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

