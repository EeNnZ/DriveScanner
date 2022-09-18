using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace WeatherGuiApp
{
    public abstract class AbstractBindable : INotifyPropertyChanged
    {
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var temp = Volatile.Read(ref PropertyChanged);
            temp?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
