<<<<<<< HEAD
# Korp_Teste_SeuNome
=======
# Korp_Teste_Carvalho

Aplicacao de emissao de notas fiscais com arquitetura de microsservicos, composta por:

- `Estoque.Api`: cadastro de produtos e controle de saldo
- `Faturamento.Api`: gestao de notas fiscais e fechamento
- `korp-notas-ui`: frontend Angular
- `PostgreSQL`: persistencia separada para estoque e faturamento
- `RabbitMQ`: comunicacao assincrona entre os servicos

## Tecnologias

- Backend: `.NET 8`, `ASP.NET Core`, `Entity Framework Core`, `FluentValidation`, `Serilog`
- Frontend: `Angular`, `Angular Material`, `RxJS`
- Infra: `Docker Compose`, `PostgreSQL`, `RabbitMQ`

## Como executar

### 1. Subir a infraestrutura

Na raiz do repositorio:

```powershell
docker compose up -d
```

Servicos esperados:

- `postgres-estoque` em `localhost:5433`
- `postgres-faturamento` em `localhost:5434`
- `rabbitmq` em `localhost:5672`
- painel do RabbitMQ em `http://localhost:15672`

### 2. Subir os backends

Execute os projetos:

- `Estoque/Estoque.Api`
- `Faturamento/Faturamento.Api`

Swagger:

- Estoque: `https://localhost:7056/swagger`
- Faturamento: `https://localhost:7290/swagger`

### 3. Subir o frontend

No terminal:

```powershell
cd .\korp-notas-ui
npm.cmd install
npm.cmd start
```

Frontend:

- `http://localhost:4200`

## Fluxo principal

1. Cadastrar produtos
2. Criar nota fiscal
3. Adicionar itens na nota
4. Fechar nota
5. O faturamento publica evento no RabbitMQ
6. O estoque processa a baixa
7. Em sucesso, a nota e fechada
8. Em falha, a nota volta para aberta e a mensagem e exibida ao usuario

## Requisitos atendidos

- Cadastro de produtos com codigo, descricao e saldo
- Cadastro de notas fiscais com numeracao sequencial automatica
- Inclusao de multiplos produtos na nota
- Edicao e exclusao de itens da nota
- Fechamento de nota com processamento assincrono
- Atualizacao de saldo no estoque
- Feedback ao usuario em caso de falha
- Persistencia real em banco de dados
- Separacao em dois microsservicos
- Idempotencia no consumo de mensagens

## Observacoes tecnicas

- O frontend usa `RxJS` para chamadas HTTP, polling e tratamento reativo do fechamento da nota
- O backend usa `LINQ` em consultas com Entity Framework Core
- As validacoes de entrada sao feitas com `FluentValidation`
- O tratamento global de excecoes retorna respostas padronizadas nas APIs
>>>>>>> 1779d6b (teste Korp Guilherme)
