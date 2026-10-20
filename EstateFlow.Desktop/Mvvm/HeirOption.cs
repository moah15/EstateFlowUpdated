using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstateFlow.Desktop.Mvvm;

// One tick-box row in an "Awarded to" heir list.
public class HeirOption : BaseViewModel
{
    public Guid BeneficiaryId { get; init; }
    public string FullName { get; init; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}