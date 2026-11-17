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

Use como base a URL mostrada no terminal quando a API iniciar. As rotas de cadastro disponíveis são `/api/Estacao` e `/api/Plataforma`. Ambas aceitam `GET` e `POST` na rota base, e `GET`, `PUT` e `DELETE` com `/{id}`.

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
