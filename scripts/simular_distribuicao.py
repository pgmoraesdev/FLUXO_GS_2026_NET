"""Comparação didática de distribuição entre vagões. Não acessa a API ou o banco."""

CAPACIDADES = [100, 100, 100]
OCUPACAO_APOS_DESEMBARQUE = [30, 40, 50]
PASSAGEIROS = 90

# Hipótese de concentração no primeiro vagão, escolhida para este exemplo.
SEM_INDICACAO = [70, 15, 5]


def distribuir_com_indicacao():
    ocupacoes = OCUPACAO_APOS_DESEMBARQUE.copy()
    embarques = [0, 0, 0]

    for _ in range(PASSAGEIROS):
        disponiveis = [i for i in range(3) if ocupacoes[i] < CAPACIDADES[i]]
        if not disponiveis:
            break

        # Cada passageiro escolhe a menor ocupação percentual.
        # Em empate, escolhe a menor posição do vagão.
        escolhido = min(disponiveis, key=lambda i: (ocupacoes[i] / CAPACIDADES[i], i))
        ocupacoes[escolhido] += 1
        embarques[escolhido] += 1

    return embarques


def mostrar(nome, embarques):
    final = [OCUPACAO_APOS_DESEMBARQUE[i] + embarques[i] for i in range(3)]
    percentuais = [100 * final[i] / CAPACIDADES[i] for i in range(3)]
    print(f"\n{nome}")
    print("Vagão | Base | Embarques | Ocupação final")
    for i in range(3):
        print(f"{i + 1:5} | {OCUPACAO_APOS_DESEMBARQUE[i]:4} | {embarques[i]:9} | {final[i]:3} ({percentuais[i]:.0f}%)")
    print(f"Embarcaram: {sum(embarques)}; ficaram: {PASSAGEIROS - sum(embarques)}")
    print(f"Diferença entre maior e menor ocupação: {max(percentuais) - min(percentuais):.0f} pontos percentuais")


def main():
    com_indicacao = distribuir_com_indicacao()
    assert sum(SEM_INDICACAO) == PASSAGEIROS
    assert sum(com_indicacao) == PASSAGEIROS
    assert all(OCUPACAO_APOS_DESEMBARQUE[i] + SEM_INDICACAO[i] <= CAPACIDADES[i] for i in range(3))

    print("FLUXO — SIMULAÇÃO DIDÁTICA, NÃO É RESULTADO DE CAMPO")
    print("Cenário separado do alerta 300/180: aqui há 90 passageiros e 180 lugares.")
    mostrar("Sem indicação (distribuição hipotética)", SEM_INDICACAO)
    mostrar("Com indicação (escolha idealizada da menor ocupação)", com_indicacao)
    print("\nHipóteses: todos seguem a indicação, contagens exatas e redistribuição imediata.")
    print("Não modela deslocamento, preferências, acessibilidade ou tempo de embarque.")
    print("Ambos embarcam 90 pessoas; o resultado demonstra distribuição, não aumento de capacidade.")


if __name__ == "__main__":
    main()
