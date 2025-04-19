#  Crypto Rate App

A lightweight .NET Core Web API that retrieves real-time cryptocurrency prices (e.g., BTC) and converts them to multiple fiat currencies (USD, EUR, BRL, GBP, AUD). This project was built as part of a technical interview task and follows clean architecture principles.

---

##  Tech Stack

- ASP.NET Core Web API
- Clean Architecture
- HttpClient with Polly (Retry & Circuit Breaker)
- In-Memory Caching
- Serilog Logging
- xUnit, Moq, FluentAssertions (Unit Testing)
- Dependency Injection & SOLID Principles

---

##  Features

- Fetches real-time crypto price from CoinMarketCap
- Converts to multiple currencies using ExchangeRates API
- Implements resilience strategies using Polly
- In-memory caching for performance
- Structured logging with Serilog
- Fully unit tested core services
- Separated responsibilities through interfaces and layers

---


