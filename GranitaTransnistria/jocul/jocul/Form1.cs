
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace Granicerul_Transnistria
{
    public partial class Form1 : Form
    {
        private Panel pnlWrapper;
        private bool isFullscreen = false;
        private Button btnFullscreenMenu;
        private Button btnFullscreenGame;

        // Containere principale (Ecrane)
        private Panel pnlMainMenu;
        private Panel pnlGame;

        // Elementele de Meniu
        private PictureBox picMenuBackground;
        private Label lblTitle;
        private Button btnStartGame;
        private Button btnExitGame;

        // Elementele de Joc (UI adaptate pentru format extins HD)
        private Label lblGreed, lblEmpathy, lblLoyalty;
        private Panel bgGreed, bgEmpathy, bgLoyalty; // Fundalul barilor (Gri)
        private Panel fgGreed, fgEmpathy, fgLoyalty; // Partea colorata a barilor
        private PictureBox picBackground;
        private Label lblStory;
        private Button[] choiceButtons;
        private Button btnSave;
        private Button btnLoad;
        private Button btnBackToMenu;

        // Datele jocului
        private GameState currentState;
        private Dictionary<string, StoryNode> storyDictionary;
        private const string SaveFilePath = "salvare_granicer.json";
        private string imagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

        public Form1()
        {
            this.Text = "Grănicerul: Control Tiraspol";
            // Am mărit rezoluția la 1280 x 720 (Format HD)
            this.Size = new Size(1280, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(25, 25, 30);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            if (!Directory.Exists(imagesPath)) Directory.CreateDirectory(imagesPath);

            InitializeMainMenu();
            InitializeGameUI();
            InitializeStoryData();

            ShowMainMenu();
        }

        private void InitializeMainMenu()
        {
            pnlMainMenu = new Panel { Dock = DockStyle.Fill };
            this.Controls.Add(pnlMainMenu);

            picMenuBackground = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            string menuImgPath = Path.Combine(imagesPath, "meniu_granita.jpg");
            if (File.Exists(menuImgPath)) picMenuBackground.Image = Image.FromFile(menuImgPath);
            pnlMainMenu.Controls.Add(picMenuBackground);

            lblTitle = new Label
            {
                Text = "DUTY AT THE BORDER",
                Font = new Font("Impact", 54, FontStyle.Regular),
                ForeColor = Color.DarkRed,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(250, 150) // Centrat mai bine pentru rezoluția nouă
            };
            picMenuBackground.Controls.Add(lblTitle);

            btnStartGame = new Button
            {
                Text = "ÎNCEPE SHIFT-UL",
                Font = new Font("Courier New", 16, FontStyle.Bold),
                Size = new Size(300, 60),
                Location = new Point(490, 320),
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.White
            };
            btnStartGame.Click += (s, e) => { StartNewGame(); ShowGame(); };
            picMenuBackground.Controls.Add(btnStartGame);

            btnExitGame = new Button
            {
                Text = "DESERTARE (IEȘIRE)",
                Font = new Font("Courier New", 16, FontStyle.Bold),
                Size = new Size(300, 60),
                Location = new Point(490, 410),
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.White
            };
            btnExitGame.Click += (s, e) => Application.Exit();
            picMenuBackground.Controls.Add(btnExitGame);
        }

        private void InitializeGameUI()
        {
            pnlGame = new Panel { Dock = DockStyle.Fill, Visible = false };
            this.Controls.Add(pnlGame);

            // Imagine mult mărită și lățită
            picBackground = new PictureBox
            {
                Location = new Point(40, 80),
                Size = new Size(1180, 320),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(15, 15, 20)
            };
            pnlGame.Controls.Add(picBackground);

            // --- BARILE DE STATUS (Mărite la 200px lățime) ---
            // LĂCOMIE (Aur/Galben)
            lblGreed = new Label { Location = new Point(40, 25), Size = new Size(90, 25), ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Text = "Lăcomie:" };
            bgGreed = new Panel { Location = new Point(130, 28), Size = new Size(200, 20), BackColor = Color.DarkGray };
            fgGreed = new Panel { Location = new Point(0, 0), Size = new Size(200, 20), BackColor = Color.Gold };
            bgGreed.Controls.Add(fgGreed);
            pnlGame.Controls.Add(lblGreed); pnlGame.Controls.Add(bgGreed);

            // EMPATIE (Verde)
            lblEmpathy = new Label { Location = new Point(380, 25), Size = new Size(90, 25), ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Text = "Empatie:" };
            bgEmpathy = new Panel { Location = new Point(470, 28), Size = new Size(200, 20), BackColor = Color.DarkGray };
            fgEmpathy = new Panel { Location = new Point(0, 0), Size = new Size(200, 20), BackColor = Color.MediumSeaGreen };
            bgEmpathy.Controls.Add(fgEmpathy);
            pnlGame.Controls.Add(lblEmpathy); pnlGame.Controls.Add(bgEmpathy);

            // LOIALITATE (Roșu Închis)
            lblLoyalty = new Label { Location = new Point(720, 25), Size = new Size(110, 25), ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Text = "Loialitate" };
            bgLoyalty = new Panel { Location = new Point(820, 28), Size = new Size(200, 20), BackColor = Color.DarkGray };
            fgLoyalty = new Panel { Location = new Point(0, 0), Size = new Size(200, 20), BackColor = Color.Crimson };
            bgLoyalty.Controls.Add(fgLoyalty);
            pnlGame.Controls.Add(lblLoyalty); pnlGame.Controls.Add(bgLoyalty);

            // Butoane Utilitare mutate în colțul dreapta-sus
            btnSave = new Button { Location = new Point(1060, 22), Size = new Size(80, 32), Text = "Salvare", BackColor = Color.Gray, ForeColor = Color.White };
            btnSave.Click += (s, e) => SaveGame();
            pnlGame.Controls.Add(btnSave);

            btnLoad = new Button { Location = new Point(1150, 22), Size = new Size(80, 32), Text = "Încarcă", BackColor = Color.Gray, ForeColor = Color.White };
            btnLoad.Click += (s, e) => LoadGame();
            pnlGame.Controls.Add(btnLoad);

            btnBackToMenu = new Button
            {
                // X este 0 (stânga). Y este înălțimea totală a ecranului de joc minus înălțimea butonului (40)
                Location = new Point(0, pnlGame.ClientSize.Height - 40),
                Size = new Size(110, 40),
                Text = "Meniu Principal",
                BackColor = Color.Gray,
                ForeColor = Color.White,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left // Se asigură că rămâne ancorat ferm în colț
            };
            btnBackToMenu.Click += (s, e) => ShowMainMenu();
            pnlGame.Controls.Add(btnBackToMenu);

            // Textul poveștii mărit considerabil
            lblStory = new Label
            {
                Location = new Point(40, 420),
                Size = new Size(1180, 100),
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 14),
                BackColor = Color.FromArgb(35, 35, 40),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10),
                TextAlign = ContentAlignment.TopLeft,
                AutoSize = false
            };
            pnlGame.Controls.Add(lblStory);

            // Butoanele de alegeri mai late și cu spațiere mai bună
            choiceButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                choiceButtons[i] = new Button
                {
                    Location = new Point(140, 535 + (i * 35)),
                    Size = new Size(1000, 30),
                    Font = new Font("Arial", 11f, FontStyle.Regular),
                    BackColor = Color.FromArgb(50, 50, 55),
                    ForeColor = Color.White,
                    Visible = false
                };
                pnlGame.Controls.Add(choiceButtons[i]);
            }
        }

        private void ShowMainMenu() { pnlGame.Visible = false; pnlMainMenu.Visible = true; }
        private void ShowGame() { pnlMainMenu.Visible = false; pnlGame.Visible = true; }

        private void InitializeStoryData()
        {
            storyDictionary = new Dictionary<string, StoryNode>();

            // INITIALIZARE CONTEXT
            storyDictionary.Add("START", new StoryNode
            {
                NodeId = "START",
                ImageName = "ghiseu.jpg",
                StoryText = "Ești sergent în cadrul serviciului vamal din Transnistria. Te afli în ghereta ta de la punctul de control.\nCare este motivația ta principală în acest regim autoritar?",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Să servesc Patria și Comandantul Suprem cu sfințenie.", NextNodeId = "NODE_1", IsEasyDifficulty = true },
                    new Choice { OptionText = "Să îmi hrănesc familia, indiferent de riscuri și reguli.", NextNodeId = "NODE_1", IsEasyDifficulty = false }
                }
            });

            // EVENT 1
            storyDictionary.Add("NODE_1", new StoryNode
            {
                NodeId = "NODE_1",
                ImageName = "batrana.jpg",
                StoryText = "O bătrână din Rezina vrea să treacă granița spre Rîbnița să își vadă nepotul bolnav.\nActele ei transnistrene au expirat acum două zile. Plânge în fața ghișeului.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Regula e regulă. O trimiți înapoi în Republica Moldova.", NextNodeId = "NODE_2", LoyaltyImpact = 15, EmpathyImpact = -10 },
                    new Choice { OptionText = "Îți riști slujba, îi pui ștampila și o lași să treacă.", NextNodeId = "NODE_2", EmpathyImpact = 20, LoyaltyImpact = -10 },
                    new Choice { OptionText = "Îi sugerezi subtil că o taxă de urgență de 50 de lei moldovenești rezolvă problema.", NextNodeId = "NODE_2", GreedImpact = 15, EmpathyImpact = 5 }
                }
            });

            // EVENT 2
            storyDictionary.Add("NODE_2", new StoryNode
            {
                NodeId = "NODE_2",
                ImageName = "marfa.jpg",
                StoryText = "Un comerciant cu o dubă plină de țigări nedeclarate oprește la barieră.\nÎți face cu ochiul și lasă un teanc gros de ruble pe tejghea.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Bagi banii în buzunar și îi deschizi bariera fără control.", NextNodeId = "NODE_3", GreedImpact = 25, LoyaltyImpact = -15 },
                    new Choice { OptionText = "Confiști marfa, confiști banii în interes propriu, dar îl lași să plece.", NextNodeId = "NODE_3", GreedImpact = 15, EmpathyImpact = 10, LoyaltyImpact = -5 },
                    new Choice { OptionText = "Alegi calea corectă: suni imediat la superiori și raportezi tentativa de mituire.", NextNodeId = "NODE_3", LoyaltyImpact = 25, GreedImpact = -10 }
                }
            });

            // EVENT 3
            storyDictionary.Add("NODE_3", new StoryNode
            {
                NodeId = "NODE_3",
                ImageName = "superior.jpg",
                StoryText = "Șeful tău direct vine în inspecție și îți cere să semnezi un raport fals\nprin care un cetățean nevinovat este acuzat de spionaj în favoarea Chișinăului.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Semnezi fără să pui întrebări. Ordinul se execută, nu se discută.", NextNodeId = "NODE_4", LoyaltyImpact = 20, EmpathyImpact = -20 },
                    new Choice { OptionText = "Refuzi categoric. Nu vrei să distrugi viața unui om.", NextNodeId = "NODE_4", EmpathyImpact = 20, LoyaltyImpact = -25 },
                    new Choice { OptionText = "Suguerezi că ai putea 'păstra secretul' dacă prima de performanță de luna asta îți revine ție.", NextNodeId = "NODE_4", GreedImpact = 20, LoyaltyImpact = -5 }
                }
            });

            // EVENT 4
            storyDictionary.Add("NODE_4", new StoryNode
            {
                NodeId = "NODE_4",
                ImageName = "disident.jpg",
                StoryText = "Un tânăr disident politic din Tiraspol încearcă să fugă în Moldova.\nActele lui sunt blocate în sistemul computerizat, fiind marcat ca 'Pericol Public'.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Tragi sirena și îl reții imediat pentru interogatoriu MGB.", NextNodeId = "NODE_5", LoyaltyImpact = 25, EmpathyImpact = -20 },
                    new Choice { OptionText = "Oprești monitorul pentru 30 de secunde și îl lași să fugă peste pod.", NextNodeId = "NODE_5", EmpathyImpact = 25, LoyaltyImpact = -20 },
                    new Choice { OptionText = "Îi ceri ceasul de aur de la mână și portofelul pentru a 'uita' să te uiți în computer.", NextNodeId = "NODE_5", GreedImpact = 25, EmpathyImpact = -5 }
                }
            });

            // EVENT 5
            storyDictionary.Add("NODE_5", new StoryNode
            {
                NodeId = "NODE_5",
                ImageName = "taxa.jpg",
                StoryText = "Ministerul de Finanțe de la Tiraspol impune o taxă ecologică absurdă, neoficială,\npentru toate mașinile cu numere străine.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "O aplici cu strictețe maximă tuturor, amenințând cu confiscarea plăcuțelor.", NextNodeId = "NODE_6", LoyaltyImpact = 15, EmpathyImpact = -10 },
                    new Choice { OptionText = "Ieși în întâmpinarea turiștilor și le ceri jumătate din taxă, direct cash, fără chitanță.", NextNodeId = "NODE_6", GreedImpact = 25, LoyaltyImpact = -10 },
                    new Choice { OptionText = "Ignori ordinul în cazul familiilor modeste care clar nu au de unde plăti.", NextNodeId = "NODE_6", EmpathyImpact = 15, LoyaltyImpact = -15 }
                }
            });

            // EVENT 6
            storyDictionary.Add("NODE_6", new StoryNode
            {
                NodeId = "NODE_6",
                ImageName = "prieten.jpg",
                StoryText = "Un vechi prieten din copilărie, stabilit la Chișinău, vine la graniță.\nAre în mașină pliante electorale considerate subversive de regimul transnistrean.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Îi confiști pliantele și îl trimiți de unde a venit ca să îl protejezi de arest.", NextNodeId = "NODE_7", EmpathyImpact = 15, LoyaltyImpact = -5 },
                    new Choice { OptionText = "Raportezi imediat cazul la baza centrală. Trădarea nu se iartă.", NextNodeId = "NODE_7", LoyaltyImpact = 30, EmpathyImpact = -30 },
                    new Choice { OptionText = "Îi ceri o sumă considerabilă ca să arzi pliantele chiar acolo în spatele gheretei.", NextNodeId = "NODE_7", GreedImpact = 20, EmpathyImpact = -5 }
                }
            });

            // EVENT 7
            storyDictionary.Add("NODE_7", new StoryNode
            {
                NodeId = "NODE_7",
                ImageName = "mgb.jpg",
                StoryText = "Un agent din serviciile secrete (MGB) vine îmbrăcat în civil, pretinzând că e un simplu\ncetățean cu documente dubioase, doar pentru a-ți testa vigilența.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Verifici la sânge actele, observi capcana și îl tratezi extrem de oficial.", NextNodeId = "NODE_8", LoyaltyImpact = 20 },
                    new Choice { OptionText = "Din milă, încerci să-i repari greșelile din acte pe loc ca să nu-l trimiți înapoi.", NextNodeId = "NODE_8", EmpathyImpact = 15, LoyaltyImpact = -15 },
                    new Choice { OptionText = "Îi ceri o mică atenție financiară pentru a închide ochii la nereguli. Ai picat testul lor!", NextNodeId = "NODE_8", GreedImpact = 25, LoyaltyImpact = -25 }
                }
            });

            // EVENT 8
            storyDictionary.Add("NODE_8", new StoryNode
            {
                NodeId = "NODE_8",
                ImageName = "medicamente.jpg",
                StoryText = "Un camion plin cu ajutoare medicale și insulină de la Crucea Roșie vrea să intre\nîn regiune, dar nu are autorizație tradusă în limba rusă.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Îi blochezi la graniță. Fără birocrație corectă, nimeni nu trece.", NextNodeId = "NODE_9", LoyaltyImpact = 20, EmpathyImpact = -20 },
                    new Choice { OptionText = "Îi lași să treacă urgent. Viețile oamenilor din spitale depind de asta.", NextNodeId = "NODE_9", EmpathyImpact = 25, LoyaltyImpact = -15 },
                    new Choice { OptionText = "Le ceri o 'taxă de procesare rapidă' în dolari pentru a trece cu vederea foaia lipsă.", NextNodeId = "NODE_9", GreedImpact = 25, EmpathyImpact = -5 }
                }
            });

            // EVENT 9
            storyDictionary.Add("NODE_9", new StoryNode
            {
                NodeId = "NODE_9",
                ImageName = "familie.jpg",
                StoryText = "Copilul tău s-a îmbolnăvit grav, iar tratamentul se găsește doar la prețuri\nexorbitante în farmacii private din Chișinău. Ai nevoie urgentă de resurse.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Rămâi fidel legii. Te rogi ca salariul mizer de stat să îți ajungă.", NextNodeId = "NODE_10", LoyaltyImpact = 20, GreedImpact = -15 },
                    new Choice { OptionText = "Inventezi nereguli la următoarele zece mașini pentru a strânge rapid bani din amenzi inventate.", NextNodeId = "NODE_10", GreedImpact = 30, EmpathyImpact = -20 },
                    new Choice { OptionText = "Împrumuți bani de la contrabandiștii locali în schimbul unor favoruri viitoare.", NextNodeId = "NODE_10", GreedImpact = 20, LoyaltyImpact = -20 }
                }
            });

            // EVENT 10
            storyDictionary.Add("NODE_10", new StoryNode
            {
                NodeId = "NODE_10",
                ImageName = "jurnalist.jpg",
                StoryText = "Un jurnalist străin încearcă să intre în Tiraspol cu viza de turist, dar are\nascunse în bagaj echipamente profesionale de filmat și microfoane.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Îi confiști aparatura, îi anulezi viza și îl trimiți înapoi în Moldova.", NextNodeId = "NODE_11", LoyaltyImpact = 20, EmpathyImpact = -10 },
                    new Choice { OptionText = "Îl raportezi la comandament pentru a fi arestat ca spion.", NextNodeId = "NODE_11", LoyaltyImpact = 25, EmpathyImpact = -25 },
                    new Choice { OptionText = "Îi propui să-i vinzi înapoi echipamentul confiscat pe o sumă frumușică în euro.", NextNodeId = "NODE_11", GreedImpact = 30, LoyaltyImpact = -15 }
                }
            });

            // EVENT 11
            storyDictionary.Add("NODE_11", new StoryNode
            {
                NodeId = "NODE_11",
                ImageName = "studenti.jpg",
                StoryText = "Un grup de studenți de la liceu se întorc dintr-o excursie.\nUnul dintre ei are în rucsac un steag al Moldovei, lucru interzis.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Arzi steagul în fața lor și le dai o amendă aspră ca avertisment.", NextNodeId = "NODE_12", LoyaltyImpact = 20, EmpathyImpact = -15 },
                    new Choice { OptionText = "Le spui discret să îl ascundă mai bine în haină și îi lași să plece acasă.", NextNodeId = "NODE_12", EmpathyImpact = 20, LoyaltyImpact = -15 },
                    new Choice { OptionText = "Le confiști telefoanele mobile și le zici că le primesc înapoi doar contra cost.", NextNodeId = "NODE_12", GreedImpact = 25, EmpathyImpact = -20 }
                }
            });

            // EVENT 12
            storyDictionary.Add("NODE_12", new StoryNode
            {
                NodeId = "NODE_12",
                ImageName = "alerta.jpg",
                StoryText = "Se declară stare de alertă. Ordinul de sus zice: 'Interzisă trecerea oricărui cetățean\nde etnie română astă-seară'. Un tată vrea să vină urgent la fiica lui.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Îl respingi brutal. Ordinele naționale sunt absolute.", NextNodeId = "NODE_13", LoyaltyImpact = 25, EmpathyImpact = -20 },
                    new Choice { OptionText = "Îl adăpostești în spatele gheretei și îl treci ilegal în timpul schimbului de tură.", NextNodeId = "NODE_13", EmpathyImpact = 25, LoyaltyImpact = -25 },
                    new Choice { OptionText = "Îi spui că un bilet special de trecere costă tot ce are în portofel.", NextNodeId = "NODE_13", GreedImpact = 30, EmpathyImpact = -10 }
                }
            });

            // EVENT 13
            storyDictionary.Add("NODE_13", new StoryNode
            {
                NodeId = "NODE_13",
                ImageName = "seif.jpg",
                StoryText = "Șeful vămii lasă sertarul securizat de bani din birou complet descuiat în timp\nce merge la masă. Sunt mii de dolari acolo adunați din confiscări.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Îl închizi tu și stai de pază ca să îți dovedești integritatea.", NextNodeId = "NODE_14", LoyaltyImpact = 25, GreedImpact = -15 },
                    new Choice { OptionText = "Sutonești o parte considerabilă din sumă pentru fondul tău secret de urgență.", NextNodeId = "NODE_14", GreedImpact = 35, LoyaltyImpact = -25 },
                    new Choice { OptionText = "Lași sertarul așa cum e, sperând că nu va observa nimeni neglijența.", NextNodeId = "NODE_14", EmpathyImpact = 10, LoyaltyImpact = -10 }
                }
            });

            // EVENT 14
            storyDictionary.Add("NODE_14", new StoryNode
            {
                NodeId = "NODE_14",
                ImageName = "protest.jpg",
                StoryText = "Un mic protest spontan izbucnește chiar lângă punctul tău de control.\nOamenii cer deschiderea granițelor. Superiorii îți ordonă să folosești forța.",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Tragi focuri de avertisment în aer și împrăștii mulțimea agresiv.", NextNodeId = "NODE_15", LoyaltyImpact = 30, EmpathyImpact = -25 },
                    new Choice { OptionText = "Refuzi să ridici arma împotriva oamenilor și încerci să negociezi calm.", NextNodeId = "NODE_15", EmpathyImpact = 30, LoyaltyImpact = -30 },
                    new Choice { OptionText = "Tragi un folos material din panică: le vinzi măști de protecție sau apă la suprapreț.", NextNodeId = "NODE_15", GreedImpact = 25, EmpathyImpact = -10 }
                }
            });

            // EVENT 15
            storyDictionary.Add("NODE_15", new StoryNode
            {
                NodeId = "NODE_15",
                ImageName = "final_schimb.jpg",
                StoryText = "Tensiunile s-au mai potolit. Schimbul tău de 72 de ore s-a încheiat.\nRaportul tău final de activitate este trimis automat la Comandamentul Central din Tiraspol...",
                Choices = new List<Choice>
                {
                    new Choice { OptionText = "Predă insigna și arma. Să vedem decizia regimului... (Calculează destinul)", NextNodeId = "ENDING_CHECK", GreedImpact = 0, LoyaltyImpact = 0, EmpathyImpact = 0 }
                }
            });
        }

        private void StartNewGame()
        {
            currentState = new GameState
            {
                Greed = 35,
                Empathy = 35,
                Loyalty = 35,
                Difficulty = DifficultyLevel.Hard,
                CurrentStoryNodeId = "START"
            };
            LoadStoryNode("START");
        }

        private void LoadStoryNode(string nodeId)
        {
            if (nodeId == "ENDING_CHECK")
            {
                CalculateEnding();
                return;
            }

            if (!storyDictionary.ContainsKey(nodeId)) return;

            currentState.CurrentStoryNodeId = nodeId;
            StoryNode node = storyDictionary[nodeId];

            lblStory.Text = node.StoryText;
            UpdateStatsUI();

            string imgPath = Path.Combine(imagesPath, node.ImageName ?? "");
            if (File.Exists(imgPath))
                picBackground.Image = Image.FromFile(imgPath);
            else
                picBackground.Image = null;

            foreach (var btn in choiceButtons) btn.Visible = false;

            for (int i = 0; i < node.Choices.Count; i++)
            {
                var choice = node.Choices[i];
                choiceButtons[i].Text = choice.OptionText;
                choiceButtons[i].Visible = true;
                choiceButtons[i].Click -= ChoiceButton_Click;
                choiceButtons[i].Tag = choice;
                choiceButtons[i].Click += ChoiceButton_Click;
            }
        }

        private void ChoiceButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Choice choice = btn.Tag as Choice;

            if (currentState.CurrentStoryNodeId == "START")
            {
                currentState.Difficulty = choice.IsEasyDifficulty ? DifficultyLevel.Easy : DifficultyLevel.Hard;
                if (currentState.Difficulty == DifficultyLevel.Easy)
                {
                    currentState.Loyalty = 60; // Bonus de inceput pe loialitate
                }
            }
            else
            {
                currentState.Greed += choice.GreedImpact;
                currentState.Empathy += choice.EmpathyImpact;
                currentState.Loyalty += choice.LoyaltyImpact;

                // Limitare intre 0 si 100
                currentState.Greed = Math.Max(0, Math.Min(100, currentState.Greed));
                currentState.Empathy = Math.Max(0, Math.Min(100, currentState.Empathy));
                currentState.Loyalty = Math.Max(0, Math.Min(100, currentState.Loyalty));
            }

            LoadStoryNode(choice.NextNodeId);
        }

        private void UpdateStatsUI()
        {
            // Deoarece lățimea bării fundal este acum 200px, valoarea statusului (0-100) trebuie înmulțită cu 2
            fgGreed.Width = currentState.Greed * 2;
            fgEmpathy.Width = currentState.Empathy * 2;
            fgLoyalty.Width = currentState.Loyalty * 2;
        }

        private void CalculateEnding()
        {
            foreach (var btn in choiceButtons) btn.Visible = false;
            UpdateStatsUI();

            string endingPath = Path.Combine(imagesPath, "verdict.jpg");
            if (File.Exists(endingPath)) picBackground.Image = Image.FromFile(endingPath);

            int l = currentState.Loyalty;
            int e = currentState.Empathy;
            int g = currentState.Greed;

            string finalStory = "";

            if (l >= e && l >= g)
            {
                finalStory = "ENDING: LOIALITATE\nAcțiunile tale impecabile au atras atenția structurilor superioare din Tiraspol. Comandantul tău îți recunoaște oficial loialitatea oarbă față de regim. Primești o invitație oficială de a te alătura Partidului Autoritar și ești promovat în structura centrală MGB. Viitorul familiei tale este complet asigurat, chiar dacă ai lăsat în urmă destine distruse la graniță. Glorie Republicii!";
            }
            else if (e >= l && e >= g)
            {
                finalStory = "ENDING: EMPATIE\nBunătatea ta a fost slăbiciunea ta în ochii regimului. Actele de caritate, trecerile trecute cu vederea și refuzul de a folosi forța au lăsat urme în log-urile computerului. Într-o dimineață friguroasă, o mașină neagră oprește în fața gheretei tale. Ești pus la pământ și arestat sub acuzația de trădare și complicitate. Îți vei petrece următorii ani într-o închisoare de maximă securitate din regiune.";
            }
            else
            {
                finalStory = "ENDING: LĂCOMIE\nNu ți-a păsat de politică, ci doar de bani. Rublele, dolarii și euro strânși ilegal din mite, șantaj și contrabandă s-au adunat într-o sumă considerabilă. Realizând că regimul devine tot mai instabil și suspect, îți iei familia în toiul nopții și mituiești chiar tu ultima patrulă de control pentru a trece definitiv în Republica Moldova. De acolo, fugiți spre vestul Europei pentru a începe o viață nouă și liberă, departe de dictatură.";
            }

            // Aici mărim caseta de text pentru a încăpea toată povestea
            lblStory.Height = 250;

            lblStory.Text = "VERDICT FINAL!\n" + finalStory;
        }
        private void SaveGame()
        {
            try
            {
                string json = JsonSerializer.Serialize(currentState);
                File.WriteAllText(SaveFilePath, json);
                MessageBox.Show("Documentele de tură au fost salvate!", "Salvare", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("Eroare la salvare: " + ex.Message); }
        }

        private void LoadGame()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    currentState = JsonSerializer.Deserialize<GameState>(json);
                    LoadStoryNode(currentState.CurrentStoryNodeId);
                    MessageBox.Show("Dosarul de tură a fost încărcat!", "Încarcare", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else MessageBox.Show("Nu există nicio fișă salvată!", "Eroare", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex) { MessageBox.Show("Eroare la încărcare: " + ex.Message); }
        }
    }

    public enum DifficultyLevel { Easy, Hard }

    public class GameState
    {
        public int Greed { get; set; }
        public int Empathy { get; set; }
        public int Loyalty { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public string CurrentStoryNodeId { get; set; }
    }

    public class Choice
    {
        public string OptionText { get; set; }
        public string NextNodeId { get; set; }
        public int GreedImpact { get; set; }
        public int EmpathyImpact { get; set; }
        public int LoyaltyImpact { get; set; }
        public bool IsEasyDifficulty { get; set; }
    }

    public class StoryNode
    {
        public string NodeId { get; set; }
        public string ImageName { get; set; }
        public string StoryText { get; set; }
        public List<Choice> Choices { get; set; }
    }
}