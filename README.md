# AgroVerde API

API desenvolvida em ASP.NET Core para o projeto AgroVerde.

O objetivo desta etapa do projeto foi começar a retirar do Flutter a responsabilidade pelas regras de negócio e pelo acesso direto aos dados, passando essas operações para uma API.

A solução foi organizada em camadas, buscando separar as responsabilidades e facilitar a manutenção do sistema.

## Integrantes

- Felipe do Nascimento Magalhães
- Maicon do A. Barbosa
- Gustavo P. Pedrosa

## Tecnologias utilizadas

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- JWT
- Swagger
- Git e GitHub
- Flutter

## Estrutura do projeto

A solução foi dividida em quatro projetos:

### AgroVerde.Domain

Camada que contém as entidades do sistema e os contratos dos repositórios.

Exemplos:

- Safra
- Talhão
- Animal
- Transação Financeira
- Interfaces dos repositórios

### AgroVerde.Application

Responsável pelos serviços e pela lógica utilizada nos casos de uso da aplicação.

Nessa camada estão principalmente:

- Services
- Interfaces dos Services
- DTOs
- Validações

### AgroVerde.Infrastructure

Responsável pelo acesso e persistência dos dados.

Nela estão:

- Implementações dos repositórios
- Entity Framework Core
- DbContext
- Configurações das entidades
- Migrations

O banco utilizado atualmente é o SQLite.

### AgroVerde.API

É a camada que recebe as requisições HTTP.

Nela estão os Controllers e as configurações da aplicação, como injeção de dependência, autenticação JWT, cache e Swagger.

De forma resumida:

```text
Domain         -> define
Application    -> processa
Infrastructure -> persiste
API            -> recebe e responde
```

## Fluxo da aplicação

O fluxo básico de uma requisição é:

```text
Flutter
   ↓
Controller
   ↓
Service
   ↓
Repository
   ↓
Entity Framework Core
   ↓
SQLite
```

O Controller recebe a requisição, o Service executa o processamento necessário e o Repository realiza o acesso aos dados.

## Módulos

Até o momento foram implementados na API:

- Usuário e autenticação
- Propriedade
- Talhão
- Safra
- Estoque
- Rebanho
- Financeiro
- Dashboard

### Usuário e autenticação

O cadastro e o login são tratados pelo `AuthController`.

Após o login é gerado um token JWT, utilizado para acessar os endpoints protegidos da API.

### Propriedade

Possui operações de cadastro, consulta, alteração e exclusão de propriedades vinculadas ao usuário.

### Talhão

Possui CRUD e regras relacionadas aos talhões das propriedades.

### Safra

Possui CRUD de safras, associação com talhões e validações relacionadas ao status das safras.

### Estoque

Responsável pelo cadastro dos itens e controle das movimentações e consumos.

### Rebanho

O módulo de rebanho possui o cadastro dos animais e registros relacionados a:

- Pesagem
- Vacinação
- Controle sanitário
- Reprodução

### Financeiro

Responsável pelo registro e consulta das transações financeiras da propriedade, como receitas e despesas.

### Dashboard

O Dashboard reúne algumas informações da propriedade:

- Quantidade de talhões
- Safras ativas
- Quantidade de animais
- Receitas
- Despesas
- Saldo

## Banco de dados

O projeto utiliza SQLite com Entity Framework Core.

O contexto do banco está localizado na camada Infrastructure:

```text
AgroVerde.Infrastructure/Data/AgroVerdeDbContext.cs
```

A string de conexão é configurada no `appsettings.json` da API.

As alterações na estrutura do banco são controladas através das migrations do Entity Framework.

Atualmente temos migrations para:

- Estrutura inicial
- Safra
- Estoque
- Usuário e Propriedade
- Rebanho e Financeiro

## Autenticação

A autenticação foi implementada utilizando JWT.

Após o usuário realizar o login, a API retorna um token que pode ser utilizado nas próximas requisições.

Os endpoints que precisam de autenticação utilizam `[Authorize]`.

## Cache

Foi utilizado `IMemoryCache` em algumas consultas da aplicação.

Atualmente o cache é utilizado principalmente nas consultas de Propriedade e Dashboard, evitando que uma mesma informação seja processada novamente durante um curto período.

## Respostas da API

Para manter um padrão entre as respostas dos serviços foi utilizado o `AppResponse<T>`.

A resposta pode conter:

- Sucesso ou falha
- Mensagem
- Dados
- Erros

## Como executar

Na raiz do projeto, restaurar as dependências:

```powershell
dotnet restore
```

Compilar:

```powershell
dotnet build
```

Aplicar as migrations:

```powershell
dotnet ef database update --project .\AgroVerde.Infrastructure --startup-project .\AgroVerde.API
```

Executar a API:

```powershell
dotnet run --project .\AgroVerde.API
```

Após iniciar a aplicação, a API pode ser consultada através do Swagger.

## Estrutura

```text
agroverde-api
│
├── AgroVerde.API
│   ├── Controllers
│   ├── Services
│   ├── Program.cs
│   └── appsettings.json
│
├── AgroVerde.Application
│   ├── DTOs
│   ├── Services
│   └── Common
│
├── AgroVerde.Domain
│   ├── Entities
│   └── Repositories
│
├── AgroVerde.Infrastructure
│   ├── Data
│   ├── Repositories
│   └── Migrations
│
└── AgroVerde.slnx
```

## Situação atual

A estrutura principal do backend está implementada e a API já possui os principais módulos do AgroVerde.

A integração completa do Flutter com os endpoints da API ainda faz parte das próximas etapas do projeto.