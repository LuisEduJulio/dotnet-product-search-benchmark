# Resultado do benchmark de busca: Elasticsearch vs MongoDB

Gerado pelo GitHub Actions em 2026-11-05 12:45 UTC. Nenhum número foi editado à mão.

## Teste de carga

### Ambiente

- **Data (UTC):** 2026-11-05 12:41
- **Máquina:** Ubuntu 26.04.1 LTS X64, 4 núcleos lógicos, .NET 8.0.33
- **Motores:** Elasticsearch 8.15.3 (heap 1 GB) e MongoDB 7.0.45 (cache WiredTiger 1 GB), na mesma máquina
- **Catálogo:** 100.000 produtos fictícios de farmácia, gerados com semente fixa
- **Carga:** 2.000 chamadas por cenário e motor, 50 simultâneas, página de 20 itens

### Indexação (carga inicial do catálogo)

| Loja | Produtos | Tempo | Produtos/s |
|---|---:|---:|---:|
| Elasticsearch | 100.000 | 6,08 s | 16.436 |
| MongoDB | 100.000 | 4,59 s | 21.763 |

### Busca sob carga

Precisão: quantos dos itens devolvidos são a resposta certa. Cobertura: quanto da página de 20 foi preenchida com respostas certas.

#### Termo exato: "dipirona"

| Motor | req/s | p50 | p95 | p99 | Itens | Precisão | Cobertura | Erros |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Elasticsearch | 323 | 146,09 ms | 220,13 ms | 262,39 ms | 20 | 100% | 100% | 0 |
| MongoText | 207 | 228,77 ms | 424,29 ms | 510,41 ms | 20 | 100% | 100% | 0 |
| MongoRegex | 1.164 | 30,70 ms | 99,18 ms | 285,67 ms | 20 | 100% | 100% | 0 |

- MongoRegex atendeu 3,6x as requisições por segundo do Elasticsearch.
- MongoRegex atendeu 5,6x as requisições por segundo do MongoText.

#### Dois termos: "dipirona gotas"

| Motor | req/s | p50 | p95 | p99 | Itens | Precisão | Cobertura | Erros |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Elasticsearch | 285 | 168,27 ms | 272,52 ms | 319,74 ms | 20 | 100% | 100% | 0 |
| MongoText | 48 | 1.032,19 ms | 1.320,93 ms | 1.482,25 ms | 20 | 100% | 100% | 0 |
| MongoRegex | 839 | 56,11 ms | 125,78 ms | 143,47 ms | 20 | 100% | 100% | 0 |

- MongoRegex atendeu 2,9x as requisições por segundo do Elasticsearch.
- MongoRegex atendeu 17,6x as requisições por segundo do MongoText.

#### Sem acento: "acido acetilsalicilico"

| Motor | req/s | p50 | p95 | p99 | Itens | Precisão | Cobertura | Erros |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Elasticsearch | 188 | 236,51 ms | 440,32 ms | 585,64 ms | 20 | 100% | 100% | 0 |
| MongoText | 182 | 293,67 ms | 414,88 ms | 454,57 ms | 20 | 100% | 100% | 0 |
| MongoRegex | 1.630 | 25,69 ms | 69,06 ms | 89,67 ms | 20 | 100% | 100% | 0 |

- MongoRegex atendeu 8,7x as requisições por segundo do Elasticsearch.
- MongoRegex atendeu 8,9x as requisições por segundo do MongoText.

#### Erro de digitação: "dipirna"

| Motor | req/s | p50 | p95 | p99 | Itens | Precisão | Cobertura | Erros |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Elasticsearch | 630 | 71,96 ms | 127,09 ms | 158,48 ms | 20 | 100% | 100% | 0 |
| MongoText | 6.856 | 3,70 ms | 11,22 ms | 17,85 ms | 0 | 0% | 0% | 0 |
| MongoRegex | 26 | 1.930,92 ms | 2.158,10 ms | 2.522,72 ms | 0 | 0% | 0% | 0 |

- MongoText atendeu 10,9x as requisições por segundo do Elasticsearch.
- MongoText atendeu 264,7x as requisições por segundo do MongoRegex.

#### Autocomplete (prefixo): "paracet"

| Motor | req/s | p50 | p95 | p99 | Itens | Precisão | Cobertura | Erros |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Elasticsearch | 539 | 85,06 ms | 153,42 ms | 185,39 ms | 20 | 100% | 100% | 0 |
| MongoText | 6.499 | 4,35 ms | 11,11 ms | 16,14 ms | 0 | 0% | 0% | 0 |
| MongoRegex | 2.015 | 21,47 ms | 50,61 ms | 71,74 ms | 20 | 100% | 100% | 0 |

- MongoText atendeu 12,0x as requisições por segundo do Elasticsearch.
- MongoText atendeu 3,2x as requisições por segundo do MongoRegex.

#### Termo + filtro de categoria: "vitamina c" + categoria "Vitaminas e Suplementos"

| Motor | req/s | p50 | p95 | p99 | Itens | Precisão | Cobertura | Erros |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Elasticsearch | 391 | 126,34 ms | 191,78 ms | 262,20 ms | 20 | 100% | 100% | 0 |
| MongoText | 76 | 667,00 ms | 805,95 ms | 875,62 ms | 20 | 100% | 100% | 0 |
| MongoRegex | 4.880 | 6,55 ms | 15,49 ms | 19,97 ms | 20 | 45% | 45% | 0 |

- MongoRegex atendeu 12,5x as requisições por segundo do Elasticsearch.
- MongoRegex atendeu 64,6x as requisições por segundo do MongoText.


## Latência por chamada, sem concorrência (BenchmarkDotNet)

| Method | Engine        | Scenario       | Mean        | Error       | StdDev      | Allocated |
|------- |-------------- |--------------- |------------:|------------:|------------:|----------:|
| **Search** | **Elasticsearch** | **com-filtro**     |  **5,348.2 μs** |   **314.93 μs** |   **294.58 μs** |  **29.24 KB** |
| **Search** | **Elasticsearch** | **dois-termos**    |  **4,468.0 μs** |    **74.16 μs** |    **65.74 μs** |  **29.29 KB** |
| **Search** | **Elasticsearch** | **erro-digitacao** |  **3,736.9 μs** |    **42.48 μs** |    **35.48 μs** |  **29.13 KB** |
| **Search** | **Elasticsearch** | **exato**          |  **3,967.4 μs** |    **74.03 μs** |    **65.62 μs** |  **29.15 KB** |
| **Search** | **Elasticsearch** | **prefixo**        |  **4,492.3 μs** |    **56.46 μs** |    **44.08 μs** |  **28.55 KB** |
| **Search** | **Elasticsearch** | **sem-acento**     | **11,284.7 μs** |   **188.09 μs** |   **157.06 μs** |  **30.14 KB** |
| **Search** | **MongoText**     | **com-filtro**     | **24,154.4 μs** |   **839.87 μs** |   **744.53 μs** |  **75.26 KB** |
| **Search** | **MongoText**     | **dois-termos**    | **34,757.9 μs** | **1,079.22 μs** |   **842.58 μs** |  **71.24 KB** |
| **Search** | **MongoText**     | **erro-digitacao** |    **513.7 μs** |     **2.97 μs** |     **2.48 μs** |  **47.54 KB** |
| **Search** | **MongoText**     | **exato**          |  **7,191.9 μs** |   **394.08 μs** |   **349.34 μs** |  **71.09 KB** |
| **Search** | **MongoText**     | **prefixo**        |    **545.7 μs** |    **54.21 μs** |    **48.05 μs** |  **47.54 KB** |
| **Search** | **MongoText**     | **sem-acento**     |  **9,172.8 μs** |   **790.22 μs** |   **739.17 μs** |  **71.76 KB** |
| **Search** | **MongoRegex**    | **com-filtro**     |    **645.2 μs** |    **29.15 μs** |    **25.84 μs** |  **86.41 KB** |
| **Search** | **MongoRegex**    | **dois-termos**    |  **2,602.6 μs** |    **28.33 μs** |    **23.65 μs** |  **77.98 KB** |
| **Search** | **MongoRegex**    | **erro-digitacao** | **86,034.3 μs** | **1,896.06 μs** | **1,583.30 μs** |  **50.68 KB** |
| **Search** | **MongoRegex**    | **exato**          |  **1,290.4 μs** |    **20.98 μs** |    **17.52 μs** |  **73.18 KB** |
| **Search** | **MongoRegex**    | **prefixo**        |  **1,142.0 μs** |    **10.45 μs** |     **8.73 μs** |  **76.42 KB** |
| **Search** | **MongoRegex**    | **sem-acento**     |  **1,347.6 μs** |    **68.29 μs** |    **60.54 μs** |  **78.68 KB** |

## Testes automatizados

| Suíte | Total | Passaram | Falharam |
|---|---:|---:|---:|
| integration | 19 | 19 | 0 |
| unit | 24 | 24 | 0 |
| **Total** | **43** | **43** | **0** |
