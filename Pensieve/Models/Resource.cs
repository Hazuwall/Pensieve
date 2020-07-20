using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Pensieve
{
    public class Resource
    {
        private string _FullName;

        public string FullName
        {
            get { return this._FullName; }
            set { this._FullName = value; }
        }
        public string Name { get { return Path.GetFileNameWithoutExtension(this._FullName); } }
        public string Extension { get { return Path.GetExtension(this._FullName); } }
        public StorageFile File { get; set; }
        public bool IsAvailable { get { return this.File!=null; } }

        public Resource(string FullName)
        {
            this._FullName = FullName;
        }
    }
}
