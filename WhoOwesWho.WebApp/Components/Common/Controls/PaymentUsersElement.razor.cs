    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using System.Linq.Expressions;
    using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;
    using WhoOwesWho.WebApp.CoreBusiness.Entities.Payments;

    namespace WhoOwesWho.WebApp.Components.Common.Controls;

    public partial class PaymentUsersElement
    {
        [Parameter] public IEnumerable<UserModel> Users { get; set; } = Enumerable.Empty<UserModel>();
        [Parameter] public UserModel? You { get; set; }
        [Parameter] public string Id { get; set; } = string.Empty;
        [Parameter] public string Caption { get; set; } = string.Empty;
        [Parameter] public IEnumerable<string>? Value { get; set; }
        [Parameter] public EventCallback<IEnumerable<string>> ValueChanged { get; set; }
        [Parameter] public Expression<Func<IEnumerable<string>>>? ValueExpression { get; set; }
        [Parameter] public EventCallback<IEnumerable<string>> HandleUsers { get; set; }
        [CascadingParameter] private EditContext? EditContext { get; set; }


        private HashSet<string> Selected = new();
        private bool IsSelected(string id) => Selected.Contains(id);
        private bool AllSelected = false;
        private FieldIdentifier FieldIdentifier;

        protected override void OnParametersSet()
        {
            Selected = Value != null
                ? [.. Value]
                : [];

            FieldIdentifier = FieldIdentifier.Create(ValueExpression!);
        }

        private async Task ToggleSelected(string id, bool isSelected)
        {
            if (isSelected)
                Selected.Add(id);
            else
                Selected.Remove(id);

            var newValue = Selected.ToList();

            await ValueChanged.InvokeAsync(newValue);
            await HandleUsers.InvokeAsync(newValue);

            EditContext?.NotifyFieldChanged(FieldIdentifier);
        }

        private async Task ToggleAll()
        {
            if (!AllSelected)
            {
                Selected = Users.Select(u => u.ProtectedId!).ToHashSet();
                AllSelected = true;
            }
            else
            {
                Selected.Clear();
                AllSelected = false;
            }

            var newValue = Selected.ToList();

            await ValueChanged.InvokeAsync(newValue);
            await HandleUsers.InvokeAsync(newValue);

            EditContext?.NotifyFieldChanged(FieldIdentifier);
        }
    }
