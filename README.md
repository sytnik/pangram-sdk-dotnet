# Pangram .NET SDK

[![NuGet](https://img.shields.io/nuget/v/Pangram.Sdk.svg)](https://www.nuget.org/packages/Pangram.Sdk)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Pangram.Sdk.svg)](https://www.nuget.org/packages/Pangram.Sdk)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Unofficial .NET SDK for the [Pangram Labs](https://pangram.com) API — AI-generated text detection and plagiarism checking.

> **Disclaimer:** This is an independent, community-built library and is not affiliated with or endorsed by Pangram Labs.

---

## Features

| Method                          | Description                                                    |
|---------------------------------|----------------------------------------------------------------|
| `PredictShortAsync`             | Fast single-score AI/human classification                      |
| `PredictAsync`                  | Full V3 windowed analysis with AI-assistance detection         |
| `PredictWithDashboardLinkAsync` | Same as `PredictAsync` but returns a public dashboard URL      |
| `CheckPlagiarismAsync`          | Plagiarism detection against a vast database of online content |

---

## Installation

```bash
dotnet add package Pangram.Sdk
```

---

## Quick start

### With an API key string

```csharp
using Pangram.Sdk;

var client = new PangramTextClient("YOUR_API_KEY");

// Full V3 analysis
var result = await client.PredictAsync("The quick brown fox jumps over the lazy dog.");
Console.WriteLine($"Prediction: {result.PredictionShort}");
Console.WriteLine($"Fraction AI: {result.FractionAi:P0}");
```

### Via the environment variable `PANGRAM_API_KEY`

```csharp
// Set PANGRAM_API_KEY in your environment, then:
var client = new PangramTextClient();
```

### With dependency injection (`IHttpClientFactory`)

```csharp
// Program.cs / Startup.cs
services.AddHttpClient<IPangramTextClient, PangramTextClient>(client =>
{
    client.DefaultRequestHeaders.Add("x-api-key", builder.Configuration["Pangram:ApiKey"]);
    client.Timeout = TimeSpan.FromSeconds(90);
});
```

---

## API reference

### `PredictShortAsync(string text)`

Classify text as AI- or human-written. Faster and lighter than the full predict endpoint.

```csharp
PredictShortResponse response = await client.PredictShortAsync("Sample text.");
// response.AiLikelihood  → 0.0 (human) – 1.0 (AI)
// response.Prediction    → descriptive label
```

### `PredictAsync(string text, bool publicDashboardLink = false)`

Full V3 classification with per-segment windowed results and AI-assistance detection.

```csharp
PredictResponse response = await client.PredictAsync("Sample text.");

Console.WriteLine(response.PredictionShort);     // "AI" | "AI-Assisted" | "Human" | "Mixed"
Console.WriteLine(response.FractionAi);          // 0.0 – 1.0
Console.WriteLine(response.FractionAiAssisted);  // 0.0 – 1.0
Console.WriteLine(response.FractionHuman);       // 0.0 – 1.0

foreach (var window in response.Windows)
{
    Console.WriteLine($"[{window.Label}] {window.Text}");
}
```

### `PredictWithDashboardLinkAsync(string text)`

Same as `PredictAsync` with `publicDashboardLink: true`. The `DashboardLink` property will be populated.

```csharp
PredictResponse response = await client.PredictWithDashboardLinkAsync("Sample text.");
Console.WriteLine(response.DashboardLink);  // https://...
```

### `CheckPlagiarismAsync(string text)`

Check text against an online content database for plagiarism.

```csharp
PlagiarismResponse response = await client.CheckPlagiarismAsync("Sample text.");

Console.WriteLine(response.PlagiarismDetected);     // true / false
Console.WriteLine($"{response.PercentPlagiarized}% plagiarised");

foreach (var match in response.PlagiarizedContent)
{
    Console.WriteLine($"Source: {match.Source}  Similarity: {match.Similarity:P0}");
}
```

---

## Error handling

All methods throw `PangramApiException` on API errors.

```csharp
try
{
    var result = await client.PredictAsync("...");
}
catch (PangramApiException ex)
{
    Console.WriteLine($"API error {ex.StatusCode}: {ex.Message}");
}
```

---

## Targets

| Framework         | Supported |
|-------------------|-----------|
| .NET 8+           | ✅         |
| .NET Standard 2.0 | ✅         |

---

## Publishing a new version

Releases are published to [NuGet.org](https://www.nuget.org/packages/Pangram.Sdk) automatically via the included GitHub
Actions workflow (`.github/workflows/publish-nuget.yml`).

1. Add a repository secret named **`NUGET_API_KEY`** containing your NuGet.org API key.
2. Create and publish a GitHub Release tagged `v1.2.3` — the workflow strips the `v` prefix and uses it as the package
   version.
3. Alternatively, trigger the workflow manually from the **Actions** tab and supply the version.

To pack locally:

```bash
dotnet pack src/Pangram.Sdk/Pangram.Sdk.csproj -c Release /p:Version=1.0.0 -o ./nupkgs
```

---

## License

[MIT](LICENSE)
