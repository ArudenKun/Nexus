using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MudBlazor;
using Nexus.Authors;
using Nexus.Permissions;
using Volo.Abp.Application.Dtos;

namespace Nexus.Blazor.Client.Pages;

public partial class Authors
{
    private MudDataGrid<AuthorDto> _dataGrid = null!;
    private MudForm? _createForm;
    private MudForm? _editForm;
    private bool _createDialogVisible;
    private bool _editDialogVisible;
    private DateTime? _newEntityBirthDate;
    private DateTime? _editingEntityBirthDate;
    private const int PageSize = 10;
    private CreateUpdateAuthorDto NewEntity { get; set; } = new();
    private CreateUpdateAuthorDto EditingEntity { get; set; } = new();
    private Guid EditingEntityId { get; set; }
    private bool HasCreatePermission { get; set; }
    private bool HasUpdatePermission { get; set; }
    private bool HasDeletePermission { get; set; }

    private DialogOptions DialogOptions =>
        new()
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseOnEscapeKey = true,
            BackdropClick = false,
        };

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        HasCreatePermission = (
            await AuthorizationService.AuthorizeAsync(NexusPermissions.Authors.Create)
        ).Succeeded;
        HasUpdatePermission = (
            await AuthorizationService.AuthorizeAsync(NexusPermissions.Authors.Edit)
        ).Succeeded;
        HasDeletePermission = (
            await AuthorizationService.AuthorizeAsync(NexusPermissions.Authors.Delete)
        ).Succeeded;
    }

    private async Task<GridData<AuthorDto>> OnDataGridReadAsync(
        GridState<AuthorDto> state,
        CancellationToken cancellationToken = default
    )
    {
        var result = await AuthorAppService.GetListAsync(
            new PagedAndSortedResultRequestDto
            {
                MaxResultCount = state.PageSize,
                SkipCount = state.Page * state.PageSize,
            }
        );
        return new GridData<AuthorDto>
        {
            Items = result.Items,
            TotalItems = (int)result.TotalCount,
        };
    }

    private Task OpenCreateDialogAsync()
    {
        NewEntity = new CreateUpdateAuthorDto { BirthDate = DateTime.Now };
        _newEntityBirthDate = NewEntity.BirthDate;
        _createDialogVisible = true;
        return Task.CompletedTask;
    }

    private void CloseCreateDialog()
    {
        _createDialogVisible = false;
    }

    private async Task CreateEntityAsync()
    {
        if (_createForm != null)
        {
            await _createForm.ValidateAsync();
            if (!_createForm.IsValid)
                return;
        }

        if (_newEntityBirthDate.HasValue)
            NewEntity.BirthDate = _newEntityBirthDate.Value;
        await AuthorAppService.CreateAsync(NewEntity);
        _createDialogVisible = false;
        await InvokeAsync(() => _dataGrid.ReloadServerData());
        Snackbar.Add(L["SuccessfullySaved"], Severity.Success);
    }

    private Task OpenEditDialogAsync(AuthorDto entity)
    {
        EditingEntityId = entity.Id;
        EditingEntity = new CreateUpdateAuthorDto
        {
            Name = entity.Name,
            BirthDate = entity.BirthDate,
            ShortBio = entity.ShortBio,
        };
        _editingEntityBirthDate = EditingEntity.BirthDate;
        _editDialogVisible = true;
        return Task.CompletedTask;
    }

    private void CloseEditDialog()
    {
        _editDialogVisible = false;
    }

    private async Task UpdateEntityAsync()
    {
        if (_editForm != null)
        {
            await _editForm.ValidateAsync();
            if (!_editForm.IsValid)
                return;
        }

        if (_editingEntityBirthDate.HasValue)
            EditingEntity.BirthDate = _editingEntityBirthDate.Value;
        await AuthorAppService.UpdateAsync(EditingEntityId, EditingEntity);
        _editDialogVisible = false;
        await InvokeAsync(() => _dataGrid.ReloadServerData());
        Snackbar.Add(L["SuccessfullySaved"], Severity.Success);
    }

    private async Task DeleteEntityAsync(AuthorDto entity)
    {
        var confirmed = await DialogService.ShowMessageBoxAsync(
            L["AreYouSure"],
            string.Format(L["AreYouSureToDelete"], entity.Name),
            yesText: L["Yes"],
            cancelText: L["Cancel"]
        );
        if (confirmed != true)
            return;
        await AuthorAppService.DeleteAsync(entity.Id);
        await InvokeAsync(() => _dataGrid.ReloadServerData());
        Snackbar.Add(L["SuccessfullyDeleted"], Severity.Success);
    }

    private async Task ExportAuthorsAsync()
    {
        var token = await AuthorAppService.GetDownloadTokenAsync();
        NavigationManager.NavigateTo(
            $"/api/app/author/as-excel-file?downloadToken={Uri.EscapeDataString(token.Token)}",
            forceLoad: true
        );
    }
}
