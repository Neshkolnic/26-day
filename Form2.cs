using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _26_day
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void salesBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.tableAdapterManager.UpdateAll(this.concertDBDataSet);

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "concertDBDataSet.Concerts". При необходимости она может быть перемещена или удалена.
            this.concertsTableAdapter.Fill(this.concertDBDataSet.Concerts);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "concertDBDataSet.Concerts". При необходимости она может быть перемещена или удалена.
            this.concertsTableAdapter.Fill(this.concertDBDataSet.Concerts);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "concertDBDataSet.Sales". При необходимости она может быть перемещена или удалена.

        }

        private void concertsBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.concertsBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.concertDBDataSet);

        }
    }
}
