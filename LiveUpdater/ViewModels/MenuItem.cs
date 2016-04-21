using FlatRedBall.Glue.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


    }
}
