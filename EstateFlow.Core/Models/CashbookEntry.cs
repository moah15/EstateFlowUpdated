using System.ComponentModel.DataAnnotations;

namespace EstateFlow.Core.Models;

// One line in the estate bank-account cashbook.
//
// EntryType values:
//   "Income after death"   receipt  – interest, dividends, rent … (feeds the L&D account and the 6% fee)
//   "Capital receipt"      receipt  – proceeds of assets sold / claims collected (bank only)
//   "Administration cost"  payment  – advertising, valuations, conveyancing … (feeds the L&D account)
//   "Creditor payment"     payment  – paying an admitted liability (bank only)
//   "Distribution to heir" payment  – cash legacy / residue paid out (bank only)
//
// Only the first and third types change the L&D figures. The others keep the bank balance
// reconcilable without being counted twice.
public class CashbookEntry
{
    [Key]
    public Guid EntryId { get; set; } = Guid.NewGuid();
    public Guid EstateId { get; set; }
    public Estate? Estate { get; set; }

    public DateTime EntryDate { get; set; }
    public string EntryType { get; set; } = "Administration cost";
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PayerOrPayee { get; set; }
    public string? Reference { get; set; }
    public decimal Amount { get; set; }
    public bool IsReconciled { get; set; }
}
