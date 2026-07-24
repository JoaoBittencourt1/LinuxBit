## ADDED Requirements

### Requirement: Obter a ISO por download automático
O sistema SHALL permitir baixar a ISO da distro selecionada, exibindo
progresso percentual e tempo restante estimado, e permitindo cancelar o
download em andamento. Um download cancelado SHALL remover o arquivo
parcial.

#### Scenario: Download concluído com sucesso
- **WHEN** o usuário inicia o download da ISO de uma distro selecionada
- **THEN** o sistema exibe progresso crescente até 100% e, ao concluir,
  disponibiliza o caminho do arquivo baixado para as próximas etapas

#### Scenario: Cancelar download em andamento
- **WHEN** o usuário clica em cancelar durante um download em progresso
- **THEN** o sistema interrompe o download, remove o arquivo parcial e
  volta ao estado anterior ao início do download

### Requirement: Obter a ISO por seleção manual
O sistema SHALL permitir selecionar manualmente um arquivo ISO local,
validando que o arquivo existe e tem tamanho mínimo plausível para uma ISO
Linux (maior que 700MB) antes de aceitá-lo.

#### Scenario: Seleção de ISO válida
- **WHEN** o usuário seleciona um arquivo `.iso` existente maior que 700MB
- **THEN** o sistema aceita o caminho e detecta a distro correspondente pelo
  nome do arquivo

#### Scenario: Seleção de ISO inválida
- **WHEN** o usuário seleciona um arquivo inexistente ou menor que 700MB
- **THEN** o sistema rejeita a seleção, exibe um erro ao usuário e não
  atualiza o caminho de ISO selecionado

### Requirement: Detectar distro a partir do nome do arquivo ISO
Ao selecionar uma ISO manualmente, o sistema SHALL tentar identificar a
distro correspondente comparando o nome do arquivo contra o catálogo único
de distros (`distro-catalog`), com um resultado de "desconhecida" como
fallback.

#### Scenario: Nome de arquivo reconhecido
- **WHEN** o nome do arquivo ISO selecionado contém o identificador de uma
  distro do catálogo
- **THEN** o sistema exibe nome e imagem dessa distro

#### Scenario: Nome de arquivo não reconhecido
- **WHEN** o nome do arquivo ISO selecionado não corresponde a nenhuma
  distro do catálogo
- **THEN** o sistema exibe uma distro "desconhecida" como fallback, sem
  travar o fluxo

### Requirement: Selecionar o alvo da instalação
O sistema SHALL permitir escolher entre dois modos de instalação —
substituir o disco inteiro ou instalar em dual-boot — e, conforme o modo,
listar os discos físicos (modo substituir) ou as partições elegíveis
(maiores que 20GB, modo dual-boot) do sistema.

#### Scenario: Modo substituir lista discos
- **WHEN** o usuário seleciona o modo "substituir"
- **THEN** o sistema lista os discos físicos disponíveis para seleção

#### Scenario: Modo dual-boot lista partições e tamanho reservado
- **WHEN** o usuário seleciona o modo "dual-boot"
- **THEN** o sistema lista as partições elegíveis (excluindo partições de
  sistema/recuperação pequenas) e permite ajustar o espaço em GB reservado
  para o Linux, limitado ao tamanho da partição selecionada

### Requirement: Coletar dados da conta de usuário
O sistema SHALL coletar nome de usuário, senha (com confirmação) e nome do
computador, sinalizando quando a senha e a confirmação não coincidem.

#### Scenario: Senha e confirmação não coincidem
- **WHEN** o usuário digita uma confirmação de senha diferente da senha
- **THEN** o sistema exibe um aviso de que as senhas não coincidem

### Requirement: Gerar e persistir a configuração de instalação
Ao confirmar a instalação, o sistema SHALL montar um `InstallerConfig` com
os dados da distro, ISO, modo de instalação, disco/partição alvo, conta de
usuário e informações de sistema (locale, timezone, keymap), e gravá-lo para
consumo pelo instalador Linux-side.

#### Scenario: Geração bem-sucedida
- **WHEN** o usuário confirma a instalação com todos os dados obrigatórios
  preenchidos (ISO selecionada, disco/partição alvo, usuário e senha)
- **THEN** o sistema grava a configuração de instalação e confirma o
  sucesso ao usuário

#### Scenario: Falha ao gerar configuração
- **WHEN** ocorre um erro ao montar ou gravar a configuração de instalação
- **THEN** o sistema exibe o erro ao usuário sem encerrar a aplicação
