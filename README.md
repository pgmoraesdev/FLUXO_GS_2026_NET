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

O comando `dotnet ef database update` aplica as migrations pendentes. Elas criam as tabelas de estações, plataformas, trens, vagões e paradas programadas.

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

## Paradas programadas e próxima chegada

Uma parada representa uma passagem específica de um trem por uma plataforma.
O mesmo trem pode ter várias paradas na mesma plataforma, em horários diferentes.
Após atualizar o código, aplique a migration `AddParadasProgramadas` no seu
schema de teste com `dotnet ef database update`.

`/api/ParadaProgramada` aceita `GET` de lista e `POST`; com `/{id}`, aceita
`GET`, `PUT` e `DELETE`. Exemplo de corpo para cadastro e alteração:

```json
{
  "tremId": 1,
  "plataformaId": 1,
  "chegadaPrevistaEm": "2026-11-18T15:00:00Z",
  "chegadaRealEm": null
}
```

Use IDs existentes e ajuste a data para o futuro ao testar a próxima chegada.
Os horários devem ser enviados em UTC, com sufixo `Z`. A chegada prevista é
obrigatória, e a real é opcional. Horários sem fuso ou IDs não positivos retornam
`400` JSON. Trem, plataforma ou parada inexistente retorna `404` JSON.
O cadastro retorna `201` com `Location`; alteração e exclusão retornam `204`.
O `PUT` substitui os quatro campos; envie `null` para limpar a chegada real.

`GET /api/Plataforma/{id}/proxima-chegada` retorna a parada com menor horário
previsto maior ou igual ao UTC atual e sem chegada real registrada. Em empate,
usa o menor ID. Retorna `404` se a plataforma não existir ou não tiver chegada
futura pendente. No MVP, uma parada atrasada cujo horário previsto já passou
não aparece nessa consulta; atualize sua previsão pelo `PUT` quando necessário.

A resposta contém `id`, `tremId`, `plataformaId`, `chegadaPrevistaEm` e
`chegadaRealEm`. Os horários retornam com `Z`; o Oracle os guarda como UTC em
`TIMESTAMP`, sem fuso. A listagem geral é ordenada por previsão e ID.

Paradas impedem a exclusão do trem, da plataforma e da estação vinculada,
retornando `409` JSON. Exclua as paradas antes desses cadastros. A coleção
Postman inclui duas passagens, consulta da próxima chegada e avanço para a
seguinte após registrar a chegada real. A limpeza respeita os vínculos.

## Ocupação simulada da plataforma

`MedicaoPlataforma` guarda a quantidade estimada de pessoas presentes na área de
embarque. Essa medida é separada das entradas e saídas nos acessos da estação.
Não é necessário enviar imagens ou vídeos: nesta etapa os dados são simulados.

Após atualizar o código, aplique a migration `AddMedicoesPlataforma` no seu
schema de teste com `dotnet ef database update`.

Exemplo de `POST /api/MedicaoPlataforma` (use uma plataforma existente e ajuste
`medidaEm` para um horário UTC atual ou passado):

```json
{
  "plataformaId": 1,
  "quantidadePessoas": 300,
  "medidaEm": "2026-11-17T15:00:00Z",
  "origem": "Simulacao"
}
```

O cadastro retorna `201`, o cabeçalho `Location` e a medição com seu `id`.
A quantidade é obrigatória e pode ser zero; valores negativos são inválidos.
O horário é obrigatório, deve usar UTC com sufixo `Z` e não pode estar no futuro.
A origem é obrigatória, tem até 50 caracteres e é salva sem espaços nas pontas.
Entrada inválida retorna `400` JSON; plataforma inexistente retorna `404` JSON.

Rotas disponíveis:

| Método | Rota | Resultado no sucesso |
|---|---|---|
| GET | `/api/MedicaoPlataforma` | `200`, histórico do horário mais recente ao mais antigo; `[]` se vazio |
| GET | `/api/MedicaoPlataforma/{id}` | `200`, medição por ID |
| POST | `/api/MedicaoPlataforma` | `201`, cadastra uma leitura |
| PUT | `/api/MedicaoPlataforma/{id}` | `204`, corrige a leitura com os mesmos quatro campos |
| DELETE | `/api/MedicaoPlataforma/{id}` | `204`, exclui a leitura |
| GET | `/api/Plataforma/{id}/ultima-ocupacao` | `200`, última leitura daquela plataforma |

As operações por ID retornam `404` se a medição não existir. A consulta da última
ocupação também retorna `404` se a plataforma não existir ou não tiver medições.
Ela usa o maior `medidaEm` que não esteja no futuro e, em empate, o maior ID.
Assim, enviar uma leitura antiga depois não muda qual é a mais recente.
A resposta inclui o horário em UTC. Ela representa a última leitura registrada,
não garante uma ocupação atual: o prazo de validade para visores e alertas será
definido na etapa seguinte. Dados antigos continuam disponíveis no histórico.

Uma plataforma com medições não pode ser excluída (resposta `409`), nem sua
estação. Exclua as medições antes desses cadastros. A coleção Postman inclui o
cenário de 300 pessoas, a correção para 280, a consulta da última ocupação,
validação de quantidade negativa e limpeza na ordem dos relacionamentos.
