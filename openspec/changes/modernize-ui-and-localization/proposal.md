## Why

O visual do LinuxHub hoje é cor hardcoded espalhada por todo XAML (`#333`,
`#444`, `#1c1c1c`...), sem tema nem componente moderno — parece um protótipo,
não um produto acabado. A navegação entre o catálogo e o detalhe de uma
distro abre uma janela do Windows separada por cima da principal
(`MainWindow.Hide()` + `new DistroDetailWindow().Show()`), o que treme e
quebra a sensação de app único. E todas as strings de UI estão hardcoded
direto no XAML/code-behind, numa mistura inconsistente de português e
inglês ("Distros" / "Instalation" / "As senhas não coincidem") — impossível
adicionar um segundo idioma sem caçar string por string.

## What Changes

- Adotar a biblioteca **WPF-UI** (Fluent Design/Windows 11) como base visual:
  `FluentWindow`, `NavigationView` como chrome de navegação principal, e os
  controles nativos dela (`Button`, `TextBox`, `PasswordBox`, `Card`,
  `ProgressBar` etc.) no lugar dos controles WPF puros com cor hardcoded.
- **BREAKING (estrutura de UI)**: `Shell/MainWindow` deixa de ser um
  `TabControl` simples e vira um `FluentWindow` com `NavigationView` (painel
  de navegação com "Distros" e "Instalação").
- Trocar a navegação Catalog → Detalhe da distro de "abrir uma `Window` nova
  por cima, escondendo a principal" para uma troca de conteúdo dentro da
  mesma janela (`ContentControl` alternando entre grid e detalhe, dirigido
  por estado da `CatalogViewModel`). `DistroDetailWindow` deixa de existir
  como `Window`; vira uma view embutida.
- Visualização de imagem em tela cheia (`ImageViewerWindow`) deixa de ser
  uma `Window` separada; vira um overlay modal dentro da mesma janela.
- Introduzir infraestrutura de localização baseada em `.resx` (`Common/Localization`):
  arquivo neutro (pt-BR) + satélite `en-US`, com um serviço de troca de
  idioma em tempo de execução (não só a cultura do Windows) e uma
  `MarkupExtension` para uso direto no XAML. Todas as strings hoje
  hardcoded em XAML/code-behind/ViewModels passam a vir daí.
- Adicionar um seletor de idioma (PT-BR / EN-US) na UI (`NavigationView`,
  rodapé do painel).
- **Fora de escopo**: nenhuma mudança de lógica de negócio — services,
  `ViewModels` de domínio (download, WMI, geração de `install.conf`)
  continuam com o mesmo contrato; esta mudança é só de apresentação
  (Views/XAML) + a nova infra de localização.

## Capabilities

### New Capabilities
- `localization`: troca de idioma em tempo de execução (PT-BR/EN-US) via
  arquivos de recurso `.resx`, com todas as strings de UI vindas de lá.

### Modified Capabilities
- `distro-catalog`: a navegação para o detalhe de uma distro e a
  visualização de imagem em tela cheia deixam de abrir janelas do Windows
  separadas — passam a ser navegação/overlay dentro da mesma janela
  principal.

## Impact

- Nova dependência: pacote NuGet `WPF-UI`.
- Arquivos afetados: `Shell/MainWindow.xaml(.cs)`, todas as Views de
  `Features/Catalog` e `Features/InstallWizard` (restilização + strings),
  `App.xaml` (dicionários de tema do WPF-UI).
- `Features/Catalog/Views/DistroDetailWindow.*` e `ImageViewerWindow.*` são
  substituídos por views embutidas (não são mais `Window`).
- Novo diretório `Common/Localization/` com os `.resx` e o serviço de troca
  de idioma.
- Nenhum service ou ViewModel de domínio muda de contrato público (só
  ganham strings localizadas onde hoje têm texto hardcoded, ex.: mensagens
  de erro em `Notify?.Invoke(...)`).
