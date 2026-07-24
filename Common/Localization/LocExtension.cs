using System.Windows.Data;
using System.Windows.Markup;

namespace LinuxHub.Common.Localization
{
    /// <summary>
    /// Uso no XAML: <c>Text="{loc:Loc Wizard_Title}"</c>. É um atalho pra um Binding
    /// no indexador do <see cref="LocalizationManager"/>, que atualiza sozinho quando
    /// o idioma muda (ver LocalizationManager.SetLanguage).
    /// </summary>
    [MarkupExtensionReturnType(typeof(object))]
    public sealed class LocExtension : MarkupExtension
    {
        public LocExtension()
        {
        }

        public LocExtension(string key)
        {
            Key = key;
        }

        [ConstructorArgument("key")]
        public string Key { get; set; } = string.Empty;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding($"[{Key}]")
            {
                Source = LocalizationManager.Instance,
                Mode = BindingMode.OneWay,
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
