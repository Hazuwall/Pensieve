using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pensieve
{
    public static class EnumExtensions
    {
        public static string GetTableName(this ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Tags:
                    return "tags";
                case ResourceType.Songs:
                    return "songs";
                case ResourceType.Docs:
                    return "docs";
                default:
                    return String.Empty;
            }
        }
    }
}
