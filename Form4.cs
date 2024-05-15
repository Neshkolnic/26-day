using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Windows.Forms;

namespace _26_day
{
    public partial class Form4 : Form
    {
        private string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ConcertDB;Integrated Security=True";

        public Form4()
        {
            InitializeComponent();
            LoadConcertTypes(); // Загрузка типов концертов при инициализации формы
            LoadTicketTypes();  // Загрузка типов билетов при инициализации формы
        }
        private int ticketCount; // Поле для хранения количества билетов
        private string ticketType; // Поле для хранения типа билета
        private int concertID; // Поле для хранения ID концерта

        // Метод для загрузки типов концертов в ComboBox1
        private void LoadConcertTypes()
        {
            comboBox1.Items.Clear(); // Очистка списка перед загрузкой

            // Запрос к базе данных для получения типов концертов
            string query = "SELECT DISTINCT GroupName FROM Concerts;";

            // Выполнить запрос и получить результаты
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    comboBox1.Items.Add(reader["GroupName"].ToString());
                }
            }
        }

        // Метод для загрузки типов билетов в ComboBox2
        private void LoadTicketTypes()
        {
            comboBox2.Items.Clear(); // Очистка списка перед загрузкой

            // Запрос к базе данных для получения типов билетов
            string query = "SELECT DISTINCT TicketType FROM Tickets;";

            // Выполнить запрос и получить результаты
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    comboBox2.Items.Add(reader["TicketType"].ToString());
                }
            }
        }

        // Обработчик нажатия кнопки "Купить билет"
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string groupName = comboBox1.SelectedItem?.ToString();
                string ticketType = comboBox2.SelectedItem?.ToString();
                ticketType = comboBox2.SelectedItem?.ToString();
                int ticketCount = int.Parse(textBox1.Text);

                BuyTickets(groupName, ticketType, ticketCount);

                MessageBox.Show("Билеты успешно куплены.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }

        // Метод для выполнения операции покупки билетов
        private void BuyTickets(string groupName, string ticketType, int ticketCount)
        {
            int concertID = 0;
            string concertQuery = "SELECT ConcertID FROM Concerts WHERE GroupName = @GroupName AND ticketType = @ticketType;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(concertQuery, connection);
                command.Parameters.AddWithValue("@GroupName", groupName);
                command.Parameters.AddWithValue("@ticketType", ticketType);
                connection.Open();
                concertID = (int)command.ExecuteScalar();
            }

            string buyQuery = "UPDATE Concerts SET TicketCount = TicketCount - @TicketCount WHERE ConcertID = @ConcertID AND TicketType = @TicketType;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(buyQuery, connection);
                command.Parameters.AddWithValue("@TicketCount", ticketCount);
                command.Parameters.AddWithValue("@ConcertID", concertID);
                command.Parameters.AddWithValue("@TicketType", ticketType);
                connection.Open();
                command.ExecuteNonQuery();

                AddSaleData(concertID, ticketType, ticketCount);
            }
        }

        // Метод для добавления данных о продаже в таблицу Sales
        private void AddSaleData(int concertID, string ticketType, int ticketCount)
        {
            string insertSaleQuery = "INSERT INTO Sales (TicketID, SaleDate, SalePrice, BuyerName) VALUES (@TicketID, @SaleDate, @SalePrice, @BuyerName);";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand insertSaleCommand = new SqlCommand(insertSaleQuery, connection);
                //  insertSaleCommand.Parameters.AddWithValue("@ConcertID", concertID);
                insertSaleCommand.Parameters.AddWithValue("@TicketID", GetTicketID(concertID, ticketType)); // Получаем ID билета по ID концерта и типу билета
                insertSaleCommand.Parameters.AddWithValue("@SaleDate", DateTime.Now);
                total = CalculateSalePrice(ticketCount, ticketType, concertID);
                insertSaleCommand.Parameters.AddWithValue("@SalePrice", CalculateSalePrice(ticketCount, ticketType, concertID)); // Предположим, что цена билета зависит от количества купленных билетов
                insertSaleCommand.Parameters.AddWithValue("@BuyerName", textBox2.Text); // Предположим, что имя покупателя введено в текстовое поле textBoxBuyerName

                connection.Open();
                insertSaleCommand.ExecuteNonQuery();
            }
        }



        // Метод для получения ID билета по ID концерта
        private int GetTicketID(int concertID, string ticketType)
        {
            string query = "SELECT TicketID FROM Tickets WHERE ConcertID = @ConcertID AND TicketType = @TicketType;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ConcertID", concertID);
                command.Parameters.AddWithValue("@TicketType", ticketType);
                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }

        public decimal total = 0;

        // Метод для расчета стоимости продажи
        private decimal CalculateSalePrice(int ticketCount, string ticketType, int concertID)
        {
            decimal ticketPrice = 0;
            decimal total = 0;

            // Запрос к базе данных для получения цены за билет определенного типа
            string query = "SELECT TicketPrice FROM Tickets WHERE TicketType = @TicketType AND ConcertID = @ConcertID;";
            // Выполнить запрос и получить результаты
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TicketType", ticketType);
                command.Parameters.AddWithValue("@ConcertID", concertID);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    ticketPrice = (decimal)reader["TicketPrice"];
                }
            }
            total = ticketPrice * ticketCount;
            // Вернуть общую стоимость продажи
            label6.Text = total.ToString();

            return total;
        }

        private decimal GetTicketPrice(int concertID, string ticketType)
        {
            decimal ticketPrice = 0;

            string query = "SELECT TicketPrice FROM Tickets WHERE ConcertID = @ConcertID AND TicketType = @TicketType;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ConcertID", concertID);
                command.Parameters.AddWithValue("@TicketType", ticketType);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    ticketPrice = (decimal)reader["TicketPrice"];
                }
            }

            return ticketPrice;
        }

        // Метод для изменения значения количества билетов
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

         // label6.Text =   GetTicketPrice( GetTicketID(concertID, ticketType), comboBox1.SelectedText).ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {


         //   label6.Text = GetTicketPrice(GetTicketID(concertID, ticketType), comboBox1.SelectedText).ToString();


        }

    }
}
