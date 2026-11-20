# .NET Concurrency & Multithreading Lab

A collection of **C# / .NET** modules exploring multithreading, thread synchronization, Task Parallel Library (TPL), and asynchronous programming patterns, each backed by automated unit tests.

## Modules

1. **`Synchronisation` (`FindingPrimeNumbers.sln`)**
   - Parallel **Sieve of Eratosthenes** implementation using data decomposition across segments, manual `Thread` management, and `lock`-based synchronization (`SieveOfEratosthenes.cs`).
   - Covered by `NumberTheory.Tests`.

2. **`Threads` (`PalindromeNumberFiltering.sln`)**
   - Multi-threaded sequence filtering using `Thread` primitives (`Selector.cs`) with unit tests (`PalindromeNumberFiltering.Tests`).

3. **`TPL` (`Tpl.sln`)**
   - Task lifecycle management, continuations, cancellation, and status handling using the **Task Parallel Library** (`TaskStatusHandler.cs`) with `Tpl.Tests`.

4. **`Async` (`ProductRepositoryAsync.sln`)**
   - Asynchronous repository pattern (`IProductRepository`, `ProductRepository`) with structured error handling and `ProductRepositoryAsync.Tests`.

## Tech Stack

- **Language & Runtime:** C# / .NET 8
- **Concurrency:** `System.Threading`, `lock` synchronization, TPL (`Task`), `async/await`
- **Code Quality & Testing:** Unit tests per module, StyleCop, `.editorconfig`

## Running Tests

Navigate to any module folder and run:

```bash
dotnet test
```
