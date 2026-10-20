using EstateFlow.Core.Models;

namespace EstateFlow.Api.Services.Letters;

// Factory Method pattern — picks the correct IDocumentGenerator based on
// the type of record being actioned, so new letter types (e.g. a future
// Deeds Office letter) can be added without changing any calling code.
public class LetterGeneratorFactory
{
    public IDocumentGenerator Create(string letterType)
    {
        return letterType switch
        {
            "Vehicle" => new TrafficDeptLetterGenerator(),
            "FinancialClaim" => new InstitutionLetterGenerator(),
            _ => throw new NotSupportedException($"No letter generator registered for type '{letterType}'.")
        };
    }
}