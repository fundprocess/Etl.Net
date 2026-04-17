---
title: Examples
---

## Read PDF content with PdfReader

```csharp
using Paillave.Pdf;

using var stream = File.OpenRead("sample.pdf");
using var reader = new PdfReader(stream);

reader.Read(new MyPdfVisitor());
```
