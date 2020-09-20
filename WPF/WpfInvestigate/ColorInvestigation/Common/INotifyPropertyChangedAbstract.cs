namespace ColorInvestigation.Common
{
    using System.ComponentModel;
    public abstract class INotifyPropertyChangedAbstract: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        internal virtual void OnPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public abstract void UpdateProperties();
    }
}
