using EstateFlow.Core.Models;

namespace EstateFlow.Api.Services.Letters;

public interface IDocumentGenerator
{
    GeneratedLetter Generate(Estate estate, object sourceRecord);
}