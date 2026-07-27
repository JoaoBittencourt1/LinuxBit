using LinuxHub.Features.InstallWizard.Services;
using Xunit;

namespace LinuxHub.Tests.Features.InstallWizard.Services
{
    public class InstallerConfigWriterTests
    {
        [Fact]
        public void EscapeForBash_WrapsInSingleQuotes()
        {
            Assert.Equal("'hello'", InstallerConfigWriter.EscapeForBash("hello"));
        }

        [Fact]
        public void EscapeForBash_NeutralizesEmbeddedSingleQuote()
        {
            // Sem escape, uma senha como "it's" fecharia a aspa simples cedo demais e
            // deixaria "s" como um comando bash separado quando install.sh der source.
            string escaped = InstallerConfigWriter.EscapeForBash("it's");

            Assert.Equal("'it'\\''s'", escaped);
        }

        [Fact]
        public void EscapeForBash_KeepsMetacharactersLiteralInsideSingleQuotes()
        {
            // Dentro de aspas simples, bash nunca expande $, `, ", ; — mesmo que a senha
            // contenha esses caracteres, o valor sourced continua sendo um literal (o
            // que importa aqui é que a string inteira fique dentro de UM par de aspas
            // simples ininterrupto, sem fechar cedo).
            string dangerous = "p@ss\"word$(rm -rf /)`whoami`;ls";
            string escaped = InstallerConfigWriter.EscapeForBash(dangerous);

            Assert.Equal($"'{dangerous}'", escaped);
            Assert.DoesNotContain("'", dangerous); // confirma que este caso não passa pelo path de escape de aspas
        }
    }
}
