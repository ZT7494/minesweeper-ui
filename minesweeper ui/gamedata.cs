using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace minesweeper_ui
{
    public partial class gamedata : Form
    {
        private int elapsedtime; //will count seconds for output
        private System.Windows.Forms.Timer timer; //timer for updating the time output 
        private System.Windows.Forms.Timer minetimer; //refreshes the amount of mines left
        public gamedata()
        {
            InitializeComponent();
            this.Location = new Point(Global.xypos[0], Global.xypos[1]);//places it at the location just to the right of the game (specified earlier)

            //Game Timer
            elapsedtime = 0;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; //1 second tick
            timer.Tick += timertick; //each tick calls timertick (below)
            checkwin();
            timer.Start();

            //Game refresh timer:
            minetimer = new System.Windows.Forms.Timer();
            minetimer.Interval = 50; //20 ticks per second - not too fast to be laggy but feels responsive 
            minetimer.Tick += refreshmines; // each tick calls refreshmines (below)
            minetimer.Start();

            label3.Text = "Difficulty: " + getdiff(); //displays difficulty

        }
        private string getdiff()
        {
            int[] difflist = Global.GlobalInts; //pulls in dimensions and mines from global
            int[] easy = new int[3] { 8, 8, 10 };
            int[] med = new int[3] { 16, 16, 40 };
            int[] hard = new int[3] { 30, 16, 99 };
            if (difflist.SequenceEqual(easy)) { return "Easy"; } //returns the set difficulties. SequenceEqual uses linq to check if 2 arrays are equal
            if (difflist.SequenceEqual(med)) { return "Medium"; }
            if (difflist.SequenceEqual(hard)) { return "Hard"; }
            else { return "Custom"; } //otherwise, assume custom game
        }
        private void timertick(object sender, EventArgs e)
        {
            if (Global.outputwin) { elapsedtime++; } //increment timer if game is not won/lost
            label1.Text = ("Timer: " + elapsedtime.ToString()); //output to label
        }

        private void refreshmines(object sender, EventArgs e)
        {
            if (Global.mines >= 0) { label2.Text = "Mines Left: " + Convert.ToString(Global.mines); }
            else { label2.Text = "Mines Left: 0"; } //ensures we dont have negative mines outputted
            if (Global.mines == 0) { checkwin(); } //if we have 0 mines left (all flagged) it checks for win
        }
        private void checkwin()
        {
            Button[,] board = Global.GlobalBoard; //pulls board from global
            int countgrey = 0; //will count revealed squares
            int countimage = 0; // will count image squares

            for (int i = 0; i < Global.GlobalInts[0]; i++) //loops through whole board 
            {
                for (int i2 = 0; i2 < Global.GlobalInts[1]; i2++)
                {
                    if (board[i, i2].BackColor == Color.Gray) { countgrey++; } //increments based on square
                    if (board[i, i2].Image != null) { countimage++; }
                }
            }
            if (countgrey + countimage == Global.GlobalInts[0] * Global.GlobalInts[1] && Global.outputwin == true) //checks if user has win
            {
                Global.outputwin = false; //was having an issue where this message box would appear 50 times, so this makes sure it only displays once
                MessageBox.Show("Win! Guesses: " + Global.guesses.ToString()); //displays win
                Environment.Exit(0); //closes all
            }
        }

        private void gamedata_Load(object sender, EventArgs e)
        {

        }
    }
}
