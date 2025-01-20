using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace minesweeper_ui //do 1-1 edge logic
{
    internal class bot
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)] //allows for mouse click simulation
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo); //the mouse click simulation is copied
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02; //defining mouse actions
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        static void LeftClick() //simulates leftclick
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0); // Mouse down
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);   // Mouse up
        }

        static void RightClick() //simulates rightclick
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0); // Mouse down
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);   // Mouse up
        }
        public async void Run() //main function for bot
        {
            await Task.Delay(2000); //waits for board to finish initializing
            Point startpos = new Point(Global.startpos[0], Global.startpos[1]); //pulls the position of the top left corner cell from global (defined when making board)
            Point firstmovepoint = new Point(startpos.X + Convert.ToInt32(Math.Round((decimal)Global.GlobalInts[0] / 2)) * 30, startpos.Y + Convert.ToInt32(Math.Round((decimal)Global.GlobalInts[1] / 2)) * 30); //centre of board
            Button[,] board = Global.GlobalBoard;
            press(board[Global.GlobalInts[0] / 2, Global.GlobalInts[1] / 2], "l"); //left clicks centre button
            await Task.Delay(100); // slight delay

            int F = 0; int N = 0; int H = 0; //initializes vars
            while (!won() && Global.botrunning) //main loop
            {
                //further det is doing the right thing, but doesnt seem to be clicking on the tiles (cursor position goes to right place).
                bool exit = false; //used to fully break each time we do something
                for (int i = 0; i < board.GetLength(0); i++)
                {
                    for (int i2 = 0; i2 < board.GetLength(1); i2++)
                    {
                        Button cur = board[i, i2];
                        //basic definitons: N is the number on a revealed cell. F is the flagged cells around it. H is the hidden (unrevealed) cells
                        N = 0;
                        if (cur.Text != "") { N = Convert.ToInt32(cur.Text); }
                        F = getdata(cur, "flags"); //gets amout of flags in surrounding cells
                        H = getdata(cur, "hidden"); // gets amount of hidden/unrevealed  cells around it
                        if (cur.Text.ToString() != "" && H > 0) //dont do anything with revealed cells of 0, or with cells where all around are revealed/flagged
                        {
                            if (N == F) { pressall(cur, "reveal"); Global.GlobalBoard = board; exit = true; break; } //if all neighbouring mines revealed, we can safely reveal all other nieghbours
                            if (H == N - F) { pressall(cur, "flag"); Global.GlobalBoard = board; exit = true; break; } // flags surrounding tiles if all unrevealed neighbours = number of surrounding mines
                            if (cur.BackColor == Color.White && H == 0 && F > 0) { pressall(cur, "flag"); Global.GlobalBoard = board; exit = true; break; } //if surrounded by flags, reveal (this seems weird but fixes some cases which i found)
                        }
                    }
                    if (exit) { break; }
                }
                //call furtherdet here
                if (!exit && !won()) { if (furtherdet()) { awfuckwegottaguess(); } else { await Task.Delay(3000); } }//if we cant play a guranteed room, we guess ----- CALL SEARCH HERE
                await Task.Delay(5);
            }
            
        }
        private static bool furtherdet()
        {
            bool next = !oto(); //if one two one is found, next = false so no continue
            if (next) { next = !otto(); }
            return next;
        }
        private static void pressall(Button b, string action) // click all hidden tiles around a specific tile with a specific action (reveal or flag)
        {
            string name = b.Name;
            string[] namestr = name.Split(' ');
            int i = Convert.ToInt32(namestr[0]);
            int i2 = Convert.ToInt32(namestr[1]);
            for (int x = -1; x <= 1; x++) //x+- var
            {
                for (int y = -1; y <= 1; y++) //y+- var
                {
                    int checkx = i + x;
                    int checky = i2 + y;
                    if (checkx >= 0 && checky >= 0 && checkx < Global.GlobalInts[0] && checky < Global.GlobalInts[1]) //bounds check
                    {
                        if (Global.GlobalBoard[checkx, checky].BackColor == Color.White && Global.GlobalBoard[checkx, checky].Image == null)//excludes grey or flagged tiles (no need to invoke press functions if already clicked)
                        {
                            if (action == "flag") { press(Global.GlobalBoard[i + x, i2 + y], "r"); } //right clicks for flag
                            if (action == "reveal") { press(Global.GlobalBoard[i + x, i2 + y], "l"); }//left clicks for reveal
                        }
                    }
                }
            }
        }
        private static void press(Button b, string lr) //takes a button, gets its position and clicks it. lr is the determiner for left/right click
        {
            int i = 0; int i2 = 0;
            string name = b.Name;
            string[] namestr = name.Split(' ');
            if (namestr != null) { i = Convert.ToInt32(namestr[0]); i2 = Convert.ToInt32(namestr[1]); }
            Cursor.Position = new Point(Global.startpos[0] + i * 30, Global.startpos[1] + i2 * 30); //converts button name into position
            if (lr == "r") { RightClick(); }//left/right click on that position
            if (lr == "l") { LeftClick(); }
        }

        private static int getdata(Button b, string type) //gets surrounding flags or hidden tiles 
        {
            string name = b.Name;
            string[] namestr = name.Split(' ');
            int i = Convert.ToInt32(namestr[0]);
            int i2 = Convert.ToInt32(namestr[1]);
            if (type == "flags") //flagged cells
            {
                int flags = 0;
                for (int x = -1; x <= 1; x++) //x+- var
                {
                    for (int y = -1; y <= 1; y++) //y+- var
                    {
                        if (x == 0 && y == 0) { continue; }
                        int checkx = i + x; //gets index to check
                        int checky = i2 + y;
                        if (checkx >= 0 && checky >= 0 && checkx < Global.GlobalInts[0] && checky < Global.GlobalInts[1])
                        {
                            if (((string[])Global.GlobalBoard[i + x, i2 + y].Tag)[1] == "F")
                            {
                                flags++;
                            }
                        }
                    }
                }
                return flags;
            }
            if (type == "hidden") //hidden cells
            {
                int unrev = 0;
                for (int x = -1; x <= 1; x++) //x+- var
                {
                    for (int y = -1; y <= 1; y++) //y+- var
                    {
                        if (x == 0 && y == 0) { continue; }
                        //if not flagged and no number
                        int checkx = i + x; //gets index to check
                        int checky = i2 + y;
                        if (checkx >= 0 && checky >= 0 && checkx < Global.GlobalInts[0] && checky < Global.GlobalInts[1])
                        {
                            if (((string[])Global.GlobalBoard[i + x, i2 + y].Tag)[1] != "F" && Global.GlobalBoard[i + x, i2 + y].BackColor == Color.White)
                            {
                                unrev++;
                            }
                        }
                    }
                }
                return unrev;
            }
            return 10; //as an exception, should never be used
        }

        private bool won() //function to tel us if won
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
                return true;
            }
            return false;
        }

        private static bool BordersGrey(int i, int i2) //finds whether a tile has any revealed tiles around it (so we dont calculate probabilities of 0 for lots of blank tiles)
        {
            for (int x = -1; x <= 1; x++) //x+- var
            {
                for (int y = -1; y <= 1; y++) //y+- var
                {
                    if (x == 0 && y == 0) { continue; }
                    if (i + x >= 0 && i2 + y >= 0 && i + x < Global.GlobalInts[0] && i2 + y < Global.GlobalInts[1])
                    {
                        if (Global.GlobalBoard[i + x, i2 + y].BackColor == Color.Gray) { return true; } //if grey neighbour found, return true
                    }
                }
            }
            return false; //if no grey neighbour, returns false;
        }


        private static int awfuckwegottaguess()
        {
            Global.guesses = Global.guesses + 1;
            //probability of a mine being there = unflagged adjacent mines / unflagged tiles = N - F / H
            Button[,] board = Global.GlobalBoard;
            float lowestprob = 1; //worst possible probability (as mine = bad)
            Button best = new Button();
            int[] tmpvals = new int[3];
            for (int i = 0; i < Global.GlobalInts[0]; i++)
            {
                for (int i2 = 0; i2 < Global.GlobalInts[1]; i2++)
                {
                    if (board[i, i2].BackColor == Color.White && Global.GlobalBoard[i, i2].Image == null && BordersGrey(i, i2)) //gets all blank tiles that we have info on 
                    {
                        tmpvals = largestN(i, i2);
                        float curprob = (float)(tmpvals[0] - tmpvals[1]) / tmpvals[2]; //calculates the probability of a cell having a mine
                        if (curprob < lowestprob) { lowestprob = curprob; best = Global.GlobalBoard[i, i2]; } //stores the smallest found probability and the button to click for that probability
                    }
                }
            }
            if (best != null) { press(best, "l"); } //left clicks our selected tile 
            return 0;
        } //main logic for guessing
        private static int[] largestN(int i, int i2) //calculates data for calculating probability
        {
            int biggestN = 0;
            int[] values = new int[3];
            Button sb = new Button();
            for (int x = -1; x <= 1; x++) //x+- var
            {
                for (int y = -1; y <= 1; y++) //y+- var
                {
                    if (x == 0 && y == 0) { continue; } //dont check self
                    if (i + x >= 0 && i2 + y >= 0 && i + x < Global.GlobalInts[0] && i2 + y < Global.GlobalInts[1] && Global.GlobalBoard[i + x, i2 + y].Text != "") //in bounds check
                    {
                        if (Global.GlobalBoard[i + x, i2 + y].BackColor == Color.Gray && Global.GlobalBoard[i + x, i2 + y].Text != "")
                        {
                            int curN = Convert.ToInt32(Global.GlobalBoard[i + x, i2 + y].Text);
                            if (curN > biggestN) { biggestN = curN; sb = Global.GlobalBoard[i + x, i2 + y]; }
                        }
                    }
                }
            }
            values = [biggestN, getdata(sb, "flags"), getdata(sb, "hidden")];
            return values;
        }
        private static bool oto()
        {
            Button[,] board = Global.GlobalBoard; bool checks; //declare vars
            for (int i = 0; i < Global.GlobalInts[0] - 2; i++) //we check the next 2 tiles after each one we iterate through, so the -2 keeps checks in bounds
            {
                for (int i2 = 1; i2 < Global.GlobalInts[1] - 1; i2++) //we dont check the bottom or top row as i think future logic will do this better
                {
                    checks = true;
                    if (getdata(board[i, i2], "hidden") == 0) { break; } //checks if we need to implement complex logic on that tile
                    for (int mod = 0; mod < 3; mod++) { Button cur = board[i + mod, i2]; if (cur.BackColor == Color.White || cur.Text == "" || cur.Image != null) { checks = false; break; } }//means we skip over if we dont have enough info on the current tiles
                    if (checks && Convert.ToInt32(board[i, i2].Text) - getdata(board[i,i2], "flags") == 1 && Convert.ToInt32(board[i + 1, i2].Text) - getdata(board[i+1, i2], "flags") == 2 && Convert.ToInt32(board[i + 2, i2].Text) - getdata(board[i+2, i2], "flags") == 1)
                    {
                        if (board[i, i2 + 1].BackColor == Color.White && board[i + 1, i2 + 1].BackColor == Color.White && board[i + 2, i2 + 1].BackColor == Color.White) { press(board[i, i2 + 1], "r"); press(board[i + 2, i2 + 1], "r"); return true; } //checks above horiz one two one and clicks the known tiles
                        if (board[i, i2 - 1].BackColor == Color.White && board[i + 1, i2 - 1].BackColor == Color.White && board[i + 2, i2 - 1].BackColor == Color.White) { press(board[i, i2 - 1], "r"); press(board[i + 2, i2 - 1], "r"); return true; }//as above, checks below
                    }
                }
            }
            for (int i = 1; i < Global.GlobalInts[0] - 1; i++)
            {
                for (int i2 = 0; i2 < Global.GlobalInts[1] - 2; i2++)
                {
                    checks = true;
                    if (getdata(board[i, i2], "hidden") == 0) { break; } //checks if we need to implement complex logic on that tile
                    for (int mod = 0; mod < 3; mod++) { Button cur = board[i, i2 + mod]; if (cur.BackColor == Color.White || cur.Text == "" || cur.Image != null) { checks = false; continue; } }
                    if (checks && Convert.ToInt32(board[i, i2].Text) - getdata(board[i, i2], "flags") == 1 && Convert.ToInt32(board[i, i2 + 1].Text) - getdata(board[i, i2 + 1], "flags") == 2 && Convert.ToInt32(board[i, i2 + 2].Text) - getdata(board[i, i2 + 2], "flags") == 1)
                    {
                        if (board[i + 1, i2].BackColor == Color.White && board[i + 1, i2 + 1].BackColor == Color.White && board[i + 1, i2 + 2].BackColor == Color.White) { press(board[i + 1, i2], "r"); press(board[i + 1, i2 + 2], "r"); return true; }//as above, for vertical 
                        if (board[i - 1, i2].BackColor == Color.White && board[i - 1, i2 + 1].BackColor == Color.White && board[i - 1, i2 + 2].BackColor == Color.White) { press(board[i - 1, i2], "r"); press(board[i - 1, i2 + 2], "r"); return true; }
                    }
                }
            }
            return false;
        }//one two one pattern check

        private static bool otto()
        {
            Button[,] board = Global.GlobalBoard; bool checks; //declare vars
            for (int i = 0; i < Global.GlobalInts[0] - 3; i++)
            {
                for (int i2 = 1; i2 < Global.GlobalInts[1] - 1; i2++)
                {
                    checks = true;
                    if (getdata(board[i, i2], "hidden") == 0) { break; } //checks if we need to implement complex logic on that tile
                    for (int mod = 0; mod < 4; mod++) { Button cur = board[i + mod, i2]; if (cur.BackColor == Color.White || cur.Text == "" || cur.Image != null) { checks = false; break; } }//means we skip over if we dont have enough info on the current tiles
                    if (checks && Convert.ToInt32(board[i, i2].Text) - getdata(board[i, i2], "flags") == 1 && Convert.ToInt32(board[i + 1, i2].Text) - getdata(board[i+1, i2], "flags") == 2 && Convert.ToInt32(board[i + 2, i2].Text) - getdata(board[i+2, i2], "flags") == 2 && Convert.ToInt32(board[i+3, i2].Text) - getdata(board[i+3, i2], "flags") == 1)
                    {
                        if (board[i, i2 + 1].BackColor == Color.White && board[i + 1, i2 + 1].BackColor == Color.White && board[i + 2, i2 + 1].BackColor == Color.White && board[i + 3, i2 + 1].BackColor == Color.White) { press(board[i + 1, i2 + 1], "r"); press(board[i + 2, i2 + 1], "r"); return true; }
                        if (board[i, i2 - 1].BackColor == Color.White && board[i + 1, i2 - 1].BackColor == Color.White && board[i + 2, i2 - 1].BackColor == Color.White && board[i + 3, i2 - 1].BackColor == Color.White) { press(board[i + 1, i2 - 1], "r"); press(board[i + 2, i2 - 1], "r"); return true; }
                    }
                }
            }
            for (int i = 1; i < Global.GlobalInts[0] - 1; i++)
            {
                for (int i2 = 0; i2 < Global.GlobalInts[1] - 3; i2++)
                {
                    checks = true;
                    if (getdata(board[i, i2], "hidden") == 0) { break; } //checks if we need to implement complex logic on that tile
                    for (int mod = 0; mod < 4; mod++) { Button cur = board[i, i2 + mod]; if (cur.BackColor == Color.White || cur.Text == "" || cur.Image != null) { checks = false; continue; } }
                    if (checks && Convert.ToInt32(board[i, i2].Text) - getdata(board[i, i2], "flags") == 1 && Convert.ToInt32(board[i, i2 + 1].Text) - getdata(board[i, i2+1], "flags") == 2 && Convert.ToInt32(board[i, i2 + 2].Text) - getdata(board[i, i2+2], "flags") == 2 && Convert.ToInt32(board[i, i2 + 3].Text) - getdata(board[i, i2 + 3], "flags") == 1)
                    {
                        if (board[i + 1, i2].BackColor == Color.White && board[i + 1, i2 + 1].BackColor == Color.White && board[i + 1, i2 + 2].BackColor == Color.White && board[i + 1, i2 + 3].BackColor == Color.White) { press(board[i + 1, i2 + 1], "r"); press(board[i + 1, i2 + 2], "r"); return true; }//as above, for vertical 
                        if (board[i - 1, i2].BackColor == Color.White && board[i - 1, i2 + 1].BackColor == Color.White && board[i - 1, i2 + 2].BackColor == Color.White && board[i - 1, i2 + 3].BackColor == Color.White) { press(board[i - 1, i2 + 1], "r"); press(board[i - 1, i2 + 2], "r"); return true; }
                    }
                }
            }
            return false;
        }//one two two one pattern check

        private static bool ooe() // one one edge pattern
        {
            Button[,] board = Global.GlobalBoard;
            for(int i = 0; i < Global.GlobalInts[0]; i++)
            {
                if (Convert.ToInt32(board[i,0].Text) - getdata(board[i, 0], "flags") == 1)
                {

                }
            }
            return false;
        }
    }
}
