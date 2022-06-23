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

        public ObservableCollection<Entries> Entries;

        public Plans()
        {
            Entries = new ObservableCollection<Entries>();
        }


        /*public Plans(string entryName, string title, string type, string duration)
        {
            this.entryName = entryName;
            this.title = title;
            this.type = type;
            this.duration = duration;
        }

        public string entryName { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string duration { get; set; }*/
    }
}

public class Entries 
{
    public Entries()
    {

    }

    private string _entryName;
    public string entryName
    {
        get { return _entryName; }
        set { _entryName = value; }
    }

    private string _title;
    public string title
    {
        get { return _title; }
        set { _title = value; }
    }

    private string _type;
    public string type
    {
        get { return _type; }
        set { _type = value; }
    }

    private string _duration;
    public string duration
    {
        get { return _duration; }
        set { _duration = value; }
    }
}
