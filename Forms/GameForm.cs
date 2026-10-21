using System.Diagnostics;
using TuxSpace.Core;
using TuxSpace.Entities;
using Timer = System.Windows.Forms.Timer;

namespace TuxSpace.Forms;

public class GameForm : Form
{
    private enum GameState
    {
        Playing,
        Paused,
        GameOver,
        Victory
    }

    // =========================================================
    // SISTEMA PRINCIPAL
    // =========================================================

    private readonly Timer gameTimer;
    private readonly Stopwatch frameClock;

    private readonly GameBuffer fundo;


    // =========================================================
    // ANIMAÇÕES
    // =========================================================

    private readonly Animation enemyAnimation;
    private readonly Animation explosionAnimation;
    private readonly Image lootImage;
    private readonly Cursor gameCursor;
    private readonly Panel pauseMenu;


    // =========================================================
    // JOGADOR
    // =========================================================

    private readonly Player player;

    private float playerInvulnerability = 0f;

    private GameState gameState = GameState.Playing;
    private GameState stateBeforePause = GameState.Playing;


    // =========================================================
    // LEVEL UP
    // =========================================================

    private bool choosingUpgrade = false;

    private float maxShootCooldown = 0.10f;


    // =========================================================
    // INIMIGOS
    // =========================================================

    private readonly List<Enemy> enemies = new();

    private static readonly int[] EnemyHealthByWave = { 6, 8, 15 };
    private int MaxWaves => EnemyHealthByWave.Length;
    private const int EnemiesPerWave = 5;
    private int currentWave = 1;
    private int spawnedEnemiesThisWave;
    private int score;
    private float waveTransitionTimer;
    private float waveAnnouncementTimer;
    private bool finalWaveCleared;


    // =========================================================
    // EXPLOSÕES
    // =========================================================

    private readonly List<Explosion> explosions = new();

    private readonly List<LootPickup> lootPickups = new();


    // =========================================================
    // SPAWN
    // =========================================================

    private float spawnTimer = 0f;

    private readonly Random random = new();


    // =========================================================
    // PROJÉTEIS
    // =========================================================

    private readonly List<Projectile> projectiles = new();

    private readonly List<Projectile> enemyProjectiles = new();


    // =========================================================
    // TIRO
    // =========================================================

    private float shootCooldown = 0f;

    private bool leftMousePressed = false;


    // =========================================================
    // CONSTRUTOR
    // =========================================================

    public GameForm(
        string naveSpritePath = "Assets/Player/tux.png",
        int pontosVida = 5,
        int pontosVelocidade = 5,
        int pontosDano = 5)
    {
        // -----------------------------------------------------
        // CONFIGURAÇÃO DA JANELA
        // -----------------------------------------------------

        WindowState = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.None;
        KeyPreview = true;
        lootImage = Image.FromFile("Assets/Loot/item_heal.png");
        gameCursor = CustomCursorFactory.LoadFromPng("Assets/cursor.png");
        FormClosed += (_, _) =>
        {
            lootImage.Dispose();
            gameCursor.Dispose();
        };


        // -----------------------------------------------------
        // FUNDO DO JOGO
        // -----------------------------------------------------

        fundo = new GameBuffer();

        fundo.Dock = DockStyle.Fill;

        fundo.BackgroundImage =
            Image.FromFile(
                "Assets/xpFundo.jpg"
            );

        fundo.BackgroundImageLayout =
            ImageLayout.Stretch;

        Controls.Add(fundo);
        fundo.Cursor = gameCursor;
        Cursor = gameCursor;

        pauseMenu = CreatePauseMenu();
        fundo.Controls.Add(pauseMenu);
        CenterPauseMenu();
        fundo.Resize += (_, _) => CenterPauseMenu();


        // -----------------------------------------------------
        // ANIMAÇÃO DOS INIMIGOS
        // -----------------------------------------------------

        enemyAnimation = new Animation();

        enemyAnimation.FrameTime = 0.8f;

        enemyAnimation.Frames.Add(
            Image.FromFile(
                "Assets/Enemy/enemy1.png"
            )
        );

        enemyAnimation.Frames.Add(
            Image.FromFile(
                "Assets/Enemy/enemy2.png"
            )
        );

        enemyAnimation.Frames.Add(
            Image.FromFile(
                "Assets/Enemy/enemy3.png"
            )
        );

        enemyAnimation.Frames.Add(
            Image.FromFile(
                "Assets/Enemy/enemy4.png"
            )
        );


        // -----------------------------------------------------
        // ANIMAÇÃO DAS EXPLOSÕES
        // -----------------------------------------------------

        explosionAnimation = new Animation();

        explosionAnimation.Loop = false;

        explosionAnimation.Frames.Add(
            Image.FromFile(
                "Assets/Explosion/explosion1.png"
            )
        );

        explosionAnimation.Frames.Add(
            Image.FromFile(
                "Assets/Explosion/explosion2.png"
            )
        );

        explosionAnimation.Frames.Add(
            Image.FromFile(
                "Assets/Explosion/explosion3.png"
            )
        );

        explosionAnimation.Frames.Add(
            Image.FromFile(
                "Assets/Explosion/explosion4.png"
            )
        );

        // A explosão completa dura exatamente 1 segundo.
        explosionAnimation.SetDuration(1.0f);


        // -----------------------------------------------------
        // JOGADOR
        // -----------------------------------------------------

        player = new Player();

        // Atributos definidos pelo jogador na Tela de Entrada.
        // As fórmulas abaixo mantêm o balanceamento original do
        // jogo quando os pontos são distribuídos igualmente (5/5/5).
        player.MaxHP = pontosVida;
        player.HP = pontosVida;
        player.Speed = 200f + (pontosVelocidade * 30f);
        player.Damage = 5 + pontosDano;

        player.Sprite =
            Image.FromFile(
                naveSpritePath
            );

        player.X = 300;
        player.Y = 300;

        player.Width = 128;
        player.Height = 128;


        // -----------------------------------------------------
        // RELÓGIO DO JOGO
        // -----------------------------------------------------

        frameClock = new Stopwatch();

        frameClock.Start();


        // -----------------------------------------------------
        // GAME TIMER
        // -----------------------------------------------------

        gameTimer = new Timer();

        gameTimer.Interval = 16;

        gameTimer.Tick += GameLoop;

        gameTimer.Start();


        // -----------------------------------------------------
        // EVENTOS
        // -----------------------------------------------------

        fundo.Paint += DrawGame;

        KeyDown += OnGameKeyDown;
        KeyUp += OnGameKeyUp;

        fundo.MouseDown += OnMouseDown;
        fundo.MouseUp += OnMouseUp;
    }

    private Panel CreatePauseMenu()
    {
        Panel menu = new DoubleBufferedPanel
        {
            Size = new Size(400, 300),
            BackColor = Color.FromArgb(0, 0, 128),
            BorderStyle = BorderStyle.Fixed3D,
            Visible = false,
            Cursor = Cursors.Default
        };

        Label title = new()
        {
            Text = "JOGO PAUSADO",
            ForeColor = Color.Yellow,
            Font = new Font("Microsoft Sans Serif", 22, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(0, 35),
            Size = new Size(menu.Width, 45),
            BackColor = menu.BackColor
        };
        menu.Controls.Add(title);

        Button continueButton = CreatePauseMenuButton("Continuar", 120);
        continueButton.Click += (_, _) => ResumeGame();
        menu.Controls.Add(continueButton);

        Button restartButton = CreatePauseMenuButton("Reiniciar", 180);
        restartButton.Click += (_, _) => Application.Restart();
        menu.Controls.Add(restartButton);

        Button exitButton = CreatePauseMenuButton("Sair para Windows", 240);
        exitButton.Click += (_, _) => Application.Exit();
        menu.Controls.Add(exitButton);

        return menu;
    }

    private static Button CreatePauseMenuButton(string text, int top)
    {
        Button button = new()
        {
            Text = text,
            Size = new Size(200, 40),
            Location = new Point(100, top),
            FlatStyle = FlatStyle.Standard,
            ForeColor = Color.Navy,
            BackColor = Color.Silver,
            UseVisualStyleBackColor = false,
            Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold),
            Cursor = Cursors.Default
        };
        return button;
    }

    private void CenterPauseMenu()
    {
        if (pauseMenu is null)
            return;

        pauseMenu.Location = new Point(
            Math.Max(0, (fundo.ClientSize.Width - pauseMenu.Width) / 2),
            Math.Max(0, (fundo.ClientSize.Height - pauseMenu.Height) / 2)
        );
    }

    private void PauseGame()
    {
        stateBeforePause = gameState;
        gameState = GameState.Paused;
        leftMousePressed = false;
        gameTimer.Stop();
        frameClock.Stop();
        UpdatePauseMenuVisibility();
    }

    private void ResumeGame()
    {
        gameState = stateBeforePause;
        UpdatePauseMenuVisibility();
        frameClock.Restart();
        gameTimer.Start();
        fundo.Focus();
    }

    private void UpdatePauseMenuVisibility()
    {
        bool isPaused = gameState == GameState.Paused;
        pauseMenu.Visible = isPaused;
        if (isPaused)
            pauseMenu.BringToFront();

        Cursor activeCursor = gameState == GameState.Playing ? gameCursor : Cursors.Default;
        fundo.Cursor = activeCursor;
        Cursor = activeCursor;
        fundo.Invalidate();
    }


    // =========================================================
    // CRIAÇÃO DE INIMIGO
    // =========================================================

    private void SpawnEnemy()
    {
        Enemy enemy = new();

        enemy.Animation =
            enemyAnimation.Clone();

        enemy.Width = 128;
        enemy.Height = 128;

        enemy.Speed = 220f + (currentWave - 1) * 20f;

        enemy.X = fundo.Width + 100;

        enemy.Y =
            random.Next(
                50,
                Math.Max(51, fundo.Height - 100)
            );

        enemy.HP = EnemyHealthByWave[currentWave - 1] * 20;

        enemy.VerticalSpeed = random.Next(120, 261) + (currentWave - 1) * 20f;
        enemy.VerticalDirection = random.Next(0, 2) == 0 ? -1 : 1;

        enemies.Add(enemy);
        spawnedEnemiesThisWave++;
    }


    // =========================================================
    // EXPLOSÃO
    // =========================================================

    private void SpawnExplosion(float x, float y)
    {
        Explosion explosion = new();

        explosion.X = x;
        explosion.Y = y;

        explosion.Width = 120;
        explosion.Height = 120;

        explosion.Animation =
            explosionAnimation.Clone();

        explosions.Add(explosion);
    }

    // =========================================================
    // LEVEL UP
    // =========================================================

    private void HandleUpgradeInput()
    {
        // -----------------------------------------------------
        // 1 - MAIS VIDA
        // -----------------------------------------------------

        if (InputManager.KeyDown(Keys.D1))
        {
            player.MaxHP++;
            player.HP++;

            choosingUpgrade = false;
        }


        // -----------------------------------------------------
        // 2 - MAIS DANO
        // -----------------------------------------------------

        if (InputManager.KeyDown(Keys.D2))
        {
            player.Damage += 2;

            choosingUpgrade = false;
        }


        // -----------------------------------------------------
        // 3 - MAIS RAM
        // -----------------------------------------------------

        if (InputManager.KeyDown(Keys.D3))
        {
            player.MaxRAM += 25;

            choosingUpgrade = false;
        }


        // -----------------------------------------------------
        // 4 - TIRO MAIS RÁPIDO
        // -----------------------------------------------------

        if (InputManager.KeyDown(Keys.D4))
        {
            maxShootCooldown =
                Math.Max(
                    0.02f,
                    maxShootCooldown - 0.02f
                );

            choosingUpgrade = false;
        }
    }

    private void UpdateLootPickups(float delta)
    {
        for (int i = lootPickups.Count - 1; i >= 0; i--)
        {
            LootPickup loot = lootPickups[i];
            loot.Update(delta);

            if (loot.Bounds.IntersectsWith(player.Bounds))
            {
                player.Heal(1);
                score += 50;
                lootPickups.RemoveAt(i);
            }
            else if (loot.IsExpired)
            {
                lootPickups.RemoveAt(i);
            }
        }
    }


    // =========================================================
    // TECLA PRESSIONADA
    // =========================================================

    private void OnGameKeyDown(
        object? sender,
        KeyEventArgs e)
    {
        bool wasKeyDown = InputManager.KeyDown(e.KeyCode);
        InputManager.KeyPressed(
            e.KeyCode
        );

        if (wasKeyDown)
            return;

        if (e.KeyCode == Keys.E)
            TryFireSpecialShot();

        if (e.KeyCode == Keys.Escape)
        {
            if (gameState == GameState.Playing)
            {
                PauseGame();
            }
            else if (gameState == GameState.Paused)
            {
                ResumeGame();
            }
            else if (gameState == GameState.GameOver || gameState == GameState.Victory)
            {
                Close();
            }

            return;
        }

        if (gameState == GameState.Paused)
        {
            if (e.KeyCode == Keys.P)
                ResumeGame();
            else if (e.KeyCode == Keys.R)
                Application.Restart();
            else if (e.KeyCode == Keys.Q)
                Application.Exit();
        }
        else if (
            (gameState == GameState.GameOver || gameState == GameState.Victory || player.HP <= 0)
            && e.KeyCode == Keys.R
        )
        {
            Application.Restart();
        }

        fundo.Invalidate();
    }


    // =========================================================
    // TECLA SOLTA
    // =========================================================

    private void OnGameKeyUp(
        object? sender,
        KeyEventArgs e)
    {
        InputManager.KeyReleased(
            e.KeyCode
        );
    }


    // =========================================================
    // MOUSE PRESSIONADO
    // =========================================================

    private void OnMouseDown(
        object? sender,
        MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            leftMousePressed = true;
        }
        else if (
            e.Button == MouseButtons.Right
        )
        {
            TryFireSpecialShot();
        }
    }

    private void TryFireSpecialShot()
    {
        if (
            gameState != GameState.Playing
            || choosingUpgrade
            || !player.TrySpendAllRAM(100)
        )
        {
            return;
        }

        projectiles.Add(
            ProjectileFactory.CreatePlayerSpecialProjectile(player)
        );
        fundo.Invalidate();
    }


    // =========================================================
    // MOUSE SOLTO
    // =========================================================

    private void OnMouseUp(
        object? sender,
        MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            leftMousePressed = false;
        }
    }


    // =========================================================
    // GAME LOOP
    // =========================================================

    private void GameLoop(
        object? sender,
        EventArgs e)
    {
        // -----------------------------------------------------
        // DELTA TIME
        // -----------------------------------------------------

        float delta =
            (float)frameClock.Elapsed.TotalSeconds;

        frameClock.Restart();


        // Evita valores absurdos caso o jogo congele
        // por alguns segundos.

        delta = Math.Min(delta, 0.1f);

        if (player.HP <= 0)
        {
            HandleGameOver();
            UpdateExplosions(delta);
            fundo.Invalidate();
            return;
        }

        if (gameState == GameState.Paused || gameState == GameState.Victory)
        {
            fundo.Invalidate();
            return;
        }

        // -----------------------------------------------------
        // MOVIMENTO DO JOGADOR
        // -----------------------------------------------------

        float horizontalInput =
            (InputManager.KeyDown(Keys.D) ? 1f : 0f)
            - (InputManager.KeyDown(Keys.A) ? 1f : 0f);
        float verticalInput =
            (InputManager.KeyDown(Keys.S) ? 1f : 0f)
            - (InputManager.KeyDown(Keys.W) ? 1f : 0f);
        player.UpdateMovement(horizontalInput, verticalInput, delta);


        // -----------------------------------------------------
        // LEVEL UP
        // -----------------------------------------------------

        if (
            gameState == GameState.Playing
            &&
            player.XP >= player.XPToNextLevel
            && !choosingUpgrade
        )
        {
            player.LevelUp();

            choosingUpgrade = true;
        }


        // -----------------------------------------------------
        // TELA DE UPGRADE
        // -----------------------------------------------------

        if (choosingUpgrade)
        {
            HandleUpgradeInput();

            fundo.Invalidate();

            return;
        }


        // -----------------------------------------------------
        // ANIMAÇÃO DO JOGADOR
        // -----------------------------------------------------

        if (player.Animation != null)
        {
            player.Animation.Update(delta);
        }


        // -----------------------------------------------------
        // ANIMAÇÃO DOS INIMIGOS
        // -----------------------------------------------------

        foreach (var enemy in enemies)
        {
            if (enemy.Animation != null)
            {
                enemy.Animation.Update(delta);
            }
        }


        // -----------------------------------------------------
        // LIMITES DO JOGADOR
        // -----------------------------------------------------

        player.ClampToBounds(fundo.Width, fundo.Height);


        // -----------------------------------------------------
        // INVULNERABILIDADE
        // -----------------------------------------------------

        if (playerInvulnerability > 0)
        {
            playerInvulnerability -= delta;
        }


        // -----------------------------------------------------
        // SPAWN E PROGRESSÃO DAS ONDAS
        // -----------------------------------------------------

        if (gameState == GameState.Playing)
        {
            if (!finalWaveCleared && waveTransitionTimer <= 0)
            {
                spawnTimer -= delta;

                if (
                    spawnedEnemiesThisWave < EnemiesPerWave
                    && spawnTimer <= 0
                    && enemies.Count < 6
                )
                {
                    SpawnEnemy();

                    spawnTimer =
                        (float)(
                            random.NextDouble() * 1.5
                            + 0.5
                        );
                }
            }

            UpdateEnemies(delta);
            UpdateEnemyProjectiles(delta);
            UpdatePlayerShooting(delta);
            UpdatePlayerProjectiles(delta);

        }

        UpdateLootPickups(delta);
        UpdateWaveProgress(delta);
        waveAnnouncementTimer = Math.Max(0, waveAnnouncementTimer - delta);

        // -----------------------------------------------------
        // EXPLOSÕES
        // -----------------------------------------------------

        UpdateExplosions(delta);


        // -----------------------------------------------------
        // DESENHA NOVAMENTE
        // -----------------------------------------------------

        fundo.Invalidate();
    }


    // =========================================================
    // GAME OVER
    // =========================================================

    private void HandleGameOver()
    {
        if (gameState == GameState.GameOver)
            return;

        gameState = GameState.GameOver;
        UpdatePauseMenuVisibility();
    }

    private void UpdateWaveProgress(float delta)
    {
        if (
            !finalWaveCleared
            && waveTransitionTimer <= 0
            && spawnedEnemiesThisWave >= EnemiesPerWave
            && enemies.Count == 0
        )
        {
            enemyProjectiles.Clear();
            projectiles.Clear();
            waveTransitionTimer = currentWave >= MaxWaves ? 10f : 4f;
            waveAnnouncementTimer = 3f;
            finalWaveCleared = currentWave >= MaxWaves;
        }

        if (waveTransitionTimer <= 0)
            return;

        waveTransitionTimer -= delta;

        if (finalWaveCleared)
        {
            if (lootPickups.Count == 0 || waveTransitionTimer <= 0)
            {
                gameState = GameState.Victory;
                UpdatePauseMenuVisibility();
            }
        }
        else if (waveTransitionTimer <= 0)
        {
            currentWave++;
            spawnedEnemiesThisWave = 0;
            spawnTimer = 1f;
            waveAnnouncementTimer = 2f;
        }
    }


    // =========================================================
    // ATUALIZA INIMIGOS
    // =========================================================

    private void UpdateEnemies(float delta)
    {
        for (
            int i = enemies.Count - 1;
            i >= 0;
            i--
        )
        {
            Enemy enemy = enemies[i];

            enemy.Update(
                delta,
                fundo.Height
            );

            if (enemy.X + enemy.Width < 0)
            {
                enemies.RemoveAt(i);
                continue;
            }


            // -------------------------------------------------
            // COLISÃO INIMIGO X JOGADOR
            // -------------------------------------------------

            if (
                enemy.Bounds.IntersectsWith(
                    player.Bounds
                )
            )
            {
                if (playerInvulnerability <= 0)
                {
                    player.TakeDamage(1);
                    SpawnExplosion(player.X, player.Y);
                    playerInvulnerability = 1f;
                }
            }


            // -------------------------------------------------
            // COOLDOWN DO INIMIGO
            // -------------------------------------------------

            enemy.ShootCooldown -= delta;


            // -------------------------------------------------
            // TIRO DO INIMIGO
            // -------------------------------------------------

            if (enemy.ShootCooldown <= 0)
            {
                enemyProjectiles.Add(
                    ProjectileFactory.CreateEnemyProjectile(enemy));


                enemy.ShootCooldown =
                    1f
                    + (float)random.NextDouble();
            }
        }
    }


    // =========================================================
    // ATUALIZA TIROS DOS INIMIGOS
    // =========================================================

    private void UpdateEnemyProjectiles(
        float delta
    )
    {
        for (
            int i = enemyProjectiles.Count - 1;
            i >= 0;
            i--
        )
        {
            Projectile projectile =
                enemyProjectiles[i];

            projectile.Update(delta);


            // -------------------------------------------------
            // COLISÃO COM JOGADOR
            // -------------------------------------------------

            if (
                projectile.Bounds.IntersectsWith(
                    player.Bounds
                )
            )
            {
                if (playerInvulnerability <= 0)
                {
                    player.TakeDamage(1);
                    SpawnExplosion(projectile.X, projectile.Y);
                    playerInvulnerability = 1f;
                }

                enemyProjectiles.RemoveAt(i);

                continue;
            }


            // -------------------------------------------------
            // SAIU DA TELA
            // -------------------------------------------------

            if (projectile.X < -50)
            {
                enemyProjectiles.RemoveAt(i);
            }
        }
    }


    // =========================================================
    // TIROS DO JOGADOR
    // =========================================================

    private void UpdatePlayerShooting(
        float delta
    )
    {
        if (shootCooldown > 0)
        {
            shootCooldown -= delta;
        }


        if (
            (
                InputManager.KeyDown(Keys.Space)
                || leftMousePressed
            )
            && shootCooldown <= 0
        )
        {
            projectiles.Add(
                ProjectileFactory.CreatePlayerProjectile(player));


            shootCooldown =
                maxShootCooldown;
        }
    }


    // =========================================================
    // PROJÉTEIS DO JOGADOR
    // =========================================================

    private void UpdatePlayerProjectiles(
        float delta
    )
    {
        for (
            int i = projectiles.Count - 1;
            i >= 0;
            i--
        )
        {
            Projectile projectile =
                projectiles[i];

            projectile.Update(delta);


            // -------------------------------------------------
            // SAIU DA TELA
            // -------------------------------------------------

            if (projectile.X > fundo.Width)
            {
                projectiles.RemoveAt(i);

                continue;
            }


            // -------------------------------------------------
            // COLISÃO COM INIMIGOS
            // -------------------------------------------------

            bool projectileRemoved = false;


            for (
                int j = enemies.Count - 1;
                j >= 0;
                j--
            )
            {
                Enemy enemy = enemies[j];


                if (
                    projectile.Bounds.IntersectsWith(
                        enemy.Bounds
                    )
                )
                {
                    // -----------------------------------------
                    // DANO
                    // -----------------------------------------

                    enemy.TakeDamage(projectile.Damage);
                    if (!projectile.IsSpecial)
                        player.RestoreRAM(5);


                    // -----------------------------------------
                    // REMOVE PROJÉTIL
                    // -----------------------------------------

                    projectiles.RemoveAt(i);

                    projectileRemoved = true;


                    // -----------------------------------------
                    // INIMIGO MORREU
                    // -----------------------------------------

                    if (enemy.HP <= 0)
                    {
                        player.GainExperience(15);
                        score += 100 * currentWave;

                        SpawnExplosion(
                            enemy.X,
                            enemy.Y
                        );

                        lootPickups.Add(
                            new LootPickup(
                                enemy.X + (enemy.Width - LootPickup.PickupSize) / 2f,
                                enemy.Y + (enemy.Height - LootPickup.PickupSize) / 2f,
                                lootImage
                            )
                        );

                        enemies.RemoveAt(j);
                    }

                    break;
                }
            }


            // -------------------------------------------------
            // MUITO IMPORTANTE:
            // se o projétil foi removido, não acessamos
            // projectiles[i] novamente.
            // -------------------------------------------------

            if (projectileRemoved)
            {
                continue;
            }
        }
    }


    // =========================================================
    // EXPLOSÕES
    // =========================================================

    private void UpdateExplosions(
        float delta
    )
    {
        for (
            int i = explosions.Count - 1;
            i >= 0;
            i--
        )
        {
            Explosion explosion =
                explosions[i];

            explosion.Update(delta);

            if (explosion.Finished)
            {
                explosions.RemoveAt(i);
            }
        }
    }


    // =========================================================
    // DESENHO DO JOGO
    // =========================================================

    private void DrawGame(
        object? sender,
        PaintEventArgs e
    )
    {
        Graphics graphics = e.Graphics;


        // -----------------------------------------------------
        // JOGADOR
        // -----------------------------------------------------

        player.Draw(graphics);


        // -----------------------------------------------------
        // HUD
        // -----------------------------------------------------

        using Font hudFont =
            new(
                "Arial",
                14,
                FontStyle.Bold
            );


        graphics.DrawString(
            $"Vida: {player.HP}/{player.MaxHP}",
            hudFont,
            Brushes.Lime,
            20,
            20
        );


        graphics.DrawString(
            $"RAM: {player.RAM}/{player.MaxRAM}",
            hudFont,
            Brushes.Cyan,
            220,
            20
        );


        graphics.DrawString(
            $"Level: {player.Level}",
            hudFont,
            Brushes.Gold,
            20,
            45
        );


        graphics.DrawString(
            $"XP: {player.XP}/{player.XPToNextLevel}",
            hudFont,
            Brushes.White,
            220,
            45
        );


        // -----------------------------------------------------
        // FASE
        // -----------------------------------------------------

        graphics.DrawString(
            $"ONDA: {currentWave}/{MaxWaves}   SCORE: {score}",
            hudFont,
            Brushes.Orange,
            450,
            20
        );
        graphics.DrawString(
            "TIRO ESPECIAL: E / BOTAO DIREITO (100 RAM)",
            hudFont,
            player.RAM >= 100 ? Brushes.Cyan : Brushes.Gray,
            450,
            45
        );


        // -----------------------------------------------------
        // PROJÉTEIS DO JOGADOR
        // -----------------------------------------------------

        foreach (var projectile in projectiles)
        {
            using SolidBrush projectileBrush = new(
                projectile.IsSpecial ? Color.Cyan : Color.Yellow
            );
            graphics.FillRectangle(
                projectileBrush,
                projectile.X,
                projectile.Y,
                projectile.Width,
                projectile.Height
            );
        }


        // -----------------------------------------------------
        // PROJÉTEIS DOS INIMIGOS
        // -----------------------------------------------------

        foreach (
            var projectile
            in enemyProjectiles
        )
        {
            graphics.FillRectangle(
                Brushes.Red,
                projectile.X,
                projectile.Y,
                projectile.Width,
                projectile.Height
            );
        }


        // -----------------------------------------------------
        // INIMIGOS
        // -----------------------------------------------------

        foreach (var enemy in enemies)
        {
            enemy.Draw(graphics);
        }

        foreach (var loot in lootPickups)
        {
            loot.Draw(graphics);
        }


        // -----------------------------------------------------
        // EXPLOSÕES
        // -----------------------------------------------------

        foreach (var explosion in explosions)
        {
            explosion.Draw(graphics);
        }


        // -----------------------------------------------------
        // GAME OVER
        // -----------------------------------------------------

        // -----------------------------------------------------
        // LEVEL UP
        // -----------------------------------------------------

        if (choosingUpgrade && gameState == GameState.Playing)
        {
            graphics.FillRectangle(
                Brushes.Black,
                0,
                0,
                fundo.Width,
                fundo.Height
            );


            using Font titleFont =
                new(
                    "Arial",
                    30,
                    FontStyle.Bold
                );


            graphics.DrawString(
                "LEVEL UP!",
                titleFont,
                Brushes.Gold,
                fundo.Width / 2 - 80,
                150
            );


            graphics.DrawString(
                "1 - +1 VIDA",
                SystemFonts.DefaultFont,
                Brushes.White,
                fundo.Width / 2 - 80,
                250
            );


            graphics.DrawString(
                "2 - +2 DANO",
                SystemFonts.DefaultFont,
                Brushes.White,
                fundo.Width / 2 - 80,
                280
            );


            graphics.DrawString(
                "3 - +25 RAM",
                SystemFonts.DefaultFont,
                Brushes.White,
                fundo.Width / 2 - 80,
                310
            );


            graphics.DrawString(
                "4 - TIRO MAIS RAPIDO",
                SystemFonts.DefaultFont,
                Brushes.White,
                fundo.Width / 2 - 80,
                340
            );


            return;
        }


        switch (gameState)
        {
            case GameState.GameOver:
                DrawStateOverlay(graphics, "GAME OVER", "R reiniciar | ESC sair", Color.Red);
                break;
            case GameState.Victory:
                DrawStateOverlay(graphics, "VITÓRIA!", $"Pontuação: {score} | R jogar novamente | ESC sair", Color.Gold);
                break;
        }

        if (waveAnnouncementTimer > 0 && gameState == GameState.Playing)
        {
            string message = finalWaveCleared
                ? "ULTIMA ONDA CONCLUIDA - COLETE OS RECURSOS"
                : spawnedEnemiesThisWave >= EnemiesPerWave
                    ? $"ONDA {currentWave} CONCLUIDA - PROXIMA EM {Math.Max(0, (int)Math.Ceiling(waveTransitionTimer))}s"
                    : $"ONDA {currentWave} - PREPARE-SE";
            using Font announcementFont = new("Consolas", 18, FontStyle.Bold);
            SizeF messageSize = graphics.MeasureString(message, announcementFont);
            graphics.DrawString(
                message,
                announcementFont,
                Brushes.Gold,
                (fundo.Width - messageSize.Width) / 2,
                85
            );
        }
    }

    private void DrawStateOverlay(Graphics graphics, string title, string detail, Color titleColor)
    {
        using SolidBrush overlayBrush = new(Color.FromArgb(190, 0, 0, 0));
        graphics.FillRectangle(overlayBrush, 0, 0, fundo.Width, fundo.Height);

        using Font titleFont = new("Arial", 36, FontStyle.Bold);
        using Font detailFont = new("Arial", 16, FontStyle.Bold);
        using SolidBrush titleBrush = new(titleColor);

        SizeF titleSize = graphics.MeasureString(title, titleFont);
        SizeF detailSize = graphics.MeasureString(detail, detailFont);

        graphics.DrawString(
            title,
            titleFont,
            titleBrush,
            (fundo.Width - titleSize.Width) / 2,
            fundo.Height / 2 - 55
        );
        graphics.DrawString(
            detail,
            detailFont,
            Brushes.White,
            (fundo.Width - detailSize.Width) / 2,
            fundo.Height / 2 + 15
        );
    }

    private sealed class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint,
                true
            );
        }
    }
}