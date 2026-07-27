## MODIFIED Requirements

### Requirement: Selecionar o alvo da instalação
O sistema SHALL permitir escolher entre dois modos de instalação —
substituir o disco inteiro ou instalar em dual-boot — e, conforme o modo,
listar os discos físicos (modo substituir) ou as partições elegíveis
(maiores que 20GB, modo dual-boot) do sistema. No modo substituir, o
sistema SHALL identificar qual disco físico hospeda a instalação Windows em
execução e impedir a seleção desse disco como alvo.

#### Scenario: Modo substituir lista discos
- **WHEN** o usuário seleciona o modo "substituir"
- **THEN** o sistema lista os discos físicos disponíveis para seleção

#### Scenario: Modo dual-boot lista partições e tamanho reservado
- **WHEN** o usuário seleciona o modo "dual-boot"
- **THEN** o sistema lista as partições elegíveis (excluindo partições de
  sistema/recuperação pequenas) e permite ajustar o espaço em GB reservado
  para o Linux, limitado ao tamanho da partição selecionada

#### Scenario: Modo substituir recusa o disco de sistema em uso
- **WHEN** o usuário seleciona, no modo "substituir", o disco físico que
  hospeda a instalação Windows atualmente em execução
- **THEN** o sistema recusa a seleção, exibe uma explicação de que o
  Windows não pode substituir o disco do qual está rodando, e não permite
  avançar no wizard com esse disco como alvo

### Requirement: Gerar e persistir a configuração de instalação
Ao confirmar a instalação, o sistema SHALL exigir confirmação explícita do
usuário para a operação de disco envolvida (encolher partição, no modo
dual-boot; ou apagar todos os dados, no modo substituir em disco
secundário) antes de executar qualquer alteração. Confirmado, o sistema
SHALL montar um `InstallerConfig` com os dados da distro, ISO, modo de
instalação, disco/partição alvo, conta de usuário e informações de sistema
(locale, timezone, keymap), executar a operação de disco reversível
correspondente (shrink, quando aplicável) via `disk-provisioning`, preparar
o boot sem USB via `boot-staging`, e gravar a configuração para consumo
pelo instalador Linux-side (`linux-install-payload`).

#### Scenario: Geração bem-sucedida
- **WHEN** o usuário confirma a instalação com todos os dados obrigatórios
  preenchidos (ISO selecionada, disco/partição alvo, usuário e senha) e
  confirma a operação de disco
- **THEN** o sistema executa o shrink (se dual-boot), prepara o staging de
  boot, grava a configuração de instalação e confirma o sucesso ao usuário,
  orientando o reboot para concluir a instalação

#### Scenario: Falha ao gerar configuração
- **WHEN** ocorre um erro ao montar ou gravar a configuração de instalação,
  ou ao executar a operação de disco/boot associada
- **THEN** o sistema exibe o erro ao usuário sem encerrar a aplicação, e
  não deixa o disco em estado parcialmente alterado sem informar o que foi
  ou não aplicado

#### Scenario: Usuário cancela na confirmação destrutiva
- **WHEN** o sistema exibe o resumo da operação de disco a ser executada e
  o usuário não confirma
- **THEN** o sistema não executa nenhuma alteração em disco e retorna ao
  wizard sem gravar configuração
