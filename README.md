# FLUXO_GS_2026_NET

API da Global Solution 2026/2 da FIAP para um protótipo de monitoramento da lotação no transporte ferroviário. O projeto usa ASP.NET Core 8, Entity Framework Core e Oracle.

## Como executar

Na pasta que contém o arquivo `Fluxo_GS_2026_2TDSPR.csproj`, tenha o SDK do .NET 8 e acesso ao Oracle da FIAP. Se o comando `dotnet ef` ainda estiver indisponível, instale a ferramenta:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.33
```

Configure a conexão localmente com User Secrets. Substitua os valores do exemplo pelos dados da sua conta Oracle:

```powershell
dotnet user-secrets set "ConnectionStrings:OracleConnection" "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=HOST:PORTA/SERVICO"
dotnet restore
dotnet ef database update
dotnet run
```

O comando `dotnet ef database update` aplica as migrations pendentes. Elas criam as tabelas de estações, plataformas, trens e vagões.

## Como testar no Postman

Use como base a URL mostrada no terminal quando a API iniciar. As rotas de cadastro disponíveis são `/api/Estacao`, `/api/Plataforma`, `/api/Trem` e `/api/Vagao`. Todas aceitam `GET` e `POST` na rota base, e `GET`, `PUT` e `DELETE` com `/{id}`.

1. Envie `POST /api/Estacao` com `{"nome":"Estação Central"}` e copie o ID retornado.
2. Envie `POST /api/Plataforma` com o JSON abaixo, usando o ID da estação. O esperado é `201 Created`.
3. Use o ID da plataforma retornado para consultá-la com `GET`, alterá-la com `PUT` ou removê-la com `DELETE`. Para o `PUT`, envie os mesmos campos com os valores atualizados. `PUT` e `DELETE` retornam `204 No Content` em caso de sucesso.

```json
{
  "nome": "Plataforma 1",
  "sentido": "Terminal Norte",
  "estacaoId": 1
}
```

A documentação interativa também está disponível em `/scalar/v1` no ambiente de desenvolvimento.

## Composição com três vagões

Cadastre um trem com `POST /api/Trem`, enviando `{"codigo":"FLUXO-01"}`.
Use o ID retornado como `tremId` em três requisições `POST /api/Vagao`,
com posições 1, 2 e 3. Exemplo para o primeiro vagão:

```json
{
  "tremId": 1,
  "posicao": 1,
  "capacidadePessoas": 100
}
```

O cadastro retorna `201 Created`, o cabeçalho `Location` e os campos `id`,
`tremId`, `posicao` e `capacidadePessoas`. Consulte os vagões em `GET /api/Vagao`
ou um registro em `GET /api/Vagao/{id}`. A listagem inclui todos os trens;
use `tremId` da resposta para identificar a composição.

Para alterar um vagão, envie os mesmos três campos em `PUT /api/Vagao/{id}`.
Para excluir, use `DELETE /api/Vagao/{id}`. Ambos retornam `204 No Content`.
Os campos de entrada devem ser inteiros maiores que zero; valores inválidos
retornam `400` em JSON. Trem ou vagão inexistente retorna `404` em JSON.
Repetir a posição dentro do mesmo trem retorna `409` em JSON, inclusive no `PUT`.
A mesma posição pode existir em trens diferentes. A exclusão de um trem remove
seus vagões em cascata, conforme a migration existente.

Importe `Fluxo.postman_collection.json` e execute a coleção na ordem exibida.
Ela cria um trem com três vagões, consulta o vínculo, verifica capacidade
inválida e posição duplicada no cadastro e na alteração, altera a capacidade,
exclui os vagões e confirma `404` após a exclusão. A limpeza do trem ocorre
por último. Execute somente no banco/schema de teste confirmado.

Não é necessária uma nova migration para esse CRUD: a entidade, a chave
estrangeira e o índice único `(TremId, Posicao)` já estão versionados.

## Contrato de estação

`POST /api/Estacao` e `PUT /api/Estacao/{id}` recebem apenas os dados
`{"nome":"Estação Central"}`. O nome é obrigatório, deve ter até 2000 caracteres
e é salvo sem espaços nas pontas. Nome ausente, nulo, vazio, somente com espaços
ou acima do limite retorna `400` com erros de validação em JSON.

As respostas de cadastro e consulta de estação contêm apenas `id` e `nome`.
Campos adicionais de entrada, como `id` e `plataformas`, não são usados para
cadastrar a estação; as plataformas são cadastradas pela rota própria.
A listagem retorna `200` com `[]` quando não existem estações, e os registros
são ordenados por ID. Recurso inexistente retorna `404` em JSON Problem Details.
O cadastro retorna `201` com `Location`; alteração e exclusão retornam `204`.

A coleção Postman inclui verificações de nome inválido no cadastro e na alteração.
Essas mudanças não exigem migration: o limite segue a coluna Oracle existente.
