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

        public IEnumerable<CustomMenuItem<InstanceReference>> AllMenuItems
        {
            get
            {
                foreach(var item in MenuItems)
                {
                    yield return item;

                    foreach(var subItem in item.AllMenuItems)
                    {
                        yield return subItem;
                    }
                }
            }
        }

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

        internal void RemoveFromParent(CustomMenuItem<InstanceReference> whatToRemove)
        {
            if (MenuItems.Contains(whatToRemove))
            {
                MenuItems.Remove(whatToRemove);
            }
            else
            {
                foreach (var item in MenuItems)
                {
                    if(item.RemoveFromParent(whatToRemove))
                    {
                        break;
                    }
                }
            }
        }
    }
}
