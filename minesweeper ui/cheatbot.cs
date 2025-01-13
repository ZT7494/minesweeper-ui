using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace minesweeper_ui
{
    internal class cheatbot
    {
        public void start() //this is just going to access the board tags and bs its way through the checkwin
        {
            Button[,] board = Global.GlobalBoard; //imports board
            for(int i = 0; i<board.GetLength(0); i++)
            {
                for (int i2 = 0; i2 < board.GetLength(1); i2++)
                {
                    string[] tags = (string[])board[i, i2].Tag;
                    if (tags[0] == "M")
                    {
                        board[i, i2].Image = Global.getflag;
                        Global.mines = Global.mines - 1;
                    }
                    else { reveal(board[i, i2], board, i , i2); }
                    Global.GlobalBoard = board;
                }
            }
        }
        private void reveal(Button b, Button[,] board, int i, int i2 )
        {
            int mines = 0;
            int checkx;
            int checky;
            if(b.BackColor!=Color.Gray)
            {
                for (int x = -1; x <= 1; x++) //x+- var
                {
                    for (int y = -1; y <= 1; y++) //y+- var
                    {
                        if (x == 0 && y == 0) { continue; } //dont check self
                        checkx = i + x;
                        checky = i2 + y;
                        if (checkx < 0 || checky < 0 || checkx >= board.GetLength(0) || checky >= board.GetLength(1)) { continue; } //dont check out of bounds
                        string[] tags = (string[])board[checkx, checky].Tag;
                        if (tags[0] == "M") { mines++; }
                    }
                }
                b.BackColor = Color.Gray;
                if (mines > 0)
                {
                    switch (mines) //for each possible amount of mines, set a color
                    {
                        case 1:
                            b.ForeColor = Color.Blue;
                            break;
                        case 2:
                            b.ForeColor = Color.LightGreen;
                            break;
                        case 3:
                            b.ForeColor = Color.DarkRed;
                            break;
                        case 4:
                            b.ForeColor = Color.DarkBlue;
                            break;
                        case 5:
                            b.ForeColor = Color.Yellow;
                            break;
                        case 6:
                            b.ForeColor = Color.DarkCyan;
                            break;
                        case 7:
                            b.ForeColor = Color.MediumPurple;
                            break;
                        case 8:
                            b.ForeColor = Color.DarkOrange;
                            break;
                    }
                    b.Text = mines.ToString(); //displays number of mines
                }
            }
        }
    }
}
