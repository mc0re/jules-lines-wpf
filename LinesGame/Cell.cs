using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace LinesGame
{
    public class Cell : INotifyPropertyChanged
    {
        private int _row;
        private int _column;
        private Color _color;
        private bool _isSelected;
        private bool _isPathHighlight;
        private bool _isBallVisible = true; // Default to true

        public int Row
        {
            get => _row;
            set
            {
                if (_row != value)
                {
                    _row = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Column
        {
            get => _column;
            set
            {
                if (_column != value)
                {
                    _column = value;
                    OnPropertyChanged();
                }
            }
        }

        public Color Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasBall)); // Notify that HasBall has changed
                }
            }
        }

        public bool HasBall => Color != Colors.Transparent;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPathHighlight
        {
            get => _isPathHighlight;
            set
            {
                if (_isPathHighlight != value)
                {
                    _isPathHighlight = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsBallVisible
        {
            get => _isBallVisible;
            set
            {
                if (_isBallVisible != value)
                {
                    _isBallVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
