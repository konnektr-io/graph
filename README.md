# Konnektr Graph

### Semantic Graph for Postgres

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![Docs](https://img.shields.io/badge/docs-docs.konnektr.io-blue)](https://docs.konnektr.io/docs/graph)
[![GitHub stars](https://img.shields.io/github/stars/konnektr-io/graph?style=social)](https://github.com/konnektr-io/graph)

**Konnektr Graph** is a semantic graph engine for PostgreSQL, built on
[Apache AGE](https://age.apache.org/). It stores DTDL-modelled entities as graph vertices,
answers Cypher natively, and exposes an HTTP API that is **compatible with Azure Digital
Twins** — so existing ADT SDKs, query strings and payloads keep working.

It is self-hosted: you run it on your own Kubernetes cluster, against your own database.

---

## Why this exists

The graph is the product. "Digital twin" is a compatibility profile on top of it — one way
to use the same data model, not a separate product.

- **Azure Digital Twins compatible** — same API surface, same DTDL models, same `$dtId` /
  `$metadata.$model` payloads, same `SELECT … FROM DIGITALTWINS` query dialect. Existing
  .NET, Python, JavaScript and Java ADT clients connect unchanged.
- **A graph, not a document store** — entities and relationships are vertices and edges, so
  traversals are Cypher instead of application-side joins.
- **One database to operate** — AGE runs inside PostgreSQL, next to your relational data,
  with normal backups, replication and tooling.
- **Change streams you own** — state changes publish to Kafka, Azure Data Explorer or MQTT
  through configurable event sinks and routes.
- **Vocabulary-neutral surface** — the same resources answer on `/entities/*` as well as
  `/digitaltwins/*`, for domains that have nothing to do with twins.

---

## Quick start

```bash
helm install graph oci://ghcr.io/konnektr-io/charts/graph \
  --namespace graph --create-namespace \
  --set api.enabled=true \
  --set cluster.enabled=true \
  --set api.config.authentication.enabled=false
```

Then [follow the quickstart](https://docs.konnektr.io/docs/graph/getting-started/quickstart)
to upload a DTDL model, create a twin and query it.

Both `api.enabled` and `cluster.enabled` default to `false`, so a bare `helm install`
creates nothing. Pass `-f values.yaml` for anything real; the chart's `values.yaml` is the
authoritative reference.

---

## The Azure Digital Twins profile

| Surface | Behaviour |
|---|---|
| Twin & relationship CRUD | ADT REST shapes (`/digitaltwins/{id}`, relationships, components) |
| Query | ADT SQL-like dialect translated to Cypher (`POST /query`) |
| Models | DTDL v2/v3 upload, `decommission`, dependency-aware delete |
| Payloads | `$dtId`, `$metadata.$model`, `$lastUpdateTime`; opt-in `$lastUpdatedBy` |
| SDKs | any client that speaks to Azure Digital Twins over HTTP |

Extensions beyond ADT — the native `Konnektr.AgeDigitalTwins` .NET SDK, direct database
access, scoped vector memory search — are opt-in and documented separately.

---

## Components

| Component | Image | What it does |
|---|---|---|
| API | `ghcr.io/konnektr-io/graph/graph-api` | HTTP surface: twins, models, query, import jobs |
| Events | `ghcr.io/konnektr-io/graph/graph-events` | Consumes the WAL stream, publishes CloudEvents |
| Chart | `oci://ghcr.io/konnektr-io/charts/graph` | API, Events and a CloudNativePG cluster |

Deployments before the 2026 rename pull the same images from
`ghcr.io/konnektr-io/pg-age-digitaltwins/agedigitaltwins-*`; those tags remain published.

---

## Documentation

- [Getting started](https://docs.konnektr.io/docs/graph/getting-started/quickstart)
- [Concepts](https://docs.konnektr.io/docs/graph/concepts/dtdl) — DTDL, querying, validation, events
- [Self-hosted deployment](https://docs.konnektr.io/docs/graph/deployment-installation/self-host)
- [Using the Azure Digital Twins SDKs](https://docs.konnektr.io/docs/graph/how-to-guides/using-azure-digital-twins-sdks)
- [SDK reference](https://docs.konnektr.io/docs/graph/reference/sdk)

---

## Contributing

1. [Open an issue](https://github.com/konnektr-io/graph/issues) to discuss the change
2. Branch, implement, and make sure `dotnet test ./src --filter 'Category!=Integration'`
   passes locally
3. Open a pull request — the PostgreSQL 16/17/18 × stock/CloudNativePG matrix has to be green

---

## Community

- [GitHub Discussions](https://github.com/konnektr-io/graph/discussions) — questions and ideas
- [Issue tracker](https://github.com/konnektr-io/graph/issues) — bugs and feature requests

---

## License

Apache License 2.0 — see [LICENSE](LICENSE).
