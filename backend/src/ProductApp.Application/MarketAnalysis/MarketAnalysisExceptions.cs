namespace ProductApp.Application.MarketAnalysis;

public sealed class MarketAnalysisPrerequisiteException(string message) : Exception(message);

public sealed class SapMaterialNotFoundException(string sapCode)
    : Exception($"SAP material '{sapCode}' was not found.");
