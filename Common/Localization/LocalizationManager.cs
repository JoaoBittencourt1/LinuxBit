using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace LinuxHub.Common.Localization
{
    public sealed record LanguageOption(CultureInfo Culture, string DisplayName);

    /// <summary>
    /// Fonte única das strings de UI. Expõe um indexador (`this[key]`) para que
    /// bindings de XAML (via <see cref="LocExtension"/>) e código C# leiam da mesma
    /// fonte, e dispara <c>PropertyChanged("Item[]")</c> ao trocar de idioma para que
    /// todo binding existente atualize sozinho — sem precisar reabrir a tela.
    /// </summary>
    public sealed class LocalizationManager : INotifyPropertyChanged
    {
        private static readonly ResourceManager ResourceManager =
            new("LinuxHub.Common.Localization.Strings", typeof(LocalizationManager).Assembly);

        // Precisa ser inicializado ANTES de Instance: o construtor de LocalizationManager
        // lê AvailableLanguages, e inicializadores estáticos rodam na ordem declarada —
        // Instance vindo primeiro deixava AvailableLanguages null nesse ponto (NullReferenceException).
        public static IReadOnlyList<LanguageOption> AvailableLanguages { get; } = new[]
        {
            new LanguageOption(new CultureInfo("pt-BR"), "PT"),
            new LanguageOption(new CultureInfo("en-US"), "EN"),
        };

        public static LocalizationManager Instance { get; } = new();

        private LocalizationManager()
        {
            var systemCulture = CultureInfo.CurrentUICulture;
            var initial = systemCulture.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase)
                ? AvailableLanguages[0]
                : AvailableLanguages[1];

            SetLanguage(initial.Culture);
        }

        public CultureInfo CurrentCulture { get; private set; } = CultureInfo.CurrentUICulture;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string this[string key] => ResourceManager.GetString(key, CurrentCulture) ?? key;

        public void SetLanguage(CultureInfo culture)
        {
            CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;

            // "Item[]" é o nome que o binding engine do WPF espera para invalidar
            // qualquer binding de indexador (ex.: {Binding [Wizard_Title]}).
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentCulture)));
        }

        public string Format(string key, params object[] args) =>
            string.Format(CurrentCulture, this[key], args);
    }
}
