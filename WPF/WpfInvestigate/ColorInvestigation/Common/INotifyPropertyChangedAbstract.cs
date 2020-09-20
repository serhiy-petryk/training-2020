using System.ComponentModel;

namespace ColorInvestigation.Common
{
    public abstract class INotifyPropertyChangedAbstract: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public abstract void UpdateProperties();
    }
}
