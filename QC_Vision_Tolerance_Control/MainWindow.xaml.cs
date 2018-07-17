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
using MySql.Data.MySqlClient;

namespace QC_Vision_Tolerance_Control
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


        }

        private void partList_DropDownOpened(object sender, EventArgs e)
        {
            //Clear and connect to DB
            this.partList.Items.Clear();
            DBConnect database = new DBConnect();

            //Read all relevant data into the combobox
            MySqlDataReader dataReader = database.Select("Select distinct partid from default_part_limits order by partid asc");
            while (dataReader.Read())
            {
                this.partList.Items.Add(dataReader.GetString("partid"));

            }

            dataReader.Close();
            database.CloseConnection();
        }

        private void partList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (partList.SelectedIndex > -1) {
                int column = 0;
                int row = 0;
                foreach (var control in primaryGrid.Children)
                    if (control.GetType() == typeof(TextBox))
                    {
                        primaryGrid.UnregisterName(((TextBox)control).Name);
                    }
                else if (control.GetType() == typeof(Button))
                {
                    primaryGrid.UnregisterName(((Button)control).Name);
                }

                {

                }
                primaryGrid.Children.Clear();
                primaryGrid.RowDefinitions.Clear();



                DBConnect database = new DBConnect();

                MySqlDataReader dataReader = database.Select("select * from default_part_limits where partid = \"" + partList.SelectedItem + "\" order by measurement asc;");


                while(dataReader.Read())
                {
                    if (dataReader.GetString("Operator") != "=")
                    {


                        RowDefinition newRow = new RowDefinition();
                        newRow.Height = new GridLength(25);

                        primaryGrid.RowDefinitions.Add(newRow);

                        if (primaryGrid.FindName(dataReader.GetString("Measurement")) == null)
                        {
                            TextBox textBox = new TextBox();
                            textBox.Text = dataReader.GetString("Measurement");
                            textBox.TextAlignment = TextAlignment.Center;
                            textBox.VerticalAlignment = VerticalAlignment.Center;
                            textBox.Background = Brushes.BlanchedAlmond;
                            textBox.Name = dataReader.GetString("Measurement") + "1";




                            primaryGrid.Children.Add(textBox);
                            Grid.SetColumn(textBox, 1);
                            Grid.SetRow(textBox, row);
                            primaryGrid.RegisterName(textBox.Name, textBox);

                            Button button = new Button();
                            button.Name = dataReader.GetString("Measurement");
                            button.Content = "Update Row";
                            button.Background = Brushes.LightGreen;
                            button.Click += updateLimits;
                            button.IsEnabled = false;

                            primaryGrid.Children.Add(button);
                            Grid.SetColumn(button, 3);
                            Grid.SetRow(button, row);
                            primaryGrid.RegisterName(button.Name, button);
                        }

                        else
                        {
                            row--;
                        }
                        TextBox textBox2 = new TextBox();
                        if (dataReader.GetString("Operator") == "<")
                        {
                            column = 0;

                            textBox2.Text = dataReader.GetString("Limits");
                            textBox2.TextAlignment = TextAlignment.Right;
                            textBox2.Name = dataReader.GetString("Measurement") + "0";

                        }
                        else if (dataReader.GetString("Operator") == ">")
                        {
                            column = 2;
                            textBox2.Text = dataReader.GetString("Limits");
                            textBox2.TextAlignment = TextAlignment.Left;
                            textBox2.Name = dataReader.GetString("Measurement") + "2";

                        }



                        textBox2.VerticalAlignment = VerticalAlignment.Center;
                        textBox2.Background = Brushes.BlanchedAlmond;
                        textBox2.TextChanged += changeText;

                        primaryGrid.Children.Add(textBox2);
                        Grid.SetColumn(textBox2, column);
                        Grid.SetRow(textBox2, row);
                        primaryGrid.RegisterName(textBox2.Name, textBox2);






                        row++;

                    }
                }


                dataReader.Close();
                database.CloseConnection();
            
        }
    
           
        }
        void changeText(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Background = Brushes.LightGreen;
            

            var button = primaryGrid.FindName(textBox.Name.Remove(textBox.Name.Length - 1, 1)) as Button;

            button.IsEnabled = true;



        }

        void updateLimits(object sender, EventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            DBConnect database = new DBConnect();

            var leftSide = primaryGrid.FindName(button.Name + "0") as TextBox;
            var rightSide = primaryGrid.FindName(button.Name + "2") as TextBox;

            if(leftSide != null)
            {
                double newValue;
                bool isDouble = Double.TryParse(leftSide.Text, out newValue);
                if (isDouble)
                {
                    database.Update("Update default_part_limits set limits = " + newValue + " where partid = \"" + partList.Text + "\" and measurement = \"" + button.Name + "\" and operator = \"<\"; commit;");
                    leftSide.Background = Brushes.BlanchedAlmond;
                }
                else
                {
                    MessageBox.Show("Please enter a number for the limits");
                    button.IsEnabled = true;
                }

            }

            if(rightSide != null)
            {

                double newValue;
                bool isDouble = Double.TryParse(rightSide.Text, out newValue);
                if (isDouble)
                {
                    database.Update("Update default_part_limits set limits = " + newValue + " where partid = \"" + partList.Text + "\" and measurement = \"" + button.Name + "\" and operator = \">\"; commit;");
                    rightSide.Background = Brushes.BlanchedAlmond;
                }
                else
                {
                    MessageBox.Show("Please enter a number for the limits");
                    button.IsEnabled = true;
                }

            }


            database.CloseConnection();

        }
    }
  

}


//Database connection class
public class DBConnect
{
    private MySqlConnection connection;
    private string server;
    private string database;
    private string uid;
    private string password;
    private string connectionString;

    public DBConnect()
    {
        Initialize();
    }

    ~DBConnect()
    {
        CloseConnection();
    }

    private void Initialize()
    {
        //Database connection information
        server = "192.168.192.49";
        database = "qcvision";
        uid = "Limits_Program";
        password = "Limits";

        connectionString = "SERVER=" + server + ";" + "DATABASE=" +
        database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";" + "SSLmode=none;";

        connection = new MySqlConnection(connectionString);

        OpenConnection();

    }
    //open connection to database
    private bool OpenConnection()
    {
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }
    }

    //Close connection
    public bool CloseConnection()
    {
        try
        {
            connection.Close();
            return true;
        }
        catch (MySqlException ex)
        {
            MessageBox.Show(ex.Message);
            return false;
        }
    }


    //Select statement
    public MySqlDataReader Select(string query)
    {
        MySqlDataReader dataReader = null;
        try
        {
            //Connection opened on class instantiation


            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            dataReader = cmd.ExecuteReader();


            return dataReader;
        }
        catch (MySqlException ex)
        {
            MessageBox.Show(ex.Message);
            return dataReader;
        }
    }

    public bool Update(string query)
    {

        try
        {
            MySqlCommand cmd = new MySqlCommand(query, connection);

            cmd.ExecuteNonQuery();
            return true;
        }

        catch (MySqlException ex)
        {
            MessageBox.Show(ex.Message);
            return false;
        }

    }

}