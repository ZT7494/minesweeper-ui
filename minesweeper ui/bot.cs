using System.Diagnostics;
using System.Runtime.InteropServices;
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
            //Point startpos = new Point(Global.startpos[0], Global.startpos[1]); //pulls the position of the top left corner cell from global (defined when making board)
            //Point firstmovepoint = new Point(startpos.X + Convert.ToInt32(Math.Round((decimal)Global.GlobalInts[0] / 2)) * 30, startpos.Y + Convert.ToInt32(Math.Round((decimal)Global.GlobalInts[1] / 2)) * 30); //centre of board
            Button[,] board = Global.GlobalBoard;
            press(board[Global.GlobalInts[0] / 2, Global.GlobalInts[1] / 2], "l"); //left clicks centre button
            //press(board[1, 1], "l");
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
                        F = getdata(cur, "flags", Global.GlobalBoard); //gets amout of flags in surrounding cells
                        H = getdata(cur, "hidden", Global.GlobalBoard); // gets amount of hidden/unrevealed  cells around it
                        if (cur.Text.ToString() != "" && H > 0) //dont do anything with revealed cells of 0, or with cells where all around are revealed/flagged
                        {
                            if (N == F) { pressall(cur, "reveal"); Global.GlobalBoard = board; exit = true; break; } //if all neighbouring mines revealed, we can safely reveal all other nieghbours
                            if (H == N - F) { pressall(cur, "flag"); Global.GlobalBoard = board; exit = true; break; } // flags surrounding tiles if all unrevealed neighbours = number of surrounding mines
                        }
                    }
                    if (exit) { break; }
                }
                //call furtherdet here
                if (!exit && !won()) { if (furtherdet()) { awfuckwegottaguess(); } else { await Task.Delay(5); } }//if we cant play a guranteed room, we guess ----- CALL SEARCH HERE
                await Task.Delay(5);
            }
            
        }
        private static bool furtherdet()
        {
            bool next = !oto(); //if one two one is found, next = false so no continue
            if (next) { next = !otto(); }
            if (next) { next = !ooe(); }
            if (next) { next = !ote(); }
            if (next) { next = !endgame(); }
            if (next) { betterguessing(); }

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

        private static int getdata(Button b, string type, Button[,] lb) //gets surrounding flags or hidden tiles 
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
                            if (((string[])lb[i + x, i2 + y].Tag)[1] == "F")
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
                            if (((string[])lb[i + x, i2 + y].Tag)[1] != "F" && lb[i + x, i2 + y].BackColor == Color.White)
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
            if (best.Name != "") { press(best, "l"); } //left clicks our selected tile 
            else
            {
                //MessageBox.Show("Debug - no move");
                for(int i = 0; i < Global.GlobalInts[0]; i++)
                {
                    for(int i2 = 0; i2 < Global.GlobalInts[1]; i2++)
                    {
                        if (board[i, i2].BackColor == Color.White && board[i, i2].Image == null)
                        {
                            press(board[i, i2], "l");
                            break;
                        }
                    }
                }
            }
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
            values = [biggestN, getdata(sb, "flags", Global.GlobalBoard), getdata(sb, "hidden", Global.GlobalBoard)];
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
                    if (getdata(board[i, i2], "hidden", Global.GlobalBoard) == 0) { break; } //checks if we need to implement complex logic on that tile
                    for (int mod = 0; mod < 3; mod++) { Button cur = board[i + mod, i2]; if (cur.BackColor == Color.White || cur.Text == "" || cur.Image != null) { checks = false; break; } }//means we skip over if we dont have enough info on the current tiles
                    if (checks && Convert.ToInt32(board[i, i2].Text) - getdata(board[i,i2], "flags", Global.GlobalBoard) == 1 && Convert.ToInt32(board[i + 1, i2].Text) - getdata(board[i+1, i2], "flags", Global.GlobalBoard) == 2 && Convert.ToInt32(board[i + 2, i2].Text) - getdata(board[i+2, i2], "flags", Global.GlobalBoard) == 1)
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
                    if (getdata(board[i, i2], "hidden", Global.GlobalBoard) == 0) { break; } //checks if we need to implement complex logic on that tile
                    for (int mod = 0; mod < 3; mod++) { Button cur = board[i, i2 + mod]; if (cur.BackColor == Color.White || cur.Text == "" || cur.Image != null) { checks = false; continue; } }
                    if (checks && Convert.ToInt32(board[i, i2].Text) - getdata(board[i, i2], "flags", Global.GlobalBoard) == 1 && Convert.ToInt32(board[i, i2 + 1].Text) - getdata(board[i, i2 + 1], "flags", Global.GlobalBoard) == 2 && Convert.ToInt32(board[i, i2 + 2].Text) - getdata(board[i, i2 + 2], "flags", Global.GlobalBoard) == 1)
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
                    if (getdata(board[i, i2], "hidden", Global.GlobalBoard) == 0) { break; } //checks if we need to implement complex logic on that tile
                    for (int mod = 0; mod < 4; mod++) { Button cur = board[i + mod, i2]; if (cur.BackColor == Color.White || cur.Text == "" || cur.Image != null) { checks = false; break; } }//means we skip over if we dont have enough info on the current tiles
                    if (checks && Convert.ToInt32(board[i, i2].Text) - getdata(board[i, i2], "flags", Global.GlobalBoard) == 1 && Convert.ToInt32(board[i + 1, i2].Text) - getdata(board[i+1, i2], "flags", Global.GlobalBoard) == 2 && Convert.ToInt32(board[i + 2, i2].Text) - getdata(board[i+2, i2], "flags", Global.GlobalBoard) == 2 && Convert.ToInt32(board[i+3, i2].Text) - getdata(board[i+3, i2], "flags", Global.GlobalBoard) == 1)
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
                    if (getdata(board[i, i2], "hidden", Global.GlobalBoard) == 0) { break; } //checks if we need to implement complex logic on that tile
                    for (int mod = 0; mod < 4; mod++) { Button cur = board[i, i2 + mod]; if (cur.BackColor == Color.White || cur.Text == "" || cur.Image != null) { checks = false; continue; } }
                    if (checks && Convert.ToInt32(board[i, i2].Text) - getdata(board[i, i2], "flags", Global.GlobalBoard) == 1 && Convert.ToInt32(board[i, i2 + 1].Text) - getdata(board[i, i2+1], "flags", Global.GlobalBoard) == 2 && Convert.ToInt32(board[i, i2 + 2].Text) - getdata(board[i, i2+2], "flags", Global.GlobalBoard) == 2 && Convert.ToInt32(board[i, i2 + 3].Text) - getdata(board[i, i2 + 3], "flags", Global.GlobalBoard) == 1)
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
            int bxr = Global.GlobalInts[0] - 1; //gets the last x row
            int byr = Global.GlobalInts[1] - 1; //gets the last y row
            for(int i = 1; i < Global.GlobalInts[0]-1; i++)
            {
                if (board[i, 0].Text == "" || board[i, 1].Text == "") { continue; }
                if (Convert.ToInt32(board[i, 0].Text) - getdata(board[i, 0], "flags", Global.GlobalBoard) == 1 && Convert.ToInt32(board[i, 1].Text) - getdata(board[i, 1], "flags", Global.GlobalBoard) == 1) //top row one one check
                {
                    if (board[i + 1, 0].BackColor == Color.White && board[i + 1, 1].BackColor == Color.White && board[i + 1, 2].BackColor == Color.White && board[i + 1, 2].Image == null) { press(board[i + 1, 2], "l"); return true; }
                    if (board[i - 1, 0].BackColor == Color.White && board[i - 1, 1].BackColor == Color.White && board[i - 1, 2].BackColor == Color.White && board[i - 1, 2].Image == null) { press(board[i - 1, 2], "l"); return true; }
                }
            }
            for(int i = 1; i < Global.GlobalInts[0] - 1; i++)
            {
                if (board[i, byr].Text == "" || board[i, byr - 1].Text == "") { continue; }
                if (Convert.ToInt32(board[i, byr].Text) - getdata(board[i, byr], "flags", Global.GlobalBoard) == 1 && Convert.ToInt32(board[i, byr - 1].Text) - getdata(board[i, byr - 1], "flags", Global.GlobalBoard) == 1) //bottom row
                {
                    if (board[i + 1, byr].BackColor == Color.White && board[i + 1, byr - 1].BackColor == Color.White && board[i + 1, byr - 2].BackColor == Color.White && board[i + 1, byr - 2].Image == null) { press(board[i + 1, byr - 2], "l"); return true; }
                    if (board[i - 1, byr].BackColor == Color.White && board[i - 1, byr - 1].BackColor == Color.White && board[i - 1, byr - 2].BackColor == Color.White && board[i - 1, byr - 2].Image == null) { press(board[i - 1, byr - 2], "l"); return true; }
                }
            }
            for(int i = 1; i < Global.GlobalInts[1] - 1; i++)
            {
                if (board[0, i].Text == "" || board[1, i].Text == "") { continue; }
                if (Convert.ToInt32(board[0, i].Text) - getdata(board[0, i], "flags", Global.GlobalBoard) == 1 && Convert.ToInt32(board[1, i].Text) - getdata(board[1, i], "flags", Global.GlobalBoard) == 1) //checks pattern on first y row
                {
                    if (board[0, i + 1].BackColor == Color.White && board[1, i + 1].BackColor == Color.White && board[2, i + 1].BackColor == Color.White && board[2, i + 1].Image == null) { press(board[2, i + 1], "l"); return true; }
                    if (board[0, i - 1].BackColor == Color.White && board[1, i - 1].BackColor == Color.White && board[2, i - 1].BackColor == Color.White && board[2, i - 1].Image == null) { press(board[2, i - 1], "l"); return true; }
                }
            }
            for(int i = 1; i < Global.GlobalInts[1] - 1; i++)
            {
                if (board[bxr, i].Text == "" || board[bxr - 1, i].Text == "") { continue; }
                if (Convert.ToInt32(board[bxr, i].Text) - getdata(board[bxr, i], "flags", Global.GlobalBoard) == 1 && Convert.ToInt32(board[bxr - 1, i].Text) - getdata(board[bxr - 1, i], "flags", Global.GlobalBoard) == 1) //checks on final y row
                {
                    if (board[bxr, i + 1].BackColor == Color.White && board[bxr - 1, i + 1].BackColor == Color.White && board[bxr - 2, i + 1].BackColor == Color.White && board[bxr - 2, i + 1].Image == null) { press(board[bxr - 2, i + 1], "l"); return true; }
                    if (board[bxr, i - 1].BackColor == Color.White && board[bxr - 1, i - 1].BackColor == Color.White && board[bxr - 2, i - 1].BackColor == Color.White && board[bxr - 2, i - 1].Image == null) { press(board[bxr - 2, i - 1], "l"); return true; }
                }
            }
            return false;
        }

        private static bool ote()//one two edge pattern
        {
            Button[,] board = Global.GlobalBoard;
            int bxr = Global.GlobalInts[0] - 1; //gets the last x row
            int byr = Global.GlobalInts[1] - 1; //gets the last y row
            for (int i = 1; i < Global.GlobalInts[0] - 1; i++)
            {
                if (board[i, 0].Text == "" || board[i, 1].Text == "") { continue; }
                if (Convert.ToInt32(board[i, 0].Text) - getdata(board[i, 0], "flags", Global.GlobalBoard) == 1 && Convert.ToInt32(board[i, 1].Text) - getdata(board[i, 1], "flags", Global.GlobalBoard) == 2) //top row one one check
                {
                    if (board[i + 1, 0].BackColor == Color.White && board[i + 1, 1].BackColor == Color.White && board[i + 1, 2].BackColor == Color.White && board[i + 1, 2].Image == null) { press(board[i + 1, 2], "r"); return true; }
                    if (board[i - 1, 0].BackColor == Color.White && board[i - 1, 1].BackColor == Color.White && board[i - 1, 2].BackColor == Color.White && board[i - 1, 2].Image == null) { press(board[i - 1, 2], "r"); return true; }
                }
            }
            for (int i = 1; i < Global.GlobalInts[0] - 1; i++)
            {
                if (board[i, byr].Text == "" || board[i, byr - 1].Text == "") { continue; }
                if (Convert.ToInt32(board[i, byr].Text) - getdata(board[i, byr], "flags", Global.GlobalBoard) == 1 && Convert.ToInt32(board[i, byr - 1].Text) - getdata(board[i, byr - 1], "flags", Global.GlobalBoard) == 2) //bottom row
                {
                    if (board[i + 1, byr].BackColor == Color.White && board[i + 1, byr - 1].BackColor == Color.White && board[i + 1, byr - 2].BackColor == Color.White && board[i + 1, byr - 2].Image == null) { press(board[i + 1, byr - 2], "r"); return true; }
                    if (board[i - 1, byr].BackColor == Color.White && board[i - 1, byr - 1].BackColor == Color.White && board[i - 1, byr - 2].BackColor == Color.White && board[i - 1, byr - 2].Image == null) { press(board[i - 1, byr - 2], "r"); return true; }
                }
            }
            for (int i = 1; i < Global.GlobalInts[1] - 1; i++)
            {
                if (board[0, i].Text == "" || board[1, i].Text == "") { continue; }
                if (Convert.ToInt32(board[0, i].Text) - getdata(board[0, i], "flags", Global.GlobalBoard) == 1 && Convert.ToInt32(board[1, i].Text) - getdata(board[1, i], "flags", Global.GlobalBoard) == 2) //checks pattern on first y row
                {
                    if (board[0, i + 1].BackColor == Color.White && board[1, i + 1].BackColor == Color.White && board[2, i + 1].BackColor == Color.White && board[2, i + 1].Image == null) { press(board[2, i + 1], "r"); return true; }
                    if (board[0, i - 1].BackColor == Color.White && board[1, i - 1].BackColor == Color.White && board[2, i - 1].BackColor == Color.White && board[2, i - 1].Image == null) { press(board[2, i - 1], "r"); return true; }
                }
            }
            for (int i = 1; i < Global.GlobalInts[1] - 1; i++)
            {
                if (board[bxr, i].Text == "" || board[bxr - 1, i].Text == "") { continue; }
                if (Convert.ToInt32(board[bxr, i].Text) - getdata(board[bxr, i], "flags", Global.GlobalBoard) == 1 && Convert.ToInt32(board[bxr - 1, i].Text) - getdata(board[bxr - 1, i], "flags", Global.GlobalBoard) == 2) //checks on final y row
                {
                    if (board[bxr, i + 1].BackColor == Color.White && board[bxr - 1, i + 1].BackColor == Color.White && board[bxr - 2, i + 1].BackColor == Color.White && board[bxr - 2, i + 1].Image == null) { press(board[bxr - 2, i + 1], "r"); return true; }
                    if (board[bxr, i - 1].BackColor == Color.White && board[bxr - 1, i - 1].BackColor == Color.White && board[bxr - 2, i - 1].BackColor == Color.White && board[bxr - 2, i - 1].Image == null) { press(board[bxr - 2, i - 1], "r"); return true; }
                }
            }
            return false;
        }

        private static bool endgame()
        {
            List<Button> whites = new List<Button>();
            Button[,] board = Global.GlobalBoard;
            
            for(int i = 0; i < Global.GlobalInts[0]; i++)
            {
                for(int i2 = 0; i2 < Global.GlobalInts[1]; i2++)
                {
                    if (board[i, i2].BackColor == Color.White && board[i, i2].Image == null) { whites.Add(board[i, i2]); }
                }
            }
            //foreach(Button b in whites) { MessageBox.Show(b.Name + "    " + Global.mines); }
            if(Global.mines == whites.Count)
            {
                foreach(Button b in whites)
                {
                    press(b, "r"); return true;
                }
            }
            
            if(Global.mines == 0)
            {
                foreach(Button b in whites)
                {
                    press(b, "l"); return true;
                }
            }
            return false;
        }

        private static void betterguessing()
        {
            //iterate through board, find all unrevealed tiles with no information on them
            //working on generating many configurations of possible board states. getting tiles hidden around each edge peice and randomly placing tiles into them
            Button[,] tmpboard = DeepClone(Global.GlobalBoard); //clones board (not as a pointer but as a fresh arr so we can mess around without screwing global arr)
            List<Button> tiles = new List<Button>();
            Random rnd = new Random();
            for (int i = 0; i < Global.GlobalInts[0]; i++)
            {
                for (int i2 = 0; i2 < Global.GlobalInts[1]; i2++)
                {
                    if (tmpboard[i, i2].BackColor == Color.Gray && tmpboard[i, i2].Image == null && tmpboard[i, i2].Text!="" && getdata(tmpboard[i,i2], "hidden", tmpboard) > 0)
                    {
                        tiles.Add(tmpboard[i, i2]); //collects all tiles with unrevealed tiles around it, and that we have a number for
                    }
                }
            }
            //start from 1 thing. do all combis in that. foreach of those combis, clone board and do combis in there
            Button start = tiles[0];
            List<Button> hiddenAround = gethiddensur(start, tmpboard);
            int requiredMines = Convert.ToInt32(start.Text) - getdata(start, "flags", tmpboard); // Mines to place around this tile
            List<List<Button>> placements = GenerateMinePlacements(hiddenAround, requiredMines);
            List<List<Button>> combis = loop(tmpboard, tiles, placements);
            testout(combis);
        }
        private static List<List<Button>> loop(Button[,] tmpboard, List<Button> tiles, List<List<Button>> placements)//this works but recursion depth.....
        {
            MessageBox.Show("loop called");
            foreach (List<Button> curcombi in placements)
            {
                Button[,] tmpboard1 = DeepClone(tmpboard);//clone tmpb
                placemines(curcombi, tmpboard1);//placemines
                List<Button> tiles1 = new List<Button>(tiles);
                tiles1.RemoveAt(0);
                if (tiles1.Count == 0) { return placements; }
                foreach (Button b in tiles1)
                {
                    List<Button> hiddenAround = gethiddensur(b, tmpboard1);
                    int requiredMines = Convert.ToInt32(b.Text) - getdata(b, "flags", tmpboard1); // Mines to place around this tile
                    placements = GenerateMinePlacements(hiddenAround, requiredMines);
                    loop(tmpboard1, tiles1, placements);
                }
            }
            return placements;
        }
        private static Button[,] placemines(List<Button> placements, Button[,] lb) //places mines on a specific board passed into func
        {

            foreach (Button c in placements)
            {
                string name = c.Name;
                string[] namestr = name.Split(' ');
                int i = Convert.ToInt32(namestr[0]);
                int i2 = Convert.ToInt32(namestr[1]);
                lb[i, i2].Image = Global.getflag;
            }
            
            return lb;
        }
        private static void testout(List<List<Button>> list) //test print
        {
            foreach(List<Button> blist in list)
            {
                MessageBox.Show("New Combi");
                foreach (Button b in blist)
                {
                    MessageBox.Show(b.Name);
                }
            }
        }
        private static List<List<Button>> GenerateMinePlacements(List<Button> hiddenAround, int numMines) //main for genning mine placements
        {
            List<List<Button>> minePlacements = new List<List<Button>>();
            IEnumerable<IEnumerable<int>> combinations = GetCombinations(Enumerable.Range(0, hiddenAround.Count), numMines);

            foreach (var combination in combinations)
            {
                List<Button> placement = new List<Button>();
                foreach (int index in combination)
                {
                    placement.Add(hiddenAround[index]); // Map indices to the actual buttons
                }
                minePlacements.Add(placement);
            }
            return minePlacements;
        }

        // Helper function to generate combinations
        public static IEnumerable<IEnumerable<T>> GetCombinations<T>(IEnumerable<T> list, int length)
        {
            if (length == 0)
                return new[] { new T[0] };

            return list.SelectMany((item, index) =>
                GetCombinations(list.Skip(index + 1), length - 1).Select(result => new[] { item }.Concat(result)));
        }


        private static List<Button> gethiddensur(Button b, Button[,] lb) //gets a list of hidden tiles around a tile
        {
            List<Button> retlist = new List<Button>();
            string name = b.Name;
            string[] namestr = name.Split(' ');
            int i = Convert.ToInt32(namestr[0]);
            int i2 = Convert.ToInt32(namestr[1]);
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
                        if (((string[])lb[i + x, i2 + y].Tag)[1] != "F" && lb[i + x, i2 + y].BackColor == Color.White)
                        {
                            retlist.Add(lb[i + x, i2 + y]);
                        }
                    }
                }
            }
            return retlist;
        }
        private static Button[,] DeepClone(Button[,] originalBoard)
        {
            int rows = originalBoard.GetLength(0);
            int cols = originalBoard.GetLength(1);

            Button[,] clonedBoard = new Button[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Button originalButton = originalBoard[i, j];
                    Button newButton = new Button
                    {
                        Name = originalButton.Name,
                        BackColor = originalButton.BackColor,
                        Image = originalButton.Image,
                        Text = originalButton.Text,
                        Tag = originalButton.Tag 
                    };
                    clonedBoard[i, j] = newButton;
                }
            }

            return clonedBoard;
        }
    }

}
