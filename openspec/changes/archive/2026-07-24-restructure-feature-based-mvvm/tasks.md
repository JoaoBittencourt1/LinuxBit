## 1. Governança (concluído antes deste change)

- [x] 1.1 Criar `constitution.md` com os padrões de arquitetura/SOLID/clean code
- [x] 1.2 Criar `.claude/rules/constitution.mdc`
- [x] 1.3 Criar `CLAUDE.md` apontando para a constitution (garante leitura automática)
- [x] 1.4 Configurar `.claude/settings.json` com allowlist de operações de dev comuns

## 2. Fundação compartilhada (Common/)

- [x] 2.1 Criar `Common/Mvvm/ObservableObject.cs` (`INotifyPropertyChanged` + `SetProperty<T>`)
- [x] 2.2 Criar `Common/Mvvm/RelayCommand.cs` (`ICommand` síncrono, com `CanExecute` opcional)
- [x] 2.3 Criar `Common/Mvvm/AsyncRelayCommand.cs` (`ICommand` assíncrono, com controle de `IsExecuting`)
- [x] 2.4 Mover `Models/DistroInfo.cs`, `DiskInfo.cs`, `PartitionInfo.cs`, `InstallMode.cs` para `Common/Models/` (ajustar namespace e usings dependentes) — `Models/Distro.cs` também removido (dead code, nunca referenciado)
- [x] 2.5 Criar `Common/Data/DistroCatalog.cs` consolidando os 11 `DistroInfo` do `MainWindow` (dados corretos: `DirectDownloadLink`, `CarouselImages`, `Family`/`Version`) + `FindByIsoFileName(string)` preservando os `Id`s de ambas as fontes originais quando divergiam
- [x] 2.6 Mover `Helpers/CryptoHelper.cs` para `Common/Helpers/`
- [x] 2.7 Criar `Common/Helpers/WindowChromeHelper.cs` extraindo o `DwmSetWindowAttribute`/dark mode duplicado entre `MainWindow` e `DistroWindow`
- [x] 2.8 Remover as pastas vazias `LinuxHub/Commands/` e `LinuxHub/Views/` (superadas pela estrutura por feature — Views vivem em `Features/<Nome>/Views`, comandos-base em `Common/Mvvm`)

## 3. Etapa 1 — Feature Catalog

- [x] 3.1 Criar `Features/Catalog/ViewModels/DistroDetailViewModel.cs` (nome, descrição, imagem, link de download, estado do carrossel)
- [x] 3.2 Criar `Features/Catalog/ViewModels/ImageViewerViewModel.cs`
- [x] 3.3 Criar `Features/Catalog/ViewModels/CatalogViewModel.cs` (`IReadOnlyList<DistroInfo>` a partir de `DistroCatalog.All` + `OpenDistroCommand`)
- [x] 3.4 Criar `Features/Catalog/Views/DistroDetailWindow.xaml(.cs)` a partir de `DistroWindow`, code-behind reduzido a renderização de mídia do carrossel (limitação de WPF) + wiring de `DataContext`
- [x] 3.5 Criar `Features/Catalog/Views/ImageViewerWindow.xaml(.cs)` a partir do `ImageViewerWindow` atual
- [x] 3.6 Criar `Features/Catalog/Views/CatalogView.xaml(.cs)` como `UserControl` com `ItemsControl`+`DataTemplate` substituindo os 11 blocos de XAML copiados, ligado a `CatalogViewModel`
- [x] 3.7 Trocar a aba "Distros" do `MainWindow.xaml` para hospedar `<catalog:CatalogView/>`
- [x] 3.8 Remover `DistroWindow.xaml(.cs)` e `ImageViewerWindow.xaml(.cs)` originais, e os 11 métodos hardcoded de distro + `RegisterDistroClicks` do `MainWindow.xaml.cs`
- [x] 3.9 `dotnet build` — 0 erros. Smoke test manual completo fica para o final do change (junto com a etapa 2), ver seção 4.19

## 4. Etapa 2 — Feature InstallWizard

- [x] 4.1 Criar `Features/InstallWizard/Services/IIsoDownloadService.cs` + `IsoDownloadService.cs` (a partir de `Models/IsoService.cs` + lógica de progresso/cancelamento hoje inline no `MainWindow`)
- [x] 4.2 Criar `Features/InstallWizard/Services/IDistroDetectionService.cs` + `DistroDetectionService.cs` sobre `Common/Data/DistroCatalog`
- [x] 4.3 Criar `Features/InstallWizard/Services/IDiskInventoryService.cs` + `DiskInventoryService.cs` (WMI disco, a partir de `LoadDisks()`)
- [x] 4.4 Criar `Features/InstallWizard/Services/IPartitionInventoryService.cs` + `PartitionInventoryService.cs` (WMI partição, a partir de `LoadPartitions()`)
- [x] 4.5 Criar `Features/InstallWizard/Services/IDiskPartitioningService.cs` + `DiskPartitioningService.cs` (a partir de `Models/DiskService.cs`, com tratamento de erro explícito — sem `catch` silencioso; sem caller de UI ainda, ver design.md Open Questions)
- [x] 4.6 Criar `Features/InstallWizard/Services/IBootConfigurationService.cs` + `BootConfigurationService.cs` (a partir de `Models/BootService.cs`, com tratamento de erro explícito + `ArgumentList` em vez de string de shell interpolada)
- [x] 4.7 Mover `installer/SystemInfo.cs` para `Features/InstallWizard/Services/ISystemInfoProvider.cs` + `SystemInfoProvider.cs`
- [x] 4.8 Mover `installer/InstallerConfig.cs` para `Features/InstallWizard/Models/InstallerConfig.cs` (remover `using System.Windows.Controls` não utilizado)
- [x] 4.9 Mover `installer/InstallerConfigWriter.cs` para `Features/InstallWizard/Services/IInstallerConfigWriter.cs` + `InstallerConfigWriter.cs`
- [x] 4.10 Criar `Features/InstallWizard/Services/InstallerConfigBuilder.cs` (lógica pura de `BuildInstallerConfig`, testável sem depender de `System.Windows.*`)
- [x] 4.11 Criar `Features/InstallWizard/ViewModels/IsoAcquisitionViewModel.cs` (fonte da ISO: download com progresso/cancelamento OU seleção manual + validação + detecção de distro)
- [x] 4.12 Criar `Features/InstallWizard/ViewModels/TargetSelectionViewModel.cs` (modo replace/dual-boot, lista de discos/partições, slider de tamanho reservado)
- [x] 4.13 Criar `Features/InstallWizard/ViewModels/AccountViewModel.cs` (usuário, senha+confirmação com toggle de visibilidade, hostname, validação de senhas coincidentes)
- [x] 4.14 Criar `Features/InstallWizard/ViewModels/InstallWizardViewModel.cs` orquestrando as 3 ViewModels acima + `InstallCommand` usando `InstallerConfigBuilder` e `IInstallerConfigWriter`
- [x] 4.15 Criar `Features/InstallWizard/Views/InstallWizardView.xaml(.cs)` como `UserControl` a partir do conteúdo atual da aba "Instalation", com bindings/commands substituindo os `Click=`/`Checked=` diretos
- [x] 4.16 Trocar a aba "Instalation" do `MainWindow.xaml` para hospedar `<wizard:InstallWizardView/>`
- [x] 4.17 Remover o restante de `MainWindow.xaml.cs` (WMI inline, formulário de conta, `BuildInstallerConfig`, handlers de evento) — fica só com `InitializeComponent` + wiring de `DataContext`
- [x] 4.18 Remover `Services/DistroDetector.cs`, `Models/DiskService.cs`, `Models/IsoService.cs`, `Models/BootService.cs`, `installer/SystemInfo.cs`, `installer/InstallerConfig.cs`, `installer/InstallerConfigWriter.cs` originais
- [x] 4.19 `dotnet build` — 0 erros, 0 warnings. Smoke test manual visual feito via skill `run` (ver resumo da sessão)

## 5. Composition root e finalização

- [x] 5.1 Atualizar `App.xaml.cs` para construir os services concretos e injetar nas ViewModels via construtor (composition root manual)
- [x] 5.2 Mover `MainWindow.xaml(.cs)` para `Shell/MainWindow.xaml(.cs)`
- [x] 5.3 Conferir que nenhum arquivo de code-behind ou ViewModel passa de ~250-300 linhas — maior arquivo final: `Common/Data/DistroCatalog.cs` com 214 linhas (majoritariamente dados)
- [x] 5.4 `dotnet build` limpo — 0 erros, 0 warnings (zero warnings novos; várias das nullable warnings pré-existentes também foram corrigidas ao reescrever os arquivos)
- [x] 5.5 Revisão final: nenhuma pasta vazia ou arquivo órfão sobrando na raiz de `LinuxHub/` fora da estrutura `Features/`, `Common/`, `Shell/`, `installer/` (payload bash)
