using LiveUpdater.ViewModels;
using OcularPlane.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LiveUpdater.Views
{
    /// <summary>
    /// Interaction logic for RuntimeObjectListView.xaml
    /// </summary>
    public partial class RuntimeObjectListView : UserControl
    {
        public ItemCollection Items
        {
            get
            {
                return TreeViewInstance.Items;
            }
        }



        public RuntimeObjectListView()
        {
            InitializeComponent();
        }

        private void TreeViewInstance_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            int m = 3;
            var viewModel = this.DataContext as RuntimeObjectListViewModel;
            if(viewModel != null)
            {
                viewModel.SelectedItem = TreeViewInstance.SelectedItem as CustomMenuItem<InstanceReference>;
            }
        }
    }
}
