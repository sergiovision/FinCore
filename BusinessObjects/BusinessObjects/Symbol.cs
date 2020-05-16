using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.BusinessObjects
{
    public class Symbol: Idable
    {
        public virtual int Id { get; set; }
        public virtual int MetasymbolId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime? Expiration { get; set; }
        public virtual bool Retired { get; set; }
    }
}
