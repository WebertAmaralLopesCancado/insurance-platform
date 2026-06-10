# Visão Geral

## Objetivo

Desenvolver uma plataforma de seguros composta por microsserviços independentes responsáveis pelo gerenciamento de propostas e contratações.

A solução deverá utilizar Arquitetura Hexagonal, princípios SOLID, DDD e boas práticas de Clean Code.

## Contexto de Negócio

A plataforma permite que clientes realizem propostas de seguro.

Uma proposta poderá passar pelos seguintes estados:

- Em Análise
- Aprovada
- Rejeitada

Somente propostas aprovadas poderão ser contratadas.

## Microsserviços

### Proposal Service

Responsável por:

- Criar proposta
- Consultar propostas
- Alterar status da proposta

### Contracting Service

Responsável por:

- Contratar proposta aprovada
- Registrar contratação
- Consultar situação da proposta

## Regras de Negócio

RN001 - Toda proposta criada inicia com status Em Análise.

RN002 - Uma proposta pode ser alterada para Aprovada ou Rejeitada.

RN003 - Somente propostas Aprovadas podem ser contratadas.

RN004 - Uma proposta não pode ser contratada mais de uma vez.

RN005 - Toda contratação deve armazenar data e hora da contratação.

## Requisitos Não Funcionais

- Arquitetura Hexagonal
- Microsserviços independentes
- Persistência relacional
- APIs REST
- Testes automatizados
- Docker
- Documentação técnica
- Banco versionado via migrations