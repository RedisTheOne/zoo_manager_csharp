using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;

namespace ZooManage
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Data Source=DESKTOP-604RL6A;Initial Catalog=ZooManager;Integrated Security=True";
        private SqlConnection sqlConnection;
        private List<Animal> animals = new List<Animal>();
        private int selectedIndex = -1;

        public MainWindow()
        {
            //INITIALIZE 
            InitializeComponent();
            sqlConnection = new SqlConnection(connectionString);
            
            //OPEN THE CONNECTION
            sqlConnection.Open();

            SetAnimalsToTheList();
        }

        public void Deconstruct()
        {
            //CLOSE CONNECTION
            sqlConnection.Close();
        }

        private void SetAnimalsToTheList()
        {
            //EXECUTE QUERY
            using (SqlCommand command = new SqlCommand("SELECT * FROM Animals", sqlConnection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    //ADD ANIMALS TO ANIMALS'S LIST
                    while (reader.Read())
                    {
                        animals.Add(new Animal(reader.GetString(0), reader.GetString(1)));
                    }
                    AddAnimalsToListBox();
                }
            }
        }

        private void AddAnimalsToListBox()
        {
            //ADD ANIMALS TO THE LIST BOX
            foreach(var i in animals)
            {
                animalsListBox.Items.Add(i.name);
            }
        }

        private void animalsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //UPDATE VARIABLES
            string selectedAnimalName = animalsListBox.SelectedItem.ToString();
            Animal selectedAnimal = animals.Find(a => a.name == selectedAnimalName);
            nameTextBlock.Text = "Name: " + selectedAnimal.name;
            colorTextBlock.Text = "Color: " + selectedAnimal.color;
            nameTextBox.Text = selectedAnimal.name;
            colorTextBox.Text = selectedAnimal.color;
            selectedIndex = animalsListBox.SelectedIndex;
        }

        private void addBtn_Click(object sender, RoutedEventArgs e)
        {
            string name = nameTextBox.Text.ToString();
            string color = colorTextBox.Text.ToString();

            //CHECK IF NAME AND COLOR ARE VALID
            if(String.IsNullOrWhiteSpace(name) && String.IsNullOrWhiteSpace(color))
                MessageBox.Show("Please fill required fields!");
            else
            {
                //CHECK IF THE ANIMAL NAME IS NOT USED BEFORE
                if(animals.Any(a => a.name == name))
                    MessageBox.Show("Animal is added once!");
                else
                {
                    //EXECUTE QUERY
                    string query = "INSERT INTO Animals(name, color) VALUES(@name, @color)";
                    SqlCommand command = new SqlCommand(query, sqlConnection);
                    using (command)
                    {
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@color", color);
                        command.ExecuteNonQuery();

                        //UPDATE VARIABLES
                        animalsListBox.Items.Add(name);
                        animals.Add(new Animal(name, color));
                        nameTextBox.Text = "";
                        colorTextBox.Text = "";
                    }
                }
            }
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            //CHECK IF AN ANIMAL IS SELECT
            if(selectedIndex == -1)
                MessageBox.Show("Please select an animal!");
            else
            {
                //EXECUTE QUERY
                string query = "DELETE FROM Animals WHERE name=@name";
                SqlCommand command = new SqlCommand(query, sqlConnection);
                using (command)
                {
                    command.Parameters.AddWithValue("@name", animals[selectedIndex].name);
                    command.ExecuteNonQuery();
                }
                animals.RemoveAt(selectedIndex);

                //REMOVE THE ITEM FROM LISTBOX AND ANIMALS LIST
                try
                {
                    animalsListBox.Items.RemoveAt(selectedIndex);
                    animals.RemoveAt(selectedIndex);
                } catch(Exception ex) { }
            }
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            //CLEAR THE TEXTBOXES
            nameTextBox.Text = "";
            colorTextBox.Text = "";
        }

        private void editBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = nameTextBox.Text;
                string color = colorTextBox.Text;
                Animal selectedAnimal = animals[selectedIndex];

                if (String.IsNullOrWhiteSpace(name) && String.IsNullOrWhiteSpace(color))
                    MessageBox.Show("Please fill required fields!");
                else
                {
                    if(selectedIndex != -1)
                    {
                        string query = "UPDATE Animals SET name=@name, color=@color WHERE name=@currentName";
                        SqlCommand command = new SqlCommand(query, sqlConnection);
                        using (command)
                        {
                            //EXECUTE QUERY
                            command.Parameters.AddWithValue("@name", name);
                            command.Parameters.AddWithValue("@currentName", selectedAnimal.name);
                            command.Parameters.AddWithValue("@color", color);
                            command.ExecuteNonQuery();

                            //UPDATE ANIMALS LIST AND LISTBOX
                            List<Animal> newAnimalsList = new List<Animal>();
                            foreach (var a in animals)
                            {
                                if (a.name == selectedAnimal.name)
                                    newAnimalsList.Add(new Animal(name, color));
                                else
                                    newAnimalsList.Add(a);
                            }

                            animals = newAnimalsList;

                            try
                            {
                                animalsListBox.Items.Clear();
                            }
                            catch (Exception ex) { }

                            //ADD ANIMALS TO LISTBOX
                            foreach (var a in animals)
                                animalsListBox.Items.Add(a.name);

                            //CHANGE TEXTBLOCKS
                            nameTextBlock.Text = "Name: " + name;
                            colorTextBlock.Text = "Color: " + color;
                        }
                    } else
                        MessageBox.Show("Please select the animal you want to update");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Please fill required fields!");
            }
        }
    }
}
