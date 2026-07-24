# distro-catalog Specification

## Purpose
TBD - created by archiving change restructure-feature-based-mvvm. Update Purpose after archive.
## Requirements
### Requirement: Single source of distro catalog data
O sistema SHALL manter uma única fonte de dados para as distros suportadas
(`Common/Data/DistroCatalog`), usada tanto para exibição no catálogo quanto
para detecção de distro a partir de um arquivo ISO. Nenhum outro lugar do
código SHALL declarar uma lista ou dicionário separado de distros.

#### Scenario: Catálogo exibido e detecção de ISO concordam
- **WHEN** uma distro é exibida no catálogo com um `Id` específico
- **THEN** a detecção de distro por nome de arquivo ISO para esse mesmo
  `Id` retorna os mesmos `Name`, `Family` e `Version` exibidos no catálogo

### Requirement: Navegar o catálogo de distros
O sistema SHALL exibir uma grade com todas as distros do catálogo (imagem e
nome), permitindo abrir a tela de detalhe de qualquer uma delas.

#### Scenario: Abrir detalhe de uma distro
- **WHEN** o usuário clica em uma distro na grade do catálogo
- **THEN** o sistema abre a tela de detalhe daquela distro com nome,
  descrição e imagem principal corretos

### Requirement: Visualizar detalhes e mídia de uma distro
A tela de detalhe de uma distro SHALL exibir sua descrição, um link de
download, e um carrossel navegável de imagens e vídeos quando existirem.

#### Scenario: Navegar o carrossel
- **WHEN** o usuário clica em avançar ou voltar no carrossel de uma distro
  com múltiplas imagens/vídeos
- **THEN** o sistema exibe o próximo/anterior item do carrossel e libera a
  mídia do item anterior (parando vídeo em reprodução, se houver)

#### Scenario: Abrir imagem em tela cheia
- **WHEN** o usuário clica em uma imagem do carrossel
- **THEN** o sistema abre essa imagem em uma janela de visualização
  dedicada

#### Scenario: Voltar da tela de detalhe
- **WHEN** o usuário clica em voltar na tela de detalhe de uma distro
- **THEN** o sistema libera a mídia carregada e retorna à janela principal

