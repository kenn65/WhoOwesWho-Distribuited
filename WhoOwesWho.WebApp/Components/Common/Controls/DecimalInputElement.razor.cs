using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;
using System.Linq.Expressions;
using WhoOwesWho.WebApp.CoreBusiness.Extensions.WhoOwesWho.Shared.Extensions;
using WhoOwesWho.WebApp.Services;

namespace WhoOwesWho.WebApp.Components.Common.Controls;
public partial class DecimalInputElement(IAlertService alertService)
{
    [Parameter] public string Id { get; set; } = string.Empty;
    [Parameter] public string Caption { get; set; } = string.Empty;
    [Parameter] public decimal? Value { get; set; }
    [Parameter] public EventCallback<decimal?> ValueChanged { get; set; }
    [Parameter] public Expression<Func<decimal?>>? ValueExpression { get; set; }
    [Parameter] public string Placeholder { get; set; } = string.Empty;
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public int DecimalPlaces { get; set; } = 2;
    [Parameter] public CultureInfo? Culture { get; set; }
    [Parameter] public string CurrencySymbol { get; set; } = string.Empty;
    [CascadingParameter] private EditContext? EditContext { get; set; }

    private CultureInfo ActiveCulture => Culture ?? CultureInfo.CurrentCulture;
    private string DisplayValue = string.Empty;
    private bool IsFocused;
    private FieldIdentifier FieldIdentifier;

    protected override void OnParametersSet()
    {
        if (!IsFocused)
        {
            DisplayValue = FormatValue(Value);
        }
        FieldIdentifier = FieldIdentifier.Create(ValueExpression!);
    }

    private void HandleFocus()
    {
        IsFocused = true;
        DisplayValue = Value.HasValue
            ? Value.Value.ToString("G", ActiveCulture)
            : string.Empty;
    }

    private async Task HandleInput(string value)
    {
        if (!decimal.TryParse(NormalizeInput(value), NumberStyles.Any, ActiveCulture, out var parsed) && !string.IsNullOrWhiteSpace(value))
        {
            await alertService.Error("Invalid amount value. Please enter a valid number.");
        }
        DisplayValue = value;
        await ValueChanged.InvokeAsync(parsed);
        EditContext?.NotifyFieldChanged(FieldIdentifier);
    }


    private async Task HandleBlur()
    {
        IsFocused = false;

        if (string.IsNullOrWhiteSpace(DisplayValue))
        {
            await ValueChanged.InvokeAsync(null);
            //EditContext?.NotifyFieldChanged(FieldIdentifier);
            DisplayValue = string.Empty;
            return;
        }


        var normalized = NormalizeInput(DisplayValue);

        if (decimal.TryParse(normalized, NumberStyles.Any, ActiveCulture, out var parsed))
        {
            await ValueChanged.InvokeAsync(parsed);
            //EditContext?.NotifyFieldChanged(FieldIdentifier);
            DisplayValue = FormatValue(parsed);
        }
        else
        {
            DisplayValue = FormatValue(Value);
        }
    }


    private string NormalizeInput(string input)
    {
        return input.FormatAmount();
    }

    private string FormatValue(decimal? value)
    {
        if (!value.HasValue)
        {
            return string.Empty;
        }
        return value.Value.ToString($"N{DecimalPlaces}", ActiveCulture);
    }
}
