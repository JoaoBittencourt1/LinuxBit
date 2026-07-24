# LinuxHub

WPF (.NET, C#) app — portal de distros Linux + instalador universal sem USB.

## Antes de qualquer mudança de código

Leia `constitution.md` (raiz) e `.claude/rules/constitution.mdc`. Eles
definem os padrões obrigatórios de arquitetura (feature-based + MVVM),
SOLID, anti-duplicação e clean code deste projeto. Não são opcionais.

## Estrutura

O projeto C# fica direto na raiz do repo (`LinuxHub.csproj` na raiz, não numa
subpasta `LinuxHub/`) — solução de projeto único, sem repo-dentro-de-repo.

- `Features/<Feature>/{Views,ViewModels,Services}` — código de app,
  organizado por feature.
- `Common/{Models,Data,Mvvm,Localization}` — compartilhado entre features.
- `installer/` — scripts/perfis bash que a app gera e grava para o
  instalador Linux-side. Não é código C# da aplicação; não reorganizar junto
  com o resto.
- `openspec/` — propostas e specs das mudanças de arquitetura/feature.
