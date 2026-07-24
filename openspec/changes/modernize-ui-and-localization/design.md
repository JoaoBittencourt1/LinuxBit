## Context

Depois do change `restructure-feature-based-mvvm`, o app já está organizado
por feature com MVVM (ver `constitution.md`). Este change é só de
apresentação: troca a casca visual e a forma de navegar, sem tocar em
services/ViewModels de domínio. Estado atual relevante:

- 3 `Window`s: `Shell/MainWindow` (host do `TabControl`),
  `Features/Catalog/Views/DistroDetailWindow` (aberta por cima, escondendo a
  principal), `Features/Catalog/Views/ImageViewerWindow` (modal por cima da
  anterior).
- Cores hardcoded em hex em pelo menos 3 arquivos XAML, nenhum
  `ResourceDictionary`/tema central.
- ~40 strings de UI hardcoded (XAML `Text=`/`Content=`/`Header=`, mais
  `MessageBox`/`Notify` em código), misturando PT-BR e EN-US sem critério.

## Goals / Non-Goals

**Goals:**
- Visual Fluent/Windows 11 consistente via WPF-UI, sem cor hardcoded solta
  em XAML.
- Navegação Catalog → Detalhe → Imagem sem abrir `Window`s novas do Windows.
- Toda string de UI vinda de `.resx`, com troca de idioma em tempo real
  (PT-BR/EN-US) por um seletor na UI, não só pela cultura do SO.

**Non-Goals:**
- Não muda lógica de negócio, contratos de service ou de ViewModel de
  domínio (`IsoAcquisitionViewModel`, `TargetSelectionViewModel`,
  `AccountViewModel`, `InstallWizardViewModel` mantêm as mesmas
  propriedades/comandos — só passam a expor mensagens localizadas onde hoje
  têm string fixa).
- Não implementa toggle de tema claro/escuro — mantém o visual escuro atual,
  só modernizado. Não foi pedido e amplia o escopo sem necessidade.
- Não persiste o idioma escolhido entre execuções (fica só na sessão) — a
  escolha inicial é pela cultura do SO (pt-* → PT-BR, resto → EN-US).
  Persistência é um change futuro pequeno se for pedida.
- Não adiciona um terceiro idioma agora — a infra fica pronta pra isso
  (arquivo `.resx` novo + entrada na lista de idiomas), mas só PT-BR/EN-US
  são entregues.

## Decisions

### 1. WPF-UI como biblioteca de UI

Pacote NuGet `WPF-UI` (Fluent Design). `App.xaml` importa os
`ResourceDictionary`s de tema/controles dela; `Shell/MainWindow` vira
`ui:FluentWindow`. Controles nativos dela (`ui:Button`, `ui:TextBox`,
`ui:PasswordBox`, `ui:Card`, `ui:ProgressBar`, `ui:NavigationView`,
`ui:SymbolIcon`) substituem os equivalentes WPF puros nas Views desta
change. Mantém o tema escuro (`ApplicationTheme.Dark`) aplicado no startup.

**Alternativa considerada**: Material Design in XAML — rejeitada porque o
usuário escolheu Fluent explicitamente, e Fluent combina mais com o
público-alvo (usuários migrando do Windows).

**Alternativa considerada**: tema próprio sem dependência nova — rejeitada
pelo usuário; mais trabalho manual pra um resultado visual inferior ao de
uma biblioteca madura.

### 2. Navegação: NavigationView para o nível principal, troca de conteúdo para o resto

- `Shell/MainWindow` usa `ui:NavigationView` só como chrome do nível
  principal (itens "Distros" / "Instalação"); o clique num item troca o
  `Frame`/conteúdo raiz entre `CatalogView` e `InstallWizardView`
  manualmente no code-behind do Shell (sem `TargetPageType`/DI automático
  do WPF-UI — mantém o mesmo estilo de composição manual do change
  anterior).
- Dentro da feature Catalog, a transição grid → detalhe da distro **não**
  usa `Frame`/`Page` navigation: `CatalogViewModel` expõe um estado
  (`SelectedDistroDetail: DistroDetailViewModel?`) e a `CatalogView` troca
  o conteúdo visível via `DataTrigger`/binding num `ContentControl` — grid
  quando `null`, detalhe quando preenchido. É navegação por dado, não por
  API de navegação — mais simples, mais fácil de testar, e evita acoplar a
  ViewModel a `Frame`/`Page` (tipos de `System.Windows.Controls`).
- Visualização de imagem em tela cheia vira um overlay modal *dentro* da
  mesma janela (um `Grid` com fundo semitransparente + imagem centralizada,
  fechando ao clicar fora), dirigido pelo mesmo tipo de estado
  (`CatalogViewModel.FullscreenImagePath: string?`).

**Alternativa considerada**: usar `Frame`+`Page` com navegação nativa do
WPF-UI `NavigationView` (`TargetPageType`) também para o drill-down
Catalog→Detalhe. Rejeitada: a navegação automática do WPF-UI espera
páginas resolvidas por DI ou construtor parameterless, o que colide com o
padrão de injeção manual via construtor já estabelecido (`DistroDetailViewModel`
precisa receber o `DistroInfo` selecionado). Dirigir a troca por estado
evita essa fricção sem perder o resultado visual (nenhuma `Window` nova).

**Alternativa considerada**: `ui:ContentDialog` do WPF-UI para a imagem em
tela cheia. Rejeitada por agora: a API de diálogo da biblioteca varia
bastante entre versões e normalmente pressupõe um `IContentDialogService`
registrado via DI — optamos por um overlay simples e próprio, com o mesmo
resultado visual (sem depender de uma API de terceiro com mais risco de
integração), reavaliável depois se o app adotar DI.

### 3. Localização com `.resx` + `MarkupExtension`

- `Common/Localization/Strings.resx` (neutro = PT-BR, já que é o idioma
  predominante hoje) + `Common/Localization/Strings.en-US.resx` (satélite).
- `Common/Localization/LocalizationManager.cs`: singleton com
  `INotifyPropertyChanged`, indexador `this[string key]` que resolve via
  `ResourceManager` na cultura atual, `SetLanguage(CultureInfo)` que troca
  `CurrentUICulture` e dispara `PropertyChanged("Item[]")` — assim todo
  binding `{Binding [Key], Source={x:Static loc:LocalizationManager.Instance}}`
  atualiza sozinho quando o idioma muda.
- `Common/Localization/LocExtension.cs`: `MarkupExtension` que encurta o
  uso pra `{loc:Loc CatalogTitle}` no XAML.
- Mensagens que hoje são strings fixas em código (`MessageBox`,
  `Notify?.Invoke(...)`) passam a buscar a string via
  `LocalizationManager.Instance["Key"]` na ViewModel/View, no lugar do
  texto fixo.

**Alternativa considerada**: classes fortemente tipadas geradas a partir do
`.resx` (`Strings.Designer.cs`, o padrão do Visual Studio). Funciona bem em
C#, mas não dá bind direto em XAML sem um wrapper — o indexador com
`MarkupExtension` cobre os dois casos (C# e XAML) com uma única fonte de
verdade e atualização automática ao trocar idioma, que é o requisito
central aqui.

### 4. Seletor de idioma

Um pequeno menu/botão no rodapé do `NavigationView` (bandeira ou sigla
"PT"/"EN") chama `LocalizationManager.Instance.SetLanguage(...)`. Sem
persistência (ver Non-Goals) — reinicia no idioma detectado do SO a cada
execução.

## Risks / Trade-offs

- **[Risco] Trocar `Window` por troca de conteúdo muda o comportamento de
  minimizar/restaurar** (hoje minimizar a `DistroDetailWindow` não afeta a
  principal; numa `ContentControl` só existe uma janela) → Aceito
  deliberadamente: é exatamente o comportamento "single window" pedido.
- **[Risco] WPF-UI é uma dependência de terceiro relativamente jovem** —
  breaking changes entre versões são comuns → Mitigação: fixar a versão
  instalada no `.csproj` (não usar wildcard), documentar a versão usada no
  `design.md` após a instalação.
- **[Trade-off] Overlay de imagem próprio em vez de `ContentDialog` do
  WPF-UI** → menos "de fábrica", mas elimina risco de API instável de
  diálogo entre versões da lib; o resultado visual para o usuário é o
  mesmo (modal centralizado, fecha ao clicar fora).
- **[Risco] `.resx` neutro em PT-BR é incomum** (a maioria dos exemplos usa
  inglês como neutro) → Aceito: reduz o trabalho de tradução imediato, já
  que a maior parte do texto hoje já está em português; o satélite
  `en-US` cobre o outro idioma pedido.

## Migration Plan

1. Adicionar WPF-UI e validar que o app builda com o pacote antes de
   restilizar qualquer view (passo isolado, fácil de reverter sozinho).
2. Montar a infraestrutura de localização e extrair as strings — pode ser
   feito em paralelo à troca de visual, já que são camadas independentes
   (XAML strings vs. cor/controles).
3. Restilizar Shell → Catalog → InstallWizard, nessa ordem (mesma lógica de
   "buildável a cada etapa" do change anterior).
4. `dotnet build` limpo ao final de cada etapa.

## Open Questions

- Nenhuma pendente — as três decisões de maior impacto (biblioteca visual,
  padrão de navegação, estratégia de idiomas) já foram fechadas com o
  usuário antes deste design.

## Registro pós-implementação

- **Versão do WPF-UI instalada**: `4.3.0` (+ `WPF-UI.Abstractions 4.3.0`),
  resolvida automaticamente pelo NuGet no momento da implementação.
- **Bugs encontrados e corrigidos durante o smoke test** (nenhum visível em
  code review antes de rodar o app de verdade — todos via crash log ou
  comportamento observado):
  1. `LocalizationManager`: ordem de inicialização de campos estáticos —
     `Instance` era inicializado antes de `AvailableLanguages`, que o
     construtor usa. `NullReferenceException` na primeira execução.
     Corrigido invertendo a ordem de declaração.
  2. `Shell/MainWindow`: `NavigationView.ReplaceContent(...)` chamado no
     construtor, antes do template do controle (com o `ContentPresenter`
     interno) ser aplicado — `NullReferenceException` dentro de
     `NavigationView.UpdateContent`. Corrigido movendo a chamada inicial
     para o evento `Loaded`.
  3. `CatalogView.xaml`: `DataContext` e um `Binding` de `Visibility` que
     dependia do `DataContext` do elemento pai, no mesmo elemento
     (`DistroDetailView`) — condição de corrida em que o binding de
     `Visibility` podia resolver contra o `DataContext` novo (o filho) em
     vez do `CatalogViewModel`, falhar silenciosamente e cair no padrão
     `Visibility.Visible`, fazendo o detalhe da distro aparecer sempre
     visível por cima da grade. Corrigido movendo o binding de
     `Visibility` para um `Grid` encapsulador, mantendo o `DataContext`
     só no `DistroDetailView` interno.
  4. `Shell/MainWindow`: a troca de conteúdo ao trocar de aba dependia de
     `NavigationViewItem.IsActive`, que não é atualizado automaticamente
     pela seleção (é só estado visual) — a troca nunca acontecia de fato.
     Corrigido lendo `NavigationView.SelectedItem` no handler de
     `SelectionChanged`. **Essa correção acabou não sendo suficiente** —
     ver item 5 abaixo.
  5. **(Reportado pelo usuário em uso real)** Mesmo com a correção do item
     4, o botão/aba "Instalação" continuava sem levar a lugar nenhum. O
     gerenciamento de conteúdo interno do `NavigationView` do WPF-UI (via
     `ReplaceContent`/`SelectedItem`) se mostrou pouco confiável pra esse
     caso de uso nesta versão da lib. **Decisão revisada**: abandonar
     `NavigationView` como host de conteúdo — o Shell agora usa dois
     `ui:Button` simples (Distros/Instalação) que trocam
     `ContentControl.Content` direto no `Click`. Perde a animação/pane
     colapsável nativa do `NavigationView`, mas ganha um mecanismo 100%
     determinístico (evento de `Button` é comportamento básico do WPF,
     sem estado interno escondido) — confirmado funcionando por
     screenshot.
  6. **(Reportado pelo usuário em uso real)** Labels (`TextBlock` puro)
     renderizavam com `Foreground` preto, ilegíveis no tema escuro. O
     WPF-UI restiliza automaticamente os controles nativos que reaproveita
     (`RadioButton`, `ComboBox`) e os seus próprios controles (`ui:*`),
     mas não o `TextBlock` simples (ele tem um `Wpf.Ui.Controls.TextBlock`
     próprio, separado, e não sobrescreve o nativo). Corrigido com um
     `Style` implícito em `App.xaml.Resources`
     (`TargetType="TextBlock"`, `Foreground="{DynamicResource
     TextFillColorPrimaryBrush}"`) — um lugar só corrige todas as labels
     da app.
- **Método de verificação usado**: screenshots via `PrintWindow` da janela
  do processo diretamente (sem `SetForegroundWindow`/mouse), e cliques via
  `InvokePattern` do UI Automation nos `ui:Button` (que o suportam,
  diferente do `NavigationViewItem` anterior) — não interfere com o que o
  usuário está fazendo na própria máquina enquanto testa. As duas
  interações reportadas como quebradas (navegação e cor de texto) foram
  confirmadas corrigidas por esse método após os ajustes acima.
- **Observação anterior (agora superada)**: a primeira rodada de smoke
  test (antes deste feedback) não tinha conseguido confirmar por
  screenshot a troca de aba, o clique numa distro, nem o botão de idioma
  (o foco de janela se comportou de forma inconsistente entre
  `SetForegroundWindow` reportar sucesso e o
  clique de fato ser entregue). O bug #4 acima só foi encontrado porque o
  código do handler foi revisado linha a linha depois da tentativa de
  clique não produzir mudança — não porque o clique funcionou. Recomenda-se
  uma passada manual pelo app antes de arquivar este change.
