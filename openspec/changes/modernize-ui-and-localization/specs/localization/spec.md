## ADDED Requirements

### Requirement: Fonte única de strings de UI
O sistema SHALL obter todo texto visível de UI (labels, botões, mensagens de
erro/confirmação) a partir de arquivos de recurso `.resx`, não de strings
literais no XAML, code-behind ou ViewModels.

#### Scenario: Nova string de UI
- **WHEN** uma nova tela ou mensagem é adicionada ao sistema
- **THEN** o texto correspondente é definido no arquivo de recurso, e a UI
  referencia a chave — nunca um literal de texto direto

### Requirement: Suporte a Português (Brasil) e Inglês (EUA)
O sistema SHALL fornecer todas as strings de UI em pt-BR e en-US.

#### Scenario: Todas as chaves têm as duas traduções
- **WHEN** uma chave de string existe no arquivo de recurso neutro (pt-BR)
- **THEN** a mesma chave existe também no arquivo de recurso en-US

### Requirement: Troca de idioma em tempo de execução
O sistema SHALL permitir trocar o idioma da interface através de um
controle na própria UI, sem precisar reiniciar a aplicação, independente da
configuração regional do sistema operacional.

#### Scenario: Usuário troca o idioma
- **WHEN** o usuário seleciona um idioma diferente no seletor de idioma
- **THEN** todo texto de UI atualmente visível é atualizado para o novo
  idioma imediatamente, sem precisar reabrir a tela

#### Scenario: Idioma inicial
- **WHEN** a aplicação inicia
- **THEN** o idioma inicial é escolhido a partir da cultura do sistema
  operacional (português quando a cultura começa com "pt", inglês nos
  demais casos), podendo ser trocado pelo usuário em seguida
