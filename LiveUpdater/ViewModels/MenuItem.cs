using FlatRedBall.Glue.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OcularPlane.Models;

namespace LiveUpdater.ViewModels
{

    public abstract class CustomMenuBase : ViewModel
    {
        public abstract object TagAsObject { get; }
        public string Title { get; set; }
    }

    public class CustomMenuItem<T>  : CustomMenuBase
    {
        public T Tag { get; set; }

        public IEnumerable<CustomMenuItem<T>> AllMenuItems
        {
            get
            {
                foreach (var item in Items)
                {
                    yield return item;

                    foreach (var subItem in item.AllMenuItems)
                    {
                        yield return subItem;
                    }
                }
            }
        }

        public override object TagAsObject
        {
            get
            {
                return Tag;
            }
        }

        public ObservableCollection<CustomMenuItem<T>> Items { get; set; }


        public CustomMenuItem()
        {
            this.Items = new ObservableCollection<CustomMenuItem<T>>();
        }

        internal bool RemoveFromParent(CustomMenuItem<T> whatToRemove)
        {
            if(Items.Contains(whatToRemove))
            {
                Items.Remove(whatToRemove);
                return true;
            }
            else
            {
                foreach(var item in Items)
                {
                    if(item.RemoveFromParent(whatToRemove))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            return "Menu Item " + Tag.ToString();
        }
    }
}
