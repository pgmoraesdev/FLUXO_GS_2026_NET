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
não garante uma ocupação atual: a sinalização de vagões usa a validade de cinco minutos descrita abaixo;
a ocupação da plataforma continua disponível como leitura histórica. Dados antigos continuam disponíveis no histórico.

Uma plataforma com medições não pode ser excluída (resposta `409`), nem sua
estação. Exclua as medições antes desses cadastros. A coleção Postman inclui o
cenário de 300 pessoas, a correção para 280, a consulta da última ocupação,
validação de quantidade negativa e limpeza na ordem dos relacionamentos.

## Ocupação simulada dos vagões

`MedicaoVagao` registra a contagem de um vagão em um horário. A API calcula:

`ocupacaoEstimada = ocupacaoInicial + embarques - desembarques`

Cada leitura é independente: `ocupacaoInicial` é a base antes dos movimentos
informados naquela requisição. Para continuar uma contagem, envie a ocupação
estimada anterior como base da próxima leitura. A API não soma automaticamente
as leituras do histórico e não recalcula outras leituras ao corrigir uma delas.

Aplique a migration `AddMedicoesVagao` no seu schema de teste antes de usar as
rotas. Exemplo de `POST /api/MedicaoVagao` (ajuste o ID e o horário):

```json
{
  "vagaoId": 1,
  "paradaProgramadaId": null,
  "ocupacaoInicial": 40,
  "embarques": 15,
  "desembarques": 5,
  "medidaEm": "2026-11-17T18:00:00Z",
  "origem": "Simulacao"
}
```

O resultado desse exemplo é `ocupacaoEstimada: 50`. Esse campo é calculado pela
API, e não recebido como entrada. A resposta inclui também os dados da leitura
e seu ID. `POST` retorna `201` com `Location`.

O vínculo com a parada é opcional. Se informado, a parada deve existir e seu
trem deve ser o mesmo do vagão. Vínculo com trem diferente retorna `400` JSON;
vagão ou parada inexistente retorna `404` JSON. Sem parada, use `null` ou omita
`paradaProgramadaId`.

As três contagens são obrigatórias, aceitam zero e não podem ser negativas.
O resultado deve ficar entre zero e 2147483647. A contagem não é limitada à
capacidade cadastrada: trata-se de uma estimativa, e excesso de ocupação deve
continuar visível para a futura sinalização. Horário UTC com `Z` é obrigatório,
não pode estar no futuro, e origem deve conter de 1 a 50 caracteres não vazios
(a API remove espaços nas pontas). Dados inválidos retornam `400` JSON.

| Método | Rota | Resultado no sucesso |
|---|---|---|
| GET | `/api/MedicaoVagao` | `200`, histórico do mais recente ao mais antigo |
| GET | `/api/MedicaoVagao/{id}` | `200`, leitura por ID |
| POST | `/api/MedicaoVagao` | `201`, registra e calcula a ocupação |
| PUT | `/api/MedicaoVagao/{id}` | `204`, corrige os dados e recalcula a ocupação |
| DELETE | `/api/MedicaoVagao/{id}` | `204`, exclui a leitura |
| GET | `/api/Vagao/{id}/ultima-ocupacao` | `200`, última leitura daquele vagão |

Operações por ID retornam `404` para medição inexistente. A última ocupação
retorna `404` para vagão inexistente ou sem leitura, usa o maior `medidaEm` que
não esteja no futuro e, em empate, o maior ID. Retorna o horário com `Z`.
Assim como na plataforma, essa consulta expõe a última leitura registrada;
a rota `/lotacao`, descrita abaixo, considera indisponíveis leituras com mais
de cinco minutos e classifica a lotação. Alertas ainda não são gerados.

Vagão ou parada com medições não pode ser excluído (`409`). A exclusão do trem
também é protegida. Trocar o trem de um vagão com medições ligadas a paradas,
ou de uma parada com medições, retorna `409`; remova esses vínculos primeiro.
A coleção Postman registra leituras dos três vagões e remove as medições antes
das paradas e vagões. Os dados da coleção são simulações.

## Sinalização de lotação por vagão

Os endpoints abaixo transformam a última medição em informação para os futuros
visores externos. A resposta contém texto e cor; esta etapa entrega a regra
pela API, ainda sem uma tela de visor.

- `GET /api/Vagao/{id}/lotacao`: sinalização de um vagão.
- `GET /api/Trem/{id}/lotacao`: sinalização de todos os vagões, em ordem de posição.

Critérios escolhidos para a simulação acadêmica:

| Ocupação em relação à capacidade | Situação | Cor |
|---|---|---|
| Menos de 50% | Com espaço | Verde |
| De 50% a menos de 80% | Ocupado | Amarelo |
| 80% ou mais | Muito cheio | Vermelho |
| Sem leitura ou leitura com mais de 5 minutos | Lotação indisponível | Cinza |

Esses limites não são normas de operação ferroviária. O percentual é calculado
por `100 × ocupacaoEstimada / capacidadePessoas`, sem limitar o resultado a 100%.
A classificação usa o valor calculado, sem arredondamento.

A consulta considera o horário UTC atual. Leituras no futuro são ignoradas,
a leitura mais recente é escolhida por horário (maior ID em empate), e a
validade é de até cinco minutos. Capacidade não positiva ou ocupação negativa
em dados legados também resulta em indisponibilidade.

Exemplo de resposta para capacidade 100 e ocupação 40:

```json
{
  "vagaoId": 1,
  "posicao": 1,
  "capacidadePessoas": 100,
  "ocupacaoEstimada": 40,
  "percentualOcupacao": 40,
  "medidaEm": "2026-11-17T18:00:00Z",
  "situacao": "Com espaço",
  "cor": "Verde"
}
```

Sem leitura válida, a resposta continua sendo `200`, com situação
“Lotação indisponível”, cor “Cinza”, ocupação e percentual `null`. `medidaEm`
guarda o horário da última leitura passada, mesmo quando antiga, ou `null`
se não houver leitura. Assim, ausência de informação não é confundida com vagão
vazio. Vagão ou trem inexistente retorna `404` JSON; trem sem vagões retorna
`200` com `[]`.

Os endpoints `/ultima-ocupacao` continuam retornando a última medição histórica,
mesmo antiga. Para sinalização atual, use `/lotacao`. A classificação é feita na
consulta, não altera medições e não exige nova migration.

A pasta “Sinalização de lotação” no Postman consulta a composição, corrige uma
leitura para demonstrar “Com espaço”, simula uma leitura antiga para demonstrar
indisponibilidade e depois restaura o horário. Execute a coleção na ordem
completa; se demorar mais de cinco minutos após os cadastros, atualize as
medições para consultar as faixas atuais. Alertas operacionais ficam para a
próxima etapa.
