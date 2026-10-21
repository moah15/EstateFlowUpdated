using EstateFlow.Core.Dtos;

namespace EstateFlow.Api.Services.Forms;

// The Master's Office forms bundled with the API (EstateFlow.Api/FormTemplates), grouped under the
// same headings as https://www.justice.gov.za/master/forms.html
public static class FormCatalog
{
    public const string Deceased = "Deceased Estates";
    public const string Insolvent = "Insolvent Estates [Liquidations]";
    public const string Trusts = "Trusts";
    public const string Tutors = "Tutors and Curators";
    public const string Guardian = "Guardian's Fund";
    public const string Fees = "Other Forms – Fees payable to the Master";

    private const string Gpw = "This is an Adobe LiveCycle (GPW) form. It cannot be filled in here: open it blank and complete it in Adobe Reader.";
    private const string Flat = "This form has no fillable boxes. Open it blank, print it or complete it by hand.";
    private const string Info = "Information sheet. Nothing to fill in.";

    // id = template file name without ".pdf"
    private static FormSummaryDto F(string group, string id, string code, string title, bool canFill = true, string? note = null) =>
        new() { Group = group, Id = id, Code = code, Title = title, CanFill = canFill, Note = note };

    public static IReadOnlyList<FormSummaryDto> All { get; } = new List<FormSummaryDto>
    {
        // ── Deceased Estates ──
        F(Deceased, "J155", "J155", "Undertaking and acceptance of Master's directions"),
        F(Deceased, "J187-Liquidation-Inspection-GPW", "J187", "Liquidator's and Other Appointees' Notice (GPW form)", false, Gpw),
        F(Deceased, "J190", "J190", "Acceptance of trust as Executor"),
        F(Deceased, "J192", "J192", "Affidavit: Particulars of Next-of-Kin"),
        F(Deceased, "J193-GPW", "J193", "Notice to Creditors in Deceased Estates (GPW form)", false, Gpw),
        F(Deceased, "J243", "J243", "Inventory / CB47"),
        F(Deceased, "J262", "J262", "Undertaking and bond of security – by interim curator / executor / foreign executor / tutor / curator"),
        F(Deceased, "J294", "J294", "Death Notice"),
        F(Deceased, "J294_a", "J294", "Sterfkennis (Death Notice – Afrikaans)"),
        F(Deceased, "J312", "J312", "Undertaking and bond of security – by curator / executor / foreign executor / tutor / curator"),
        F(Deceased, "J468", "J468", "Undertaking and bond of security – by provisional liquidator(s) / liquidator(s)"),
        F(Deceased, "JM33_42", "JM33_42", "Section 42(2) – Application for Endorsement in terms of section 42(2) of the Administration of Estates Act"),
        F(Deceased, "ReportingAffidavit_moh", "", "Affidavit/Declaration for an appointment as Executor/Executrix"),
        F(Deceased, "Declaration_SubsistingMarriages_moh", "", "Declaration of Subsisting Marriages"),
        F(Deceased, "moh-LetterOfNomination", "", "Nomination to Act as Executor or Master's Representative (Letter of Nomination)"),
        F(Deceased, "section80", "Section 80", "Application for Endorsement in terms of section 80 of the Administration of Estates Act", false, Flat),
        F(Deceased, "Deceased-Estate-Checklist-DBN-eForm", "", "Checklist for reporting a deceased estate at the Durban Master's Office"),
        F(Deceased, "moh-MBU-9A-AnnexB", "MBU 09 (A)", "Inventory: Obtain value of bank account – Annexure B (Chief Master Directive 09/2023)"),
        F(Deceased, "moh-MBU-9B-AnnexC", "MBU 09 (B)", "Inventory: Obtain value of policy – Annexure C (Chief Master Directive 09/2023)"),
        F(Deceased, "moh-MBU-12-AnnexD", "MBU 12", "Obtaining funds from a bank for funeral purposes – Annexure D (Chief Master Directive 09/2023)"),
        F(Deceased, "moh-MBU-19-AnnexA-Affidavit-Permanent-Life-Partnership", "MBU 19", "Affidavit / Affirmation – Annexure A (permanent life partnership)"),

        // ── Insolvent Estates [Liquidations] ──
        F(Insolvent, "J28-GPW", "J28", "Insolvent Estates, Companies Or Close Corporations Sequestrated Or Wound Up Provisionally (GPW form)", false, Gpw),
        F(Insolvent, "J475", "J475", "Section 9(3) – Bond of security (Insolvency Act 24 of 1936, Companies Act 61 of 1973, Close Corporations Act 69 of 1984)"),
        F(Insolvent, "J187-Liquidation-Inspection-GPW", "J187", "Liquidation and Distribution Accounts in Deceased Estates Lying for Inspection (GPW form)", false, Gpw),
        F(Insolvent, "moh-insolv-AffidavitNEW-eForm", "", "Affidavit for persons applying to be placed on the National List of Liquidators"),
        F(Insolvent, "moh-insolv-AffidavitRENEW-eForm", "", "Renewal Affidavit for persons applying to remain on the National List of Liquidators"),
        F(Insolvent, "J29-GPW", "J29", "First meetings of Creditors, Contributories, Members or Debenture-Holders of Sequestrated Estates or Companies being wound up (GPW form)", false, Gpw),
        F(Insolvent, "J29-CC-GPW", "J29 CC", "First meetings of Creditors Members of Close Corporations being wound up (GPW form)", false, Gpw),
        F(Insolvent, "Insolvency-Form01-GPW", "Form 1", "Appointment of Trustees and Liquidators and proof of claims (GPW form)", false, Gpw),
        F(Insolvent, "Insolvency-Form02-GPW", "Form 2", "Meeting of Creditors (GPW form)", false, Gpw),
        F(Insolvent, "Insolvency-Form03-GPW", "Form 3", "Extension of time within which to lodge Liquidation Accounts and plans of distribution or contribution (GPW form)", false, Gpw),
        F(Insolvent, "Insolvency-Form04-GPW", "Form 4", "Liquidation Accounts and plans of distribution or contribution (GPW form)", false, Gpw),
        F(Insolvent, "Insolvency-Form05-GPW", "Form 5", "Payment of dividends and collection of contributions (GPW form)", false, Gpw),
        F(Insolvent, "Insolvency-Form06-GPW", "Form 6", "Application for Rehabilitation (GPW form)", false, Gpw),
        F(Insolvent, "Insolvency-Form07-GPW", "Form 7", "Notice of Trustees (GPW form)", false, Gpw),
        F(Insolvent, "Insolvency-Form08-GPW", "Form 8", "Dates fixed for Creditors to prove claims (GPW form)", false, Gpw),
        F(Insolvent, "Insolvency-Form09-GPW", "Form 9", "Notice of surrender of a Debtor's Estate (GPW form)", false, Gpw),

        // ── Trusts ──
        F(Trusts, "J401_trust-registration-amendment-eform", "J401", "Trust registration or amendment form"),
        F(Trusts, "J405_acceptance-auditor-eform", "J405", "Acceptance of Auditor"),
        F(Trusts, "J417_acceptance-trustee-eform", "J417", "Acceptance of Trusteeship"),
        F(Trusts, "moh-SwornAffTrustee", "", "Sworn Affidavit by Independent Trustee"),
        F(Trusts, "J450_beneficiaries-declaration-eform", "J450", "Beneficiaries Declaration form"),
        F(Trusts, "J262", "J262", "Undertaking and bond of security – by interim curator / executor / foreign executor / tutor / curator"),
        F(Trusts, "JM46-annex-c", "JM46", "Application for Certified Copies"),
        F(Trusts, "moh_Trusts-anxB_PrescFee", "Annexure B", "Payment of Master's Prescribed Fee – Inter Vivos Trust (Circular 90/2017)"),
        F(Trusts, "moh-NewAffidavit-NonInterest", "", "Affidavit of Non-Interest (for appointment as Trustee / Liquidator)"),
        F(Trusts, "JM21", "JM21", "Testamentary trust requirements (memorandum)", false, Flat),

        // ── Tutors and Curators ──
        F(Tutors, "J197", "J197", "Application for appointment as curator or tutor"),
        F(Tutors, "J243", "J243", "Inventory"),
        F(Tutors, "J295", "J295", "Notice of Curator and Tutor (GPW form)", false, Gpw),
        F(Tutors, "J297", "J297", "Election of executors, curators and tutors (GPW form)", false, Gpw),
        F(Tutors, "J262", "J262", "Undertaking and bond of security – by interim curator / executor / foreign executor / tutor / curator"),
        F(Tutors, "J312", "J312", "Undertaking and bond of security – by curator / executor / foreign executor / tutor / curator"),
        F(Tutors, "J344", "J344", "Undertaking and bond of security"),
        F(Tutors, "J468", "J468", "Undertaking and bond of security – by provisional liquidator(s) / liquidator(s)"),

        // ── Guardian's Fund ──
        F(Guardian, "J251-Inheritance", "J251", "Application for monies from the Guardian's Fund"),
        F(Guardian, "J251", "J251", "Application for monies from the Guardian's Fund (general)"),
        F(Guardian, "J251A-InheritanceForeignPayments", "J251A", "Foreign Application for monies from the Guardian's Fund"),
        F(Guardian, "J341-ApplicationMinor", "J341", "Application for allowance or interest"),
        F(Guardian, "J341A-ApplicationMinorForeign", "J341A", "Foreign Application for allowance or interest"),
        F(Guardian, "GF_CEFTU3", "Ceftu 3", "Bank & Personal details"),
        F(Guardian, "GF_AffidavitForm", "", "Guardian fund affidavit form"),
        F(Guardian, "GF_AllowanceReq", "", "Applications for maintenance / allowance from the guardians fund: information, requirements and documentation needed", false, Info),
        F(Guardian, "GF_AppAllORInterest_disability", "", "Application for allowance or interest (person under disability)"),
        F(Guardian, "GF_AppMonies_executor", "", "Application for monies from guardians fund by the executor / representative"),
        F(Guardian, "GF_AppMonies_LIQorTRUSTEE", "", "Application for monies from guardians fund by the liquidator / trustee"),
        F(Guardian, "GF_AppUnclaimedMonies_creditor", "", "Application for refund of unclaimed monies by a Creditor"),
        F(Guardian, "GF_AssistancePurchaseVehicle", "", "Assistance required to purchase a motor vehicle from guardians fund"),
        F(Guardian, "GF_AFFPmtNotReceived_cheque", "", "Affidavit: payment not received (cheque)"),
        F(Guardian, "GF_AFFPmtNotReceived_eft", "", "Affidavit: payment not received (EFT)"),
        F(Guardian, "GF_Indemnity", "", "Indemnity"),
        F(Guardian, "GF_InheritanceReq", "", "Applications for monies from the guardians fund: information, requirements and documentation needed", false, Info),
        F(Guardian, "GF_Investment", "", "Application for payment of money from the guardian's fund to natural guardian for investment for and on behalf of a minor", false, Flat),
        F(Guardian, "GF_NatureOfAss", "", "Nature of assistance required from guardians fund, and how arrived at"),

        // ── Other forms: fees payable to the Master ──
        F(Fees, "JM46-annex-c", "JM46", "Application for Certified Copies"),
        F(Fees, "moh_Trusts-anxB_PrescFee", "Annexure B", "Payment of Master's Prescribed Fee – Inter Vivos Trust (Circular 90/2017)"),
        F(Fees, "J034", "J34", "File Cover: Estates")
    };

    public static List<FormGroupDto> Groups() =>
        All.GroupBy(f => f.Group)
           .Select(g => new FormGroupDto { Heading = g.Key, Forms = g.ToList() })
           .ToList();

    public static FormSummaryDto? Find(string id) =>
        All.FirstOrDefault(f => string.Equals(f.Id, id, StringComparison.OrdinalIgnoreCase));
}
