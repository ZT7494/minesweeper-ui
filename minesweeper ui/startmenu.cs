namespace minesweeper_ui
{
    public partial class StartMenu : Form
    {
        public StartMenu()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(checkedListBox1.CheckedItems.Count != 0)
            {
                string diff = Convert.ToString(checkedListBox1.CheckedItems[0]); //gets a string of the difficulty selected
                Global.GlobalVar = diff; //stores difficulty as a global var
                this.Close();
            }
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
