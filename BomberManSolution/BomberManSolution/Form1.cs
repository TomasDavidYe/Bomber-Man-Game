using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BomberManSolution
{
    public enum DruhyObjektu { Sud, BezpecnePole, Hrac, Boost};
    public enum UkolAIHrace { SebratBoost, OdpalitSud, OhrozitHrace, SkrytSe, Zadny};
    public enum StavHry { Hra, VyhraHrace1, VyhraHrace2, ProhraZivychHracu};
    public enum PushedButton { Up, Down, Left, Right, DropBomb, NA};
    public enum TypyBoostu { Health, Range, BombCapacity, None};

    public partial class Form1 : Form
    {

              
        public static int velikostMapy;
        public static int delkaHranyFormlulare;
        public static int hrana;
        public static int cisloMapy = 1;
        public static bool viceHracu = false;
        public static bool Pauznuto;
        //konstanty udavajici hratelnost hry
        const int BombTimer = 7;
        const int maxRange = 4;
        const int maxBombCapacity = 3;
        const int StartingRange = 1; 
        const int BoostSpawningRate = 1;
        static Dictionary<char, PushedButton> PlayerKeys = new Dictionary<char, PushedButton>()
        {
            {'A', PushedButton.NA },
            {'E', PushedButton.NA },
        };
        static PictureBox[,] pozice;
        static Dictionary<char, Postava> Hrdinove;
        static List<ScoreScreen> tabuleVysledku;
        static int pocetHrdinu = 0;
        static int pocetZivychHracu = 0;
        static StavHry Stav; 
        static Timer GameTimer;
        public Form1()
        {
            InitializeComponent();        
        }       
       
        private void StartButton_Click(object sender, EventArgs e)
        {
            button1.Hide();
            button4.Hide();
            buttonPauze.Hide();
            buttonQuit.Hide();
            Pauznuto = false;
            buttonPauze.Text = "Pauze";
            if (sender is Button) viceHracu = Convert.ToBoolean(((Button)sender).Tag);
            label1.Hide();
            label2.Hide();
            pocetHrdinu = 0;
            pocetZivychHracu = 0;
            BackColor = Color.White;
            Mapa.InitMapa();
            Mapa.InitObrazky();         
            Mapa.Generate();       
            GameTimer = new Timer();
            GameTimer.Interval = 250;
            GameTimer.Start();
            Stav = StavHry.Hra;
            GameTimer.Tick += Mapa.Play;            
        }
        private void Player_KeyDown(object sender, KeyEventArgs e)
        {
            if (PlayerKeys['A'] == PushedButton.NA)
            {
                switch (e.KeyCode)
                {
                    case Keys.W:
                        PlayerKeys['A'] = PushedButton.Up;
                        return;
                    case Keys.S:
                        PlayerKeys['A'] = PushedButton.Down;
                        return;
                    case Keys.A:
                        PlayerKeys['A'] = PushedButton.Left;
                        return;
                    case Keys.D:
                        PlayerKeys['A'] = PushedButton.Right;
                        return;
                    case Keys.B:
                        PlayerKeys['A'] = PushedButton.DropBomb;                        
                        return;
                }
                
            }
            if (PlayerKeys['E'] == PushedButton.NA)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        PlayerKeys['E'] = PushedButton.Up;
                        return;
                    case Keys.Down:
                        PlayerKeys['E'] = PushedButton.Down;
                        return;
                    case Keys.Left:
                        PlayerKeys['E'] = PushedButton.Left;
                        return;
                    case Keys.Right:
                        PlayerKeys['E'] = PushedButton.Right;
                        return;
                    case Keys.NumPad5:
                        PlayerKeys['E'] = PushedButton.DropBomb;
                        return;
                }
            }
        }
        private void ExitGameButton_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void NewGameButton_Click(object sender, EventArgs e)
        {
            button2.Hide();
            button3.Hide();           
            button1.Show();
            button4.Show();
        }
        private void buttonQuit_Click(object sender, EventArgs e)
        {
            GoToMenu();
        }
        private void buttonPauze_Click(object sender, EventArgs e)
        {
            if (Pauznuto)
            {
                GameTimer.Start();
                Pauznuto = false;
                buttonPauze.Text = "Pauze";
            }
            else
            {
                GameTimer.Stop();
                Pauznuto = true;
                buttonPauze.Text = "Continue";
            }

        }
        public  void GoToMenu()
        {
            foreach (PictureBox picBox in pozice) picBox.Parent = null;
            foreach (ScoreScreen skore in tabuleVysledku)
            {
                skore.Ikona.Parent = null;
                skore.Zivoty.Parent = null;
            }
            label1.Show();
            label2.Show();
            button2.Show();
            button3.Show();
            Width = 600;
            Height = 600;
            BackColor = Color.Chocolate;
            GameTimer.Stop();
        }

        public class ScoreScreen
        {
            public PictureBox Ikona;
            public Label Zivoty;
            public ScoreScreen(int indexHrdiny)
            {
                Ikona = new PictureBox();
                Ikona.Parent = ActiveForm;
                Ikona.Height = 100;
                Ikona.Width = 70;
                Ikona.Top = 0;
                Ikona.Left = (indexHrdiny - 1) * 170;
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string jmenoSouboru = "Icons\\Player" + indexHrdiny.ToString() + ".png";
                string cesta = path + jmenoSouboru;
                Bitmap pom = new Bitmap(cesta);
                Ikona.Image = new Bitmap(pom, new Size(hrana, hrana));
                Zivoty = new Label();
                Zivoty.Parent = ActiveForm;
                Zivoty.Width = 50;
                Zivoty.Height = 50;
                Zivoty.Top = 25;
                Zivoty.Left = 70 + (indexHrdiny - 1) * 170;
                Zivoty.Text = "3";
                Zivoty.Font = new Font("Comics Sans", 25, FontStyle.Bold);
                Zivoty.TextAlign = ContentAlignment.MiddleCenter; 
            }
        }

        public static class Mapa
        {
            static char[,] mapaInChar;
            static int[,] mapaZapalenosti;            
            static Dictionary<char, Bitmap> obrazkyObjektu;
            public static List<Objekt> seznamObjektu;

            //metody zajistujici prubeh hry
            public static void InitObrazky()  
            {
                
                string[] typyObrazku =
                    {
                    "Wall X",
                    "Barrel Q",
                    "Volno .",
                    "Bomb *",
                    "BombFire +",
                    "Fire T",
                    "FireBarrel S",
                    "DeadPlayer U",
                    "DeadPlayerBomb V",
                    "Player1 A",
                    "Player1Bomb B",
                    "Player1Fire C" ,
                    "Player1BombFire D",
                    "Player2 E",
                    "Player2Bomb F",
                    "Player2Fire G", 
                    "Player2BombFire H",
                    "Player3 I",
                    "Player3Bomb J",
                    "Player3Fire K",
                    "Player3BombFire L",
                    "Player4 M",
                    "Player4Bomb N",
                    "Player4Fire O",
                    "Player4BombFire P",
                    "Health 1",
                    "HealthFire 2",
                    "RangeInc 3",
                    "RangeIncFire 4",
                    "BombCountInc 5",
                    "BombCountIncFire 6"
                    };
                obrazkyObjektu = new Dictionary<char, Bitmap>();               
                foreach (var typ in typyObrazku)
                { 
                    string jmeno = typ.Split(' ')[0];
                    char znak = typ.Split(' ')[1][0];
                    string path = AppDomain.CurrentDomain.BaseDirectory;
                    string jmenoSouboru = "Icons\\" + jmeno + ".png";
                    string cesta = path + jmenoSouboru;
                    Bitmap pom = new Bitmap(cesta);
                    Bitmap obrazek = new Bitmap(pom, new Size(hrana, hrana));
                    obrazkyObjektu.Add(znak, obrazek);
                }
            }
            public static void InitMapa()

            {                
                tabuleVysledku = new List<ScoreScreen>();
                Hrdinove = new Dictionary<char, Postava>();               
                string path = System.AppDomain.CurrentDomain.BaseDirectory;
                string jmenoSouboru = "Mapy\\Mapa" + cisloMapy.ToString() + ".txt";
                string cesta = path + jmenoSouboru;
                string[] vstup = System.IO.File.ReadAllLines(cesta);
                delkaHranyFormlulare = Convert.ToInt32(vstup[0]);
                velikostMapy = Convert.ToInt32(vstup[1]);
                hrana = delkaHranyFormlulare / velikostMapy;
                mapaInChar = new char[velikostMapy, velikostMapy];
                ActiveForm.Height = delkaHranyFormlulare + 150;
                ActiveForm.Width = delkaHranyFormlulare;
                pozice = new PictureBox[velikostMapy, velikostMapy];
                AIMind.mapaOhrozenosti = new int[velikostMapy, velikostMapy];
                mapaZapalenosti = new int[velikostMapy, velikostMapy];
                seznamObjektu = new List<Objekt>();
                for (int j = 0; j < velikostMapy; j++)
                {
                    for (int i = 0; i < velikostMapy; i++)
                    {
                        mapaInChar[j, i] = vstup[j + 2][i];
                        AIMind.mapaOhrozenosti[j, i] = 0;
                        mapaZapalenosti[j, i] = 0;
                        PictureBox pom = new PictureBox();
                        pom.Parent = ActiveForm;
                        pom.Width = hrana;
                        pom.Height = hrana;
                        pom.Top = j * hrana + 100;
                        pom.Left = i * hrana;
                        pozice[j, i] = pom;                        
                        //budu to muset vylepsit podle toho, kolik hracu bude AI a ktere budou lidske
                        switch (mapaInChar[j, i])
                        {
                            case 'A':                         
                                {
                                    pocetHrdinu++;
                                    pocetZivychHracu++;
                                    Postava novyObjekt = new Hrac(mapaInChar[j, i], j, i);
                                    Hrdinove.Add(mapaInChar[j, i], novyObjekt);
                                    seznamObjektu.Add(novyObjekt);
                                }
                                break;
                            case 'E':
                                {
                                    Postava novyObjekt;
                                    pocetHrdinu++;
                                    if (viceHracu)
                                    {
                                        pocetZivychHracu++;
                                        novyObjekt = new Hrac(mapaInChar[j, i], j, i);
                                    }
                                    else { novyObjekt = new AIHrac(mapaInChar[j, i], j, i); }
                                    Hrdinove.Add(mapaInChar[j, i], novyObjekt);
                                    seznamObjektu.Add(novyObjekt);
                                }
                                break;
                            case 'I':
                            case 'M':
                                {
                                    pocetHrdinu++;                               
                                    Postava novyObjekt = new AIHrac(mapaInChar[j, i], j, i);
                                    Hrdinove.Add(mapaInChar[j, i], novyObjekt);
                                    seznamObjektu.Add(novyObjekt);
                                }
                                break;

                            case 'Q':
                                {
                                    Sud novyObjekt = new Sud(j, i);
                                    seznamObjektu.Add(novyObjekt);
                                    break;
                                }
                            case '1':
                                {
                                    Boost novyObjekt = new Boost(TypyBoostu.Health,j,i);
                                    seznamObjektu.Add(novyObjekt);
                                    break;
                                }
                            case '3':
                                {
                                    Boost novyObjekt = new Boost(TypyBoostu.Range, j, i);
                                    seznamObjektu.Add(novyObjekt);
                                    break;
                                }
                            case '5':
                                {
                                    Boost novyObjekt = new Boost(TypyBoostu.BombCapacity, j, i);
                                    seznamObjektu.Add(novyObjekt);
                                    break;
                                }
                        }
                    }
                }
            }
            public static void Generate()
            {

                for (int i = 0; i < velikostMapy; i++)
                {
                    for (int j = 0; j < velikostMapy; j++)
                    {

                        char znak = mapaInChar[j, i];
                        pozice[j, i].Image = obrazkyObjektu[znak];
                    }
                }
                PlayerKeys['A'] = PushedButton.NA;
                PlayerKeys['E'] = PushedButton.NA;
                if (ActiveForm != null) ActiveForm.Refresh();
            }
            public static void Play(object sender, EventArgs e)
            {
                if (Stav == StavHry.Hra)
                {
                    PohniVsemiPrvky();
                    return;
                }
                else KonecHry();

            }
            public static void PohniVsemiPrvky()
            {
                //pridavam do seznamu v dobe prochazeni seznamu, potrebuju oddelit
                var pomocnySeznam = seznamObjektu.ToList();
                foreach (Objekt prvek in pomocnySeznam)
                {
                    prvek.Krok();
                }
                Generate();
            }
            public static void KonecHry()
            {
                GameTimer.Stop();
                switch (Stav)
                {                    
                    case StavHry.VyhraHrace1:
                        MessageBox.Show("Player 1 Wins!!!");
                        break;
                    case StavHry.VyhraHrace2:
                        MessageBox.Show("Player 2 Wins!!!");
                        break;
                    case StavHry.ProhraZivychHracu:
                        MessageBox.Show("Game Over!!!");                                                
                        break;                    
                }
                Form1 formular = (Form1)ActiveForm;
                formular.GoToMenu();
            }  

            //pomocne metody pomahajici objektum v mape plnit svoje poslani
            public static char VratZnakNaSouradnici(int y, int x)
            {
                return mapaInChar[y, x];
            }
            public static bool JeBomba(int y, int x)
            {
                char[] prvkySBombou = { '*', 'B', 'F', 'G', 'H' };
                return prvkySBombou.Contains(mapaInChar[y, x]);
            }
            public static bool JeVolno(int y, int x)
            {
                char[] volnaPole = { '.', '1', '3', '5' };
                return volnaPole.Contains(mapaInChar[y, x]);
            }
            public static void PresunHrdinu(int puvY, int puvX, int noveY, int noveX)
            {
                if (puvY != noveY || puvX != noveX)
                {
                    char znak = mapaInChar[puvY, puvX];
                    switch (znak)
                    {
                        case 'B':                            
                        case 'F':   
                        case 'J':
                        case 'N':
                            mapaInChar[puvY, puvX] = '*';
                            mapaInChar[noveY, noveX] = (char) ((int)znak - 1);
                            break;
                        case 'C':
                        case 'G':
                        case 'K':
                        case 'O':
                            {
                                char znakHrdiny = (char)((int)znak - 2);
                                Postava hrdina = Hrdinove[znakHrdiny];
                                if (hrdina.VratPocetZivotu() > 0)
                                {
                                    mapaInChar[puvY, puvX] = '.';
                                    mapaInChar[noveY, noveX] = znakHrdiny;
                                }
                                else mapaInChar[puvY, puvX] = 'U';
                                break;
                            }
                        case 'D':
                        case 'H':
                        case 'L':
                        case 'P':
                            {
                                char znakHrdiny = (char)((int)znak - 3);
                                Postava hrdina = Hrdinove[znakHrdiny];
                                if (hrdina.VratPocetZivotu() > 0)
                                {
                                    mapaInChar[puvY, puvX] = '+';
                                    mapaInChar[noveY, noveX] = znakHrdiny;
                                }
                                else mapaInChar[puvY, puvX] = 'V';
                                break;
                            }
                        default:
                            mapaInChar[puvY, puvX] = '.';
                            mapaInChar[noveY, noveX] = znak;
                            break;
                    }
                }
            }
            public static void NovaBomba(Postava vlastnik, int range ,char znak, int y, int x)
            {
                znak++;
                Bomba novaBomba = new Bomba(y, x, vlastnik, range);
                mapaInChar[y, x] = znak;
                seznamObjektu.Add(novaBomba);
                //potrebuju dat AI hracum vedet, ktera pole jsou ohrozena
                char[] zapalitelnePrvky = {'.', '*','+', 'T', '1', '2', '3', '4', '5', '6', 'A', 'B', 'C', 'D',
                    'E','F','G','H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P','Q','S'
                };
                AIMind.Ohrozuj(y, x);
                int i = 1;
                while (i <= range && zapalitelnePrvky.Contains(mapaInChar[y + i, x])) 
                {
                    AIMind.Ohrozuj(y + i, x);
                    if (mapaInChar[y + i, x] == 'Q' || mapaInChar[y + i, x] == 'S') break;
                    else i++;
                }
                i = 1;
                while (i <= range && zapalitelnePrvky.Contains(mapaInChar[y - i, x]))
                {
                    AIMind.Ohrozuj(y - i, x);
                    if (mapaInChar[y - i, x] == 'Q' || mapaInChar[y - i, x] == 'S') break;
                    else i++;
                }
                i = 1;
                while (i <= range && zapalitelnePrvky.Contains(mapaInChar[y, x + i]))
                {
                    AIMind.Ohrozuj(y, x + i);
                    if (mapaInChar[y, x + i] == 'Q' || mapaInChar[y, x + i] == 'S') break;
                    else i++;
                }
                i = 1;
                while (i <= range && zapalitelnePrvky.Contains(mapaInChar[y, x - i]))
                {
                    AIMind.Ohrozuj(y, x - i);
                    if (mapaInChar[y, x - i] == 'Q' || mapaInChar[y, x - i] == 'S') break;
                    else i++;
                }
                return;
               
            }
            public static bool JeOhen(int y, int x)
            {
                char[] zapaleneObjekty = { 'T', 'C', 'G', 'K', 'O', 'D', 'H', 'L', 'P', 'S' };
                char znak = mapaInChar[y, x];
                return zapaleneObjekty.Contains(znak);
            }           
            public static bool Zapal(int y, int x)
            {
                Ohen novyOhen = new Ohen(y, x);
              
                switch (mapaInChar[y, x])
                {
                    case 'X':
                        return false;
                    //zapalitelne predmety maji svuj zapaleny obrazek o dva dal nez svuj puvodni                    
                    case 'Q':
                        seznamObjektu.Add(novyOhen);
                        mapaInChar[y, x] += (char) 2;
                        mapaZapalenosti[y, x]++;
                        return false;
                    case 'A':
                    case 'E':
                    case 'I':
                    case 'M':                    
                        seznamObjektu.Add(novyOhen);
                        mapaInChar[y, x] += (char)2;
                        mapaZapalenosti[y, x]++;
                        return true;
                    //pripad, kdyby bomba vybouchla na hrdinovy    
                    case 'B':
                    case 'F':
                    case 'J':
                    case 'N':
                        seznamObjektu.Add(novyOhen);
                        mapaInChar[y, x] += (char)2;
                        mapaZapalenosti[y, x]++;
                        return true;
                    

                    //zapualujeme prazdno nebo uz zapalene misto                      
                    case '.':
                    case 'T':                    
                        seznamObjektu.Add(novyOhen);
                        mapaInChar[y, x] = 'T';
                        mapaZapalenosti[y, x]++;
                        return true;
                    case '1':
                    case '3':
                    case '5':
                    case '*':
                        seznamObjektu.Add(novyOhen);
                        mapaZapalenosti[y, x]++;
                        mapaInChar[y, x]++;
                        return true;
                    default:                        
                        return false;

                }
            }
            public static void Uhas(Ohen ohen, int y, int x)
            {
                seznamObjektu.Remove(ohen);
                AIMind.mapaOhrozenosti[y, x]--;
                mapaZapalenosti[y, x]--;
                switch (mapaInChar[y, x])
                {
                    //kdyz hasim hrdinu, musim vedet, jestli je hrdina mrtvy nebo zivy
                    case 'C':
                    case 'G':
                    case 'K':
                    case 'O':
                        {
                            char pom = (char)((int)mapaInChar[y, x] - 2);
                            Postava danyHrdina = Hrdinove[pom];
                            int zivoty = danyHrdina.VratPocetZivotu();
                            if (zivoty > 0) mapaInChar[y, x] = pom;
                            else mapaInChar[y, x] = 'U';
                            break;
                        }
                    case 'D':
                    case 'H':
                    case 'L':
                    case 'P':
                        {
                            char pom = (char)((int)mapaInChar[y, x] - 3);
                            Postava danyHrdina = Hrdinove[pom];
                            int zivoty = danyHrdina.VratPocetZivotu();
                            if (zivoty > 0) mapaInChar[y, x] = (char)(pom + 1);
                            else mapaInChar[y, x] = 'V';
                        }
                        break;

                    case '1':
                    case '3':
                    case '5':
                    case 'U':
                    case 'A':
                    case 'E':
                    case 'I':
                    case 'M':                   
                        break;
                    case '2':
                    case '4':
                    case '6':
                    case '+':
                        mapaInChar[y, x]--;
                        break;

                    default:
                        if (mapaZapalenosti[y, x] == 0) mapaInChar[y, x] = '.';
                        break;
                }
            }
            public static void OdstranBombu(Bomba bomba, int y, int x)
            {
                seznamObjektu.Remove(bomba);
                switch (mapaInChar[y, x])
                {
                    case 'B':
                    case 'F':
                    case 'J':
                    case 'N':                        
                        mapaInChar[y, x]--;
                        break;
                    case 'D':
                    case 'H':
                    case 'L':
                    case 'P':
                        mapaInChar[y, x] -= (char)3;
                        break;
                    default:
                        mapaInChar[y, x] = 'T';
                        break;
                }
                        
            }            
            public static void OdstranPrvek(Objekt prvek, int y, int x)
            {
                seznamObjektu.Remove(prvek);
                mapaInChar[y, x] = '.';
            }
            public static void ZabijHrdinu(Postava hrdina, char symbol, int y, int x)
            {
                seznamObjektu.Remove(hrdina);
                Hrdinove.Remove(symbol);
                if (mapaInChar[y, x] == 'V') mapaInChar[y, x] = '*';
                else mapaInChar[y, x] = '.';
                pocetHrdinu--;
                if (hrdina is Hrac) pocetZivychHracu--;
                if (pocetZivychHracu == 0) Stav = StavHry.ProhraZivychHracu;
                else if(pocetHrdinu == 1)
                {
                    Postava Vitez = Hrdinove.Values.First();
                    if (Vitez is AIHrac) Stav = StavHry.ProhraZivychHracu;
                    else
                    {
                        char znakViteze = Hrdinove.Keys.First();
                        if (znakViteze == 'A') Stav = StavHry.VyhraHrace1;
                        else Stav = StavHry.VyhraHrace2;
                    }
                    
                }
            }
            public static void Boostni(Boost boost,TypyBoostu typBoostu, int y, int x)
            {               
                
                char znak = mapaInChar[y, x];
                char[] znakyHrdinu = { 'A', 'E', 'I', 'M' };
                if(znakyHrdinu.Contains(znak))
                {
                    seznamObjektu.Remove(boost);
                    Postava hrdina = Hrdinove[znak];
                    hrdina.Vylepsi(typBoostu);
                }                
            }
            public static void OdstranSud(Sud sud, TypyBoostu boost, int y, int x)
            {
                seznamObjektu.Remove(sud);
                Boost novyBoost = new Boost(boost, y, x);
                switch (boost)
                {
                    case TypyBoostu.Health:
                        mapaInChar[y, x] = '1';
                        seznamObjektu.Add(novyBoost);
                        break;
                    case TypyBoostu.Range:
                        mapaInChar[y, x] = '3';
                        seznamObjektu.Add(novyBoost);
                        break;
                    case TypyBoostu.BombCapacity:
                        seznamObjektu.Add(novyBoost);
                        mapaInChar[y, x] = '5';
                        break;
                    case TypyBoostu.None:
                        mapaInChar[y, x] = '.';                     
                        break;                    
                }
            }
        }
        //pomocna trida pro prohledavani do sirky
        public class Souradnice
        {
            public int y, x;
            public Souradnice(int y, int x)
            {
                this.y = y;
                this.x = x;
            }
        }

        //trida urcena pouze pro vystupy metod tridy AIMind
        public class UdajOPoloze
        {
            public int y, x;           
            public Stack<Souradnice> cesta;
            public int vzdalenost;                   
            public UdajOPoloze(int y, int x, int vzdalenost)
            {
                this.y = y;
                this.x = x;
                this.vzdalenost = vzdalenost;                
            }
        }

        public static class AIMind
        {            
            public static int[,] mapaOhrozenosti;
            public static void Ohrozuj(int y, int x)
            {
                mapaOhrozenosti[y, x]++;
            }
            public static void PrestanOhrozovat(int y, int x)
            {
                mapaOhrozenosti[y, x]--;
            }
            public static bool JeOhrozeno(int y, int x)
            {
                return mapaOhrozenosti[y, x] > 0;
            }
            public static UdajOPoloze ProhledaniDoSirky(int fromY, int fromX, DruhyObjektu hledanyObjekt)  
            {

                char[] znakyHledanehoObjektu = new char[1];
                char ZnakDanehoHrdiny = Mapa.VratZnakNaSouradnici(fromY, fromX);
                List<char> znakyHrdinu = new List<char>() { 'A', 'B', 'C', 'D','E','F','G','H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P' };
                //musim si rozskatulkovat, ktera pismenka reprezentuji ktere objekty
                switch (hledanyObjekt)
                {
                    case DruhyObjektu.Sud:
                        {
                            char[] pom = {'Q'};
                            znakyHledanehoObjektu = pom;
                        }
                        break;
                    case DruhyObjektu.BezpecnePole:
                        {
                            char[] pom = {'T','.' };
                            znakyHledanehoObjektu = pom;
                        }
                        break;
                    case DruhyObjektu.Hrac:
                        {                            
                            znakyHrdinu.RemoveAll(c => ZnakDanehoHrdiny <= c && c <= ZnakDanehoHrdiny + 3);
                            znakyHledanehoObjektu = znakyHrdinu.ToArray();
                        }
                        break;
                    case DruhyObjektu.Boost:
                        {
                            char[] pom = { '1', '2', '3', '4', '5', '6' };
                            znakyHledanehoObjektu = pom;
                        }
                        break;
                    default:
                        break;
                }

                int[,] pole = new int[velikostMapy,velikostMapy];
                for (int j = 0; j < velikostMapy; j++)
                {
                    for (int i = 0; i < velikostMapy; i++)
                    {
                        if(Mapa.VratZnakNaSouradnici(j,i) != 'X') pole[j, i] = -1;
                        else pole[j, i] = -5;
                    }
                }
                pole[fromY, fromX] = 0;
                Queue<Souradnice> fronta = new Queue<Souradnice>();
                fronta.Enqueue(new Souradnice(fromY, fromX));
                         
                while(fronta.Count > 0)
                {                    
                    Souradnice pom = fronta.Dequeue();
                    int y = pom.y, x = pom.x;
                    //kdyz uz najdu nejaky prvek hledaneho typu, najdu k nemu nejkratsi cestu a vratim se
                    if (znakyHledanehoObjektu.Contains(Mapa.VratZnakNaSouradnici(y, x)) && !JeOhrozeno(y,x))
                    {
                        int vzdalenost = pole[y, x];
                        UdajOPoloze vysledek = new UdajOPoloze(y, x, vzdalenost);
                        vysledek.cesta = new Stack<Souradnice>();
                        vysledek.cesta.Push(new Souradnice(y, x));
                        for (int k = pole[y,x]; k > 0; k--)
                        {
                            if (pole[y + 1, x] == k)
                            {
                                vysledek.cesta.Push(new Souradnice(y + 1, x));
                                y++;
                            }
                            else if (pole[y - 1, x] == k)
                            {
                                vysledek.cesta.Push(new Souradnice(y - 1, x));
                                y--;
                            }
                            else if (pole[y, x + 1] == k)
                            {
                                vysledek.cesta.Push(new Souradnice(y, x + 1));
                                x++;
                            }
                            else if (pole[y, x - 1] == k)
                            {
                                vysledek.cesta.Push(new Souradnice(y, x - 1));
                                x--;
                            }
                        }
                        return vysledek;                  
                    }                
                    else if (Mapa.JeOhen(y, x) || Mapa.JeVolno(y, x) || Mapa.VratZnakNaSouradnici(y,x) == ZnakDanehoHrdiny ) 
                    {
                        
                        if (Mapa.VratZnakNaSouradnici(y + 1, x) != 'X' && pole[y + 1, x] == -1)
                        {
                            fronta.Enqueue(new Souradnice(y + 1, x));
                            pole[y + 1, x] = pole[y, x] + 1;
                        }
                        if (Mapa.VratZnakNaSouradnici(y - 1, x) != 'X' && pole[y - 1, x] == -1)
                        {
                            fronta.Enqueue(new Souradnice(y - 1, x));
                            pole[y - 1, x] = pole[y, x] + 1;
                        }
                        if (Mapa.VratZnakNaSouradnici(y, x - 1) != 'X' && pole[y, x - 1] == -1)
                        {
                            fronta.Enqueue(new Souradnice(y, x - 1));
                            pole[y, x - 1] = pole[y, x] + 1;
                        }
                        if (Mapa.VratZnakNaSouradnici(y, x + 1) != 'X' && pole[y, x + 1] == -1)
                        {
                            fronta.Enqueue(new Souradnice(y, x + 1));
                            pole[y, x + 1] = pole[y, x] + 1;
                        }
                    }
                    
                                        
                }
                //kdyz hledany objekt neni v mape nebo neni v dosahu, vratim null
                return null;
            }
            public static UdajOPoloze NejblizsiBezpecnePole(int y, int x)
            {
                UdajOPoloze vysledek = ProhledaniDoSirky(y, x, DruhyObjektu.BezpecnePole);
                return vysledek;
            }            
            public static UdajOPoloze NejblizsiSud(int y, int x)
            {
                UdajOPoloze vysledek = ProhledaniDoSirky(y, x, DruhyObjektu.Sud);
                return vysledek;
            }
            public static UdajOPoloze NejblizsiHrac(char znakHrace, int y, int x)
            {
                UdajOPoloze vysledek = ProhledaniDoSirky(y, x, DruhyObjektu.Hrac);
                return vysledek;
            }
            public static UdajOPoloze NejblizsiBoost(int y, int x)
            {
                UdajOPoloze vysledek = ProhledaniDoSirky(y, x, DruhyObjektu.Boost);
                return vysledek;
            }
        }

        // y a j indexuji radky, x a i indexuji sloupce
        public abstract class Objekt
        {
            protected int y, x;
            public abstract void Krok();
        }

        public abstract class Postava : Objekt
        {
            //vlastnosti daneho hrace
            protected char symbol;
            protected int pocetZivotu;
            protected int range;
            protected int pocetBomb;
            protected ScoreScreen skore;
            //metody spolecne vsem hracum
            public Postava(int indexHrdiny) 
            {
                pocetBomb = 1;
                pocetZivotu = 3;
                range = StartingRange;
                skore = new ScoreScreen(indexHrdiny);
                tabuleVysledku.Add(skore);
            }
            public void AktualizujZivot(bool incZivoty)
            {
                if (incZivoty) pocetZivotu++;
                else pocetZivotu--;
                skore.Zivoty.Text = pocetZivotu.ToString();
            }
            public void PolozBombu()
            {
                if (pocetBomb > 0 && !Mapa.JeBomba(y,x) && pocetZivotu > 0)
                {
                    Mapa.NovaBomba(this, range, symbol, y, x);
                    pocetBomb--;
                }
            }
            public int VratPocetZivotu()
            {
                return this.pocetZivotu;
            }
            public void PridejBombu()
            {
                pocetBomb++;
            }
            public void Vylepsi(TypyBoostu boost)
            {
                switch (boost)
                {
                    case TypyBoostu.Health:
                        AktualizujZivot(true);
                        break;
                    case TypyBoostu.Range:
                        if (range < maxRange) range++;
                        break;
                    case TypyBoostu.BombCapacity:
                        if (pocetBomb < maxBombCapacity) pocetBomb++;
                        break;
                    case TypyBoostu.None:
                        break;                    
                }
            }
            public void Umri()
            {
                Mapa.ZabijHrdinu(this, symbol, y, x);
                skore.Zivoty.Font = new Font("Comics Sans", 8, FontStyle.Bold);
                skore.Zivoty.Text = "DEAD";
            }
               
        }
        //AI hrac a lidsky hrac se budou lisit pouze v metode, ktera dela dalsi krok
        public class Hrac : Postava
        {
            public Hrac(char symbol, int y, int x) : base(pocetHrdinu)
            {
                this.symbol = symbol;
                this.y = y;
                this.x = x;
            }
            public override void Krok() 
            {
                if (pocetZivotu > 0)
                {
                    int noveY = y, noveX = x;
                    if (Mapa.JeOhen(y, x))
                    {
                        AktualizujZivot(false);
                    }
                    else
                    {
                        switch (PlayerKeys[symbol])
                        {
                            case PushedButton.Up:
                                if (Mapa.JeVolno(y - 1, x) && pocetZivotu > 0)
                                {
                                    noveY--;
                                    Mapa.PresunHrdinu(y, x, noveY, noveX);
                                }
                                break;

                            case PushedButton.Down:
                                if (Mapa.JeVolno(y + 1, x) && pocetZivotu > 0)
                                {
                                    noveY++;
                                    Mapa.PresunHrdinu(y, x, noveY, noveX);
                                }
                                break;

                            case PushedButton.Left:
                                if (Mapa.JeVolno(y, x - 1) && pocetZivotu > 0)
                                {
                                    noveX--;
                                    Mapa.PresunHrdinu(y, x, noveY, noveX);
                                }
                                break;

                            case PushedButton.Right:
                                if (Mapa.JeVolno(y, x + 1) && pocetZivotu > 0)
                                {
                                    noveX++;
                                    Mapa.PresunHrdinu(y, x, noveY, noveX);
                                }
                                break;

                            case PushedButton.DropBomb:
                                PolozBombu();
                                break;

                            case PushedButton.NA:
                                break;
                        }
                        y = noveY;
                        x = noveX;
                    }                  
                }
                else Umri();

            }
        }

        public class AIHrac : Postava
        {
            //momentalni cil, kam by chtel AI hrac jit a polozit Bombu
            int cilY, cilX;
            Stack<Souradnice> cestaKCili;
            UkolAIHrace momentalniCil; 
            public AIHrac(char symbol,int y , int x) : base(pocetHrdinu)
            {
                momentalniCil = UkolAIHrace.Zadny;
                this.symbol = symbol;
                this.y = y;
                this.x = x;
                cilX = x;
                cilY = y;
                cestaKCili = new Stack<Souradnice>();
            }
            public override void Krok()
            {
                if (pocetZivotu > 0)
                {
                    int noveY = y, noveX = x;
                    if (Mapa.JeOhen(y, x))
                    {
                        AktualizujZivot(false);
                    }
                    else
                    {
                        if (AIMind.JeOhrozeno(y, x) && momentalniCil != UkolAIHrace.SkrytSe && !Mapa.JeOhen(y, x))
                        {
                            SkryjSe();
                        }
                        else
                        {
                            switch (momentalniCil)
                            {
                                case UkolAIHrace.SebratBoost:
                                    {
                                        if (y == cilY && x == cilX)
                                        {
                                            RozhodniSe();
                                        }
                                        else
                                        {
                                            Souradnice dalsiKrok = cestaKCili.First();
                                            if (!AIMind.JeOhrozeno(dalsiKrok.y, dalsiKrok.x) && Mapa.JeVolno(dalsiKrok.y, dalsiKrok.x))
                                            {
                                                Mapa.PresunHrdinu(y, x, dalsiKrok.y, dalsiKrok.x);
                                                y = dalsiKrok.y;
                                                x = dalsiKrok.x;
                                                cestaKCili.Pop();
                                            }
                                        }
                                        break;
                                    }

                                case UkolAIHrace.OdpalitSud:
                                    {                                        
                                        Souradnice dalsiKrok = cestaKCili.First();
                                        if (cestaKCili.Count == 1)
                                        {
                                            PolozBombu();                                            
                                        }
                                        else if (!AIMind.JeOhrozeno(dalsiKrok.y, dalsiKrok.x) && Mapa.JeVolno(dalsiKrok.y, dalsiKrok.x))
                                        {
                                            Mapa.PresunHrdinu(y, x, dalsiKrok.y, dalsiKrok.x);
                                            y = dalsiKrok.y;
                                            x = dalsiKrok.x;
                                            cestaKCili.Pop();
                                        }
                                        break;
                                    }

                                case UkolAIHrace.OhrozitHrace:
                                    break;

                                case UkolAIHrace.SkrytSe:
                                    SkryjSe();
                                    break;    
                                                                 
                                default:
                                    //kdyz neni v ohrozeni a zaroven nema zadny konkretni cil, musi se rozhodnout, co chce delat
                                    RozhodniSe();
                                    break;
                            }
                        }
                    }
                }
                else Umri();
            }     
            public void SkryjSe()
            {
                //jsem na cilovem poli a je tu bezpecno znamena, ze nemam co delat a mel bych si rozmyslet dalsi krok
                if (y == cilY && x == cilX && !AIMind.JeOhrozeno(y, x)) RozhodniSe();
                //jsem ohrozen a puvodni plan nebyl se skryt => prepnu si cil na skryvani, najdu nejblizsi bezpecne pole a vydam se tim smerem
                else if (AIMind.JeOhrozeno(y, x) && momentalniCil != UkolAIHrace.SkrytSe)
                {
                    UdajOPoloze nejblizsiBezpecnePole = AIMind.NejblizsiBezpecnePole(y, x);
                    if (nejblizsiBezpecnePole != null)
                    {
                        cilY = nejblizsiBezpecnePole.y;
                        cilX = nejblizsiBezpecnePole.x;
                        cestaKCili = nejblizsiBezpecnePole.cesta;
                        momentalniCil = UkolAIHrace.SkrytSe;
                        Souradnice dalsiKrok = cestaKCili.First();
                        if (Mapa.JeVolno(dalsiKrok.y,dalsiKrok.x))
                        {
                            Mapa.PresunHrdinu(y, x, dalsiKrok.y, dalsiKrok.x);
                            x = dalsiKrok.x;
                            y = dalsiKrok.y;
                            cestaKCili.Pop();
                        }
                    }
                }
                //mam v planu se skryt
                else if (momentalniCil == UkolAIHrace.SkrytSe && cestaKCili.Count > 0)
                {
                    //kdyz je z minulosti nejblizsi bezpecne pole stale bezpecne, musim se k nemu dostat, i kdybych sel pres ohrozene pole
                    Souradnice dalsiKrok = cestaKCili.First();
                    if (!AIMind.JeOhrozeno(cilY,cilX) && Mapa.JeVolno(dalsiKrok.y, dalsiKrok.x))
                    {
                        Mapa.PresunHrdinu(y, x, dalsiKrok.y, dalsiKrok.x);
                        x = dalsiKrok.x;
                        y = dalsiKrok.y;
                        cestaKCili.Pop();
                    }
                    //puvodni cilove pole jiz neni bezpecne => najdu si nove blizke pezpecne pole a vydam se tim smerem
                    else
                    {
                        UdajOPoloze nejblizsiBezpecnePole = AIMind.NejblizsiBezpecnePole(y, x);
                        if (nejblizsiBezpecnePole != null)
                        {
                            cilY = nejblizsiBezpecnePole.y;
                            cilX = nejblizsiBezpecnePole.x;
                            cestaKCili = nejblizsiBezpecnePole.cesta;
                            momentalniCil = UkolAIHrace.SkrytSe;
                            dalsiKrok = cestaKCili.First();
                            if (Mapa.JeVolno(dalsiKrok.y, dalsiKrok.x))
                            {
                                Mapa.PresunHrdinu(y, x, dalsiKrok.y, dalsiKrok.x);
                                x = dalsiKrok.x;
                                y = dalsiKrok.y;
                                cestaKCili.Pop();

                            }
                        }
                    }
                }
                
            }
            public void RozhodniSe()
            {
                UdajOPoloze nejblizsiSud = AIMind.NejblizsiSud(y, x);
                UdajOPoloze nejblizsiBoost = AIMind.NejblizsiBoost(y, x);                
                if (nejblizsiBoost != null && nejblizsiBoost.vzdalenost <= 5)
                {
                    cilY = nejblizsiBoost.y;
                    cilX = nejblizsiBoost.x;
                    cestaKCili = nejblizsiBoost.cesta;
                    momentalniCil = UkolAIHrace.SebratBoost;
                    Souradnice dalsiKrok = cestaKCili.First();
                    if(!AIMind.JeOhrozeno(dalsiKrok.y,dalsiKrok.x) && Mapa.JeVolno(dalsiKrok.y,dalsiKrok.x))
                    {
                        Mapa.PresunHrdinu(y, x, dalsiKrok.y, dalsiKrok.x);
                        y = dalsiKrok.y;
                        x = dalsiKrok.x;
                        cestaKCili.Pop();
                    }
                }
                else if (nejblizsiSud != null && nejblizsiSud.vzdalenost <= 5)
                {
                    cilY = nejblizsiSud.y;
                    cilX = nejblizsiSud.x;
                    cestaKCili = nejblizsiSud.cesta;
                    momentalniCil = UkolAIHrace.OdpalitSud;
                    Souradnice dalsiKrok = cestaKCili.First();
                    if(cestaKCili.Count == 1)
                    {
                        PolozBombu();                        
                    }
                    else if (!AIMind.JeOhrozeno(dalsiKrok.y, dalsiKrok.x) && Mapa.JeVolno(dalsiKrok.y, dalsiKrok.x))
                    {
                        Mapa.PresunHrdinu(y, x, dalsiKrok.y, dalsiKrok.x);
                        y = dalsiKrok.y;
                        x = dalsiKrok.x;
                        cestaKCili.Pop();
                    }
                }
                //situace, ve ktere jsou boosty a sudy daleko nebo vubec neexistuji
                //budu muset nejak nahodne vybrat dalsi krok
                else 
                {
                    momentalniCil = UkolAIHrace.Zadny;
                    List<Souradnice> seznamTahu = new List<Souradnice>();
                    if (Mapa.JeVolno(y + 1, x) && !AIMind.JeOhrozeno(y + 1, x)) seznamTahu.Add(new Souradnice(y + 1, x));
                    if (Mapa.JeVolno(y - 1, x) && !AIMind.JeOhrozeno(y - 1, x)) seznamTahu.Add(new Souradnice(y - 1, x));
                    if (Mapa.JeVolno(y, x + 1) && !AIMind.JeOhrozeno(y, x + 1)) seznamTahu.Add(new Souradnice(y, x + 1));
                    if (Mapa.JeVolno(y, x - 1) && !AIMind.JeOhrozeno(y, x - 1)) seznamTahu.Add(new Souradnice(y, x - 1));
                    //jestli je seznam prazdny, tak jsou sousedni pole zablokovana nebo ohrozena, bude muset pockat na dalsi tah
                    if(seznamTahu.Count > 0)
                    {
                        seznamTahu.Add(null);
                        Random numGen = new Random();
                        int nahCislo = numGen.Next(seznamTahu.Count);
                        Souradnice novyTah = seznamTahu[nahCislo];
                        if (novyTah == null) PolozBombu();
                        else
                        {
                            Mapa.PresunHrdinu(y, x, novyTah.y, novyTah.x);
                            y = novyTah.y;
                            x = novyTah.x;
                        }
                           
                    }
                    
                }
            }
        }
        
        

        public class Sud : Objekt
        {
            TypyBoostu boostVSudu;
            public override void Krok()
            {

                if (Mapa.JeOhen(y, x))
                {                       
                    Mapa.OdstranSud(this,boostVSudu, y, x);
                }
            }
            public Sud(int y, int x)
            {
                this.y = y;
                this.x = x;
                this.boostVSudu = BoostGen();
            }
            public TypyBoostu BoostGen()
            {
                List<TypyBoostu> pom = new List<TypyBoostu>()
                {
                    TypyBoostu.Health,
                    TypyBoostu.BombCapacity,
                    TypyBoostu.Range,
                };
                for (int i = 0; i < BoostSpawningRate; i++)
                {
                    pom.Add(TypyBoostu.None);
                }
                Random numGen = new Random();
                int randNum = numGen.Next(pom.Count);
                return pom[randNum];
            }
        }

        public class Bomba : Objekt
        {
            //attributy bomby
            int timer;
            int range;
            Postava vlastnik;
            public Bomba(int y, int x, Postava vlastnik, int range)
            {
                timer = BombTimer;
                this.range = range;
                this.y = y;
                this.x = x;
                this.vlastnik = vlastnik;
            }
            public override void Krok()
            {
                if (timer > 0) timer--;
                else Vybouchni();
            }            
            public void Vybouchni()
            {
                Mapa.OdstranBombu(this, y, x);
                vlastnik.PridejBombu();
                Mapa.Zapal(y, x);
                int i = 1;
                while (Mapa.Zapal(y + i, x) && i < range) i++;
                i = 1;
                while (Mapa.Zapal(y - i, x) && i < range) i++;
                i = 1;
                while (Mapa.Zapal(y, x + i) && i < range) i++;
                i = 1;
                while (Mapa.Zapal(y, x - i) && i < range) i++;
            }
        }

        public class Ohen : Objekt
        {
            public override void Krok()
            {
                Mapa.Uhas(this, y, x);
            }
            public Ohen(int y, int x)
            {
                this.y = y;
                this.x = x;
            }
        }

        public class Boost : Objekt
        {
            TypyBoostu typBoostu;
            public Boost(TypyBoostu typBoostu, int y, int x)
            {
                this.typBoostu = typBoostu;
                this.y = y;
                this.x = x;
            }
            public override void Krok()
            {
                Mapa.Boostni(this, typBoostu, y, x);
            }
        }

       
    }
}
