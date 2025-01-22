
//bot is ran on line 315 if needed changing
namespace minesweeper_ui
{
    static class Global //stores data across forms instead of tracking parameters through 8 functions
    {
        private static string diff = ""; //initializes variables
        private static int[] customvars = new int[3];
        private static Button[,] bd;
        private static Image flag;
        private static Image mine;
        private static bool a = true;
        private static int b = 0;
        private static bool c = true;
        private static int[] d = new int[2];
        private static int[] e = new int[2];
        private static bool f = true;
        private static int g = 0;
        public static string GlobalVar //each block is near identical, just allows for storage and access of data
        {
            get { return diff; }
            set { diff = value; }
        }

        public static int[] GlobalInts
        {
            get { return customvars; }
            set { customvars = value; }
        }

        public static Button[,] GlobalBoard
        {
            get { return bd; }
            set { bd = value; }
        }
        public static Image getflag
        {
            get { return flag; }
            set { flag = value; }
        }
        public static Image getmine
        {
            get { return mine; }
            set { mine = value; }
        }

        public static bool firstmove
        {
            get { return a; }
            set { a = value; }
        }

        public static int mines
        {
            get { return b; }
            set { b = value; }
        }

        public static bool outputwin
        {
            get { return c; }
            set { c = value; }
        }

        public static int[] xypos
        {
            get { return d; }
            set { d = value; }
        }
        public static int[] startpos
        {
            get { return e; }
            set { e = value; }
        }
        public static bool botrunning
        {
            get { return f; }
            set { f = value; }
        }
        public static int guesses
        {
            get { return g; }
            set { g = value; }
        }
    }

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static int[] getsizes(string diff) //returns generic sizes per difficulty, or runs form3 for to take custom vars
        {
            if (diff == "Easy") { return new int[3]{8, 8, 10}; }
            if (diff == "Medium") { return new int [3]{16, 16, 40}; }
            if (diff == "Hard") { return new int[3]{ 30, 16, 99}; }
            if (diff == "Custom")
            {
                Application.Run(new CustomInputs());
                return Global.GlobalInts; 
            }
            //get user sizes for custom 
            return [0, 0, 0]; 
        }
        static void prettycolor (Button button, int mines) //gives each number of mines a nice color
        {
            switch (mines) //for each possible amount of mines, set a color
            {
                case 1:
                    button.ForeColor = Color.Blue;
                    break;
                case 2:
                    button.ForeColor = Color.LightGreen;
                    break;
                case 3:
                    button.ForeColor = Color.DarkRed;
                    break;
                case 4:
                    button.ForeColor = Color.DarkBlue;
                    break;
                case 5:
                    button.ForeColor = Color.Yellow;
                    break;
                case 6:
                    button.ForeColor = Color.DarkCyan;
                    break;
                case 7:
                    button.ForeColor = Color.MediumPurple;
                    break;
                case 8:
                    button.ForeColor= Color.DarkOrange;
                    break;
            }
            button.Text = mines.ToString(); //displays number of mines
        }
        static int neighbours (Button b) //checks neighbouring mines and recursively calls neighbours that arent mines
        {
            string[] tags = (string[])b.Tag; //pulls in global and processes data
            if (tags[1] == "M") { return 0; }
            int nmines = 0;
            string name = b.Name;
            string[] namestr = name.Split(' ');
            int i = Convert.ToInt32(namestr[0]);
            int i2 = Convert.ToInt32(namestr[1]);
            Button[,] board = Global.GlobalBoard;
            Button tmp = new Button();
            string[] tmptags = new string[2];
            
            for(int x = -1; x <= 1; x++) //x+- var
            {
                for(int y = -1; y<=1; y++) //y+- var
                {
                    if (x == 0 && y == 0) { continue; } //dont check self
                    int checkx = i + x; //gets index to check
                    int checky = i2 + y;
                    if (checkx >= 0 && checky >= 0 && checkx<board.GetLength(0) && checky<board.GetLength(1)) //checks our button is in the board range
                    {
                        tmp = board[checkx, checky];
                        tmptags = (string[])tmp.Tag; //gets the tags
                        if (tmptags[0] == "M")
                        {
                            nmines++; //increments mines
                        }
                    }
                }
            }

            //if no mines neighbour it, check all neighbours
            if (nmines == 0)
            {
                if(b.BackColor != Color.Gray)
                {
                    b.BackColor = Color.Gray;
                    for (int x = -1; x <= 1; x++) //x+- var
                    {
                        for (int y = -1; y <= 1; y++) //y+- var
                        {
                            if (x == 0 && y == 0) { continue; } //dont check self
                            int checkx = i + x; //adds
                            int checky = i2 + y;
                            if (checkx >= 0 && checky >= 0 && checkx < board.GetLength(0) && checky < board.GetLength(1)) //checks our button is in the board range
                            {
                                tmp = board[checkx, checky]; //gets tags
                                if (tmp.BackColor != Color.Gray)
                                {
                                    neighbours(tmp); //calls neighbours on each neighbour
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                b.BackColor = Color.Gray;
                prettycolor (b, nmines); //clears that spot and gives a nice color
            }
            Global.GlobalBoard = board;
            return 0;
        }
        static void placemines(Button exclude) //puts mines on the board. exclude is is the clicked mine, so we dont place mines directly on or next to the first clicked space
        {
            string name = exclude.Name; //pulls in data and initializes
            string[] namestr = name.Split(' ');
            int i = Convert.ToInt32(namestr[0]);
            int i2 = Convert.ToInt32(namestr[1]);
            Button[,] board = Global.GlobalBoard;
            Random rnd = new Random();
            int rndone;
            int rndtwo;
            for(int j = 0; j < Global.GlobalInts[2];) // for every mine
            {
                rndone = rnd.Next(0, board.GetLength(0));
                rndtwo = rnd.Next(0, board.GetLength(1));
                bool placeable = true;
                for (int x = -1; x <= 1; x++) //x+- var
                {
                    for (int y = -1; y <= 1; y++) //y+- var
                    {
                        if (rndone == i + x && rndtwo == i2 + y)
                        {
                            placeable = false; //sets placeable to false if its in the "exclusion zone" around the users clicked space
                        }
                    }
                }
                if (((string[])board[rndone, rndtwo].Tag)[0] == "M")
                {
                    placeable = false; //sets placeable to false if already a mine
                }
                if (placeable)
                {
                    board[rndone, rndtwo].Tag = new string[2] { "M", "" }; //places mine
                    j++; //increments. this is not done in the loop to ensure the right amount of mines are placed
                }
            }
            Global.GlobalBoard = board; //redifines board with the mines
        }
        static void press(object sender, MouseEventArgs e) //code for each user click
        {
            Button cur = (Button)sender;
            string[] tags = (string[])cur.Tag;
            bool doloop = Global.firstmove;
            if (doloop) //stuff to be done on the first move
            {
                placemines(cur); //places mines
                Global.firstmove = false;
                Global.mines = Global.GlobalInts[2];
                Form gamedata = new gamedata(); 
                gamedata.Show(); //runs gamedata form (which shows timer, mines left etc.)
            }
            
            if (e.Button == MouseButtons.Left && cur.Image == null) //Left click logic (only activates if the space is not flagged)
            {
                if (tags[0] == "M") //if you hit a mine, program dies after displaying a mine
                {
                    cur.Image = Global.getmine;
                    Global.botrunning = false;
                    //MessageBox.Show("You Hit a mine. Guesses: " + Global.guesses.ToString());
                    string FileName = "winr.txt";
                    TextReader TextReader = new StreamReader(FileName);
                    int wins = Convert.ToInt32(TextReader.ReadLine());
                    int attempts = Convert.ToInt32(TextReader.ReadLine());
                    TextReader.Close();
                    TextWriter TextWriter = new StreamWriter(FileName);
                    TextWriter.WriteLine(wins);
                    TextWriter.WriteLine(attempts + 1);
                    TextWriter.Close();
                    Environment.Exit(0);
                }
                neighbours(cur); //calls neighbours which then handles the logic
            }
            if (e.Button == MouseButtons.Right && cur.BackColor != Color.Gray) //Right click flagging logic (cant flag an already revealed tile
            {
                //get tags to see if flagged already
                if (tags[1] == "F") //unflags tile
                {
                    cur.BackColor = Color.White; //wipes the tile
                    cur.Image = null;
                    tags[1] = ""; //removes flagged tag
                    Global.mines = Global.mines+1; //adjusts remaining mines
                }
                else //flags tile
                {
                    cur.Image = Global.getflag; //puts flag image on
                    tags[1] = "F"; //adds flagged tag
                    Global.mines = Global.mines-1; //adjusts remaining mines
                }
                cur.Tag = tags;
            }
            
        }
        static void makeboard(string difficulty) //creates a board of set sizes
        {
            Form2 form = new Form2(); //form2 is the main minesweeper game
            int[] sizes = getsizes(difficulty); //gets sizes from user
            Global.GlobalInts = sizes;//unpacks data
            int size1 = sizes[0];
            int size2 = sizes[1];
            int mines = sizes[2];

            Button[,] board = new Button[size1, size2];

            form.ClientSize = new Size(size1 * 30, size2 * 30); //for 30x30 buttons. clientsize ignores the headers and margins
            for (int i = 0; i < size1; i++) //initializes every button 
            {
                for (int i2 = 0; i2 < size2; i2++)
                {
                    board[i, i2] = new Button();
                    board[i, i2].Text = "";
                    board[i, i2].BackColor = Color.White;
                    board[i, i2].Name = i + " " + i2; //naming convention for each button is this format. this allows us to unpack this later using name
                    board[i, i2].Size = new Size(30, 30); 
                    board[i, i2].Location = new Point(i * 30, i2 * 30); //places it based on index
                    board[i, i2].MouseDown += new MouseEventHandler(press); //binds it to the press function which handles inputs 
                    board[i, i2].Tag = new string[2] { "", "" }; //initializes tags
                    board[i, i2].ImageAlign = System.Drawing.ContentAlignment.MiddleCenter; //ensures images appear centred (for flags + mines visualisation)
                    form.Controls.Add(board[i, i2]); 
                }
            }
            Global.GlobalBoard = board; //puts board into global
            Global.xypos = [Screen.PrimaryScreen.Bounds.Width/2 + form.Width/2,  Screen.PrimaryScreen.Bounds.Height/2 - 100]; //stores x and y for gamedata form to be placed at
            Global.startpos = [(Screen.PrimaryScreen.Bounds.Width - form.Width) / 2 + 15, (Screen.PrimaryScreen.Bounds.Height - form.Height) / 2 + 15];
            bot bot = new bot();
            bot.Run();
            form.ShowDialog(); //runs the form
        }

        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Image flag = Image.FromFile("C:\\Users\\ZACKT\\Pictures\\flag.png"); //pulls and resizes flag image from files, then makes it global
            //laptop: "C:\\Users\\ZACKT\\Pictures\\flag.png"
            //home: ""C:\\Users\\Zack Thompson\\Pictures\\flag.png"
            flag = flag.GetThumbnailImage(30,30,null,IntPtr.Zero);
            Global.getflag = flag; 

            Image mineimg = Image.FromFile("C:\\Users\\ZACKT\\Pictures\\mine.png");//pulls and resizes mine image from files, then makes it global
            //laptop: "C:\\Users\\ZACKT\\Pictures\\mine.png"
            //home: "C:\\Users\\Zack Thompson\\Pictures\\mine.png"
            mineimg = mineimg.GetThumbnailImage(30, 30, null, IntPtr.Zero);
            Global.getmine = mineimg;

            //Application.Run(new StartMenu()); //runs difficulty selecting form
            Global.GlobalVar = "Hard";
            string diff = Global.GlobalVar; //makes difficulty global
            makeboard(diff); //creates board
        }
    }
}