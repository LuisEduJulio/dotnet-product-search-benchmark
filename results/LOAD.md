# Teste de carga

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

