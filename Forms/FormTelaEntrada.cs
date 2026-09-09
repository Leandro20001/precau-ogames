using System.Drawing;
using System.Windows.Forms;

namespace TuxSpace.Forms;

/// <summary>
/// Tela de Entrada: permite ao jogador escolher a nave e distribuir
/// pontos de atributo antes de iniciar a partida.
/// </summary>
public class FormTelaEntrada : Form
{
    // -----------------------------------------------------
    // NAVES DISPONÍVEIS
    // -----------------------------------------------------

    private readonly string[] navesCaminhos =
    {
        "Assets/Player/tux.png",
        "Assets/Player/freebsd.png",
    };

    private readonly string[] navesNomes =
    {
        "Tux",
        "FreeBSD",
    };

    private int naveEscolhida = 0;

    // -----------------------------------------------------
    // PONTOS DE ATRIBUTO
    // -----------------------------------------------------

    private const int PontosTotais = 15;

    private int pontosVida = 5;
    private int pontosVelocidade = 5;
    private int pontosDano = 5;

    private int PontosGastos =>
        pontosVida + pontosVelocidade + pontosDano;

    private int PontosRestantes =>
        PontosTotais - PontosGastos;

    // -----------------------------------------------------
    // CONTROLES
    // -----------------------------------------------------

    private Panel painelConteudo = null!;
    private PictureBox previewNave = null!;
    private Label lblNomeNave = null!;
    private Label lblPontosRestantes = null!;

    private Label lblVida = null!;
    private Label lblVelocidade = null!;
    private Label lblDano = null!;

    private Button btnIniciar = null!;

    // -----------------------------------------------------
    // RESULTADO (lido pelo Program.cs após o ShowDialog)
    // -----------------------------------------------------

    public string NaveSelecionadaCaminho => navesCaminhos[naveEscolhida];
    public int PontosVida => pontosVida;
    public int PontosVelocidade => pontosVelocidade;
    public int PontosDano => pontosDano;

    public FormTelaEntrada()
    {
        Text = "TuxSpace - Tela de Entrada";
        Size = new Size(640, 480);
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.Black;
        BackgroundImage = Image.FromFile("Assets/WinXPfundo.png");
        BackgroundImageLayout = ImageLayout.Stretch;

        Resize += (s, e) => CentralizarConteudo();

        MontarInterface();
        AtualizarPreview();
        AtualizarLabelsDePontos();
    }

    // =========================================================
    // MONTAGEM DA INTERFACE
    // =========================================================

    private void MontarInterface()
    {
        painelConteudo = new Panel
        {
            Size = new Size(560, 420),
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.None,
        };
        Controls.Add(painelConteudo);
        CentralizarConteudo();

        var titulo = new Label
        {
            Text = "Escolha sua nave e distribua seus pontos",
            ForeColor = Color.FromArgb(245, 245, 245),
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(25, 15),
        };
        painelConteudo.Controls.Add(titulo);

        var painelNave = new Panel
        {
            Location = new Point(35, 70),
            Size = new Size(200, 255),
            BackColor = Color.FromArgb(18, 18, 18),
            BorderStyle = BorderStyle.FixedSingle,
        };
        painelConteudo.Controls.Add(painelNave);

        // ---------------- Seleção de nave ----------------

        previewNave = new PictureBox
        {
            Location = new Point(17, 15),
            Size = new Size(165, 165),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(50, 50, 50),
            Anchor = AnchorStyles.None,
        };
        painelNave.Controls.Add(previewNave);

        lblNomeNave = new Label
        {
            ForeColor = Color.FromArgb(245, 245, 245),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = false,
            Size = new Size(165, 30),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(17, 188),
            BackColor = Color.FromArgb(60, 60, 60),
        };
        painelNave.Controls.Add(lblNomeNave);

        var btnAnterior = new Button
        {
            Text = "<",
            Location = new Point(17, 220),
            Size = new Size(70, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
        };
        btnAnterior.FlatAppearance.BorderSize = 1;
        btnAnterior.FlatAppearance.BorderColor = Color.Black;
        btnAnterior.Click += (s, e) => TrocarNave(-1);
        painelNave.Controls.Add(btnAnterior);

        var btnProxima = new Button
        {
            Text = ">",
            Location = new Point(112, 220),
            Size = new Size(70, 30),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
        };
        btnProxima.FlatAppearance.BorderSize = 1;
        btnProxima.FlatAppearance.BorderColor = Color.Black;
        btnProxima.Click += (s, e) => TrocarNave(1);
        painelNave.Controls.Add(btnProxima);

        // ---------------- Distribuição de pontos ----------------

        var painelAtributos = new Panel
        {
            Location = new Point(285, 70),
            Size = new Size(225, 260),
            BackColor = Color.FromArgb(18, 18, 18),
            BorderStyle = BorderStyle.FixedSingle,
        };
        painelConteudo.Controls.Add(painelAtributos);

        var lblTituloPontos = new Label
        {
            Text = "Distribua seus pontos",
            ForeColor = Color.FromArgb(245, 245, 245),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 18),
        };
        painelAtributos.Controls.Add(lblTituloPontos);

        lblPontosRestantes = new Label
        {
            ForeColor = Color.FromArgb(242, 242, 242),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 45),
        };
        painelAtributos.Controls.Add(lblPontosRestantes);

        lblVida = CriarLinhaDeAtributo(
            painelAtributos,
            "Vida", 20, 90,
            () => pontosVida,
            v => pontosVida = v);

        lblVelocidade = CriarLinhaDeAtributo(
            painelAtributos,
            "Velocidade", 20, 145,
            () => pontosVelocidade,
            v => pontosVelocidade = v);

        lblDano = CriarLinhaDeAtributo(
            painelAtributos,
            "Dano", 20, 200,
            () => pontosDano,
            v => pontosDano = v);

        // ---------------- Botão iniciar ----------------

        btnIniciar = new Button
        {
            Text = "Iniciar Jogo",
            Size = new Size(180, 42),
            Location = new Point((painelConteudo.Width - 180) / 2, 350),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.Black,
        };
        btnIniciar.FlatAppearance.BorderSize = 1;
        btnIniciar.FlatAppearance.BorderColor = Color.Black;
        btnIniciar.Click += (s, e) =>
        {
            DialogResult = DialogResult.OK;
            Close();
        };
        painelConteudo.Controls.Add(btnIniciar);
    }

    private void CentralizarConteudo()
    {
        if (painelConteudo == null)
            return;

        painelConteudo.Location = new Point(
            (ClientSize.Width - painelConteudo.Width) / 2,
            (ClientSize.Height - painelConteudo.Height) / 2);
    }

    /// <summary>
    /// Cria uma linha com nome do atributo, botões -/+ e valor atual.
    /// Retorna o Label do valor para permitir atualização posterior.
    /// </summary>
    private Label CriarLinhaDeAtributo(
        Control painelPai,
        string nome,
        int x,
        int y,
        Func<int> getValor,
        Action<int> setValor)
    {
        var lblNome = new Label
        {
            Text = nome,
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(x, y + 6),
        };
        painelPai.Controls.Add(lblNome);

        var btnMenos = new Button
        {
            Text = "-",
            Size = new Size(30, 28),
            Location = new Point(x + 110, y),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
        };
        btnMenos.FlatAppearance.BorderSize = 1;
        btnMenos.FlatAppearance.BorderColor = Color.Black;
        painelPai.Controls.Add(btnMenos);

        var lblValor = new Label
        {
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(x + 150, y + 6),
        };
        painelPai.Controls.Add(lblValor);

        var btnMais = new Button
        {
            Text = "+",
            Size = new Size(30, 28),
            Location = new Point(x + 180, y),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
        };
        btnMais.FlatAppearance.BorderSize = 1;
        btnMais.FlatAppearance.BorderColor = Color.Black;
        painelPai.Controls.Add(btnMais);

        btnMenos.Click += (s, e) =>
        {
            // Não deixa o atributo ficar negativo.
            if (getValor() > 0)
            {
                setValor(getValor() - 1);
                AtualizarLabelsDePontos();
            }
        };

        btnMais.Click += (s, e) =>
        {
            // Só permite gastar se ainda houver pontos disponíveis.
            if (PontosRestantes > 0)
            {
                setValor(getValor() + 1);
                AtualizarLabelsDePontos();
            }
        };

        return lblValor;
    }

    // =========================================================
    // ATUALIZAÇÕES DE TELA
    // =========================================================

    private void TrocarNave(int direcao)
    {
        naveEscolhida =
            (naveEscolhida + direcao + navesCaminhos.Length)
            % navesCaminhos.Length;

        AtualizarPreview();
    }

    private void AtualizarPreview()
    {
        previewNave.Image?.Dispose();

        previewNave.Image =
            Image.FromFile(navesCaminhos[naveEscolhida]);

        lblNomeNave.Text = navesNomes[naveEscolhida];
    }

    private void AtualizarLabelsDePontos()
    {
        lblVida.Text = pontosVida.ToString();
        lblVelocidade.Text = pontosVelocidade.ToString();
        lblDano.Text = pontosDano.ToString();

        lblPontosRestantes.Text =
            $"Pontos restantes: {PontosRestantes}";

        // Impede iniciar o jogo com pontos ainda não distribuídos,
        // para forçar o jogador a usar tudo o que tem disponível.
        btnIniciar.Enabled = PontosRestantes == 0;
        btnIniciar.BackColor = btnIniciar.Enabled ? Color.White : Color.FromArgb(80, 80, 80);
        btnIniciar.ForeColor = btnIniciar.Enabled ? Color.Black : Color.FromArgb(200, 200, 200);
    }
}
