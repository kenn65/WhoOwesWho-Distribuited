using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using WhoOwesWho.WebApp.CoreBusiness.Entities.Account.Users;

namespace WhoOwesWho.WebApp.Components.Common.Controls;
public partial class PaymentUsersElement
{
    [Parameter] public IEnumerable<UserModel> Users { get; set; } = Enumerable.Empty<UserModel>();
    [Parameter] public UserModel? You { get; set; }
    [Parameter] public string Id { get; set; } = string.Empty;
    [Parameter] public string Caption { get; set; } = string.Empty;
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public Expression<Func<string?>> ValueExpression { get; set; } = default!;
    [Parameter] public EventCallback<IEnumerable<string>> HandleUsers { get; set; }

    private HashSet<string> SelectedUserIdsSet { get; } = new HashSet<string>();
    private IEnumerable<string> SelectedUserIds => SelectedUserIdsSet;
    private bool IsSelected(string id) => SelectedUserIdsSet.Contains(id);
    private bool AllSelected = false;
    
    private async Task ToggleSelected(string id, bool isSelected)
    {
        if (isSelected)
        {
            SelectedUserIdsSet.Add(id);
        }
        else
        {
            SelectedUserIdsSet.Remove(id);
        }
        await HandleUsers.InvokeAsync(SelectedUserIds);
    }

    private async Task ToggleAll()
    {
        if (!AllSelected)
        {
            foreach (var user in Users)
            {
                SelectedUserIdsSet.Add(user.ProtectedId!);
            }
            AllSelected = true;
        }
        else
        {
            SelectedUserIdsSet.Clear();
            AllSelected = false;
        }
        await HandleUsers.InvokeAsync(SelectedUserIds);
    }
}
