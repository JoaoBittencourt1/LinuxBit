## Context

O LinuxHub é um app WPF de janela única com um `TabControl` de 2 abas
("Distros" e "Instalation"). Hoje isso é implementado como:

- `MainWindow.xaml.cs` (841 linhas): catálogo hardcoded de 11 distros,
  download de ISO com progresso/cancelamento, validação de ISO, chamada ao
  detector de distro, enumeração de disco/partição via WMI direto no
  code-behind, formulário de conta de usuário, montagem e gravação do
  `InstallerConfig`.
- `DistroWindow.xaml.cs` (210 linhas): tela de detalhe de uma distro com
  carrossel de imagens/vídeo — abre como janela separada, escondendo a
  `MainWindow` (`this.Hide()` / `window.Show()`).
- `Services/DistroDetector.cs`: dicionário próprio de distros (divergente do
  catálogo do `MainWindow`) usado só para inferir a distro a partir do nome
  do arquivo ISO selecionado manualmente.
- `Models/DiskService.cs`, `IsoService.cs`, `BootService.cs`: services
  soltos, nunca referenciados por ninguém — a pasta `Models/` deveria conter
  DTOs, não lógica de processo (diskpart, download, bcdedit).
- `installer/InstallerConfig.cs`, `InstallerConfigWriter.cs`,
  `SystemInfo.cs`: C# real, mas vive na mesma pasta que `install.sh` e as
  pastas `core/`, `distros/`, `profiles/` (payload bash hoje vazio) do
  instalador universal Linux-side.
- `Commands/` e `Views/`: pastas vazias — evidência de uma tentativa anterior
  de ir para MVVM que nunca saiu do lugar.

`constitution.md` (raiz do repo) já define os padrões obrigatórios para este
change: feature-based, MVVM completo, SOLID, zero duplicação, arquivos
pequenos e focados, lógica de negócio testável sem WPF, tratamento de erro
explícito em operações de disco/boot.

## Goals / Non-Goals

**Goals:**
- Reorganizar por feature, alinhado com as 2 abas já existentes na UI.
- Migrar para MVVM completo (Views passivas, ViewModels com estado/comandos,
  services atrás de interfaces).
- Consolidar o catálogo de distros em uma única fonte de dados.
- Dar um destino real (não descartar sem revisão) à lógica que os services
  mortos tentavam encapsular.
- Manter o app buildável e funcional ao final de cada etapa da migração.

**Non-Goals:**
- Não estamos adicionando features novas de produto (novas distros, novos
  modos de instalação) — só reorganizando o que já existe.
- Não estamos trazendo um container de DI (Microsoft.Extensions.DependencyInjection
  ou similar) — o app é pequeno o bastante para injeção manual via
  construtor na composition root.
- Não estamos adicionando um framework MVVM de terceiros (CommunityToolkit.Mvvm,
  Prism) — `ObservableObject`/`RelayCommand` locais em `Common/Mvvm` bastam
  para o tamanho do projeto e evitam uma dependência nova sem necessidade
  clara.
- Não mexemos em `installer/install.sh`, `installer/core/`,
  `installer/distros/`, `installer/profiles/` (payload bash gerado/gravado
  pela app, fora do domínio C#).
- Não estamos escrevendo testes automatizados neste change — só deixando o
  código testável (lógica de negócio sem `System.Windows.*`). Testes ficam
  para um change futuro.

## Decisions

### 1. Estrutura de pastas final

```
LinuxHub/
  App.xaml(.cs)                         # composition root
  Shell/
    MainWindow.xaml(.cs)                # host do TabControl, sem lógica
  Features/
    Catalog/
      Views/
        CatalogView.xaml(.cs)           # UserControl da aba "Distros"
        DistroDetailWindow.xaml(.cs)    # era DistroWindow
        ImageViewerWindow.xaml(.cs)
      ViewModels/
        CatalogViewModel.cs
        DistroDetailViewModel.cs
        ImageViewerViewModel.cs
    InstallWizard/
      Views/
        InstallWizardView.xaml(.cs)     # UserControl da aba "Instalation"
      ViewModels/
        InstallWizardViewModel.cs       # orquestra os 3 abaixo + gera config
        IsoAcquisitionViewModel.cs
        TargetSelectionViewModel.cs
        AccountViewModel.cs
      Models/
        InstallerConfig.cs              # (movido de installer/)
      Services/
        IIsoDownloadService.cs / IsoDownloadService.cs
        IDistroDetectionService.cs / DistroDetectionService.cs
        IDiskInventoryService.cs / DiskInventoryService.cs
        IPartitionInventoryService.cs / PartitionInventoryService.cs
        IDiskPartitioningService.cs / DiskPartitioningService.cs
        IBootConfigurationService.cs / BootConfigurationService.cs
        ISystemInfoProvider.cs / SystemInfoProvider.cs  # (movido de installer/SystemInfo.cs)
        IInstallerConfigWriter.cs / InstallerConfigWriter.cs
        InstallerConfigBuilder.cs       # monta InstallerConfig a partir do estado das VMs
  Common/
    Models/
      DistroInfo.cs
      DiskInfo.cs
      PartitionInfo.cs
      InstallMode.cs
    Data/
      DistroCatalog.cs                  # fonte única de verdade
    Mvvm/
      ObservableObject.cs
      RelayCommand.cs
      AsyncRelayCommand.cs
    Helpers/
      CryptoHelper.cs                   # (já existe, só muda de pasta)
      WindowChromeHelper.cs             # extrai DwmSetWindowAttribute duplicado
  installer/                            # inalterado — payload bash
    install.sh
    core/  distros/  profiles/
```

### 2. MVVM sem framework externo

`Common/Mvvm/ObservableObject.cs` implementa `INotifyPropertyChanged` com um
`SetProperty<T>` genérico. `RelayCommand`/`AsyncRelayCommand` implementam
`ICommand` recebendo `Action`/`Func<Task>` + `Func<bool>` opcional de
`CanExecute`. É a mesma forma que o CommunityToolkit.Mvvm gera via source
generator, só que escrita à mão — evita a dependência para um app deste
tamanho, mantendo a porta aberta para trocar por ele depois se o projeto
crescer (a decisão fica documentada aqui, não é definitiva para sempre).

**Alternativa considerada**: `CommunityToolkit.Mvvm` (source generators
`[ObservableProperty]`/`[RelayCommand]`). Rejeitada por agora — dependência
externa nova não paga seu custo no tamanho atual do projeto; pode ser
revisitada em um change futuro se `Common/Mvvm` começar a crescer demais.

### 3. Catálogo único de distros

`Common/Data/DistroCatalog.cs` expõe:
- `IReadOnlyList<DistroInfo> All` — os 11 (ou mais) `DistroInfo` completos
  (incluindo `DirectDownloadLink` e `CarouselImages`, hoje só presentes no
  `MainWindow`, e `Family`/`Version` corretos, hoje só presentes ou
  divergentes no `DistroDetector`).
- `DistroInfo? FindByIsoFileName(string fileName)` — substitui a lógica de
  `DistroDetector.Detect`, casando pelo `Id` como substring do nome do
  arquivo (mesma heurística atual), agora contra a mesma lista exibida na
  UI.

`CatalogViewModel` consome `DistroCatalog.All` diretamente.
`DistroDetectionService` (em `Features/InstallWizard/Services`) chama
`DistroCatalog.FindByIsoFileName` — assim os dois consumidores finalmente
concordam.

**Dado divergente encontrado que precisa de decisão ao portar**: onde
`MainWindow` e `DistroDetector` descrevem a mesma distro com valores
diferentes (ex.: `Family`/`Version`), o catálogo consolidado usa os valores
do `MainWindow` (é o que o usuário vê e o que tem `DirectDownloadLink`
válido) — o dado do `DistroDetector` era usado só internamente e nunca
validado contra a UI.

### 4. Services por trás de interfaces + composition root manual

Cada service morto vira uma implementação de verdade, com interface
correspondente:

| Interface | Implementação | Origem da lógica |
|---|---|---|
| `IIsoDownloadService` | `IsoDownloadService` | `Models/IsoService.cs` + lógica de progresso/cancelamento hoje inline no `MainWindow` |
| `IDistroDetectionService` | `DistroDetectionService` | `Services/DistroDetector.cs`, agora sobre `DistroCatalog` |
| `IDiskInventoryService` | `DiskInventoryService` | consulta WMI de disco hoje inline em `LoadDisks()` |
| `IPartitionInventoryService` | `PartitionInventoryService` | consulta WMI de partição hoje inline em `LoadPartitions()` |
| `IDiskPartitioningService` | `DiskPartitioningService` | `Models/DiskService.cs` (diskpart via processo elevado) |
| `IBootConfigurationService` | `BootConfigurationService` | `Models/BootService.cs` (bcdedit via processo elevado) |
| `ISystemInfoProvider` | `SystemInfoProvider` | `installer/SystemInfo.cs` |
| `IInstallerConfigWriter` | `InstallerConfigWriter` | `installer/InstallerConfigWriter.cs` |

`App.xaml.cs` constrói as implementações concretas e injeta via construtor
nas ViewModels (`new InstallWizardViewModel(new IsoDownloadService(), new
DistroDetectionService(), ...)`). Nenhuma ViewModel instancia um service
diretamente com `new`.

`IDiskPartitioningService` e `IBootConfigurationService` continuam sem uso
ativo de UI por enquanto (não há botão hoje que chame diskpart/bcdedit
diretamente — `InstallButton_Click` só gera o `install.conf`), mas migram
como services de verdade, prontos para serem chamados quando essa parte do
fluxo for implementada. Isso é diferente de "código morto": a interface e a
implementação existem e são testáveis, só não têm um caller de UI ainda —
constitution.md pede que sejam integrados de verdade ou removidos, e aqui
são integrados como serviços prontos para uso, documentados como tal.

### 5. Tratamento de erro em operações de disco/boot

`constitution.md` proíbe `catch (Exception) { }` silencioso em operações de
disco/boot. `DiskPartitioningService` e `BootConfigurationService` propagam
exceções (não engolem `Process.Start` falhando) — quem chama (ViewModel)
decide como reportar ao usuário. Isso é uma mudança de comportamento em
relação ao código atual do `Models/DiskService.cs`/`BootService.cs`
original, que não tinha tratamento de erro nenhum (nem silencioso nem
explícito, porque nunca era chamado).

### 6. Migração em duas etapas, app sempre buildável

**Etapa 1 — Catalog**: cria `Common/*`, extrai `DistroCatalog`, cria
`CatalogViewModel`/`CatalogView`/`DistroDetailWindow`/`ImageViewerWindow`,
troca a aba "Distros" do `MainWindow` para hospedar `CatalogView`. A aba
"Instalation" continua exatamente como está (ainda no `MainWindow.xaml.cs`)
até a etapa 2 — as duas convivem no mesmo `MainWindow` durante a transição.

**Etapa 2 — InstallWizard**: extrai os services, cria as ViewModels do
wizard, cria `InstallWizardView`, troca a aba "Instalation" para hospedá-la,
remove o que sobrou de lógica do `MainWindow.xaml.cs` (que fica reduzido a
só hospedar as duas views).

Ao final de cada etapa: `dotnet build` sem erros e o app abre normalmente
(verificação manual, já que não há testes automatizados ainda).

## Risks / Trade-offs

- **[Risco] Migrar 841 linhas de code-behind para MVVM pode introduzir
  regressões sutis de UI** (ex.: visibilidade de painéis que hoje depende de
  checagem manual de vários controles null no `IsoOptionChanged`) →
  Mitigação: cada ViewModel expõe propriedades booleanas dedicadas para
  visibilidade (`IsManualIsoVisible`, etc.) ligadas por binding, e a
  migração etapa-por-etapa permite testar manualmente cada aba isolada antes
  de seguir para a próxima.
- **[Risco] `DiskPartitioningService`/`BootConfigurationService` mexem em
  disco/boot do usuário e não têm caller de UI ainda** → Mitigação: ficam
  implementados e testáveis, mas não expostos por nenhum botão até um change
  futuro que defina a UX de confirmação exigida por constitution.md §6.
- **[Trade-off] Sem container de DI** → composition root manual em
  `App.xaml.cs` fica mais verbosa conforme o número de services cresce, mas
  evita dependência externa desnecessária agora; se `App.xaml.cs` passar a
  ficar longa/repetitiva, isso é sinal para reconsiderar em um change
  futuro (mesmo raciocínio do item "framework MVVM externo").
- **[Risco] Consolidar o catálogo pode mudar o resultado de detecção de
  ISO para nomes de arquivo que hoje batem no dicionário do
  `DistroDetector` mas não bateriam no catálogo do `MainWindow`** (IDs
  diferentes, ex. `"pop"` vs `"popos"`) → Mitigação: o catálogo consolidado
  preserva os `Id`s originais de ambas as fontes quando divergem, garantindo
  que qualquer nome de arquivo que batia antes continua batendo.

## Migration Plan

1. Merge deste change não afeta usuários finais (app desktop, sem deploy
   contínuo) — não há rollback de produção a planejar.
2. Se a migração de uma etapa quebrar o build, reverter o commit daquela
   etapa é suficiente (etapas são commits/PRs separados, não um único commit
   gigante).
3. Após as duas etapas, arquivar este change (`openspec-archive-change`) e
   abrir specs modificadas em `openspec/specs/` para refletir o estado final.

## Open Questions

- `IDiskPartitioningService`/`IBootConfigurationService` ficam prontos mas
  sem UI que os chame — a UX de confirmação para essas operações
  destrutivas (exigida por constitution.md §6) fica para decidir em um
  change futuro dedicado a isso, ou entra neste mesmo change se o usuário
  preferir? (assumindo "fica para depois" a menos que o usuário diga o
  contrário ao revisar esta proposta.)
