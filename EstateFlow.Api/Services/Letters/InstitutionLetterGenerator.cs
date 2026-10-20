using System.Text;
using EstateFlow.Core.Models;

namespace EstateFlow.Api.Services.Letters;

// Handles letters to financial institutions (banks/policies), per client
// requirement F in the documentation.
public class InstitutionLetterGenerator : IDocumentGenerator
{
    public GeneratedLetter Generate(Estate estate, object sourceRecord)
    {
        if (sourceRecord is not FinancialClaim claim)
            throw new InvalidOperationException("InstitutionLetterGenerator requires a FinancialClaim.");

        var sb = new StringBuilder();
        sb.AppendLine(claim.InstitutionName);
        sb.AppendLine(claim.InstitutionAddress);
        sb.AppendLine();
        sb.AppendLine("Re: Notification of Death — Claim Against Deceased Estate");
        sb.AppendLine();
        sb.AppendLine($"Estate File Number: {estate.FileNumber}");
        sb.AppendLine($"Master's Reference: {estate.MastersReference}");
        sb.AppendLine($"Deceased: {estate.Deceased?.FullName}");
        sb.AppendLine($"Identity Number: {estate.Deceased?.IdNumber}");
        sb.AppendLine($"Date of Death: {estate.DateOfDeath:dd MMMM yyyy}");
        sb.AppendLine();
        sb.AppendLine($"Account/Policy Reference: {claim.AccountReference}");
        sb.AppendLine($"Estimated Value: R {claim.Value:N2}");
        sb.AppendLine();
        sb.AppendLine("Kindly confirm the balance held and provide the necessary documentation");
        sb.AppendLine("required to release these funds to the deceased estate.");
        sb.AppendLine();
        sb.AppendLine("Yours faithfully,");
        var signatureName =
            estate.FirmActsAsAgent && !string.IsNullOrWhiteSpace(estate.AgentName)
                ? estate.AgentName
                : "Estate Administration Office";

        sb.AppendLine(signatureName);

        if (estate.FirmActsAsAgent &&
            !string.IsNullOrWhiteSpace(estate.AgentReference))
        {
            sb.AppendLine($"Agent reference: {estate.AgentReference}");
        }

        return new GeneratedLetter
        {
            FileName = $"{estate.FileNumber}_{claim.InstitutionName.Replace(" ", "")}_{DateTime.UtcNow:yyyyMMdd}.txt",
            Content = sb.ToString()
        };
    }
}