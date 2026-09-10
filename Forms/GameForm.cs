using System.Diagnostics;
using TuxSpace.Core;
using TuxSpace.Entities;
using Timer = System.Windows.Forms.Timer;

namespace TuxSpace.Forms;

public class GameForm : Form
{
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


    // =========================================================
    // JOGADOR
    // =========================================================

    private readonly Player player;

    private float playerInvulnerability = 0f;


    // =========================================================
    // LEVEL UP
    // =========================================================

    private bool choosingUpgrade = false;

    private float maxShootCooldown = 0.10f;


    // =========================================================
    // INIMIGOS
    // =========================================================

    private readonly List<Enemy> enemies = new();

    private int enemiesKilled = 0;


    // =========================================================
    // EXPLOSÕES
    // =========================================================

    private readonly List<Explosion> explosions = new();


    // =========================================================
    // FASES
    // =========================================================

    private int currentPhase = 1;

    private bool phase1Completed = false;

    private bool waitingPhase2 = false;


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

        enemy.Speed = 220f;

        enemy.X = fundo.Width + 100;

        enemy.Y =
            random.Next(
                50,
                Math.Max(51, fundo.Height - 100)
            );

        enemy.HP = 20;

        enemy.TargetOffset =
            random.Next(-350, 350);

        enemy.WaveOffset =
            (float)(
                random.NextDouble()
                * Math.PI
                * 2
            );

        enemy.WaveSpeed =
            2f +
            (float)random.NextDouble() * 3f;

        enemy.FollowStrength =
            0.3f +
            (float)random.NextDouble() * 1.2f;

        enemy.AttackDistance =
            random.Next(150, 450);

        enemies.Add(enemy);
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


    // =========================================================
    // TECLA PRESSIONADA
    // =========================================================

    private void OnGameKeyDown(
        object? sender,
        KeyEventArgs e)
    {
        InputManager.KeyPressed(
            e.KeyCode
        );

        // Precisa ser checado aqui, e não dentro do GameLoop,
        // porque o gameTimer é parado no Game Over — então o
        // GameLoop (e HandleGameOver) param de rodar. O evento
        // de teclado do form, porém, continua disparando normalmente.
        if (
            player.HP <= 0
            && e.KeyCode == Keys.R
        )
        {
            Application.Restart();
        }
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


        // -----------------------------------------------------
        // MOVIMENTO DO JOGADOR
        // -----------------------------------------------------

        float playerSpeed = player.Speed;

        if (InputManager.KeyDown(Keys.W))
        {
            player.Move(0, -playerSpeed, delta);
        }

        if (InputManager.KeyDown(Keys.A))
        {
            player.Move(-playerSpeed, 0, delta);
        }

        if (InputManager.KeyDown(Keys.S))
        {
            player.Move(0, playerSpeed, delta);
        }

        if (InputManager.KeyDown(Keys.D))
        {
            player.Move(playerSpeed, 0, delta);
        }


        // -----------------------------------------------------
        // ESC
        // -----------------------------------------------------

        if (InputManager.KeyDown(Keys.Escape))
        {
            Close();
            return;
        }


        // -----------------------------------------------------
        // GAME OVER
        // -----------------------------------------------------

        if (player.HP <= 0)
        {
            HandleGameOver();

            fundo.Invalidate();

            return;
        }


        // -----------------------------------------------------
        // LEVEL UP
        // -----------------------------------------------------

        if (
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

        if (player.X < 0)
        {
            player.X = 0;
        }

        if (player.Y < 0)
        {
            player.Y = 0;
        }


        // -----------------------------------------------------
        // FASE 1 COMPLETA
        // -----------------------------------------------------

        if (enemiesKilled >= 15)
        {
            waitingPhase2 = true;
        }


        // -----------------------------------------------------
        // PASSAGEM PARA FASE 2
        // -----------------------------------------------------

        if (
            waitingPhase2
            && player.X > fundo.Width
        )
        {
            currentPhase = 2;

            waitingPhase2 = false;

            player.X = 100;

            player.Y =
                fundo.Height / 2;

            enemies.Clear();

            projectiles.Clear();

            enemyProjectiles.Clear();

            MessageBox.Show(
                "FASE 2 - CHUVA DE METEOROS"
            );
        }


        // -----------------------------------------------------
        // LIMITES HORIZONTAIS
        // -----------------------------------------------------

        if (!waitingPhase2)
        {
            if (
                player.X + player.Width
                > fundo.Width
            )
            {
                player.X =
                    fundo.Width
                    - player.Width;
            }
        }


        // -----------------------------------------------------
        // LIMITE INFERIOR
        // -----------------------------------------------------

        if (
            player.Y + player.Height
            > fundo.Height
        )
        {
            player.Y =
                fundo.Height
                - player.Height;
        }


        // -----------------------------------------------------
        // INVULNERABILIDADE
        // -----------------------------------------------------

        if (playerInvulnerability > 0)
        {
            playerInvulnerability -= delta;
        }


        // -----------------------------------------------------
        // SPAWN DOS INIMIGOS
        // -----------------------------------------------------

        spawnTimer -= delta;

        if (
            !phase1Completed
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


        // -----------------------------------------------------
        // ATUALIZAÇÃO DOS INIMIGOS
        // -----------------------------------------------------

        UpdateEnemies(delta);


        // -----------------------------------------------------
        // PROJÉTEIS DOS INIMIGOS
        // -----------------------------------------------------

        UpdateEnemyProjectiles(delta);


        // -----------------------------------------------------
        // TIROS DO JOGADOR
        // -----------------------------------------------------

        UpdatePlayerShooting(delta);


        // -----------------------------------------------------
        // PROJÉTEIS DO JOGADOR
        // -----------------------------------------------------

        UpdatePlayerProjectiles(delta);


        // -----------------------------------------------------
        // VERIFICA FIM DA FASE 1
        // -----------------------------------------------------

        if (
            enemiesKilled >= 15
            && !phase1Completed
        )
        {
            phase1Completed = true;

            waitingPhase2 = true;

            enemies.Clear();

            enemyProjectiles.Clear();
        }


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
        // O gameTimer NÃO é parado aqui de propósito: o form
        // precisa continuar "vivo" para o KeyDown (Keys.R) do
        // OnGameKeyDown continuar disparando e permitir reiniciar.
        // O próprio GameLoop já retorna cedo quando player.HP <= 0,
        // então o resto da lógica do jogo não é mais executado.
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
                player
            );


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

                    enemy.TakeDamage(player.Damage);


                    // -----------------------------------------
                    // RECUPERA RAM
                    // -----------------------------------------

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

                        SpawnExplosion(
                            enemy.X,
                            enemy.Y
                        );

                        enemies.RemoveAt(j);


                        if (!phase1Completed)
                        {
                            enemiesKilled++;
                        }
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
            $"FASE: {currentPhase}",
            hudFont,
            Brushes.Orange,
            450,
            20
        );


        // -----------------------------------------------------
        // PROJÉTEIS DO JOGADOR
        // -----------------------------------------------------

        foreach (var projectile in projectiles)
        {
            graphics.FillRectangle(
                Brushes.Yellow,
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

        if (player.HP <= 0)
        {
            using Font gameOverFont =
                new(
                    "Arial",
                    40,
                    FontStyle.Bold
                );

            graphics.DrawString(
                "GAME OVER XDDDD",
                gameOverFont,
                Brushes.Red,
                fundo.Width / 2 - 200,
                fundo.Height / 2
            );

            graphics.DrawString(
                "Pressione R para reiniciar",
                new Font(
                    "Arial",
                    20,
                    FontStyle.Bold
                ),
                Brushes.White,
                fundo.Width / 2 - 150,
                fundo.Height / 2 + 60
            );
        }


        // -----------------------------------------------------
        // LEVEL UP
        // -----------------------------------------------------

        if (choosingUpgrade)
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


        // -----------------------------------------------------
        // FASE 1 COMPLETA
        // -----------------------------------------------------

        if (phase1Completed)
        {
            using Font phaseFont =
                new(
                    "Arial",
                    30,
                    FontStyle.Bold
                );

            graphics.DrawString(
                "FASE 1 COMPLETA!",
                phaseFont,
                Brushes.Lime,
                fundo.Width / 2 - 180,
                fundo.Height / 2 - 100
            );
        }


        // -----------------------------------------------------
        // ESPERA PARA FASE 2
        // -----------------------------------------------------

        if (waitingPhase2)
        {
            using Font phaseFont =
                new(
                    "Arial",
                    32,
                    FontStyle.Bold
                );

            graphics.DrawString(
                "FASE 1 COMPLETA!",
                phaseFont,
                Brushes.Lime,
                fundo.Width / 2 - 180,
                fundo.Height / 2 - 60
            );


            graphics.DrawString(
                "Voe para a direita",
                new Font(
                    "Arial",
                    20
                ),
                Brushes.White,
                fundo.Width / 2 - 100,
                fundo.Height / 2
            );
        }
    }
}