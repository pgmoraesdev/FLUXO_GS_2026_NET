# FLUXO — roteiro de demonstração

## Preparação

Configure a conexão Oracle e aplique as migrations conforme o README. A migration `RemoveEntidadesHistoricas` remove as quatro tabelas antigas de medições, paradas e alertas, com seus registros. Confira o schema antes de executá-la.

Inicie a API na pasta do projeto.

```powershell
dotnet run --launch-profile http
```

Importe `Fluxo.demonstracao.postman_collection.json` no Postman. Confira a variável `baseUrl`, configurada inicialmente como `http://localhost:5248`.

1. Execute a pasta **Preparação**. Ela cadastra uma estação, uma plataforma, um trem e três vagões.
2. Preserve as variáveis `demo...` preenchidas pelas respostas. No Runner, habilite a opção de manter os valores das variáveis ao terminar, quando disponível.
3. Execute a pasta **Apresentação** para consultar a lotação e comparar a demanda.
4. Se passarem cinco minutos, execute **Atualizar ocupação** e repita as consultas.
5. Ao terminar, execute **Limpeza**. A execução da coleção inteira também inclui essa limpeza.

Faça a limpeza antes de repetir a preparação para continuar usando os IDs da execução correta.

## Apresentação sugerida — aproximadamente quatro minutos

### 1. Explicar o problema e a proposta

> Em uma estação lotada, o passageiro tem dificuldade para saber em qual vagão há mais espaço. A companhia também precisa acompanhar quantas pessoas estão esperando. O FLUXO recebe contagens e transforma esses dados em informação para orientar o embarque e apoiar a avaliação de reforço do serviço.

Explique que os números apresentados são simulados e enviados pelo Postman. O protótipo entrega a API, com persistência no Oracle. As respostas representam as informações que poderão aparecer nos visores e em uma interface da operadora.

### 2. Mostrar a informação ao passageiro

Execute **13 — Mostrar sinalização dos três vagões**, na pasta **Apresentação**.

| Vagão | Capacidade | Pessoas | Situação | Cor |
| --- | --- | --- | --- | --- |
| 1 | 100 | 30 | Com espaço | Verde |
| 2 | 100 | 70 | Ocupado | Amarelo |
| 3 | 100 | 90 | Muito cheio | Vermelho |

> O passageiro consegue identificar o vagão com mais espaço. A resposta combina texto e cor para comunicar a lotação.

Mostre `ocupacaoEstimada`, `percentualOcupacao`, `situacao` e `cor`. A contagem vem de `QuantidadePessoas`, e `medidaEm` corresponde ao horário `AtualizadoEm` do vagão.

### 3. Mostrar a avaliação da operadora

Execute **Comparar plataforma e trem — 300 pessoas e 110 lugares atuais**.

O cálculo usa as contagens atuais dos três vagões. Há 70 lugares livres no primeiro, 30 no segundo e 10 no terceiro. O total é de 110 lugares disponíveis para 300 pessoas esperando.

| Campo da resposta | Resultado |
| --- | --- |
| `quantidadePessoas` | 300 |
| `capacidadeDisponivel` | 110 |
| `pessoasSemEspaco` | 190 |
| `avaliarReforco` | `true` |

> Pelas contagens atuais, 190 pessoas podem ficar sem espaço nesse trem. A API indica que a companhia deve avaliar um reforço. A decisão operacional fica com a companhia.

A consulta recebe explicitamente o ID do trem e o da plataforma. A comparação é calculada no momento da requisição.

### 4. Mostrar uma atualização e uma validação

Para demonstrar um vagão vazio, envie `PUT /api/Vagao/{id}` com o ID do primeiro vagão e os campos completos abaixo. Use o ID real do trem.

```json
{
  "tremId": 1,
  "posicao": 1,
  "capacidadePessoas": 100,
  "quantidadePessoas": 0
}
```

O esperado é `204 No Content`. Consulte a lotação para mostrar zero pessoas com situação **Com espaço**. Ao enviar `quantidadePessoas: null`, a consulta passa a mostrar **Lotação indisponível**. O horário é preenchido automaticamente quando há uma contagem.

Uma quantidade negativa no `PUT` retorna `400` com mensagem JSON. Para retomar o cenário inicial, execute **Atualizar ocupação**.

## Cenário alternativo de capacidade

Na coleção principal `Fluxo.postman_collection.json`, a pasta **Avaliação simplificada de capacidade** prepara outro cenário. Execute-a na sequência da coleção principal, que preenche os IDs usados pelas requisições.

Com capacidades de 100 e ocupações de 30, 40 e 50, o trem tem 180 lugares disponíveis. Para 300 pessoas esperando, a API retorna 120 pessoas sem espaço. Com a plataforma atualizada para 100 pessoas, ela retorna zero pessoas sem espaço e `avaliarReforco: false`.

Os números deste cenário são diferentes das ocupações de 30, 70 e 90 usadas na apresentação das três faixas de lotação.

## Conferência antes da apresentação

- A API inicia e o Oracle está acessível com as migrations aplicadas.
- O cadastro retorna `201`, alteração e exclusão retornam `204`.
- Os dados permanecem após encerrar e iniciar a API com a mesma conexão.
- Um ID inexistente retorna `404` e uma quantidade negativa retorna `400` em JSON.
- As três faixas aparecem na ordem das posições dos vagões.
- O cenário principal retorna 300 pessoas, 110 lugares e 190 pessoas sem espaço.
- Uma contagem `null` ou de mais de cinco minutos gera **Lotação indisponível**.
- A avaliação de capacidade exige leituras válidas da plataforma e de todos os vagões.
- A pasta **Limpeza** remove os registros da demonstração.

## Benefício e evolução

> O benefício proposto ao passageiro é ter informação para escolher onde embarcar. Para a operadora, é ter uma comparação entre demanda e espaço disponível. Um piloto futuro poderá medir a distribuição entre vagões e a quantidade de pessoas que permanecem na plataforma após cada partida.

Apresente esses benefícios como objetivos a validar. Os resultados numéricos da demonstração são cálculos de um cenário simulado.
