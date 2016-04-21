using FlatRedBall.Glue.MVVM;
using OcularPlane.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveUpdater.ViewModels
{
    public class RuntimeObjectListViewModel : ViewModel
    {
        public ObservableCollection<CustomMenuItem<InstanceReference>> MenuItems
        {
            get;
            private set;
        } = new ObservableCollection<CustomMenuItem<InstanceReference>>();

        CustomMenuItem<InstanceReference> selectedItem;
        public CustomMenuItem<InstanceReference> SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                if(selectedItem != value)
                {
                    selectedItem = value;
                    base.NotifyPropertyChanged(nameof(SelectedItem));
                }
            }
        }
    }
}
