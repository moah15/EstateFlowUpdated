using System.Text;
using EstateFlow.Core.Models;

namespace EstateFlow.Api.Services.Letters;

// Handles vehicle-transfer letters to the Municipality Traffic Department,
// per client requirement F in the documentation.
public class TrafficDeptLetterGenerator : IDocumentGenerator
{
    public GeneratedLetter Generate(Estate estate, object sourceRecord)
    {
        if (sourceRecord is not MovableAsset asset || !asset.IsVehicle)
            throw new InvalidOperationException("TrafficDeptLetterGenerator requires a vehicle MovableAsset.");

        var sb = new StringBuilder();
        sb.AppendLine("MUNICIPALITY TRAFFIC DEPARTMENT");
        sb.AppendLine("Re: Transfer of Vehicle Ownership — Deceased Estate");
        sb.AppendLine();
        sb.AppendLine($"Estate File Number: {estate.FileNumber}");
        sb.AppendLine($"Master's Reference: {estate.MastersReference}");
        sb.AppendLine($"Deceased: {estate.Deceased?.FullName}");
        sb.AppendLine($"Date of Death: {estate.DateOfDeath:dd MMMM yyyy}");
        sb.AppendLine();
        sb.AppendLine("Vehicle Details:");
        sb.AppendLine($"  Description: {asset.Description}");
        sb.AppendLine($"  Make: {asset.Make}");
        sb.AppendLine($"  Registration Number: {asset.RegistrationNumber}");
        sb.AppendLine($"  VIN: {asset.VinNumber}");
        sb.AppendLine();
        sb.AppendLine("We kindly request that the above vehicle be transferred into the name of");
        sb.AppendLine("the deceased estate pending distribution to the appointed heir/executor,");
        sb.AppendLine("in accordance with the Administration of Estates Act 66 of 1965.");
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
            FileName = $"{estate.FileNumber}_TrafficDept_{asset.RegistrationNumber}_{DateTime.UtcNow:yyyyMMdd}.txt",
            Content = sb.ToString()
        };
    }
}