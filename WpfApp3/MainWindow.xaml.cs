using Neo4j.Driver;
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
using Neo4j;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        neo4j n;
        project p;
        List<string[]> li;
        public MainWindow()
        {
            InitializeComponent();
            n = new neo4j();
            n.ConnectToDatabase();
            p = new project();
            //AssetGroup project = p.loadProject();
            li = n.GetListOfNodes("Machine");
            foreach (string[] s in li)
            {
                treeView.Items.Add(s[0]);
            }
        }

        private void buttonAddProject(object sender, RoutedEventArgs e)
        {
            //int projectId = n.AddProject();
            //treeView.Items.Add("New Project");
            //MessageBox.Show("New Project is made! id of project in database is " + projectId + ".");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //p.readProjectFromExcel();
            //n.insertAssetGroup(p.projectFile, -1);
            li = n.GetListOfNodes("Machine");
            foreach (string[] s in li)
            {
                treeView.Items.Add(s[0]);
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            string[] a = li.Find(x => x[0] == e.NewValue.ToString()) ;
            datagrid.Items.Clear();
            textblock.Text = "";
            int id = Convert.ToInt32(a[1]);
            textblock.Text = a[2];
            List<List<Property>> llp = n.getCharacteristics(id);
            foreach (List<Property> lp in llp)
            {
                Characteristic c = new Characteristic();
                foreach (Property p in lp)
                {
                    switch (p.name)
                    {
                        case "Description":
                            c.description = p.value;
                            break;
                        case "Name":
                            c.name = p.value;
                            break;
                        case "Prefix":
                            c.prefix = p.value;
                            break;
                        case "Value":
                            c.value = p.value;
                            break;
                        case "Unit":
                            c.unit = p.value;
                            break;
                    }
                }
                datagrid.Items.Add(c);
            }
        }

        private void datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
