# Arquitetura Hexagonal

## Objetivo

Definir a organização arquitetural da plataforma utilizando o padrão Ports and Adapters.

A aplicação deverá manter o domínio isolado de detalhes externos como banco de dados, APIs, frameworks, mensageria e infraestrutura.

---

## Princípio Central

A regra principal da arquitetura é:

- O domínio não depende de infraestrutura.
- A aplicação depende do domínio.
- A infraestrutura depende da aplicação e do domínio.
- A API depende da aplicação.
- Frameworks são detalhes externos.

---

## Camadas por Microsserviço

Cada microsserviço será organizado em quatro projetos principais:

```txt
Service.Api
Service.Application
Service.Domain
Service.Infrastructure