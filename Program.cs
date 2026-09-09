using TuxSpace.Forms;

namespace TuxSpace;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var telaEntrada = new FormTelaEntrada();

        // Se o jogador fechar a Tela de Entrada sem clicar em
        // "Iniciar Jogo", o programa é encerrado sem abrir o GameForm.
        if (telaEntrada.ShowDialog() != DialogResult.OK)
            return;

        Application.Run(
            new GameForm(
                telaEntrada.NaveSelecionadaCaminho,
                telaEntrada.PontosVida,
                telaEntrada.PontosVelocidade,
                telaEntrada.PontosDano
            )
        );
    }
}