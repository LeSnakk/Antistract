using antistract.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace antistract.Core
{
    public class Plans
    {
        public ObservableCollection<EntryNames> EntryNames;


        public Plans()
        {
            EntryNames = new ObservableCollection<EntryNames>();
        }

    }
}

public class EntryNames 
{
    public EntryNames()
    {

    }

    private string _entryName;
    private bool _isChecked;
    public string entryName
    {
        get { return _entryName; }
        set { _entryName = value; }
    }
    public bool isChecked 
    { 
        get { return _isChecked; } 
        set { _isChecked = value; } 
    }
}