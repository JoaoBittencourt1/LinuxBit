## Why

O código atual do LinuxHub concentra praticamente toda a lógica da aplicação em
dois code-behind gigantes (`MainWindow.xaml.cs` com 841 linhas, `DistroWindow.xaml.cs`
com 210 linhas), com dados de distro duplicados e divergentes entre dois lugares,
três services criados e nunca usados, e uma aba inteira de XAML construída por
cópia e cola de 11 blocos quase idênticos. Isso já está no limite do que dá para
manter, e o autor pretende voltar a desenvolver o projeto ativamente — nesse
estado, qualquer feature nova (nova distro, novo tipo de instalação) exige
editar o mesmo arquivo monolítico e arrisca reintroduzir a duplicação existente.
Esta é a base necessária antes de qualquer outra evolução do produto.

## What Changes

- Introduzir uma governança de padrões (`constitution.md` + regra em
  `.claude/rules/`) que passa a valer para toda mudança futura no projeto.
- Reorganizar o código de `LinuxHub/` por feature (`Features/Catalog`,
  `Features/InstallWizard`, `Common/{Models,Data,Mvvm}`), abandonando a
  organização por tipo de arquivo.
- Migrar de code-behind com lógica embutida para MVVM completo: Views passivas,
  ViewModels com estado e `ICommand`s, services por trás de interfaces.
- **BREAKING (estrutura interna)**: `MainWindow.xaml.cs` deixa de conter lógica
  de negócio — vira apenas host de um `TabControl` com duas views de feature.
  `DistroWindow` e `ImageViewerWindow` são absorvidas pela feature `Catalog`.
- Eliminar a duplicação do catálogo de distros: fonte única em
  `Common/Data`, usada tanto para exibir a lista quanto para detectar a distro
  a partir do nome do arquivo ISO.
- Substituir os 11 blocos de XAML copiados da aba "Distros" por um
  `ItemsControl` + `DataTemplate` ligado a uma coleção observável.
- Dar destino aos três services mortos (`Models/DiskService.cs`,
  `IsoService.cs`, `BootService.cs`): a lógica que tentam encapsular
  (diskpart, download, bcdedit) é real e hoje está reimplementada inline no
  `MainWindow`. Viram a base de services de verdade, atrás de interfaces
  (`IDiskInventoryService`, `IDiskPartitioningService`, `IIsoDownloadService`,
  `IBootConfigurationService`), com tratamento de erro explícito.
- Introduzir um composition root manual em `App.xaml.cs` (construtor
  injection direto, sem container de DI) para permitir que ViewModels
  dependam de abstrações, não de implementações concretas.
- A migração é feita em duas etapas sequenciais e o app deve continuar
  compilando e funcionando ao final de cada uma: primeiro a feature
  `Catalog`, depois a feature `InstallWizard`.

## Capabilities

### New Capabilities
- `distro-catalog`: navegação pelo catálogo de distros Linux (grid, detalhes,
  carrossel de imagens/vídeos) a partir de uma única fonte de dados.
- `install-wizard`: fluxo de configuração de instalação — obtenção da ISO
  (download ou seleção manual + detecção), escolha do disco/partição alvo e
  modo de instalação, dados da conta de usuário, e geração do
  `install.conf` consumido pelo instalador Linux-side.

### Modified Capabilities
- (nenhuma — não existem specs prévias em `openspec/specs/`; este change
  documenta pela primeira vez o comportamento hoje implícito no código.)

## Impact

- Código afetado: praticamente todo `LinuxHub/`. Dentro de
  `LinuxHub/installer/`, apenas `install.sh` e as pastas `core/`,
  `distros/`, `profiles/` (payload bash, hoje vazias) ficam fora de escopo —
  `InstallerConfig.cs`, `InstallerConfigWriter.cs` e `SystemInfo.cs` são C#
  de verdade, usados pelo wizard, e migram para `Features/InstallWizard/`.
- Arquivos que deixam de existir na forma atual: `MainWindow.xaml.cs`,
  `DistroWindow.xaml.cs`, `Services/DistroDetector.cs`,
  `Models/DiskService.cs`, `Models/IsoService.cs`, `Models/BootService.cs`,
  `installer/InstallerConfig.cs`, `installer/InstallerConfigWriter.cs`,
  `installer/SystemInfo.cs` (conteúdo reorganizado, não simplesmente
  apagado).
- Nenhuma dependência externa nova (sem container de DI, sem framework MVVM
  de terceiros — `RelayCommand`/`ObservableObject` implementados localmente
  em `Common/Mvvm`).
- Sem mudança de comportamento visível para o usuário final além da correção
  de dados divergentes do catálogo (algumas distros passam a ter a versão e
  imagem corretas de forma consistente entre exibição e detecção).
