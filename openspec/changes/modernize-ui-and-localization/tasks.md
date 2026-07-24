## 1. WPF-UI: instalação e casca

- [x] 1.1 Adicionar o pacote NuGet `WPF-UI` ao `LinuxHub.csproj` — instalado `WPF-UI 4.3.0` (+ `WPF-UI.Abstractions 4.3.0`)
- [x] 1.2 Configurar `App.xaml` com os `ResourceDictionary`s de tema/controles do WPF-UI (`ThemesDictionary`, `ControlsDictionary`), tema `Dark` aplicado no startup, + `Style` implícito de `TextBlock` (ver Finalização)
- [x] 1.3 `dotnet build` — confirmado

## 2. Infraestrutura de localização

- [x] 2.1 Criar `Common/Localization/Strings.resx` (neutro, pt-BR)
- [x] 2.2 Criar `Common/Localization/Strings.en-US.resx`
- [x] 2.3 Criar `Common/Localization/LocalizationManager.cs`
- [x] 2.4 Criar `Common/Localization/LocExtension.cs`
- [x] 2.5 Seletor de idioma (PT/EN) na `TitleBar` do Shell — **confirmado por screenshot**: troca em tempo real, sem reiniciar, todas as strings visíveis mudam de idioma junto (Distros/Installation, Tipo de instalação, placeholders etc.)

## 3. Shell: navegação

- [x] 3.1 Shell vira `ui:FluentWindow` com `ui:TitleBar` (botão de idioma) — **confirmado por screenshot**
- [x] 3.2 **Revisão pós-feedback do usuário**: a primeira versão usava `ui:NavigationView` com `SelectionChanged`/`ReplaceContent` para trocar entre Catalog/InstallWizard — na prática, clicar em "Instalação" não levava a lugar nenhum (bug real reportado pelo usuário, não só falta de confirmação visual). Causa: a troca dependia de `NavigationView.SelectedItem`, cujo comportamento se mostrou pouco confiável nesta versão da lib. Substituído por navegação manual e determinística: dois `ui:Button` (Distros/Instalação) + `ContentControl.Content` trocado direto no `Click`. **Confirmado por screenshot**: clicar em "Instalação" agora troca o conteúdo de verdade.
- [x] 3.3 Strings do Shell em `{loc:Loc ...}`
- [x] 3.4 `dotnet build` + abrir o app — **confirmado por screenshot**, navegação Distros ↔ Instalação funcionando

## 4. Feature Catalog: navegação embutida + visual

- [x] 4.1–4.4 Estado de navegação (`SelectedDistroDetail`/`FullscreenImagePath`), `DistroDetailWindow`→`DistroDetailView` embutida, overlay de imagem — sem `Window` nova
- [x] 4.5 Grade de distros com `ui:Card` — **confirmado por screenshot**
- [x] 4.6 Detalhe da distro restilizado — build limpo
- [x] 4.7 Strings extraídas
- [x] 4.8 `DistroDetailWindow`/`ImageViewerWindow`/`ImageViewerViewModel` removidos
- [x] 4.9 `dotnet build` — 0 erros

## 5. Feature InstallWizard: visual

- [x] 5.1 `ui:Card`/`ui:TextBox`/`ui:PasswordBox`(`RevealButtonEnabled`)/`ui:InfoBar` — **confirmado por screenshot**: disco/partição reais (WMI), radio buttons, slider, campos de usuário todos legíveis e funcionais
- [x] 5.2 Strings extraídas — **confirmado por screenshot** em pt-BR e en-US
- [x] 5.3 `dotnet build` — 0 erros

## 6. Finalização

- [x] 6.1 Nenhuma cor hex hardcoded fora de 2 exceções deliberadas (`#CC000000` overlay, `#FF5C5C` erro de senha)
- [x] 6.2 Nenhuma string de UI hardcoded restante
- [x] 6.3 Troca de idioma em tempo real — **confirmado por screenshot**
- [x] 6.4 `dotnet build` limpo — 0 erros, 0 warnings
- [x] 6.5 Versão do WPF-UI: `4.3.0`
- [x] 6.6 **(Adicionado após feedback do usuário)** `TextBlock` puro renderizava com `Foreground` padrão (preto), ilegível no tema escuro — o WPF-UI só restiliza automaticamente os controles nativos que reaproveita (`RadioButton`, `ComboBox`) e seus próprios controles (`ui:*`), não o `TextBlock` simples. Corrigido com um `Style` implícito em `App.xaml` (`Foreground="{DynamicResource TextFillColorPrimaryBrush}"`). **Confirmado por screenshot**: todas as labels legíveis (brancas/cinza claro) no tema escuro.

### Bugs reportados pelo usuário e corrigidos nesta rodada

1. **Botão/aba "Instalação" não levava a lugar nenhum** — a versão anterior deste change já suspeitava desse bug (não confirmado por screenshot na época) mas a causa raiz estava errada (achava que era só `IsActive` vs `SelectedItem`). O usuário confirmou em uso real que continuava quebrado. Causa de fato: depender do gerenciamento de conteúdo interno do `NavigationView` do WPF-UI, que não é confiável pra esse caso de uso. Resolvido trocando por navegação manual (Button.Click + ContentControl.Content) — mecanismo simples, sem estado escondido, fácil de confirmar visualmente.
2. **Labels pretas em fundo escuro** — `Style` implícito de `TextBlock` faltando em `App.xaml` (ver 6.6).

Ambos confirmados corrigidos por screenshot (capturado via `PrintWindow`, sem tomar o foco/mouse do usuário).
