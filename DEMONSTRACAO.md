# FLUXO — roteiro de demonstração

## Cenário simplificado — preparação dos passos 1 e 2

O roteiro final será reduzido para uma estação, uma plataforma, um trem e três
vagões. Nesta etapa, cadastre a estação e o trem pelas rotas existentes. Em
`POST /api/Plataforma`, envie também `quantidadePessoas: 300`. Cadastre três
vagões de capacidade 100, nas posições 1, 2 e 3, com `quantidadePessoas` igual
a 30, 70 e 90, respectivamente. Use os exemplos completos do README e os IDs
retornados pela API.

Consulte os registros com `GET` para mostrar as contagens e o horário automático.
Use `PUT` com os campos completos para substituir uma contagem, mostrar um
vagão vazio com zero ou representar ausência de leitura com `null`.

As próximas etapas adaptarão a classificação de lotação e a comparação com o
espaço do trem para esses campos. O roteiro histórico abaixo continua usando
as medições e paradas enquanto essa adaptação está em andamento.

Todos os dados deste roteiro são **simulados**. A API não está conectada a
câmeras, sensores ou à operação de uma linha real. O objetivo é demonstrar como
contagens podem apoiar o passageiro e a avaliação do Centro de Controle
Operacional (CCO).

## Preparar a API

1. Configure sua conexão Oracle fora do código, conforme o [README](README.md).
2. No seu schema de teste, aplique as migrations e inicie a API:

```powershell
dotnet restore Fluxo_GS_2026_2TDSPR.csproj
dotnet ef database update --project Fluxo_GS_2026_2TDSPR.csproj
dotnet run --project Fluxo_GS_2026_2TDSPR.csproj --launch-profile http
```

3. Importe `Fluxo.demonstracao.postman_collection.json` no Postman.
4. Ajuste `baseUrl` para a URL exibida no terminal; o padrão é
   `http://localhost:5248`.
5. Execute a pasta **Preparação** uma vez. Ela cria uma estação, uma plataforma,
   um trem, três vagões, uma parada futura e quatro medições. Os IDs ficam nas
   variáveis `demo...` da coleção. Se usar o Runner por pasta, mantenha a opção
   de guardar os valores das variáveis ao terminar para continuar a apresentação
   e fazer a limpeza com os mesmos IDs.
6. Execute a pasta **Apresentação**, na ordem. Abra as respostas dos passos
   indicados abaixo para explicar os resultados.

As medições valem por cinco minutos. Se a demonstração demorar, execute a pasta
**Atualizar medições** e repita as consultas. Essa pasta atualiza os registros
existentes; não cadastra outros e não reabre o alerta encerrado. Para apresentar
novamente a criação de um alerta `201`, limpe a execução anterior e comece uma
nova Preparação.

Não reinicie a Preparação antes de limpar a execução anterior: ela substitui os
IDs guardados na coleção. Cada execução usa nomes e código de trem próprios.

## Apresentação sugerida — aproximadamente quatro minutos

### 1. Problema e cenário

> O passageiro precisa saber onde há espaço antes de embarcar. A operadora
> precisa estimar se o próximo trem atende às pessoas esperando. O FLUXO usa
> contagens para informar a lotação e apoiar a avaliação do CCO.

Mostre a consulta **12 — Consultar a próxima chegada**. Existe uma passagem
específica de um trem com três vagões pela plataforma. O horário é gerado uma
hora à frente durante a preparação.

### 2. Informação ao passageiro

Abra **13 — Mostrar sinalização dos três vagões**. As respostas representam a
informação destinada aos visores externos, com texto e cor:

| Visor simulado | Ocupação | Texto | Cor |
|---|---:|---|---|
| Vagão 1 | 30 de 100 | Com espaço | Verde |
| Vagão 2 | 70 de 100 | Ocupado | Amarelo |
| Vagão 3 | 90 de 100 | Muito cheio | Vermelho |

> A informação permite identificar o vagão com mais espaço. Texto acompanha
> a cor para que a leitura não dependa apenas da percepção das cores.

Os limites de 50% e 80% e a validade de cinco minutos são escolhas do protótipo.
Leitura ausente ou antiga retorna “Lotação indisponível”, sem apresentar zero
como se o vagão estivesse vazio. Não existe ainda uma tela de visor físico;
a demonstração mostra os dados que uma interface consumiria.

### 3. Avaliação da operadora

Abra **14 — Avaliar demanda e abrir alerta**. O cenário é:

| Vagão | Capacidade | Ocupação atual | Desembarques previstos | Ocupação após desembarque | Espaço |
|---|---:|---:|---:|---:|---:|
| 1 | 100 | 30 | 0 | 30 | 70 |
| 2 | 100 | 70 | 30 | 40 | 60 |
| 3 | 100 | 90 | 40 | 50 | 50 |
| Total | 300 | 190 | 70 | 120 | 180 |

A plataforma tem **300 pessoas** estimadas. Após os desembarques previstos,
o trem terá **180 lugares disponíveis**. Portanto, **cerca de 120 pessoas
podem ficar para a próxima viagem**.

> No MVP, usamos a contagem atual da plataforma como projeção de demanda para
> a chegada. Não estimamos novas entradas e saídas nesse intervalo. Os
> desembarques futuros são informados manualmente na simulação, separados dos
> desembarques já registrados nas medições.

Mostre a resposta `201`, o `alertaId` e os números. Execute **15 — Reavaliar sem
duplicar alerta**: deve retornar `200` com o mesmo ID. Mostre **16 — Consultar
os números da abertura**: esses números ficam preservados para revisão.

### 4. Decisão humana

Execute **17 — Operador registra avaliação** e mostre a consulta **18**:

```json
{
  "status": "EmAnalise",
  "acaoOperador": "Solicitada avaliação de trem reserva"
}
```

> O alerta apoia a decisão. A solicitação de reforço depende do operador;
> a API não envia um trem reserva automaticamente.

Finalize com **19 — Operador encerra o alerta** e **20 — Confirmar que a avaliação
não reabre o alerta**. O status permanece `Encerrado` mesmo com uma nova
avaliação que ainda aponte risco. A ação descrita é um registro simulado,
não uma solicitação enviada a outro sistema.

## Comparação simulada: distribuição com e sem indicação

Esta comparação é **outro cenário**, pois, com 300 pessoas disputando 180 lugares,
a sinalização não cria capacidade adicional. Para observar a distribuição,
usamos 90 passageiros e a mesma ocupação após desembarque: `[30, 40, 50]`, com
capacidade 100 em cada vagão.

- **Sem indicação:** adotamos como hipótese 70 embarques no primeiro vagão,
  15 no segundo e 5 no terceiro. É uma distribuição escolhida para o exemplo,
  não observada em campo.
- **Com indicação:** um algoritmo idealizado encaminha cada passageiro ao vagão
  com menor ocupação percentual. Em empate, usa a menor posição. Assume adesão
  de todos e redistribuição imediata.

| Cenário | Embarques por vagão | Ocupação final | Diferença entre maior e menor ocupação |
|---|---|---|---:|
| Sem indicação, hipótese | 70 / 15 / 5 | 100% / 55% / 55% | 45 pontos percentuais |
| Com indicação, idealizado | 40 / 30 / 20 | 70% / 70% / 70% | 0 pontos percentuais |

**Nos dois casos embarcam 90 pessoas e ninguém fica.** O resultado ilustra
uma distribuição mais equilibrada sob essas hipóteses. Não demonstra aumento
de capacidade, redução real de espera ou eficácia comprovada da sinalização.
O modelo não contempla deslocamento, preferências, restrições de acessibilidade,
tempo de embarque ou erro das contagens.

Para reproduzir as contas, opcionalmente use Python 3:

```powershell
python scripts/simular_distribuicao.py
```

Esse script didático não acessa nem altera a API ou o banco. Python não é uma
dependência da API ASP.NET; o roteiro principal funciona pelo Postman.

## Limpeza e execução automática

Ao terminar, execute a pasta **Limpeza**. Ela remove somente os registros cujos
IDs foram guardados nesta execução, na ordem dos relacionamentos: alerta,
medições, parada, vagões, trem, plataforma e estação. O último `GET` deve
retornar `404` para a estação da demonstração.

Executar a coleção inteira também executa a limpeza. Para manter os registros
para consultas manuais, execute somente Preparação e Apresentação.

Se usar Newman (opcional, com Node.js instalado), na pasta do projeto:

```powershell
npx newman@6.2.1 run Fluxo.demonstracao.postman_collection.json --env-var "baseUrl=http://localhost:5248" --bail
```

Esse comando executa o roteiro completo e a limpeza. A coleção não contém
credenciais Oracle: ela chama a API que você já iniciou e configurou.

## O que ainda precisa ser conferido na entrega

- Aplicar e validar as migrations e o roteiro no Oracle da FIAP. A verificação
  na nuvem desta etapa usa SQLite temporário e não substitui essa validação.
- Ensaiar a apresentação e conferir o material completo da rubrica da disciplina.
- Não apresentar câmeras ao vivo, contagem de acessos da estação, interface de
  visor ou painel do CCO como funcionalidades já entregues. O fluxo atual usa
  contagens simuladas, respostas da API e registro da ação do operador.

## Verificação deste roteiro

Em 17/11/2026, a coleção completa foi executada pelo Newman 6.2.1 contra os
controllers reais em um host temporário com SQLite em memória: **37 requisições
e 56 verificações, sem falhas**, incluindo a limpeza e o `404` final.
O script da distribuição reproduziu os valores das duas linhas da tabela.
Não houve alteração nos controllers ou nas regras da API nesta etapa.
Essa verificação não aplicou migrations nem acessou o Oracle.
