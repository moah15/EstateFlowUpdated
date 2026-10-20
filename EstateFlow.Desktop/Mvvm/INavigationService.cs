namespace EstateFlow.Desktop.Mvvm;

public interface INavigationService
{
    void NavigateToLogin();
    void NavigateToDashboard();
    void NavigateToEstates();
    void NavigateToEstateDetail(Guid estateId);
    void NavigateToDiary();
    void NavigateToReports();
    void NavigateToDocumentScan();
    void NavigateToNewEstate();
    void NavigateToEditEstate(Guid estateId);
}