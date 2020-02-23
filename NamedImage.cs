using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Pensieve
{
    public class NamedFile 
    {
        public string CustomName { get; set; }
        public StorageFile EmbeddedFile { get; set; }
        public NamedFile(StorageFile file, string name)
        {
            this.EmbeddedFile = file;
            this.CustomName = name;
        }
        public NamedFile(StorageFile file) : this(file, file.DisplayName) { }
    }
}
