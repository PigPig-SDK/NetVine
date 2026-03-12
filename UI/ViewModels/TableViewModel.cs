using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using static UI.ViewModels.TableViewModel;

namespace UI.ViewModels
{




    //Future improvements:
    //TODO: Simplify filtering -- better syntax, dropdown menu instead?
    //TODO: Make it so that in the live feed, cosed programs aren't seen. I guess we could do this with the seen hashSet
    public partial class TableViewModel : ViewModelBase
    {
        public TableViewModel()
        {
            _RowLookup = [];
            SystemHistory.Instance.OnSnapshotTaken += OnSnapshotLive;
            LiveViewModel.ViewChanged += _ => NotifyColumnVisibility();
            //PopulateTableWithDummyData();

        }

        
        //Main table data storage
        public ObservableCollection<TableRow> TableRows { get; set; } = [];





 


    }
}
