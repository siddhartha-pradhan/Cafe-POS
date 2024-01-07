﻿using Cafe.POS.Models;
using Cafe.POS.Services;
using Microsoft.AspNetCore.Components;

namespace Cafe.POS.Components.Pages;

public partial class AddIns
{
    [CascadingParameter] 
    private GlobalState _globalState { get; set; }

    private List<AddIn> _addIns { get; set; }
    
    private bool _showUpsertAddInDialog { get; set; }

    private bool _showDeleteAddInDialog { get; set; }
    
    private AddIn? _addInModel { get; set; }
    
    private string _dialogTitle { get; set; }
    
    private string _dialogOkLabel { get; set; }
    
    private string _upsertAddInErrorMessage { get; set; }
    
    private string _orderAddInErrorMessage { get; set; }
    
    private string _deleteAddInErrorMessage { get; set; }
    
    private string _tabFilter = "All";
    
    private string _sortBy = "addIn";
    
    private string _sortDirection = "ascending";
    
    private readonly string _addInsPath = UtilityService.GetAppAddInsFilePath();

    protected override void OnInitialized()
    {
        _addIns = AddInService.GetAll(_addInsPath);
    }

    private void OpenAddAddInDialog()
    {
        _dialogTitle = "Add a new add in";

        _dialogOkLabel = "Add";

        _upsertAddInErrorMessage = "";

        _addInModel = new AddIn
        {
            Id = Guid.Empty
        };

        _showUpsertAddInDialog = true;
    }

    private void OpenEditAddInDialog(AddIn addIn)
    {
        _dialogTitle = "Edit an existing addIn";

        _dialogOkLabel = "Save";

        _upsertAddInErrorMessage = "";

        _addInModel = addIn;

        _showUpsertAddInDialog = true;
    }

    private void OpenDeleteAddInDialog(AddIn addIn)
    {
        _dialogTitle = "Delete a addIn";

        _dialogOkLabel = "Confirm";

        _addInModel = addIn;

        _showDeleteAddInDialog = true;
    }

    private void SortByHandler(string sortByName)
    {
        if (_sortBy == sortByName)
        {
            _sortDirection = _sortDirection == "ascending" ? "descending" : "ascending";
        }
        else
        {
            _sortBy = sortByName;

            _sortDirection = "ascending";
        }
    }

    private void TabFilter(string status)
    {
        _tabFilter = status;
    }

    private void SearchAddInName(ChangeEventArgs e)
    {
        var searchItem = e.Value.ToString();

        if (!string.IsNullOrEmpty(searchItem) && searchItem.Length > 2)
        {
            _addIns = AddInService.GetAll(_addInsPath).Where(p => p.Name.ToLower().Contains(searchItem.ToLower()))
                .ToList();
        }
        else
        {
            _addIns = AddInService.GetAll(_addInsPath);
        }
    }

    private void OnUpsertAddInDialogClose(bool isClosed)
    {
        if (isClosed)
        {
            _showUpsertAddInDialog = false;
        }
        else
        {
            try
            {
                _upsertAddInErrorMessage = "";

                if (_addInModel.Id == Guid.Empty)
                {
                    var addIn = new AddIn()
                    {
                        Name = _addInModel.Name,
                        Unit = _addInModel.Unit,
                        Description = _addInModel.Description,
                        Price = _addInModel.Price,
                        IsActive = true,
                        CreatedBy = _globalState.User.Id,
                    };

                    _addIns = AddInService.Create(addIn);
                }

                else
                {
                    var addIn = new AddIn()
                    {
                        Id = _addInModel.Id,
                        Name = _addInModel.Name,
                        Unit = _addInModel.Unit,
                        Description = _addInModel.Description,
                        Price = _addInModel.Price,
                        IsActive = true,
                        LastModifiedBy = _globalState.User.Id,
                        LastModifiedOn = DateTime.Now
                    };

                    _addIns = AddInService.Update(addIn);
                }

                _showUpsertAddInDialog = false;
            }
            catch (Exception e)
            {
                _upsertAddInErrorMessage = e.Message;

                Console.WriteLine(e.Message);
            }
        }
    }

    private void OnDeleteAddInDialogClose(bool isClosed)
    {
        if (isClosed)
        {
            _showDeleteAddInDialog = false;

            _addInModel = null;
        }
        else
        {
            try
            {
                _deleteAddInErrorMessage = "";

                _addIns = AddInService.Delete(_addInModel.Id);

                _showDeleteAddInDialog = false;

                _addInModel = null;
            }
            catch (Exception e)
            {
                _deleteAddInErrorMessage = e.Message;

                Console.WriteLine(e.Message);
            }
        }
    }
}