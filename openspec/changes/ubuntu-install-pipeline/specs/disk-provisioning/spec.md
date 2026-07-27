## ADDED Requirements

### Requirement: Detectar o tipo de firmware corretamente
O sistema SHALL determinar se a máquina usa firmware UEFI ou BIOS legado
usando a API do sistema operacional dedicada a essa finalidade, não uma
heurística de existência de arquivo/pasta.

#### Scenario: Máquina UEFI é detectada corretamente
- **WHEN** o sistema consulta o tipo de firmware numa máquina que inicializa
  em modo UEFI
- **THEN** o sistema reporta UEFI, independentemente da presença ou não de
  pastas específicas do Windows Boot Manager no disco

#### Scenario: Máquina BIOS legado é detectada corretamente
- **WHEN** o sistema consulta o tipo de firmware numa máquina que inicializa
  em modo BIOS legado (sem UEFI)
- **THEN** o sistema reporta BIOS legado

### Requirement: Localizar a EFI System Partition por tipo, não por índice
Em máquinas UEFI, o sistema SHALL localizar a EFI System Partition
consultando o GUID de tipo de partição GPT correspondente, nunca assumindo
um índice de partição fixo.

#### Scenario: ESP não está na primeira posição do disco
- **WHEN** o disco alvo tem uma partição de recuperação, MSR ou OEM antes
  da EFI System Partition
- **THEN** o sistema ainda assim identifica corretamente qual partição é a
  EFI System Partition, pelo seu GUID de tipo

### Requirement: Encolher uma partição existente para liberar espaço
No modo dual-boot, o sistema SHALL encolher (shrink) a partição selecionada
pelo usuário no wizard — não uma partição fixa — para liberar espaço não
alocado do tamanho solicitado, sem criar uma nova partição ou sistema de
arquivos nessa etapa.

#### Scenario: Shrink aplicado na partição selecionada
- **WHEN** o usuário seleciona uma partição específica (não
  necessariamente a partição do Windows) e um tamanho em GB no wizard
- **THEN** o sistema encolhe exatamente essa partição pelo tamanho
  solicitado, deixando o espaço liberado como não alocado

#### Scenario: Shrink falha por espaço insuficiente
- **WHEN** o tamanho solicitado excede o espaço disponível para encolher na
  partição selecionada
- **THEN** o sistema não executa a operação e reporta o erro ao usuário sem
  alterar a partição

### Requirement: Recusar o modo substituir no disco de sistema em uso
O sistema SHALL identificar qual disco físico hospeda a instalação Windows
atualmente em execução e impedir que esse disco seja usado como alvo do
modo substituir, já que reparticionar/formatar o próprio disco de boot em
execução não é uma operação segura nem suportada pelo Windows.

#### Scenario: Disco de sistema é excluído da seleção de replace
- **WHEN** o sistema lista os discos elegíveis para o modo substituir
- **THEN** o disco físico que hospeda o volume de boot do Windows em
  execução é marcado como não elegível, com uma explicação visível ao
  usuário

### Requirement: Exigir confirmação explícita antes de qualquer operação de disco
O sistema SHALL exibir um resumo da operação de disco prestes a ser
executada (partição/disco alvo, tipo de operação, dados afetados) e exigir
confirmação explícita do usuário antes de executar qualquer alteração,
mesmo operações reversíveis como o shrink.

#### Scenario: Confirmação obrigatória para shrink
- **WHEN** o usuário avança para a etapa de confirmação no modo dual-boot
- **THEN** o sistema exibe qual partição será encolhida e por quantos GB, e
  só prossegue após confirmação explícita

#### Scenario: Confirmação reforçada para replace
- **WHEN** o usuário avança para a etapa de confirmação no modo substituir
- **THEN** o sistema exige uma ação de confirmação mais explícita que um
  clique simples (ex.: digitar uma palavra de confirmação), dado que todos
  os dados do disco selecionado serão perdidos
