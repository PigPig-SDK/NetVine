using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core;
using System.Collections.ObjectModel;


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

     
    }
}
