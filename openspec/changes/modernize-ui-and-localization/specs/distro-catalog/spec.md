## MODIFIED Requirements

### Requirement: Navegar o catálogo de distros
O sistema SHALL exibir uma grade com todas as distros do catálogo (imagem e
nome), permitindo ver o detalhe de qualquer uma delas sem abrir uma nova
janela do sistema operacional.

#### Scenario: Abrir detalhe de uma distro
- **WHEN** o usuário clica em uma distro na grade do catálogo
- **THEN** o sistema troca o conteúdo exibido, dentro da mesma janela, para
  o detalhe daquela distro com nome, descrição e imagem principal corretos

### Requirement: Visualizar detalhes e mídia de uma distro
O detalhe de uma distro SHALL exibir sua descrição, um link de download, e
um carrossel navegável de imagens e vídeos quando existirem — tudo dentro
da mesma janela principal, sem abrir janelas adicionais do sistema
operacional.

#### Scenario: Navegar o carrossel
- **WHEN** o usuário clica em avançar ou voltar no carrossel de uma distro
  com múltiplas imagens/vídeos
- **THEN** o sistema exibe o próximo/anterior item do carrossel e libera a
  mídia do item anterior (parando vídeo em reprodução, se houver)

#### Scenario: Abrir imagem em tela cheia
- **WHEN** o usuário clica em uma imagem do carrossel
- **THEN** o sistema exibe essa imagem em um overlay modal sobre a mesma
  janela, sem abrir uma janela separada

#### Scenario: Fechar a imagem em tela cheia
- **WHEN** o usuário clica fora da imagem em tela cheia (ou num botão de
  fechar)
- **THEN** o overlay é fechado e o detalhe da distro volta a ficar visível

#### Scenario: Voltar da tela de detalhe
- **WHEN** o usuário aciona voltar no detalhe de uma distro
- **THEN** o sistema libera a mídia carregada e volta a exibir a grade do
  catálogo, na mesma janela
